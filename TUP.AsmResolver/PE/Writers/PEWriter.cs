using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace TUP.AsmResolver.PE.Writers
{

    // Pre-pre-pre alpha version =D.

    internal class PEWriter
    {
        internal PEWriter(Win32Assembly original, WritingParameters parameters)
        {
            this.OriginalAssembly = original;
            this.Parameters = parameters;

            this.Tasks = new IWriterTask[] {
                new PEReconstructor(this),     // Fix all offsets
                new PEHeaderWriter(this),       // Write vital pe headers
                new SectionWriter(this),        // Temp solution to make exe working. Need to be removed once all rebuilding is done, or maybe just write the code section(s).
                new DataDirectoryWriter(this),  // Rewrite data directories that don't need to be rebuilded.
                new ImportExportWriter(this),   // Rewrite exports and imports if specified.
                new ResourceWriter(this),       // Rewrite resources if specified.
                new ManagedDataWriter(this),    // Rewrite managed data if specified.
            };


        }


        internal Win32Assembly OriginalAssembly { get; private set; }
        internal WritingParameters Parameters { get; private set; }
        internal IWriterTask[] Tasks { get; private set; }
        internal Stream OutputStream { get; private set; }
        internal BinaryWriter BinWriter { get; private set; }


        internal void WriteExecutable(Stream outputStream)
        {
            OutputStream = outputStream;

            if (!outputStream.CanSeek || !outputStream.CanWrite)
                throw new ArgumentException("Output stream must be writable and seekable", "outputStream");

            outputStream.Seek(0, SeekOrigin.Begin);
            BinWriter = new BinaryWriter(outputStream);
            foreach (IWriterTask task in Tasks)
                task.RunProcedure();

        }

        internal void MoveToOffset(uint fileOffset)
        {
            if (fileOffset > OutputStream.Length)
            {
                OutputStream.Seek(OutputStream.Length, SeekOrigin.Begin);
                WritePaddingZeros(fileOffset);
            }
            OutputStream.Seek(fileOffset, SeekOrigin.Begin);
        }

        internal void WriteStructure<T>(T structure) where T : struct
        {
            ASMGlobals.WriteStructureToWriter<T>(BinWriter, structure);
        }
        internal void WritePaddingZeros(uint endoffset)
        {
            if (endoffset < OutputStream.Position)
                throw new ArgumentException("Padding cannot be written because the end offset is smaller than the current offset");

            BinWriter.Write(new byte[endoffset - OutputStream.Position]);
        }

        internal void WriteAsciiZString(string stringValue)
        {
            BinWriter.Write(Encoding.ASCII.GetBytes(stringValue));
            BinWriter.Write((byte)0);
        }

    }
}
