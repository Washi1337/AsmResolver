using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using System.IO;


namespace TUP.AsmResolver.PE.Readers
{
    internal unsafe class ImportExportTableReader
    {

        private readonly List<ExportMethod> exports = new List<ExportMethod>();
        public List<ExportMethod> Exports { get { return exports; } }

        private readonly List<LibraryReference> imports = new List<LibraryReference>();
        public List<LibraryReference> Imports { get { return imports; } }

        private readonly List<Section> _sections = new List<Section>();
        public List<Section> Sections { get { return _sections; } }
        
        NTHeader header;
        PeImage image;
        DataDirectory importDataDir;
        OffsetConverter offsetConverter;
        internal Structures.IMAGE_EXPORT_DIRECTORY exportDirectory;

        public ImportExportTableReader(NTHeader header)
        {
            this.header = header;
            image = header._assembly._peImage;
            LoadExports();
            LoadImports();
        }

        private void LoadExports()
        {
            // TODO: Unnamed exports (detect exports with only an ordinal).

            string libraryname = header._assembly._path.Substring(header._assembly._path.LastIndexOf('\\') + 1);
            DataDirectory exportdatadir = header.OptionalHeader.DataDirectories[(int)DataDirectoryName.Export];

            if (exportdatadir._targetOffset.FileOffset == 0)
                return;

            image.SetOffset(exportdatadir.TargetOffset.FileOffset);

            exportDirectory = image.ReadStructure<Structures.IMAGE_EXPORT_DIRECTORY>();
            
            OffsetConverter offsetConverter = new OffsetConverter(exportdatadir.Section);
            uint functionoffset = offsetConverter.RvaToFileOffset(exportDirectory.AddressOfFunctions);
            uint functionnameoffset = offsetConverter.RvaToFileOffset(exportDirectory.AddressOfNames);
            uint functionnameordinaloffset = offsetConverter.RvaToFileOffset(exportDirectory.AddressOfNameOrdinals);

            for (uint i = 0; i < exportDirectory.NumberOfFunctions; i++)
            {
                image.SetOffset(functionoffset);
                uint functionRVA = image.Reader.ReadUInt32();
                image.SetOffset(functionnameoffset);
                uint functionNameRVA = image.Reader.ReadUInt32();
                image.SetOffset(functionnameordinaloffset);
                uint functionNameOrdinal = image.Reader.ReadUInt32();

                string name = image.ReadZeroTerminatedString(offsetConverter.RvaToFileOffset(functionNameRVA));

                exports.Add(new ExportMethod(libraryname, name, functionNameRVA, functionRVA, (ushort)(i + exportDirectory.Base)));

                functionoffset += 4;
                functionnameoffset += 4;
                functionnameordinaloffset += 4;

            }

        }

        private void LoadImports()
        {
            importDataDir = header.OptionalHeader.DataDirectories[(int)DataDirectoryName.Import];
            
            if (importDataDir.TargetOffset.FileOffset != 0)
            {
                offsetConverter = new OffsetConverter(importDataDir.Section);
                image.SetOffset(importDataDir.TargetOffset.FileOffset);
                
                LibraryReference libraryRef = null;
                
                while (true)
                {

                    libraryRef = ReadLibraryImport();

                    if (libraryRef == null)
                        break;
                    else
                    {
                        foreach (ImportMethod method in libraryRef.ImportMethods)
                            method.ParentLibrary = libraryRef;
                        imports.Add(libraryRef);
                    }
                }
            }

        }

        private LibraryReference ReadLibraryImport()
        {
            uint importDirOffset = (uint)image.Position;
            var rawImportDir = image.ReadStructure<Structures.IMAGE_IMPORT_DESCRIPTOR>();

            if (ASMGlobals.IsEmptyStructure(rawImportDir)) 
                return null;

            string libName = ReadLibraryName(rawImportDir);
            ImportMethod[] methods = ReadImportMethods(rawImportDir);
            
            LibraryReference libReference = new LibraryReference(image, importDirOffset, rawImportDir, libName, methods);
           
            // advance to next datadir.
            image.SetOffset(importDirOffset + sizeof(Structures.IMAGE_IMPORT_DESCRIPTOR));

            return libReference;
        }

        private string ReadLibraryName(Structures.IMAGE_IMPORT_DESCRIPTOR rawImportDir)
        {
            uint nameoffset = offsetConverter.RvaToFileOffset(rawImportDir.NameRVA);
            return image.ReadZeroTerminatedString(nameoffset);
           

        }

