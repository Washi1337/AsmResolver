﻿using System;
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
        /// <param name="is32Bit">Whether the parent <see cref="ModuleDefinition"/> is 32 bit</param>
        /// <returns>The size of <paramref name="typeDefinition"/></returns>
        /// <exception cref="SizeCalculationException">It isn't possible to compute the size statically</exception>
        public static int CalculateSize(TypeDefinition typeDefinition, bool? is32Bit = null)
        {
            if (!typeDefinition.IsValueType)
            {
                if (is32Bit.HasValue)
                {
                    return is32Bit.Value ? 4 : 8;
                }
                
                throw new SizeCalculationException("Cannot calculate the size of a reference type");
            }

            return typeDefinition.Fields.Sum(f => CalculateSize(f.Signature.FieldType, is32Bit));
        }

        /// <summary>
        /// Calculates the size of a <see cref="TypeSignature"/>
        /// </summary>
        /// <param name="typeSignature">The <see cref="TypeSignature"/> to calculate the size of</param>
        /// <param name="is32Bit">Whether the parent <see cref="ModuleDefinition"/> is 32 bit</param>
        /// <returns>The size of <paramref name="typeSignature"/></returns>
        /// <exception cref="SizeCalculationException">It isn't possible to compute the size statically</exception>
        public static int CalculateSize(TypeSignature typeSignature, bool? is32Bit = null)
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

                case ElementType.Array:
                case ElementType.SzArray:
                case ElementType.Boxed:
                case ElementType.Class:
                case ElementType.Object:
                case ElementType.I:
                case ElementType.U:
                case ElementType.String:
                case ElementType.ByRef:
                case ElementType.TypedByRef:
                case ElementType.Ptr:
                case ElementType.FnPtr:
                {
                    if (is32Bit is null)
                    {
                        throw new NotSupportedException();
                    }

                    return is32Bit.Value ? 4 : 8;
                }
                
                case ElementType.Enum:
                {
                    return CalculateSize(typeSignature.GetUnderlyingTypeDefOrRef().ToTypeSignature(), is32Bit);
                }
                
                case ElementType.ValueType:
                {
                    return CalculateSize(typeSignature.Resolve(), is32Bit);
                }

                default:
                {
                    throw new SizeCalculationException($"Unsupported type {typeSignature.Name}");
                }
            }
        }

        /// <summary>
        /// Calculates the size of a <see cref="TypeSpecification"/>
        /// </summary>
        /// <param name="typeSpecification">The <see cref="TypeSpecification"/> to calculate the size of</param>
        /// <param name="is32Bit">Whether the parent <see cref="ModuleDefinition"/> is 32 bit</param>
        /// <returns>The size of <paramref name="typeSpecification"/></returns>
        public static int CalculateSize(TypeSpecification typeSpecification, bool? is32Bit = null)
        {
            return CalculateSize(typeSpecification.Signature, is32Bit);
        }
    }
}