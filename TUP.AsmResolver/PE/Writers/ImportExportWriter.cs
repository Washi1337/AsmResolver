using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TUP.AsmResolver.PE.Writers
{
    internal unsafe class ImportExportWriter : IWriterTask , IReconstructionTask
    {
        DataDirectory exportDirectory;
        DataDirectory importDirectory;
        LibraryReference[] importLibraries;
        OffsetConverter importOffsetConverter;

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
            // Might not working correctly since I haven't tested it that much.
            // TODO: Fix positions of fts.
            // TODO: 64 bit support.

            /* [PLAN] Writing structure of imports [PLAN] 
             * 
             * - Descriptors array          --> Use starting offset of library names table and add to 
             *                                  library name index. Calculate to Rva to get NameOfRva field.
             *                                  
             * - Library name tables        --> Calculate descriptors array size to get starting offset
             *                                  of array.
             *                                  
             * - OFT Tables                 --> Add to relative offsets of method hint-name table entries
             *                                  the actual starting offset and calculate to RVA to make an
             *                                  OFT value.
             *                                  
             * - <random space?>
             * 
             * - Method hint-name tables    --> Calculate size of talbe and set indexes.
             *                                  Use size of oft tables + library name tables +
             *                                  descriptor array + import dir fileoffset to get 
             *                                  correct starting offset.
             */

            importLibraries = Writer.OriginalAssembly.LibraryImports.ToArray();
            uint sizeOfLibDescriptors = (uint)(sizeof(Structures.IMAGE_IMPORT_DESCRIPTOR) * (importLibraries.Length + 1)); // descriptor count + clear terminator.

            uint[] libraryNameOffsets;
            uint sizeOfLibNames = CalculateLibraryNamesSize(out libraryNameOffsets);

            uint[,] methodNameOffsets;
            uint methodsNameSize = CalculateMethodNamesSize(out methodNameOffsets);

            uint sizeOfOfts = CalculateSizeOfOFTs();
            
            uint oftStartingOffset = importDirectory.TargetOffset.FileOffset + sizeOfLibDescriptors;
            uint libraryNameStartingOffset = oftStartingOffset + sizeOfOfts;
            uint methodNameStartingOffet = libraryNameStartingOffset + sizeOfLibNames;

            uint[,] oftsOffsets;
            UpdateOFTs(out oftsOffsets, ref methodNameOffsets, methodNameStartingOffet);
            UpdateLibraryDescriptors(libraryNameOffsets, libraryNameStartingOffset, oftsOffsets, oftStartingOffset);

            uint[,] ftsOffsets;
            uint sizeOfFts = CalculateFTsSize(out ftsOffsets);
        }

        private uint CalculateSizeOfOFTs()
        {
            uint size = 0;
            foreach (LibraryReference libRef in importLibraries)
            {
                size += (uint)(sizeof(uint) * (libRef.ImportMethods.Length + 1)); // import ofts + 0 terminator.
            }
            return size;
        }

        private uint CalculateLibraryNamesSize(out uint[] rOffsets)
        {
            rOffsets = new uint[importLibraries.Length];

            uint currentOffset = 0;
            for (int i = 0; i < importLibraries.Length; i++)
            {
                rOffsets[i] = currentOffset;
                currentOffset += (uint)Encoding.ASCII.GetBytes(importLibraries[i].LibraryName).Length + 1;// name + terminator.
            }

            return currentOffset;
        }

        private uint CalculateMethodNamesSize(out uint[,] rOffsetsByLibIndex)
        {
            int count = 0;
            foreach (LibraryReference libRef in importLibraries)
                count += libRef.ImportMethods.Length;

            rOffsetsByLibIndex = new uint[importLibraries.Length, count];
            uint currentOffset = 0; 

            for (int libIndex = 0; libIndex < importLibraries.Length; libIndex++)
            {
                for (int i = 0; i < importLibraries[libIndex].ImportMethods.Length; i++)
                {
                    rOffsetsByLibIndex[libIndex, i] = currentOffset; // save relative offset.
                    ImportMethod method = importLibraries[libIndex].ImportMethods[i];

                    currentOffset += (uint)(sizeof(ushort) + Encoding.ASCII.GetBytes(method.Name).Length + 1); // hint + data length + terminator.
                }
                //currentOffset++; // descriptor terminator.
            }

            return currentOffset;
        }

        private void UpdateOFTs(out uint[,] rOffsetsByLibIndex, ref uint[,] methodNameOffsets, uint startNameOffset)
        {
            int count = 0;
            foreach (LibraryReference libRef in importLibraries)
                count += libRef.ImportMethods.Length;

            rOffsetsByLibIndex = new uint[importLibraries.Length, count];
            uint currentOffset = 0;

            for (int libIndex = 0; libIndex < importLibraries.Length; libIndex++)
            {
                for (int i = 0; i < importLibraries[libIndex].ImportMethods.Length; i++)
                {
                    // save index.
                    rOffsetsByLibIndex[libIndex, i] = currentOffset;
                    // update oft roffset to actual file offset.
                    methodNameOffsets[libIndex, i] += startNameOffset;
                    // calculate oft (rva hint-name)
                    uint newOFT = importOffsetConverter.FileOffsetToRva(methodNameOffsets[libIndex, i]);
                    // update oft.
                    importLibraries[libIndex].ImportMethods[i].OriginalThunkValue = newOFT;

                    // add oft size.
                    currentOffset += sizeof(uint);
                }
                currentOffset += sizeof(uint); // import descriptor terminator.
            }
            //return currentOffset;
        }

        private void UpdateLibraryDescriptors(uint[] libnames, uint libNameStartingOffset, uint[,] oftOffsets, uint oftStartingOffset)
        {
            for (int i = 0; i < importLibraries.Length; i++)
            {
                uint libNameRva = importOffsetConverter.FileOffsetToRva(libNameStartingOffset + libnames[i]);
                importLibraries[i].rawDescriptor.NameRVA = libNameRva;

                uint firstOFTptr = importOffsetConverter.FileOffsetToRva(oftOffsets[i, 0] + oftStartingOffset);
                importLibraries[i].rawDescriptor.OriginalFirstThunk = firstOFTptr;
            }
        }

        private uint CalculateFTsSize(out uint[,] rOffsetsByLibIndex)
        {
            int count = 0;
            foreach (LibraryReference libRef in importLibraries)
                count += libRef.ImportMethods.Length;

            rOffsetsByLibIndex = new uint[importLibraries.Length, count];

            uint currentOffset = 0;

            for (int libIndex = 0; libIndex < importLibraries.Length; libIndex++)
            {
                for (int i = 0; i < importLibraries[libIndex].ImportMethods.Length; i++)
                {
                    currentOffset += sizeof(uint); // size of ft value.
                }
                currentOffset += sizeof(uint); // terminator
            }

            return currentOffset;
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
                        WriteFunctionValue((ulong)libRef.ImportMethods[i].ThunkValue, ref ftOffset);
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
