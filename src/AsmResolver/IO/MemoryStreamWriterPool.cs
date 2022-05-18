using System;
using System.Collections.Concurrent;
using System.IO;

namespace AsmResolver.IO
{
    /// <summary>
    /// Provides a pool of reusable instances of <see cref="BinaryStreamWriter"/> that are meant to be used for
    /// constructing byte arrays.
    /// </summary>
    public class MemoryStreamWriterPool
    {
        private readonly ConcurrentQueue<BinaryStreamWriter> _writers = new();

        /// <summary>
        /// Rents a single binary stream writer.
        /// </summary>
        /// <returns>The writer.</returns>
        public RentedWriter Rent()
        {
            if (!_writers.TryDequeue(out var writer))
                writer = new BinaryStreamWriter(new MemoryStream());

            writer.BaseStream.SetLength(0);
            return new RentedWriter(this, writer);
        }

        private void Return(BinaryStreamWriter writer) => _writers.Enqueue(writer);

        /// <summary>
        /// Represents a single instance of a <see cref="BinaryStreamWriter"/> that is rented by a writer pool.
        /// </summary>
        public readonly struct RentedWriter : IDisposable
        {
            internal RentedWriter(MemoryStreamWriterPool pool, BinaryStreamWriter writer)
            {
                Pool = pool;
                Writer = writer;
            }

            /// <summary>
            /// Gets the pool the writer was rented from.
            /// </summary>
            public MemoryStreamWriterPool Pool
            {
                get;
            }

            /// <summary>
            /// Gets the writer instance.
            /// </summary>
            public BinaryStreamWriter Writer
            {
                get;
            }

            /// <summary>
            /// Gets the data that was written to the temporary stream.
            /// </summary>
            /// <returns></returns>
            public byte[] GetData() => ((MemoryStream) Writer.BaseStream).ToArray();

            /// <inheritdoc />
            public void Dispose() => Pool.Return(Writer);
        }
    }
}
