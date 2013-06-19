using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUP.AsmResolver.PE.Writers
{
    public class PEConstructor 
    {
        public event LogEventHandler LogEvent;

        private Stream currentStream;
        internal BinaryWriter currentWriter;
        internal RebuildingTask[] Tasks;

        public PEConstructor(Win32Assembly assembly)
        {
            OriginalAssembly = assembly;

            Tasks = new RebuildingTask[]
            {
                new PreparationTask(this),
                new MetaDataBuilderTask(this),
                new MsilMethodBuilder(this),
            };
        }

        public Win32Assembly OriginalAssembly 
        {
            get;
            private set; 
        }

        public Stream OutputStream
        {
            get { return currentStream; }
            set
            {
                currentStream = value;
                currentWriter = new BinaryWriter(currentStream);
            }
        }

        public void RebuildAndWrite(WritingParameters parameters)
        {
            Workspace workspace = new Workspace(parameters);
            
            foreach (RebuildingTask task in Tasks)
            {   
                task.RunProcedure(workspace);
            }
        }

        internal void Log(string message)
        {
            Log(message, LogMessageType.Message, null);
        }

        internal void Log(string message, LogMessageType type)
        {
            Log(message, type, null);
        }

        internal virtual void Log(string message, LogMessageType type, Exception ex)
        {
            if (LogEvent != null)
                LogEvent(this, new LogEventArgs(message, type, ex));
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
            ASMGlobals.WriteStructureToWriter(currentWriter, structure);
        }

        internal void WritePaddingZeros(uint endoffset)
        {
            if (endoffset < OutputStream.Position)
                throw new ArgumentException("Padding cannot be written because the end offset is smaller than the current offset");

            currentWriter.Write(new byte[endoffset - OutputStream.Position]);
        }

        internal void WriteAsciiZString(string stringValue)
        {
            currentWriter.Write(Encoding.ASCII.GetBytes(stringValue));
            currentWriter.Write((byte)0);
        }
    }

    public delegate void LogEventHandler(object sender, LogEventArgs e);

    public class LogEventArgs : EventArgs
    {
        public LogEventArgs(string message)
            :this(message,LogMessageType.Message, null)
        {
        }

        public LogEventArgs(string message, LogMessageType type)
            : this(message, type, null)
        {
        }

        public LogEventArgs(string message, LogMessageType type, Exception exception)
        {
            Message = message;
            Type = type;
            Exception =exception;
        }

        public string Message {get; private set;}
        public LogMessageType Type {get; private set;}
        public Exception Exception {get; private set;}

    }

    public enum LogMessageType
    {
        Message,
        Warning,
        Error
    }
}
