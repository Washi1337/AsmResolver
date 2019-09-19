using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
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
        /// <summary>
        /// Reads a single array type signature at the current position of the provided stream reader.
        /// </summary>
        /// <param name="image">The image the array was defined in.</param>
        /// <param name="reader">The reader to use.</param>
        /// <returns>The read array.</returns>
        public static ArrayTypeSignature FromReader(MetadataImage image, IBinaryStreamReader reader)
        {
            return FromReader(image, reader, new RecursionProtection());
        }
        
        /// <summary>
        /// Reads a single array type signature at the current position of the provided stream reader.
        /// </summary>
        /// <param name="image">The image the array was defined in.</param>
        /// <param name="reader">The reader to use.</param>
        /// <param name="protection">The recursion protection that is used to detect malicious loops in the metadata.</param>
        /// <returns>The read array.</returns>
        public static ArrayTypeSignature FromReader(MetadataImage image, IBinaryStreamReader reader, RecursionProtection protection)
        {
            var signature = new ArrayTypeSignature(TypeSignature.FromReader(image, reader, false, protection));

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
                var dimension = new ArrayDimension();
                if (i < numSizes)
                    dimension.Size = (int)sizes[i];
                if (i < numLoBounds)
                    dimension.LowerBound = (int)loBounds[i];
                signature.Dimensions.Add(dimension);
            }
            
            return signature;
        }

        public ArrayTypeSignature(TypeSignature baseType)
            : base(baseType)
        {
            Dimensions = new List<ArrayDimension>();
        }

        /// <inheritdoc />
        public override ElementType ElementType => ElementType.Array;

        /// <summary>
        /// Gets a collection of dimensions this array type defines.
        /// </summary>
        public IList<ArrayDimension> Dimensions
        {
            get;
        }

        /// <inheritdoc />
        public override string Name => BaseType.Name + GetDimensionsString();

        public override TypeSignature InstantiateGenericTypes(IGenericContext context)
        {
            var arrayType = new ArrayTypeSignature(BaseType.InstantiateGenericTypes(context));
            foreach (var dimension in Dimensions)
                arrayType.Dimensions.Add(new ArrayDimension(dimension.Size, dimension.LowerBound));
            return arrayType;
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
            foreach (var dimension in Dimensions)
            {
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

        public override uint GetPhysicalLength(MetadataBuffer buffer)
        {
            if (!Validate())
                throw new InvalidOperationException();

            uint numSizes = 0u;
            uint numLoBounds = 0u;
            uint sizesAndLoBoundsLength = 0u;

            foreach (var dimension in Dimensions)
            {
                if (dimension.Size.HasValue)
                {
                    numSizes++;
                    sizesAndLoBoundsLength += dimension.Size.Value.GetCompressedSize();
                }
                if (dimension.LowerBound.HasValue)
                {
                    numLoBounds++;
                    sizesAndLoBoundsLength += dimension.LowerBound.Value.GetCompressedSize();
                }
            }

            return sizeof (byte) +
                   BaseType.GetPhysicalLength(buffer) + 
                   Dimensions.Count.GetCompressedSize() +
                   numSizes.GetCompressedSize() +
                   numLoBounds.GetCompressedSize() +
                   sizesAndLoBoundsLength + 
                   base.GetPhysicalLength(buffer);
        }

        public override void Prepare(MetadataBuffer buffer)
        {
        }

        public override void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            if (!Validate())
                throw new InvalidOperationException();

            writer.WriteByte((byte)ElementType);
            BaseType.Write(buffer, writer);
            writer.WriteCompressedUInt32((uint)Dimensions.Count);

            var sizedDimensions = Dimensions.Where(x => x.Size.HasValue).ToArray();
            writer.WriteCompressedUInt32((uint)sizedDimensions.Length);
            foreach (var sizedDimension in sizedDimensions)
                writer.WriteCompressedUInt32((uint)sizedDimension.Size.Value);

            var boundedDimensions = Dimensions.Where(x => x.LowerBound.HasValue).ToArray();
            writer.WriteCompressedUInt32((uint)boundedDimensions.Length);
            foreach (var boundedDimension in boundedDimensions)
                writer.WriteCompressedUInt32((uint)boundedDimension.LowerBound.Value);

            base.Write(buffer, writer);
        }
    }
}
