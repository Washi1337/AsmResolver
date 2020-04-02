using System.Linq;

namespace AsmResolver.DotNet.Analysis.SizeCalculation
{
    /// <summary>
    /// Returns the size of the type's biggest field, this is needed for types with an explicit layout
    /// </summary>
    public sealed class BiggestSizeCalculationStrategy : ISizeCalculationStrategy
    {
        /// <inheritdoc />
        public int CalculateSize(ITypeDescriptor typeDescriptor, bool? is32Bit)
        {
            return typeDescriptor.Resolve().Fields.Select(f =>
                f.FieldOffset.GetValueOrDefault() + f.Signature.FieldType.CalculateSize(is32Bit)
            ).Max();
        }
    }
}