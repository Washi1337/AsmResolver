using System.Collections.Generic;
using System.Threading;
using AsmResolver.Collections;
using AsmResolver.IO;
using AsmResolver.PE.Relocations;

namespace AsmResolver.PE.Tls
{
    /// <summary>
    /// Represents the data directory containing Thread-Local Storage (TLS) data.
    /// </summary>
    public class TlsDirectory : SegmentBase
    {
        private readonly LazyVariable<TlsDirectory, IReadableSegment?> _templateData;
        private ReferenceTable? _callbackFunctions;
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

        /// <summary>
        /// Gets or sets the block of data that is used as a template to initialize TLS data.  The system copies all
        /// of this data each time a thread is created.
        /// </summary>
        public IReadableSegment? TemplateData
        {
            get => _templateData.GetValue(this);
            set => _templateData.SetValue(value);
        }

        /// <summary>
        /// The location to receive the TLS index, which the loader assigns
        /// </summary>
        public ISegmentReference Index
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a table of function callbacks that need to be called upon every thread creation.
        /// </summary>
        public ReferenceTable CallbackFunctions
        {
            get
            {
                if (_callbackFunctions is null)
                    Interlocked.CompareExchange(ref _callbackFunctions, GetCallbackFunctions(), null);
                return _callbackFunctions;
            }
        }

        /// <summary>
        /// Gets or sets the number of zero bytes that need to be appended after the template data referenced by
        /// <see cref="TemplateData"/>.
        /// </summary>
        public uint SizeOfZeroFill
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the characteristics that are assigned to this directory.
        /// </summary>
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
        protected virtual ReferenceTable GetCallbackFunctions() => new(ReferenceTableAttributes.Va | ReferenceTableAttributes.Adaptive | ReferenceTableAttributes.ZeroTerminated);

        /// <summary>
        /// Obtains a collection of base address relocations that need to be applied to the TLS data directory
        /// after the image was loaded into memory.
        /// </summary>
        /// <returns>The required base relocations.</returns>
        public IEnumerable<BaseRelocation> GetRequiredBaseRelocations()
        {
            int pointerSize = _is32Bit ? sizeof(uint) : sizeof(ulong);
            var type = _is32Bit ? RelocationType.HighLow : RelocationType.Dir64;

            var result = new List<BaseRelocation>(4 + CallbackFunctions.Count);

            // TLS directory does not have to define template data. We then don't have to define relocs for it either.
            if (TemplateData is not null)
            {
                result.Add(new BaseRelocation(type, this.ToReference(0 * pointerSize)));
                result.Add(new BaseRelocation(type, this.ToReference(1 * pointerSize)));
            }

            // TLS index and callback table addresses.
            result.Add(new BaseRelocation(type, this.ToReference(2 * pointerSize)));
            result.Add(new BaseRelocation(type, this.ToReference(3 * pointerSize)));

            // All callbacks are also VAs, so we need relocations for them as well.
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
