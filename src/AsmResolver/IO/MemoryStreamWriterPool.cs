using System;
using System.Collections.Concurrent;
using System.IO;

namespace AsmResolver.IO
{
    /// <summary>
    /// Provides a pool of reusable instances of <see cref="AsmResolver.IO.BinaryStreamWriter"/> that are meant to be used for
    /// constructing byte arrays.
    /// </summary>
    /// <remarks>
    /// This class is thread-safe. All threads are allowed to rent and return writers from this pool simultaneously.
    /// </remarks>
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
        /// Represents a single instance of a <see cref="AsmResolver.IO.BinaryStreamWriter"/> that is rented by a writer pool.
        /// </summary>
        public ref struct RentedWriter
        {
            private bool _isDisposed = false;
            private readonly BinaryStreamWriter _writer;

            internal RentedWriter(MemoryStreamWriterPool pool, BinaryStreamWriter writer)
            {
                Pool = pool;
                _writer = writer;
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
                get
                {
                    if (_isDisposed)
                        throw new ObjectDisposedException(nameof(Writer));
                    return _writer;
                }
            }

            /// <summary>
            /// Gets the data that was written to the temporary stream.
            /// </summary>
            /// <returns></returns>
            public byte[] GetData() => ((MemoryStream) Writer.BaseStream).ToArray();

            /// <summary>
            /// Returns the stream writer to the pool.
            /// </summary>
            public void Dispose()
            {
                if (_isDisposed)
                    return;

                Pool.Return(Writer);
                _isDisposed = true;
            }
        }
    }
}
