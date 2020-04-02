namespace AsmResolver.DotNet.Analysis.SizeCalculation
{
    /// <summary>
    /// An interface that different size calculating strategies implement
    /// </summary>
    public interface ISizeCalculationStrategy
    {
        /// <summary>
        /// Calculates the size of <paramref name="typeDescriptor"/>
        /// </summary>
        /// <param name="typeDescriptor">The <see cref="ITypeDescriptor"/> to calculate the size of</param>
        /// <param name="is32Bit"></param>
        /// <returns>The size of <paramref name="typeDescriptor"/></returns>
        int CalculateSize(ITypeDescriptor typeDescriptor, bool? is32Bit);
    }
}