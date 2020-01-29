using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet.Builder;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Represents a single (complex) array type signature, which encodes a variable amount of array dimensions,
    /// as well as their sizes and lower bounds.
    /// </summary>
    /// <remarks>
    /// For simple single-dimension arrays, use <see cref="SzArrayTypeSignature"/> instead.
    /// </remarks>
    public class ArrayTypeSignature : TypeSpecificationSignature
    {
        internal new static ArrayTypeSignature FromReader(ModuleDefinition parentModule, IBinaryStreamReader reader, 
            RecursionProtection protection)
        {
            var signature = new ArrayTypeSignature(TypeSignature.FromReader(parentModule, reader, protection));

            // Rank
            if (!reader.TryReadCompressedUInt32(out uint rank))
                return signature;

            // Sizes.
            if (!reader.TryReadCompressedUInt32(out uint numSizes))
                return signature;

            var sizes = new List<uint>();
            for (int i = 0; i < numSizes; i++)
            {
                if (!reader.TryReadCompressedUInt32(out uint size))
                    return signature;
                sizes.Add(size);
            }

            // Lower bounds.
            if (!reader.TryReadCompressedUInt32(out uint numLoBounds))
                return signature;

            var loBounds = new List<uint>();
            for (int i = 0; i < numLoBounds; i++)
            {
                if (!reader.TryReadCompressedUInt32(out uint bound))
                    return signature;
                loBounds.Add(bound);
            }

            // Create dimensions.
            for (int i = 0; i < rank; i++)
            {
                int? size = null, lowerBound = null;
                
                if (i < numSizes)
                    size = (int)sizes[i];
                if (i < numLoBounds)
                    lowerBound= (int)loBounds[i];

                signature.Dimensions.Add(new ArrayDimension(size ,lowerBound));
            }
            
            return signature;
        } 
        
        /// <summary>
        /// Creates a new array type signature.
        /// </summary>
        /// <param name="baseType">The element type.</param>
        public ArrayTypeSignature(TypeSignature baseType)
            : base(baseType)
        {
        }

        /// <inheritdoc />
        public override ElementType ElementType => ElementType.Array;

        /// <inheritdoc />
        public override string Name => BaseType.Name + GetDimensionsString();

        /// <inheritdoc />
        public override bool IsValueType => false;

        /// <summary>
        /// Gets a collection of dimensions.
        /// </summary>
        public IList<ArrayDimension> Dimensions
        {
            get;
        } = new List<ArrayDimension>();

        private string GetDimensionsString()
        {
            return "[" + string.Join(",", Dimensions.Select(x =>
            {
                if (x.LowerBound.HasValue)
                {
                    if (x.Size.HasValue)
                        return FormatDimensionBounds(x.LowerBound.Value, x.Size.Value);
                    return x.LowerBound.Value + "...";
                }
                if (x.Size.HasValue)
                    return FormatDimensionBounds(0, x.Size.Value);
                return string.Empty;
            })) + "]";
        }

        private static string FormatDimensionBounds(int low, int size)
        {
            return $"{low}...{low + size - 1}";
        }

        protected override void WriteContents(IBinaryStreamWriter writer, ITypeCodedIndexProvider provider)
        {
            base.WriteContents(writer, provider);
        }
    }
}