        private ImportMethod[] ReadImportMethods(Structures.IMAGE_IMPORT_DESCRIPTOR rawImportDir)
        {
            List<ImportMethod> methods = new List<ImportMethod>();

            int currentIndex = 0;
            uint baseoffset = offsetConverter.RvaToFileOffset(rawImportDir.OriginalFirstThunk);
            uint baseft= offsetConverter.RvaToFileOffset(rawImportDir.FirstThunk);

            while (true)
            {
                
                uint methodOffset = 0;
                uint ftOffset = 0;

                ulong ofunction = ReadFunctionValue(baseoffset, currentIndex, out methodOffset);
                ulong ft = ReadFunctionValue(baseft, currentIndex, out ftOffset);

                if (ofunction == 0 && ft == 0)
                    break;

                ushort hint = 0;
                string name = ReadFunctionName(ofunction, out hint);

                uint rva = (uint)(rawImportDir.FirstThunk + (currentIndex *(header.OptionalHeader.Is32Bit ? sizeof(uint) : sizeof(ulong))));
                methods.Add(new ImportMethod((uint)ofunction, (uint)ft, rva , hint, name));
                
                //advance to next function value
                image.SetOffset(methodOffset + (header.OptionalHeader.Is32Bit ? sizeof(uint) : sizeof(ulong)));
                currentIndex++;
            }
            return methods.ToArray();
        }

        private ulong ReadFunctionValue(uint baseOffset, int currentIndex, out uint offset)
        {
            if (baseOffset == 0)
            {
                offset = 0;
                return 0;
            }

            if (!header.OptionalHeader.Is32Bit)
            {
                image.SetOffset(baseOffset + (currentIndex * sizeof(ulong)));
                offset = (uint)image.Position;
                return image.Reader.ReadUInt64();
            }
            else
            {
                image.SetOffset(baseOffset + (currentIndex * sizeof(uint)));
                offset = (uint)image.Position;
                return image.Reader.ReadUInt32();
            }
        }
        private string ReadFunctionName(ulong ofunction, out ushort hint)
        {
            if (ofunction >> 63 == 1)
            {
                hint = (ushort)(ofunction - (0x8 << 64));
                return string.Empty;

            }
            if (ofunction >> 31 == 1)
            {
                hint = (ushort)(ofunction - 0x80000000);
                return string.Empty;
            }
            else
            {
                if (ofunction > 0 && image.TrySetOffset(offsetConverter.RvaToFileOffset((uint)ofunction)))
                {
                    hint = image.Reader.ReadUInt16();
                    return image.ReadZeroTerminatedString((uint)image.Position);
                }
                hint = 0;
                return string.Empty;
            }
        }
    
