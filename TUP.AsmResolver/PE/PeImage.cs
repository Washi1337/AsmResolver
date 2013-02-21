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

        internal MemoryStream stream;
        internal BinaryWriter writer;
        internal BinaryReader reader;
        internal Win32Assembly assembly;
        internal static PeImage LoadFromAssembly(Win32Assembly assembly) 
        {

            return new PeImage(assembly.path) { assembly = assembly }; 
        }

        internal PeImage(string file)
        {
            using (FileStream streamToRead = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                WriteToMemoryStream(streamToRead);
                streamToRead.Flush();
                streamToRead.Close();
                
            }

            writer = new BinaryWriter(stream);
            reader = new BinaryReader(stream);
        }

        private void WriteToMemoryStream(Stream sourceStream)
        {
            this.stream = new MemoryStream();
            byte[] buffer = new byte[0x1000];
            int byteLength = 0;
            do
            {
                byteLength = sourceStream.Read(buffer, 0, buffer.Length);
                this.stream.Write(buffer, 0, byteLength);

            } while (byteLength != 0);

            this.stream.Position = 0;

        }



        #region Write

        public void Write(int offset, byte value)
        {
            SetOffset(offset);
            writer.Write(value);
        }
        public void Write(int offset, byte[] value)
        {
            SetOffset(offset);
            writer.Write(value);
        }
        public void Write(int offset, int value)
        {
            SetOffset(offset);
            writer.Write(value);
        }
        public void Write(int offset, uint value)
        {
            SetOffset(offset);
            writer.Write(value);
        }
        public void Write(int offset, ushort value)
        {
            SetOffset(offset);
            writer.Write(value);
        }
        public void Write(int offset, long value)
        {
            SetOffset(offset);
            writer.Write(value);
        }
        public void Write(int offset, bool value)
        {
            SetOffset(offset);
            writer.Write(value);
        }
        public void Write(int offset, double value)
        {
            SetOffset(offset);
            writer.Write(value);
        }
        public void Write(int offset, float  value)
        {
            SetOffset(offset);
            writer.Write(value);
        }
        public void Write(int offset, string value, Encoding encoding)
        {
            SetOffset(offset);
            writer.Write(encoding.GetBytes(value));
        }


        #endregion

        #region Read

        public byte ReadByte()
        {
            return ReadBytes(stream.Position, 1)[0];
        }
        public byte ReadByte(int offset)
        {
            return ReadBytes(offset, 1)[0];
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
                byte[] bytes = reader.ReadBytes(Marshal.SizeOf(typeof(T)));
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
            return reader.ReadBytes(length);
        }

        public byte[] ReadBytes(int length)
        {
            return reader.ReadBytes(length);
        }

        public Stream ReadStream(int length)
        {
            return ReadStream(length, 0x1000);
        }
        public Stream ReadStream(int length, int bufferSize)
        {
            //Debugger.Break();
            long endoffset = stream.Position + length;
            Stream outputStream = new MemoryStream();
            byte[] buffer = new byte[bufferSize];
            int bytesRead = 0;
            do
            {
                long bytesToGo = endoffset - stream.Position;
                long bytesToRead = bufferSize;
                if (bytesToGo < bufferSize)
                    bytesToRead = bytesToGo;

                bytesRead = stream.Read(buffer, 0, (int)bytesToRead);
                outputStream.Write(buffer, 0, bytesRead);

            } while (stream.Position < endoffset);

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
                lastByte = ReadByte();

            } while (lastByte != 0);

            int endoffset = (int)Position - 1;

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

        #endregion

        #region Other

        public void SetOffset(long offset)
        {
            stream.Seek(offset, SeekOrigin.Begin);
        }



        #endregion

        public long Position { get { return stream.Position; } }
        public long Length { get { return stream.Length; } }
        public MemoryStream Stream { get { return stream; } }

        public void Dispose()
        {
            reader.Close();
            reader.Dispose();
            writer.Close();
            writer.Dispose();
            stream.Close();
            stream.Dispose();
            assembly = null;
        }
    }
}
