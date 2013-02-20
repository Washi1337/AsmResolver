using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TUP.AsmResolver.PE;
using TUP.AsmResolver.ASM;
using TUP.AsmResolver;
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using TUP.AsmResolver.NET;
namespace TUP.AsmResolver
{
    /// <summary>
    /// Reprensents a Win32 Assembly decompiled in 32-bit assembly code.
    /// </summary>
    public class Win32Assembly : IDisposable
    {

        #region Events
        /// <summary>
        /// A special event handler for the <see cref="TUP.AsmResolver.Win32Assembly.ReadingProcessChanged"/> event.
        /// </summary>
        /// <param name="sender">The object who send the event.</param>
        /// <param name="e">The event args of the event.</param>
        public delegate void ReadingProcessChangedEventHandler(object sender, ReadingProcessChangedEventArgs  e);
        /// <summary>
        /// A special event handler for the <see cref="TUP.AsmResolver.Win32Assembly.ReadingFinished"/> event.
        /// </summary>
        /// <param name="sender">The object who send the event.</param>
        /// <param name="e">The event args of the event.</param>
        public delegate void ReadingFinishedEventHandler(object sender, ReadingFinishedEventArgs e);

        /// <summary>
        /// This event is raised when the asynchronised instructions loading process is finished.
        /// </summary>
        public event ReadingFinishedEventHandler ReadingFinished;
        /// <summary>
        /// This event is raised when the asynchronised instructions loading process' progress is changed.
        /// </summary>
        public event ReadingProcessChangedEventHandler ReadingProcessChanged;

        private void OnReadingFinished(ReadingFinishedEventArgs e)
        {
            
            ReadingFinishedEventHandler handler = ReadingFinished;
            if (handler != null)
                synchroniser.Send(raiseReadingFinished, e);
        }
        private void OnReadingProcessChanged(ReadingProcessChangedEventArgs e)
        {
            ReadingProcessChangedEventHandler handler = ReadingProcessChanged;
            if (handler != null)
                synchroniser.Send(raiseReadingProcessChanged, e);

        }

        void raiseReadingProcessChanged(object e)
        {
            ReadingProcessChanged(this, (ReadingProcessChangedEventArgs)e);
        }
        void raiseReadingFinished(object e)
        {
            ReadingFinished(this, (ReadingFinishedEventArgs)e);
        }
        #endregion

        #region Variables
        //BinaryReader reader;

        private readonly SynchronizationContext synchroniser = SynchronizationContext.Current;
        internal PeImage peImage;
        internal bool particularmode = false;
        internal string path;
        internal NTHeader ntheader;
        internal MZHeader mzheader;
        internal NETHeader netheader;
        internal PeHeaderReader headerreader;
        internal x86Assembler assembler;
        internal Thread readingthread;
        internal ImportExportTableReader importexporttablereader;
        internal ResourcesReader resourcesreader;
        internal x86Disassembler disassembler;
        #endregion
    
        #region Properties
  
        /// <summary>
        /// Gets the assembly code processor which can write assembly instructions.
        /// </summary>
        public x86Assembler Assembler
        {
            get
            {
                return assembler;
            }
            internal set
            {
                assembler = value;
            }
        }

        public ReadingArguments ReadingArguments { get; private set; }
        /// <summary>
        /// Gets the location of the loaded assembly.
        /// </summary>
        public string Path
        {
            get
            {
                return path;
            }
        }

        /// <summary>
        /// Gets the current byte offset to read.
        /// </summary>
        public int CurrentByteOffset
        {
            get { return (int)peImage.stream.Position; }
            set { peImage.SetOffset(value); }
        }

        /// <summary>
        /// Gets the imported methods of the Win32 Assembly
        /// </summary>
        public List<LibraryReference> LibraryImports
        {
            get
            {
                return importexporttablereader.Imports;
            }
        }
        /// <summary>
        /// Gets the exports of the Win32 Assembly
        /// </summary>
        public List<ExportMethod> LibraryExports
        {
            get
            {
                return importexporttablereader.Exports;
            }
        }

        /// <summary>
        /// Gets the NT header representation of the loaded portable executable file.
        /// </summary>
        public NTHeader NTHeader
        {
            get { return ntheader; }
        }
        /// <summary>
        /// Gets the MZ header representation of the loaded portable executable file.
        /// </summary>
        public MZHeader MZHeader
        {
            get
            {
                return mzheader;
            }
        }
        /// <summary>
        /// Gets the .NET header (if available) of the loaded portable executable file. 
        /// </summary>
        public NETHeader NETHeader
        {
            get { return netheader; }
        }

        /// <summary>
        /// Gets the root resource directory containing all the resources stored in the assembly.
        /// </summary>
        public ResourceDirectory RootResourceDirectory
        {
            get { return resourcesreader.rootDirectory; }
        }


        /// <summary>
        /// Gets the disassembler of this Win32 Assembly.
        /// </summary>
        public x86Disassembler Disassembler
        {
            get { return disassembler; }
        }

