using System.Collections.Generic;
using System.Threading;
using AsmResolver.IO;
using AsmResolver.PE.Relocations;

namespace AsmResolver.PE.Tls
{
    /// <summary>
    /// Provides a basic implementation of the <see cref="ITlsDirectory"/> interface.
    /// </summary>
    public class TlsDirectory : SegmentBase, ITlsDirectory
    {
        private readonly LazyVariable<TlsDirectory, IReadableSegment?> _templateData;
        private TlsCallbackCollection? _callbackFunctions;
        private ulong _imageBase = 0x00400000;
        private bool _is32Bit = true;

        /// <summary>
        /// Initializes a new empty TLS data directory.
        /// </summary>
        public TlsDirectory()
        {
            _templateData = new LazyVariable<TlsDirectory, IReadableSegment?>(x => x.GetTemplateData());
            Index = SegmentReference.Null;
        }

        /// <inheritdoc />
        public IReadableSegment? TemplateData
        {
            get => _templateData.GetValue(this);
            set => _templateData.SetValue(value);
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
        public override void UpdateOffsets(in RelocationParameters parameters)
        {
            _imageBase = parameters.ImageBase;
            _is32Bit = parameters.Is32Bit;
            base.UpdateOffsets(in parameters);
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

        /// <inheritdoc />
        public IEnumerable<BaseRelocation> GetRequiredBaseRelocations()
        {
            int pointerSize = _is32Bit ? sizeof(uint) : sizeof(ulong);
            var type = _is32Bit ? RelocationType.HighLow : RelocationType.Dir64;

            var result = new List<BaseRelocation>(4 + CallbackFunctions.Count);
            for (int i = 0; i < 4; i++)
                result.Add(new BaseRelocation(type, this.ToReference(i * pointerSize)));
            for (int i = 0; i < CallbackFunctions.Count; i++)
                result.Add(new BaseRelocation(type, CallbackFunctions.ToReference(i * pointerSize)));

            return result;
        }

        /// <inheritdoc />
        public override uint GetPhysicalSize()
        {
            int pointerSize = _is32Bit ? sizeof(uint) : sizeof(ulong);
            return (uint) (pointerSize * 4 + 2 * sizeof(uint));
        }

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            ulong imageBase = _imageBase;
            bool is32Bit = _is32Bit;

            if (TemplateData is { } data)
            {
                writer.WriteNativeInt(imageBase + data.Rva, is32Bit);
                writer.WriteNativeInt(imageBase + data.Rva + data.GetPhysicalSize(), is32Bit);
            }
            else
            {
                writer.WriteNativeInt(0, is32Bit);
                writer.WriteNativeInt(0, is32Bit);
            }

            writer.WriteNativeInt(imageBase + Index.Rva, is32Bit);
            writer.WriteNativeInt(imageBase + CallbackFunctions.Rva, is32Bit);
            writer.WriteUInt32(SizeOfZeroFill);
            writer.WriteUInt32((uint) Characteristics);
        }
    }
}
