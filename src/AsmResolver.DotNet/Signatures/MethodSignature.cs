using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet.Builder;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.IO;

namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Represents the signature of a method defined or referenced by a .NET executable file.
    /// </summary>
    public class MethodSignature : MethodSignatureBase
    {
        /// <summary>
        /// Reads a single method signature from an input stream.
        /// </summary>
        /// <param name="context">The blob reader context.</param>
        /// <param name="reader">The blob input stream.</param>
        /// <returns>The method signature.</returns>
        public static MethodSignature FromReader(ref BlobReadContext context, ref BinaryStreamReader reader)
        {
            var result = new MethodSignature(
                (CallingConventionAttributes) reader.ReadByte(),
                context.ReaderContext.ParentModule.CorLibTypeFactory.Void,
                Enumerable.Empty<TypeSignature>());

            // Generic parameter count.
            if (result.IsGeneric)
            {
                if (!reader.TryReadCompressedUInt32(out uint genericParameterCount))
                {
                    context.ReaderContext.BadImage("Invalid generic parameter count in method signature.");
                    return result;
                }

                result.GenericParameterCount = (int) genericParameterCount;
            }

            result.ReadParametersAndReturnType(ref context, ref reader);
            return result;
        }

        /// <summary>
        /// Creates a new parameter-less method signature for a static method.
        /// </summary>
        /// <param name="returnType">The return type of the method.</param>
        /// <returns>The signature.</returns>
        public static MethodSignature CreateStatic(TypeSignature returnType) =>
            new(0, returnType, Enumerable.Empty<TypeSignature>());

        /// <summary>
        /// Creates a method signature for a static method that has a number of parameters.
        /// </summary>
        /// <param name="returnType">The return type of the method.</param>
        /// <param name="parameterTypes">The parameter types.</param>
        /// <returns>The signature.</returns>
        public static MethodSignature CreateStatic(TypeSignature returnType, params TypeSignature[] parameterTypes)
            => new(0, returnType, parameterTypes);

        /// <summary>
        /// Creates a generic method signature for a static method that has a number of parameters.
        /// </summary>
        /// <param name="returnType">The return type of the method.</param>
        /// <param name="genericParameterCount">The number of generic parameters this method defines.</param>
        /// <param name="parameterTypes">The parameter types.</param>
        /// <returns>The signature.</returns>
        public static MethodSignature CreateStatic(TypeSignature returnType, int genericParameterCount, params TypeSignature[] parameterTypes)
        {
            return new MethodSignature(genericParameterCount > 0 ? CallingConventionAttributes.Generic : 0, returnType, parameterTypes)
            {
                GenericParameterCount = genericParameterCount
            };
        }

        /// <summary>
        /// Creates a method signature for a static method that has a number of parameters.
        /// </summary>
        /// <param name="returnType">The return type of the method.</param>
        /// <param name="parameterTypes">The parameter types.</param>
        /// <returns>The signature.</returns>
        public static MethodSignature CreateStatic(TypeSignature returnType, IEnumerable<TypeSignature> parameterTypes)
            => new(0, returnType, parameterTypes);

        /// <summary>
        /// Creates a generic method signature for a static method that has a number of parameters.
        /// </summary>
        /// <param name="returnType">The return type of the method.</param>
        /// <param name="genericParameterCount">The number of generic parameters this method defines.</param>
        /// <param name="parameterTypes">The parameter types.</param>
        /// <returns>The signature.</returns>
        public static MethodSignature CreateStatic(TypeSignature returnType, int genericParameterCount, IEnumerable<TypeSignature> parameterTypes)
        {
            return new MethodSignature(genericParameterCount > 0 ? CallingConventionAttributes.Generic : 0, returnType, parameterTypes)
            {
                GenericParameterCount = genericParameterCount
            };
        }

        /// <summary>
        /// Creates a new parameter-less method signature for an instance method.
        /// </summary>
        /// <param name="returnType">The return type of the method.</param>
        /// <returns>The signature.</returns>
        public static MethodSignature CreateInstance(TypeSignature returnType)
            => new(CallingConventionAttributes.HasThis, returnType, Enumerable.Empty<TypeSignature>());

        /// <summary>
        /// Creates a method signature for an instance method that has a number of parameters.
        /// </summary>
        /// <param name="returnType">The return type of the method.</param>
        /// <param name="parameterTypes">The parameter types.</param>
        /// <returns>The signature.</returns>
        public static MethodSignature CreateInstance(TypeSignature returnType, params TypeSignature[] parameterTypes)
            => new(CallingConventionAttributes.HasThis, returnType, parameterTypes);

        /// <summary>
        /// Creates a generic method signature for an instance method that has a number of parameters.
        /// </summary>
        /// <param name="returnType">The return type of the method.</param>
        /// <param name="genericParameterCount">The number of generic parameters this method defines.</param>
        /// <param name="parameterTypes">The parameter types.</param>
        /// <returns>The signature.</returns>
        public static MethodSignature CreateInstance(TypeSignature returnType, int genericParameterCount, params TypeSignature[] parameterTypes)
        {
            var attributes = genericParameterCount > 0
                ? CallingConventionAttributes.HasThis | CallingConventionAttributes.Generic
                : CallingConventionAttributes.HasThis;

            return new MethodSignature(attributes, returnType, parameterTypes)
            {
                GenericParameterCount = genericParameterCount
            };
        }

        /// <summary>
        /// Creates a method signature for an instance method  that has a number of parameters.
        /// </summary>
        /// <param name="returnType">The return type of the method.</param>
        /// <param name="parameterTypes">The parameter types.</param>
        /// <returns>The signature.</returns>
        public static MethodSignature CreateInstance(TypeSignature returnType, IEnumerable<TypeSignature> parameterTypes)
            => new(CallingConventionAttributes.HasThis, returnType, parameterTypes);

        /// <summary>
        /// Creates a generic method signature for an instance method that has a number of parameters.
        /// </summary>
        /// <param name="returnType">The return type of the method.</param>
        /// <param name="genericParameterCount">The number of generic parameters this method defines.</param>
        /// <param name="parameterTypes">The parameter types.</param>
        /// <returns>The signature.</returns>
        public static MethodSignature CreateInstance(TypeSignature returnType, int genericParameterCount, IEnumerable<TypeSignature> parameterTypes)
        {
            var attributes = genericParameterCount > 0
                ? CallingConventionAttributes.HasThis | CallingConventionAttributes.Generic
                : CallingConventionAttributes.HasThis;

            return new MethodSignature(attributes, returnType, parameterTypes)
            {
                GenericParameterCount = genericParameterCount
            };
        }

        /// <summary>
        /// Creates a new method signature with the provided return and parameter types.
        /// </summary>
        /// <param name="attributes">The attributes.</param>
        /// <param name="returnType">The return type of the method.</param>
        /// <param name="parameterTypes">The types of the parameter the method defines.</param>
        public MethodSignature(CallingConventionAttributes attributes, TypeSignature returnType, IEnumerable<TypeSignature> parameterTypes)
            : base(attributes, returnType, parameterTypes)
        {
        }

        /// <summary>
        /// Gets or sets the number of generic parameters this method defines.
        /// </summary>
        public int GenericParameterCount
        {
            get;
            set;
        }

        /// <summary>
        /// Substitutes any generic type parameter in the method signature with the parameters provided by
        /// the generic context.
        /// </summary>
        /// <param name="context">The generic context.</param>
        /// <returns>The instantiated method signature.</returns>
        /// <remarks>
        /// When the type signature does not contain any generic parameter, this method might return the current
        /// instance of the method signature.
        /// </remarks>
        public MethodSignature InstantiateGenericTypes(GenericContext context)
            => GenericTypeActivator.Instance.InstantiateMethodSignature(this, context);

        /// <summary>
        /// Constructs a new function pointer type signature based on this method signature.
        /// </summary>
        /// <returns>The new type signature.</returns>
        public FunctionPointerTypeSignature MakeFunctionPointerType() => new(this);

        /// <inheritdoc />
        protected override void WriteContents(in BlobSerializationContext context)
        {
            var writer = context.Writer;

            writer.WriteByte((byte) Attributes);

            if (IsGeneric)
                writer.WriteCompressedUInt32((uint) GenericParameterCount);

            WriteParametersAndReturnType(context);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            string prefix = HasThis ? "instance " : string.Empty;
            string fullName = ReturnType.FullName;

            string genericsString = GenericParameterCount > 0
                ? $"<{string.Join(", ", new string('?', GenericParameterCount))}>"
                : string.Empty;

            string parameterTypesString = string.Join(", ", ParameterTypes);

            string sentinelSuffix;
            if (IsSentinel)
            {
                sentinelSuffix = ParameterTypes.Count > 0
                    ? ", ..."
                    : " ...";}
            else
            {
                sentinelSuffix = string.Empty;
            }

            return $"{prefix}{fullName} *{genericsString}({parameterTypesString}{sentinelSuffix})";
        }

        /// <summary>
        /// Imports the method signature using the provided reference importer object.
        /// </summary>
        /// <param name="importer">The reference importer to us.</param>
        /// <returns>The imported signature.</returns>
        public MemberSignature ImportWith(ReferenceImporter importer) => importer.ImportMethodSignature(this);

        /// <inheritdoc />
        protected override CallingConventionSignature ImportWithInternal(ReferenceImporter importer) => ImportWith(importer);
    }
}
