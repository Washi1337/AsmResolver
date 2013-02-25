using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.PE.Writers
{
    internal class ImportExportWriter : IWriterTask
    {
        DataDirectory exportDirectory;
        DataDirectory importDirectory;
        

        internal ImportExportWriter(PEWriter writer)
        {
            Writer = writer;
            exportDirectory = Writer.OriginalAssembly.NTHeader.OptionalHeader.DataDirectories[(int)DataDirectoryName.Export];
            importDirectory = Writer.OriginalAssembly.NTHeader.OptionalHeader.DataDirectories[(int)DataDirectoryName.Import];
        }

        public PEWriter Writer
        {
            get;
            private set;
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
                var exportDirHeader = Writer.OriginalAssembly.importexporttablereader.exportDirectory;
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
                foreach (LibraryReference libRef in Writer.OriginalAssembly.LibraryImports)
                {
                    Writer.WriteStructure<Structures.IMAGE_IMPORT_DESCRIPTOR>(libRef.rawDescriptor);
                }
                foreach (LibraryReference libRef in Writer.OriginalAssembly.LibraryImports)
                {
                    Writer.MoveToOffset(converter.RvaToFileOffset(libRef.rawDescriptor.NameRVA));
                    Writer.WriteAsciiZString(libRef.LibraryName);

                    uint oftOffset = converter.RvaToFileOffset(libRef.rawDescriptor.OriginalFirstThunk);
                    uint ftOffset = converter.RvaToFileOffset(libRef.rawDescriptor.FirstThunk);

                    for (int i = 0; i < libRef.ImportMethods.Length; i++)
                    {
                        WriteFunctionName(libRef.ImportMethods[i], converter);
                        WriteFunctionValue((ulong)libRef.ImportMethods[i].OriginalThunkValue, ref oftOffset);
                        WriteFunctionValue((ulong)libRef.ImportMethods[i].OriginalThunkValue, ref ftOffset);
                    }
                }
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
