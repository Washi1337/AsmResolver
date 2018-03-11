
using System;
using AsmResolver.Net.Cts;

namespace AsmResolver.Net
{
    public static class NetAssemblyFactory
    {
        public static WindowsAssembly CreateAssembly(string name, bool isDll)
        {
            var assembly = new WindowsAssembly();
            assembly.RelocationDirectory = new ImageRelocationDirectory();
            InitializeNtHeaders(assembly.NtHeaders, isDll);
            InitializeNetDirectory(assembly.NetDirectory = new ImageNetDirectory());
            assembly.ImportDirectory.ModuleImports.Add(CreateMscoreeImport(isDll));

            var image = assembly.NetDirectory.MetadataHeader.LockMetadata();

            image.Assembly.Name = name;
            image.Assembly.Version = new Version(1, 0, 0, 0);

            var mainModule = new ModuleDefinition(name + (isDll ? ".dll" :".exe"));
            mainModule.Mvid = Guid.NewGuid();
            image.Assembly.Modules.Add(mainModule);

            mainModule.TopLevelTypes.Add(new TypeDefinition(null, "<Module>"));

            assembly.NetDirectory.MetadataHeader.UnlockMetadata();

            return assembly;
        }

        private static ImageModuleImport CreateMscoreeImport(bool isDll)
        {
            var module = new ImageModuleImport("mscoree.dll");
            module.SymbolImports.Add(new ImageSymbolImport(new HintName(0, isDll ? "_CorDllMain" : "_CorExeMain")));
            return module;
        }

        private static void InitializeNtHeaders(ImageNtHeaders headers, bool isDll)
        {
            headers.Signature = 0x00004550;
            InitializeFileHeader(headers.FileHeader, isDll);
            InitializeOptionalHeader(headers.OptionalHeader);
        }

        private static void InitializeOptionalHeader(ImageOptionalHeader optionalHeader)
        {
            optionalHeader.Magic = OptionalHeaderMagic.Pe32;
            optionalHeader.ImageBase = 0x400000;
            optionalHeader.SectionAlignment = 0x2000;
            optionalHeader.FileAlignment = 0x200;
            optionalHeader.MajorOperatingSystemVersion = 4;
            optionalHeader.MajorSubsystemVersion = 4;
            optionalHeader.MajorLinkerVersion = 6;
            optionalHeader.Subsystem = ImageSubSystem.WindowsCui;
            optionalHeader.SizeOfStackReserve = 0x100000;
            optionalHeader.SizeOfStackCommit = 0x1000;
            optionalHeader.SizeOfHeapReserve = 0x100000;
            optionalHeader.SizeOfHeapCommit = 0x1000;
            optionalHeader.NumberOfRvaAndSizes = 0x10;
            optionalHeader.DllCharacteristics = ImageDllCharacteristics.DynamicBase |
                ImageDllCharacteristics.NxCompat |
                ImageDllCharacteristics.NoSeh |
                ImageDllCharacteristics.TerminalServerAware;

            for (int i = 0; i < 0x10; i++)
                optionalHeader.DataDirectories.Add(new ImageDataDirectory());

        }

        private static void InitializeFileHeader(ImageFileHeader fileHeader, bool isDll)
        {
            fileHeader.Machine = ImageMachineType.I386;
            fileHeader.SizeOfOptionalHeader = 0xE0;
            fileHeader.Characteristics = ImageCharacteristics.Image | ImageCharacteristics.LineNumsStripped |
                                         ImageCharacteristics.LocalSymsStripped | ImageCharacteristics.Machine32Bit;
            if (isDll)
                fileHeader.Characteristics |= ImageCharacteristics.Dll;
        }
        
        private static void InitializeNetDirectory(ImageNetDirectory directory)
        {
            directory.Cb = 0x48;
            directory.MajorRuntimeVersion = 2;
            directory.Flags = ImageNetDirectoryFlags.IlOnly;
            InitializeMetadata(directory.MetadataHeader);
        }

        private static void InitializeMetadata(MetadataHeader header)
        {
            header.Signature = 0x424A5342;
            header.MajorVersion = 1;
            header.MinorVersion = 1;
            header.VersionLength = 0xC;
            header.VersionString = "v4.0.30319";
            header.Flags = 0;

            var tableStream = new TableStream
            {
                SortedBitVector = 0x000016003325FA00,
                MajorVersion = 2,
                Reserved2 = 1
            };

            header.StreamHeaders.Add(new MetadataStreamHeader("#~", tableStream));
            header.StreamHeaders.Add(new MetadataStreamHeader("#Strings", new StringStream()));
            header.StreamHeaders.Add(new MetadataStreamHeader("#US", new UserStringStream()));
            header.StreamHeaders.Add(new MetadataStreamHeader("#GUID", new GuidStream()));
            header.StreamHeaders.Add(new MetadataStreamHeader("#Blob", new BlobStream()));
        }
    }
}