        /// <summary>
        /// Gets the raw PE image
        /// </summary>
        public PeImage Image
        {
            get { return peImage; }
        }

        #endregion

        #region Public Methods

        public static Win32Assembly LoadFile(string file)
        {
            return LoadFile(file, new ReadingArguments());
        }
        /// <summary>
        /// Loads an assembly from a specific file.
        /// </summary>
        /// <param name="file">The file to read.</param>
        /// <returns></returns>
        /// <exception cref="System.BadImageFormatException"></exception>
        public static Win32Assembly LoadFile(string file, ReadingArguments arguments)
        {
            
            try
            {
                

                Win32Assembly a = new Win32Assembly();


                a.path = file;
                a.ReadingArguments = arguments;
                a.peImage = PeImage.LoadFromAssembly(a);

                a.headerreader = PeHeaderReader.FromAssembly(a);
                a.ntheader = NTHeader.FromAssembly(a);
                a.mzheader = MZHeader.FromAssembly(a);
                a.headerreader.LoadData(arguments.IgnoreDataDirectoryAmount);
                

                if (!arguments.OnlyManaged)
                {
                    a.disassembler = new x86Disassembler(a);
                    a.Assembler = new x86Assembler(a);
                    a.importexporttablereader = new ImportExportTableReader(a.ntheader);
                    a.resourcesreader = new ResourcesReader(a.ntheader);
                }
               

                a.netheader = NETHeader.FromAssembly(a);
                a.peImage.SetOffset(a.ntheader.OptionalHeader.HeaderSize);

                return a;
                
                
            }
            catch (Exception ex)
            {
                throw new BadImageFormatException("The file is not a valid Portable Executable File.", ex);
            }
           

        }

        /// <summary>
        /// Saves the assembly to the harddisk to a specific path.
        /// </summary>
        /// <param name="path">The path to save the assembly.</param>
        public void Save(string path)
        {
            File.WriteAllBytes(path, peImage.stream.ToArray());
        }
        /// <summary>
        /// Closes streams and cleans up the Win32Assembly.
        /// </summary>
        public void Dispose()
        {
           
            path = null;
            assembler = null;
            readingthread = null;

            peImage.Dispose();
            
            if (netheader != null)
                netheader.Dispose();
                
        }



        


        #endregion

        #region Private Methods

        internal Win32Assembly()
        {
        }

        #endregion

    }

 
    /// <summary>
    /// Provides data for the <see cref="TUP.AsmResolver.Win32Assembly.ReadingFinished"/> event.
    /// </summary>
    public class ReadingFinishedEventArgs
    {
        /// <summary>
        /// Creates a new instance of the ReadingFinishedEventArgs containing the parsed instructions.
        /// </summary>
        /// <param name="instructions">The instructions the assembly has read.</param>
        public ReadingFinishedEventArgs(InstructionCollection instructions)
        {
            instr = instructions;
        }
        /// <summary>
        /// Creates a new instance of the ReadingFinishedEventArgs containing error data.
        /// </summary>
        /// <param name="ex">The error that occured.</param>
        public ReadingFinishedEventArgs(Exception ex)
        {
            this.ex = ex;
        }

        private InstructionCollection instr;
        private Exception ex;
        /// <summary>
        /// The Instructions that are read from the file. This value is null when the Error property is not null.
        /// </summary>
        public InstructionCollection Instructions
        {
            get
            {
                return instr;
            }
        }
        /// <summary>
        /// The error that had occured. This value is null when the Instructions property is not null.
        /// </summary>
        public Exception Error
        {
            get
            {
                return ex;
            }
        }

    }
    /// <summary>
    /// Provides data for the <see cref="TUP.AsmResolver.Win32Assembly.ReadingProcessChanged"/> event.
    /// </summary>
    public class ReadingProcessChangedEventArgs
    {
        /// <summary>
        /// Creates a new instance of the ReadingProcessChangedEventArgs containing information about the new instruction and offset.
        /// </summary>
        /// <param name="totallength">The total lenght of the bytes.</param>
        /// <param name="currentoffset">The current offset of the bytes.</param>
        /// <param name="NewInstruction">The new instruction that is read from the bytes.</param>
        public ReadingProcessChangedEventArgs(long totallength, int currentoffset, x86Instruction NewInstruction)
        {
            filelength  = totallength ;
            current = currentoffset;
            newinstruction = NewInstruction;
        }


        private long filelength;
        private int current;
        private x86Instruction newinstruction;
        /// <summary>
        /// The new instruction that is read from the bytes.
        /// </summary>
        public x86Instruction NewInstruction
        {
            get
            {
                return newinstruction;
            }
        }
        /// <summary>
        /// The current offset of the bytes.
        /// </summary>
        public int CurrentOffset
        {
            get
            {
                return current;
            }
        }
        /// <summary>
        /// The total lenght of the bytes of the corresponding <see cref="TUP.AsmResolver.Win32Assembly"/>.
        /// </summary>
        public long TotalLength
        {
            get
            {
                return filelength;
            }
        }


    }
}
