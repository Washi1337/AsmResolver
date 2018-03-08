using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public class ArrayTypeSignature : TypeSpecificationSignature
    {
        public new static ArrayTypeSignature FromReader(MetadataImage image, IBinaryStreamReader reader)
        {
            long position = reader.Position;
            var signature = new ArrayTypeSignature(TypeSignature.FromReader(image, reader));
            
            uint rank;
            if (!reader.TryReadCompressedUInt32(out rank))
                return signature;

            uint numSizes;
            if (!reader.TryReadCompressedUInt32(out numSizes))
                return signature;

            var sizes = new uint[numSizes];
            for (int i = 0; i < numSizes; i++)
            {
                if (!reader.TryReadCompressedUInt32(out sizes[i]))
                    return signature;
            }

            uint numLoBounds;
            if (!reader.TryReadCompressedUInt32(out numLoBounds))
                return signature;

            var loBounds = new uint[numLoBounds];
            for (int i = 0; i < numLoBounds; i++)
            {
                if (!reader.TryReadCompressedUInt32(out loBounds[i]))
                    return signature;
            }

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

        public override ElementType ElementType
        {
            get { return ElementType.Array; }
        }

        public IList<ArrayDimension> Dimensions
        {
            get;
            private set;
        }

        public override string Name
        {
            get { return BaseType.Name + GetDimensionsString(); }
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
            return string.Format("{0}...{1}", low, low + size - 1);
        }
        
        public bool Validate()
        {
            var allowSizes = true;
            var allowLowBounds = true;
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

        public override uint GetPhysicalLength()
        {
            if (!Validate())
                throw new InvalidOperationException();

            var numSizes = 0u;
            var numLoBounds = 0u;
            var sizesAndLoBoundsLength = 0u;

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
                   BaseType.GetPhysicalLength() + 
                   Dimensions.Count.GetCompressedSize() +
                   numSizes.GetCompressedSize() +
                   numLoBounds.GetCompressedSize() +
                   sizesAndLoBoundsLength;
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
        }
    }
}
