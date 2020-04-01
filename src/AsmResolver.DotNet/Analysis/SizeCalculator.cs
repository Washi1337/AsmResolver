using System;
using System.Linq;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Analysis
{
    /// <summary>
    /// Statically computes sizes of Types 
    /// </summary>
    public static class SizeCalculator
    {
        /// <summary>
        /// Calculates the size of a <see cref="TypeDefinition"/>
        /// </summary>
        /// <param name="typeDefinition">The <see cref="TypeDefinition"/> to calculate the size of</param>
        /// <returns>The size of <paramref name="typeDefinition"/></returns>
        /// <exception cref="NotSupportedException">It isn't possible to compute the size statically</exception>
        public static int CalculateSize(TypeDefinition typeDefinition)
        {
            if (!typeDefinition.IsValueType)
            {
                throw new NotSupportedException();
            }

            return typeDefinition.Fields.Sum(f => CalculateSize(f.Signature.FieldType));
        }

        /// <summary>
        /// Calculates the size of a <see cref="TypeSignature"/>
        /// </summary>
        /// <param name="typeSignature">The <see cref="TypeSignature"/> to calculate the size of</param>
        /// <returns>The size of <paramref name="typeSignature"/></returns>
        /// <exception cref="NotSupportedException">It isn't possible to compute the size statically</exception>
        public static int CalculateSize(TypeSignature typeSignature)
        {
            switch (typeSignature.ElementType)
            {
                case ElementType.Boolean:
                case ElementType.I1:
                case ElementType.U1:
                {
                    return 1;
                }

                case ElementType.Char:
                case ElementType.I2:
                case ElementType.U2:
                {
                    return 2;
                }

                case ElementType.I4:
                case ElementType.U4:
                case ElementType.R4:
                {
                    return 4;
                }

                case ElementType.I8:
                case ElementType.U8:
                case ElementType.R8:
                {
                    return 8;
                }
                
                case ElementType.Enum:
                {
                    return CalculateSize(typeSignature.GetUnderlyingTypeDefOrRef().ToTypeSignature());
                }
                
                case ElementType.ValueType:
                {
                    return CalculateSize(typeSignature.Resolve());
                }
                
                default:
                {
                    throw new NotSupportedException();
                }
            }
        }

        /// <summary>
        /// Calculates the size of a <see cref="TypeSpecification"/>
        /// </summary>
        /// <param name="typeSpecification">The <see cref="TypeSpecification"/> to calculate the size of</param>
        /// <returns>The size of <paramref name="typeSpecification"/></returns>
        public static int CalculateSize(TypeSpecification typeSpecification)
        {
            return CalculateSize(typeSpecification.Signature);
        }
    }
}