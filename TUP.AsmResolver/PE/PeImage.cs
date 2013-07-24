using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;

#pragma warning disable 1591

namespace TUP.AsmResolver.PE
{
    /// <summary>
    /// A class to read from and write to an executable image.
    /// </summary>
    public class PeImage : IDisposable
    {

        public MemoryStream Stream { get; private set; }
        public BinaryWriter Writer { get; private set; }
        public BinaryReader Reader { get; private set; }
        public Win32Assembly ParentAssembly { get; private set; }
        public long Position { get { return Stream.Position; } }
        public long Length { get { return Stream.Length; } }

        internal static PeImage LoadFromAssembly(Win32Assembly assembly) 
        {
            return new PeImage(assembly._path) { ParentAssembly = assembly }; 
        }

        internal PeImage(string file)
        {
            using (FileStream streamToRead = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                WriteToMemoryStream(streamToRead);
                streamToRead.Flush();
                streamToRead.Close();
            }
        }

        internal void WriteToMemoryStream(Stream sourceStream)
        {
            this.Stream = new MemoryStream();
            byte[] buffer = new byte[0x1000];
            int byteLength = 0;
            do
            {
                byteLength = sourceStream.Read(buffer, 0, buffer.Length);
                this.Stream.Write(buffer, 0, byteLength);

            } while (byteLength != 0);

            this.Stream.Position = 0;

            Writer = new BinaryWriter(Stream);
            Reader = new BinaryReader(Stream);

        }

        public void Write(int offset, string value, Encoding encoding)
        {
            SetOffset(offset);
            Writer.Write(encoding.GetBytes(value));
        }

        public T ReadStructure<T>()
        {
            return ReadStructure<T>(Position);
        }

        public T ReadStructure<T>(long offset)
        {
            SetOffset(offset);
            try
            {
                byte[] bytes = Reader.ReadBytes(Marshal.SizeOf(typeof(T)));
                GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                T theStructure = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
                handle.Free();
                return theStructure;
            }
            catch
            {
                return default(T);
            }
        }

        public byte[] ReadBytes(long offset, int length)
        {
            SetOffset(offset);
            return Reader.ReadBytes(length);
        }

        public byte[] ReadBytes(int length)
        {
            return Reader.ReadBytes(length);
        }

        public Stream ReadStream(int length)
        {
            return ReadStream(length, 0x1000);
        }

        public Stream ReadStream(int length, int bufferSize)
        {
            //Debugger.Break();
            long endoffset = Stream.Position + length;
            Stream outputStream = new MemoryStream();
            byte[] buffer = new byte[bufferSize];
            int bytesRead = 0;
            do
            {
                long bytesToGo = endoffset - Stream.Position;
                long bytesToRead = bufferSize;
                if (bytesToGo < bufferSize)
                    bytesToRead = bytesToGo;

                bytesRead = Stream.Read(buffer, 0, (int)bytesToRead);
                outputStream.Write(buffer, 0, bytesRead);

            } while (Stream.Position < endoffset);

            outputStream.Position = 0;
            
            return outputStream;
            

        }

        public string ReadZeroTerminatedString(uint offset)
        {
            return ReadByteTerminatedString(offset, 0);
        }

        public string ReadByteTerminatedString(uint offset, byte stopByte)
        {
            SetOffset(offset);
            byte lastByte = 0;
            do
            {
                if (Stream.Position >= Stream.Length)
                    break;
                lastByte = Reader.ReadByte();

            } while (lastByte != stopByte);

            int endoffset = (int)Position - 1;

            if (endoffset <= offset)
                return string.Empty;

            SetOffset(offset);

            return System.Text.Encoding.ASCII.GetString(ReadBytes(endoffset - (int)offset));
        }

        public string ReadASCIIString(long offset)
        {
            SetOffset(offset);
            List<byte> bytes = new List<byte>();
            bytes.Add(ReadBytes(offset, 1)[0]);
            int i = 1;
            while (bytes.Last() != 0)
            {
                bytes.Add(ReadBytes(offset + i, 1)[0]);
                i++;
            }
            return Encoding.ASCII.GetString(bytes.ToArray());
        }

        public void SetOffset(long offset)
        {
            Stream.Seek(offset, SeekOrigin.Begin);
        }

        public bool TrySetOffset(long offset)
        {
            try
            {
                if (!ContainsOffset(offset))
                    return false;
                Stream.Seek(offset, SeekOrigin.Begin);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ContainsOffset(long offset)
        {
            return (offset >= 0 && offset < Stream.Length);
        }

        public void Dispose()
        {
            Reader.Close();
            Reader.Dispose();
            Writer.Close();
            Writer.Dispose();
            Stream.Close();
            Stream.Dispose();
            ParentAssembly = null;
        }
    }
}
