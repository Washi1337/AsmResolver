using System.Linq;

namespace AsmResolver.DotNet.Analysis.SizeCalculation
{
    /// <summary>
    /// Aligns every field while summing them up
    /// </summary>
    public sealed class AlignEachCalculationStrategy : ISizeCalculationStrategy
    {
        /// <summary>
        /// Creates a new instance of <see cref="AlignEachCalculationStrategy"/> with
        /// the provided <paramref name="alignment"/>
        /// </summary>
        /// <param name="alignment">The alignment to align by</param>
        public AlignEachCalculationStrategy(int alignment)
        {
            _alignment = alignment;
        }

        private readonly int _alignment;
        
        /// <inheritdoc />
        public int CalculateSize(ITypeDescriptor typeDescriptor, bool is32Bit)
        {
            return typeDescriptor.Resolve().Fields.Select(f =>
                (int)((uint)f.Signature.FieldType.CalculateSize(is32Bit)).Align((uint)_alignment)
            ).Sum();
        }
    }
}