       // private void LoadImports()
       // {
       //    
       //     DataDirectory importdatadir = header.OptionalHeader.DataDirectories[(int)DataDirectoryName.Import];
       //     if (importdatadir.TargetOffset.FileOffset == 0)
       //         return;
       //     image.SetOffset(importdatadir.TargetOffset.FileOffset);
       //
       //     uint currentimportdiroffset = (uint)image.stream.Position;
       //     var currentimportdir = image.ReadStructure<Structures.IMAGE_IMPORT_DESCRIPTOR>();
       //
       //
       //     while (currentimportdir.OriginalFirstThunk != 0)
       //     {
       //         //read name
       //         uint nameoffset = currentimportdir.Name - importdatadir.Section.RVA + importdatadir.Section.RawOffset;
       //         image.SetOffset(nameoffset);
       //         string name = "";
       //         name += image.reader.ReadChar();
       //         while (name[name.Length - 1] != '\0')
       //             name += image.reader.ReadChar();
       //         name = name.Substring(0, name.Length - 1);
       //
       //         //add lib ref
       //         LibraryReference library = new LibraryReference(currentimportdir.OriginalFirstThunk, name);
       //         imports.Add(library);
       //
       //         
       //         uint counter = 0;
       //
       //         // get first thunk
       //         uint thunkoffset = currentimportdir.OriginalFirstThunk - importdatadir.Section.RVA + importdatadir.Section.RawOffset;
       //         image.stream.Seek(thunkoffset, SeekOrigin.Begin);
       //
       //         uint addressofdata = image.reader.ReadUInt32();
       //         
       //         //get all thunks
       //         while (addressofdata != 0)
       //         {
       //             //ordinal
       //             uint hintoffset = addressofdata - importdatadir.Section.RVA + importdatadir.Section.RawOffset;
       //             ushort hint = 0;
       //             if (addressofdata < 0x80000000)
       //             {
       //                 image.stream.Seek(hintoffset, SeekOrigin.Begin);
       //
       //                 hint = image.reader.ReadUInt16();
       //             }
       //
       //             //name
       //             string thunkname = "";
       //             thunkname += image.reader.ReadChar();
       //             while (thunkname[thunkname.Length - 1] != '\0')
       //                 thunkname += image.reader.ReadChar();
       //             thunkname = thunkname.Substring(0, thunkname.Length - 1);
       //
       //             //add to list
       //             library.Methods.Add(new ImportMethod(library, currentimportdir.FirstThunk + (counter * 4), addressofdata, hint, thunkname));
       //
       //
       //             //step to next thunk
       //             counter++;
       //             thunkoffset += sizeof(int);
       //             image.stream.Seek(thunkoffset, SeekOrigin.Begin);
       //             addressofdata = image.reader.ReadUInt32();
       //         }
       //
       //
       //
       //
       //         //step to next import descriptor
       //         currentimportdiroffset += (uint)sizeof(Structures.IMAGE_IMPORT_DESCRIPTOR);
       //         image.stream.Seek(currentimportdiroffset, SeekOrigin.Begin);
       //         currentimportdir = image.ReadStructure<Structures.IMAGE_IMPORT_DESCRIPTOR>();
       //
       //
       //     }
       //
       //    // var hMod = (void*)loadedImage.MappedAddress;
       //    //
       //    // if (hMod != null)
       //    // {
       //    //
       //    //     uint size;
       //    //     var pImportDir =
       //    //         (Structures.IMAGE_IMPORT_DESCRIPTOR*)
       //    //         CommonAPIs.ImageDirectoryEntryToData(hMod, false,
       //    //                                             Constants.IMAGE_DIRECTORY_ENTRY_IMPORT, out size);
       //    //     if (pImportDir != null)
       //    //     {
       //    //         while (pImportDir->OriginalFirstThunk != 0)
       //    //         {
       //    //             try
       //    //             {
       //    //                 var szName = (char*)CommonAPIs.RvaToVa(loadedImage, pImportDir->Name);
       //    //                 string name = Marshal.PtrToStringAnsi((IntPtr)szName);
       //    //
       //    //               
       //    //                 var pr = new LibraryReference(pImportDir->OriginalFirstThunk, name, new List<MethodReference>());
       //    //                 _imports.Add(pr);
       //    //
       //    //
       //    //                 var pThunkOrg = (Structures.THUNK_DATA*)CommonAPIs.RvaToVa(loadedImage, pImportDir->OriginalFirstThunk);
       //    //
       //    //                 int counter = 0;
       //    //                 while (pThunkOrg->AddressOfData != IntPtr.Zero)
       //    //                 {
       //    //                     
       //    //                     uint ord;
       //    //
       //    //                     if ((pThunkOrg->Ordinal & 0x80000000) > 0)
       //    //                     {
       //    //                         ord = pThunkOrg->Ordinal & 0xffff;
       //    //                     }
       //    //                     else
       //    //                     {
       //    //                         var pImageByName =
       //    //                             (Structures.IMAGE_IMPORT_BY_NAME*)CommonAPIs.RvaToVa(loadedImage, pThunkOrg->AddressOfData);
       //    //
       //    //                         if (
       //    //                             !CommonAPIs.IsBadReadPtr(pImageByName, (uint)sizeof(Structures.IMAGE_IMPORT_BY_NAME)))
       //    //                         {
       //    //                             
       //    //                             ord = pImageByName->Hint;
       //    //                             var szImportName = pImageByName->Name;
       //    //                             string sImportName = Marshal.PtrToStringAnsi((IntPtr)szImportName);
       //    //                            
       //    //                             MethodReference a = new MethodReference(pr, (uint)(pImportDir->FirstThunk + (counter * 4)), pThunkOrg->Function, ord, sImportName);
       //    //                             pr.Methods.Add(a);
       //    //                         }
       //    //                         else
       //    //                         {
       //    //                             Log("Bad ReadPtr Detected or EOF on Imports");
       //    //                             break;
       //    //                         }
       //    //                     }
       //    //                     counter++;
       //    //                     pThunkOrg++;
       //    //                 }
       //    //             }
       //    //             catch (AccessViolationException e)
       //    //             {
       //    //                 Log("An Access violation occured\n" +
       //    //                                     "this seems to suggest the end of the imports section\n");
       //    //                 Log(e.ToString());
       //    //             }
       //    //
       //    //             pImportDir++;
       //    //         }
       //    //
       //    //     }
       //    //
       //    // }
       // }


    }

}