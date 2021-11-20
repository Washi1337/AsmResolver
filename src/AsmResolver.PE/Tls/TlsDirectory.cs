using System.Threading;

namespace AsmResolver.PE.Tls
{
    /// <summary>
    /// Provides a basic implementation of the <see cref="ITlsDirectory"/> interface.
    /// </summary>
    public class TlsDirectory : ITlsDirectory
    {
        private readonly LazyVariable<IReadableSegment?> _templateData;
        private TlsCallbackCollection? _callbackFunctions;

        /// <summary>
        /// Initializes a new empty TLS data directory.
        /// </summary>
        public TlsDirectory()
        {
            _templateData = new LazyVariable<IReadableSegment?>(GetTemplateData);
            Index = SegmentReference.Null;
        }

        /// <inheritdoc />
        public IReadableSegment? TemplateData
        {
            get => _templateData.Value;
            set => _templateData.Value = value;
        }

        /// <inheritdoc />
        public ISegmentReference Index
        {
            get;
            set;
        }

        /// <inheritdoc />
        public TlsCallbackCollection CallbackFunctions
        {
            get
            {
                if (_callbackFunctions is null)
                    Interlocked.CompareExchange(ref _callbackFunctions, GetCallbackFunctions(), null);
                return _callbackFunctions;
            }
        }

        /// <inheritdoc />
        public uint SizeOfZeroFill
        {
            get;
            set;
        }

        /// <inheritdoc />
        public TlsCharacteristics Characteristics
        {
            get;
            set;
        }

        /// <inheritdoc />
        public ulong ImageBase
        {
            get;
            set;
        }

        /// <inheritdoc />
        public bool Is32Bit
        {
            get;
            set;
        }

        /// <summary>
        /// Obtains the block of template data.
        /// </summary>
        /// <returns>The template data.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="TemplateData"/> property.
        /// </remarks>
        protected virtual IReadableSegment? GetTemplateData() => null;

        /// <summary>
        /// Obtains the table of callback functions.
        /// </summary>
        /// <returns>The callback functions.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="CallbackFunctions"/> property.
        /// </remarks>
        protected virtual TlsCallbackCollection GetCallbackFunctions() => new(this);
    }
}
