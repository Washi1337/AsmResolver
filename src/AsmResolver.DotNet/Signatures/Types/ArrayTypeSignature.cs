using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Signatures.Types
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
            Dimensions = new List<ArrayDimension>(0);
        }

        /// <summary>
        /// Creates a new array type signature with the provided dimensions count.
        /// </summary>
        /// <param name="baseType">The element type.</param>
        /// <param name="dimensionCount">The number of dimensions.</param>
        public ArrayTypeSignature(TypeSignature baseType, int dimensionCount)
            : base(baseType)
        {
            if (dimensionCount < 0)
                throw new ArgumentException("Number of dimensions cannot be negative.");

            Dimensions = new List<ArrayDimension>(dimensionCount);
            for (int i = 0; i < dimensionCount;i++)
                Dimensions.Add(new ArrayDimension());
        }

        /// <summary>
        /// Creates a new array type signature with the provided dimensions count.
        /// </summary>
        /// <param name="baseType">The element type.</param>
        /// <param name="dimensions">The dimensions.</param>
        public ArrayTypeSignature(TypeSignature baseType, params ArrayDimension[] dimensions)
            : base(baseType)
        {
            Dimensions = new List<ArrayDimension>(dimensions);
        }

        /// <inheritdoc />
        public override ElementType ElementType => ElementType.Array;

        /// <inheritdoc />
        public override string Name => (BaseType?.Name ?? NullTypeToString) + GetDimensionsString();

        /// <inheritdoc />
        public override bool IsValueType => false;

        /// <summary>
        /// Gets a collection of dimensions.
        /// </summary>
        public IList<ArrayDimension> Dimensions
        {
            get;
        }

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
        
        /// <summary>
        /// Verifies that the array signature only contains dimensions that are valid.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// An array signature is valid if all bounded dimensions are in the front of the dimensions list.
        /// </remarks>
        public bool Validate()
        {
            bool allowSizes = true;
            bool allowLowBounds = true;
            for (int i = 0; i < Dimensions.Count; i++)
            {
                var dimension = Dimensions[i];
                if (dimension.Size.HasValue)
                {
                    if (!allowSizes)
                        return false;
                }
                else
                {
                    allowSizes = false;
                }

                if (dimension.LowerBound.HasValue)
                {
                    if (!allowLowBounds)
                        return false;
                }
                else
                {
                    allowLowBounds = false;
                }
            }

            return true;
        }

        /// <inheritdoc />
        public override TResult AcceptVisitor<TResult>(ITypeSignatureVisitor<TResult> visitor) => 
            visitor.VisitArrayType(this);

        /// <inheritdoc />
        protected override void WriteContents(BlobSerializationContext context)
        {
            if (!Validate())
                throw new InvalidOperationException();

            var writer = context.Writer;

            writer.WriteByte((byte) ElementType);
            WriteBaseType(context);
            writer.WriteCompressedUInt32((uint) Dimensions.Count);

            // Sized dimensions.
            var sizedDimensions = Dimensions
                .Where(x => x.Size.HasValue)
                .ToArray();
            
            writer.WriteCompressedUInt32((uint) sizedDimensions.Length);
            foreach (var sizedDimension in sizedDimensions)
                writer.WriteCompressedUInt32((uint) sizedDimension.Size.Value);

            // Bounded dimensions.
            var boundedDimensions = Dimensions
                .Where(x => x.LowerBound.HasValue)
                .ToArray();
            writer.WriteCompressedUInt32((uint) boundedDimensions.Length);
            foreach (var boundedDimension in boundedDimensions)
                writer.WriteCompressedUInt32((uint) boundedDimension.LowerBound.Value);
        }
    }
}