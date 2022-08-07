using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.IO;

namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Represents the signature that is assigned to a property. This includes the type of the property, as well as the
    /// types of the parameters that it defines.
    /// </summary>
    public class PropertySignature : MethodSignatureBase
    {
        /// <summary>
        /// Reads a single property signature from an input stream.
        /// </summary>
        /// <param name="context">The blob reader context.</param>
        /// <param name="reader">The blob input stream.</param>
        /// <returns>The property signature.</returns>
        public static PropertySignature? FromReader(in BlobReadContext context, ref BinaryStreamReader reader)
        {
            var attributes = (CallingConventionAttributes) reader.ReadByte();
            if ((attributes & CallingConventionAttributes.Property) == 0)
            {
                context.ReaderContext.BadImage("Input stream does not point to a valid property signature.");
                return null;
            }

            var result = new PropertySignature(
                attributes,
                context.ReaderContext.ParentModule.CorLibTypeFactory.Object,
                Enumerable.Empty<TypeSignature>());

            result.ReadParametersAndReturnType(context, ref reader);
            return result;
        }

        /// <summary>
        /// Creates a new parameter-less method signature for a static method.
        /// </summary>
        /// <param name="returnType">The return type of the method.</param>
        /// <returns>The signature.</returns>
        public static PropertySignature CreateStatic(TypeSignature returnType)
            => new PropertySignature(0, returnType, Enumerable.Empty<TypeSignature>());

        /// <summary>
        /// Creates a method signature for a static method that has a number of parameters.
        /// </summary>
        /// <param name="returnType">The return type of the method.</param>
        /// <param name="parameterTypes">The parameter types.</param>
        /// <returns>The signature.</returns>
        public static PropertySignature CreateStatic(TypeSignature returnType, params TypeSignature[] parameterTypes)
            => new PropertySignature(0, returnType, parameterTypes);

        /// <summary>
        /// Creates a method signature for a static method that has a number of parameters.
        /// </summary>
        /// <param name="returnType">The return type of the method.</param>
        /// <param name="parameterTypes">The parameter types.</param>
        /// <returns>The signature.</returns>
        public static PropertySignature CreateStatic(TypeSignature returnType, IEnumerable<TypeSignature> parameterTypes)
            => new PropertySignature(0, returnType, parameterTypes);

        /// <summary>
        /// Creates a new parameter-less method signature for an instance method.
        /// </summary>
        /// <param name="returnType">The return type of the method.</param>
        /// <returns>The signature.</returns>
        public static PropertySignature CreateInstance(TypeSignature returnType)
            => new PropertySignature(CallingConventionAttributes.HasThis, returnType, Enumerable.Empty<TypeSignature>());

        /// <summary>
        /// Creates a method signature for an instance method that has a number of parameters.
        /// </summary>
        /// <param name="returnType">The return type of the method.</param>
        /// <param name="parameterTypes">The parameter types.</param>
        /// <returns>The signature.</returns>
        public static PropertySignature CreateInstance(TypeSignature returnType, params TypeSignature[] parameterTypes)
            => new PropertySignature(CallingConventionAttributes.HasThis, returnType, parameterTypes);

        /// <summary>
        /// Creates a method signature for an instance method  that has a number of parameters.
        /// </summary>
        /// <param name="returnType">The return type of the method.</param>
        /// <param name="parameterTypes">The parameter types.</param>
        /// <returns>The signature.</returns>
        public static PropertySignature CreateInstance(TypeSignature returnType, IEnumerable<TypeSignature> parameterTypes)
            => new PropertySignature(CallingConventionAttributes.HasThis, returnType, parameterTypes);

        /// <summary>
        /// Initializes a new property signature with the provided property type and a list of parameter types.
        /// </summary>
        /// <param name="attributes">The attributes.</param>
        /// <param name="propertyType">The property type.</param>
        /// <param name="parameterTypes">The parameter types.</param>
        public PropertySignature(CallingConventionAttributes attributes, TypeSignature propertyType, IEnumerable<TypeSignature> parameterTypes)
            : base(attributes | CallingConventionAttributes.Property, propertyType, parameterTypes)
        {
        }

        /// <summary>
        /// Substitutes any generic type parameter in the property signature with the parameters provided by
        /// the generic context.
        /// </summary>
        /// <param name="context">The generic context.</param>
        /// <returns>The instantiated property signature.</returns>
        /// <remarks>
        /// When the type signature does not contain any generic parameter, this method might return the current
        /// instance of the property signature.
        /// </remarks>
        public PropertySignature InstantiateGenericTypes(GenericContext context)
            => GenericTypeActivator.Instance.InstantiatePropertySignature(this, context);

        /// <inheritdoc />
        protected override void WriteContents(BlobSerializationContext context)
        {
            context.Writer.WriteByte((byte) Attributes);
            WriteParametersAndReturnType(context);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            string prefix = HasThis ? "instance " : string.Empty;
            string parameterTypesString = ParameterTypes.Count > 0
                ? $"[{string.Join(", ", ParameterTypes)}]"
                : string.Empty;

            return $"{prefix}{ReturnType.FullName} *{parameterTypesString}";
        }

        /// <summary>
        /// Imports the property signature using the provided reference importer object.
        /// </summary>
        /// <param name="importer">The reference importer to us.</param>
        /// <returns>The imported signature.</returns>
        public PropertySignature ImportWith(ReferenceImporter importer) => importer.ImportPropertySignature(this);

        /// <inheritdoc />
        protected override CallingConventionSignature ImportWithInternal(ReferenceImporter importer) => ImportWith(importer);
    }
}
