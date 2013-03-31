using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace TUP.AsmResolver.PE.Writers
{
    internal unsafe class ImportExportWriter : IWriterTask , IReconstructionTask, ICalculationTask
    {
        
        DataDirectory exportDirectory;
        DataDirectory importDirectory;
        LibraryReference[] importLibraries;
        OffsetConverter importOffsetConverter;

        Table<Structures.IMAGE_IMPORT_DESCRIPTOR> importDescriptors;
        Table<string> libNameTable;
        Table<HintName> functionNameTable;
        Table<uint?> oftTable;
        Table<uint?> ftTable;

        internal ImportExportWriter(PEWriter writer)
        {
            Writer = writer;
            exportDirectory = Writer.OriginalAssembly.NTHeader.OptionalHeader.DataDirectories[(int)DataDirectoryName.Export];
            importDirectory = Writer.OriginalAssembly.NTHeader.OptionalHeader.DataDirectories[(int)DataDirectoryName.Import];
            importOffsetConverter = new OffsetConverter(importDirectory.targetSection);
        }


        public PEWriter Writer
        {
            get;
            private set;
        }


        public uint NewSize
        {
            get;
            private set;
        }

        public void CalculateOffsetsAndSizes()
        {
            importLibraries = Writer.OriginalAssembly.LibraryImports.ToArray();
            NewSize = (uint)(sizeof(Structures.IMAGE_IMPORT_DESCRIPTOR) * (importLibraries.Length + 1)); // descriptor count + clear terminator.
            importDirectory.rawDataDir.Size = NewSize;
        }

        public void Reconstruct()
        {
            if (Writer.Parameters.RebuildExportTable)
                RebuildExports();
            if (Writer.Parameters.RebuildImportTable)
                RebuildImports();
        }

        private void RebuildExports()
        {
            // TODO...
        }

        private void RebuildImports()
        {
            // Might not work correctly since I haven't tested it that much.
            // TODO: Fix positions of fts.
            // TODO: 64 bit support.

            /* [PLAN] Writing structure of imports [PLAN] 
             * 
             * - Descriptors table
             *      - Uses emptry structure terminator
             * - Library name table
             * - Function name table
             * - Original thunk value table 
             *      - Uses null terminators
             * - Thunk value table
             *      - Uses null terminators
             */

            importDescriptors = CollectLibDescriptors();
            importDescriptors.StartingRVA = importDirectory.TargetOffset.Rva;

            libNameTable = new Table<string>();
            functionNameTable = new Table<HintName>();
            oftTable = new Table<uint?>();
            ftTable = new Table<uint?>();

            
            foreach (LibraryReference libRef in Writer.OriginalAssembly.LibraryImports)
            {
                libNameTable.Add(libRef.LibraryName);
                AddNamesToTable(ref functionNameTable, libRef);
                Addx86FtsToTable(ref ftTable, libRef);

                // create space to be filled in later after name rva calculation
                for (int i = 0; i < libRef.ImportMethods.Length; i++)
                    oftTable.Add(0);

                // add terminator oft.
                oftTable.Add(null);
            }

            // add starting rvas to every table.
            Table[] tables = new Table[] {importDescriptors, libNameTable, functionNameTable, oftTable, ftTable };
            CalculateRvas(ref tables, importDescriptors.StartingRVA);

            // set ofts to rvas of function names.
            UpdateOfts(ref oftTable, functionNameTable);

            // update rvas in descriptors.
            UpdateLibraryRvas(ref importDescriptors, libNameTable, oftTable, ftTable);

        }

        private Table<Structures.IMAGE_IMPORT_DESCRIPTOR> CollectLibDescriptors()
        {
            Table<Structures.IMAGE_IMPORT_DESCRIPTOR> descriptors = new Table<Structures.IMAGE_IMPORT_DESCRIPTOR>();
            foreach (LibraryReference libRef in Writer.OriginalAssembly.LibraryImports)
                descriptors.Add(libRef.rawDescriptor);

            descriptors.Add(default(Structures.IMAGE_IMPORT_DESCRIPTOR));
            return descriptors;
        }

        private void AddNamesToTable(ref Table<HintName> sourceTable, LibraryReference libRef)
        {
            foreach (ImportMethod method in libRef.ImportMethods)
                sourceTable.Add(new HintName(method.Ordinal, method.Name));
        }

        private void Addx86FtsToTable(ref Table<uint?> sourceTable, LibraryReference libRef)
        {
            foreach (ImportMethod method in libRef.ImportMethods)
                sourceTable.Add(method.ThunkValue);

            sourceTable.Add(null); // terminator
        }

        private void CalculateRvas(ref Table[] tables, uint startingRva)
        {
            foreach (Table table in tables)
            {
                table.StartingRVA = startingRva;
                table.AddStartingRvas();
                startingRva += table.CalculateSize();
            }
        }

        private void UpdateOfts(ref Table<uint?> sourceTable, Table<HintName> names)
        {
            uint[] funcNameRvas = names.Items.Keys.ToArray();

            var items = sourceTable.Items.ToArray();
            sourceTable.Items.Clear();

            int currentIndex = 0;
            foreach (var item in items)
            {
                // check if terminator, don't increment name index.
                if (!item.Value.HasValue)
                    sourceTable.Items.Add(item.Key, item.Value);
                else
                {
                    sourceTable.Items.Add(item.Key, funcNameRvas[currentIndex]);
                    currentIndex++;
                }
            }
        }

        private void UpdateLibraryRvas(
            ref Table<Structures.IMAGE_IMPORT_DESCRIPTOR> descriptors, 
            Table<string> nameTable,
            Table<uint?> oftTable,
            Table<uint?> ftTable)
        {
            uint[] libNameRvas = nameTable.Items.Keys.ToArray();
            uint[] ofts = oftTable.Items.Keys.ToArray();
            uint[] fts = ftTable.Items.Keys.ToArray();

            var items = descriptors.Items.ToArray();
            descriptors.Items.Clear();

            int currentIndex = 0;
            int lastOFTIndex = 0;
            int lastFTIndex = 0;
            foreach (var item in items)
            {
                var descriptor = item.Value;
                if (ASMGlobals.IsEmptyStructure(descriptor))
                    descriptors.Items.Add(item.Key, item.Value); // don't process terminator descriptor.
                else
                {
                    descriptor.NameRVA = libNameRvas[currentIndex];
                    descriptor.OriginalFirstThunk = ofts[lastOFTIndex];
                    descriptor.FirstThunk = fts[lastFTIndex];

                    descriptors.Items.Add(item.Key, descriptor);

                    currentIndex++;
                    lastOFTIndex = GetNextTerminatorIndex(oftTable, lastOFTIndex) + 1;
                    lastFTIndex = GetNextTerminatorIndex(ftTable, lastFTIndex) + 1;
                }
            }

        }

        private int GetNextTerminatorIndex(Table<uint?> table, int previousIndex)
        {
            uint?[] values = table.Items.Values.ToArray();
            for (int i = previousIndex; i < values.Length; i++)
                if (!values[i].HasValue)
                    return i;
            return -1;
        }

        public void RunProcedure()
        {
            if (Writer.Parameters.RebuildExportTable)
                WriteExports();
            if (Writer.Parameters.RebuildImportTable)
                WriteImports();
        }

        private void WriteExports()
        {
            if (exportDirectory.TargetOffset.FileOffset != 0)
            {
                OffsetConverter converter = new OffsetConverter(exportDirectory.Section);

                Writer.MoveToOffset(exportDirectory.TargetOffset.FileOffset);
                var exportDirHeader = Writer.OriginalAssembly.importExportTableReader.exportDirectory;
                Writer.WriteStructure<Structures.IMAGE_EXPORT_DIRECTORY>(exportDirHeader);

                uint functionOffset = converter.RvaToFileOffset(exportDirHeader.AddressOfFunctions);
                uint functionNameOffset = converter.RvaToFileOffset(exportDirHeader.AddressOfNames);
                uint functionNameOrdinalOffset = converter.RvaToFileOffset(exportDirHeader.AddressOfNameOrdinals);


                for (int i = 0; i < exportDirHeader.NumberOfFunctions; i++)
                {
                    // TODO: methods with only ordinals.

                    ExportMethod method = Writer.OriginalAssembly.LibraryExports[i];
                    Writer.MoveToOffset(functionOffset);
                    Writer.BinWriter.Write(method.RVA);

                    Writer.MoveToOffset(functionNameOffset);
                    Writer.BinWriter.Write(method.nameRva);

                    uint nameOffset = converter.RvaToFileOffset(method.nameRva);
                    Writer.MoveToOffset(nameOffset);
                    Writer.WriteAsciiZString(method.Name);

                    Writer.MoveToOffset(functionNameOrdinalOffset);
                    Writer.BinWriter.Write(method.Ordinal);

                    functionOffset += 4;
                    functionNameOffset += 4;
                    functionNameOrdinalOffset += 4;
                }

            }
        }

        private void WriteImports()
        {
            if (importDirectory.TargetOffset.FileOffset != 0)
            {
                OffsetConverter converter = new OffsetConverter(importDirectory.Section);
                Writer.MoveToOffset(importDirectory.TargetOffset.FileOffset);

                foreach (var libRef in importDescriptors.Items)
                {
                    Writer.MoveToOffset(converter.RvaToFileOffset(libRef.Key));
                    Writer.WriteStructure<Structures.IMAGE_IMPORT_DESCRIPTOR>(libRef.Value);
                }
                foreach (var libName in libNameTable.Items)
                {
                    Writer.MoveToOffset(converter.RvaToFileOffset(libName.Key));
                    Writer.WriteAsciiZString(libName.Value);
                }
                foreach (var funcName in functionNameTable.Items)
                {
                    Writer.MoveToOffset(converter.RvaToFileOffset(funcName.Key));
                    Writer.BinWriter.Write(funcName.Value.Hint);
                    Writer.WriteAsciiZString(funcName.Value.Name);
                }
                foreach (var oftValue in oftTable.Items)
                {
                    Writer.MoveToOffset(converter.RvaToFileOffset(oftValue.Key));
                    if (oftValue.Value.HasValue)
                        Writer.BinWriter.Write(oftValue.Value.Value);
                    else
                        Writer.BinWriter.Write((uint)0);
                }
                foreach (var ftValue in ftTable.Items)
                {
                    Writer.MoveToOffset(converter.RvaToFileOffset(ftValue.Key));
                    if (ftValue.Value.HasValue)
                        Writer.BinWriter.Write(ftValue.Value.Value);
                    else
                        Writer.BinWriter.Write((uint)0);
                }

                //foreach (LibraryReference libRef in Writer.OriginalAssembly.LibraryImports)
                //{
                //    Writer.WriteStructure<Structures.IMAGE_IMPORT_DESCRIPTOR>(libRef.rawDescriptor);
                //}
                //foreach (LibraryReference libRef in Writer.OriginalAssembly.LibraryImports)
                //{
                //    Writer.MoveToOffset(converter.RvaToFileOffset(libRef.rawDescriptor.NameRVA));
                //    Writer.WriteAsciiZString(libRef.LibraryName);
                //
                //    uint oftOffset = converter.RvaToFileOffset(libRef.rawDescriptor.OriginalFirstThunk);
                //    uint ftOffset = converter.RvaToFileOffset(libRef.rawDescriptor.FirstThunk);
                //
                //    for (int i = 0; i < libRef.ImportMethods.Length; i++)
                //    {
                //        WriteFunctionName(libRef.ImportMethods[i], converter);
                //        WriteFunctionValue((ulong)libRef.ImportMethods[i].OriginalThunkValue, ref oftOffset);
                //      //  WriteFunctionValue((ulong)libRef.ImportMethods[i].ThunkValue, ref ftOffset);
                //    }
                //}
            }
        }

        private void WriteFunctionValue(ulong value, ref uint offset)
        {
            Writer.MoveToOffset(offset);
            if (Writer.OriginalAssembly.NTHeader.OptionalHeader.Is32Bit)
            {
                Writer.BinWriter.Write((uint)value);
                offset += 4;
            }
            else
            {
                Writer.BinWriter.Write(value);
                offset += 8;
            }
        }

        private void WriteFunctionName(ImportMethod method, OffsetConverter converter)
        {
            if (method.OriginalThunkValue >> 63 != 1 && method.OriginalThunkValue >> 31 != 1)
            {
                uint nameOffset = converter.RvaToFileOffset(method.OriginalThunkValue);
                Writer.MoveToOffset(nameOffset);
                Writer.BinWriter.Write(method.Ordinal);
                Writer.WriteAsciiZString(method.Name);
            }
        }


    }
}
