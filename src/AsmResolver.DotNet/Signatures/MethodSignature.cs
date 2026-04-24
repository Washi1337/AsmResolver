using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.Shims;

namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Represents the signature of a method defined or referenced by a .NET executable file.
    /// </summary>
    public class MethodSignature : MethodSignatureBase
    {
        private List<TypeSignature>? _sentinelTypes;

        /// <summary>
        /// Reads a single method signature from an input stream.
        /// </summary>
        /// <param name="context">The blob reader context.</param>
        /// <param name="reader">The blob input stream.</param>
        /// <returns>The method signature.</returns>
        public static MethodSignature FromReader(ref BlobReaderContext context, ref BinaryStreamReader reader)
        {
            var result = new MethodSignature(
                (CallingConventionAttributes) reader.ReadByte(),
                context.ReaderContext.ParentModule.CorLibTypeFactory.Void,
                null
            );

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

            // Parameter count.
            if (!reader.TryReadCompressedUInt32(out uint parameterCount))
            {
                context.ReaderContext.BadImage("Invalid number of parameters in signature.");
                return result;
            }

            // Return type.
            result.ReturnType = TypeSignature.FromReader(ref context, ref reader);

            // (Sentinel) parameter types.
            if (parameterCount > 0)
            {
                result.ParameterTypes = new List<TypeSignature>((int) parameterCount);

                result.IncludeSentinel = false;
                for (int i = 0; i < parameterCount; i++)
                {
                    var parameterType = TypeSignature.FromReader(ref context, ref reader);

                    if (parameterType.ElementType == ElementType.Sentinel)
                    {
                        result.IncludeSentinel = true;
                        i--;
                        result._sentinelTypes = new List<TypeSignature>((int) parameterCount - result.ParameterTypes.Count);
                    }
                    else if (result.IncludeSentinel)
                    {
                        result._sentinelTypes!.Add(parameterType);
                    }
                    else
                    {
                        result.ParameterTypes.Add(parameterType);
                    }
                }
            }

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
        /// Creates a generic method signature for an instance method that has a number of parameters.
        /// </summary>
        /// <param name="returnType">The return type of the method.</param>
        /// <param name="genericParameterCount">The number of generic parameters this method defines.</param>
        /// <param name="parameterTypes">The parameter types.</param>
        /// <returns>The signature.</returns>
        public static MethodSignature CreateInstance(TypeSignature returnType, int genericParameterCount, TypeSignature[] parameterTypes)
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
        public MethodSignature(
            CallingConventionAttributes attributes,
            TypeSignature returnType,
            IEnumerable<TypeSignature>? parameterTypes)
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
        /// Gets or sets a value indicating whether sentinel parameters should be included in the signature.
        /// </summary>
        public bool IncludeSentinel
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether there are any sentinel parameters present in this signature.
        /// </summary>
        public bool HasSentinelParameterTypes => _sentinelTypes is { Count: > 0 };

        /// <summary>
        /// Gets an ordered list of types indicating the types of the sentinel parameters that this member defines.
        /// </summary>
        /// <remarks>
        /// For any of the sentinel parameter types to be emitted to the output module, the <see cref="IncludeSentinel"/>
        /// must be set to <c>true</c>.
        /// </remarks>
        public IList<TypeSignature> SentinelParameterTypes
        {
            get
            {
                if (_sentinelTypes is null)
                    Interlocked.CompareExchange(ref _sentinelTypes, [], null);
                return _sentinelTypes;
            }
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
        public override bool IsImportedInModule(ModuleDefinition module)
        {
            if (!base.IsImportedInModule(module))
                return false;

            for (int i = 0; i < SentinelParameterTypes.Count; i++)
            {
                var x = SentinelParameterTypes[i];
                if (!x.IsImportedInModule(module))
                    return false;
            }

            return true;
        }

        /// <inheritdoc />
        public override int GetTotalParameterCount()
        {
            return base.GetTotalParameterCount() + (_sentinelTypes?.Count ?? 0);
        }

        /// <inheritdoc />
        protected override void WriteContents(in BlobSerializationContext context)
        {
            var writer = context.Writer;

            // Attributes
            writer.WriteByte((byte) Attributes);

            // Generic parameter count
            if (IsGeneric)
                writer.WriteCompressedUInt32((uint) GenericParameterCount);

            // Parameter count
            uint totalCount = 0;
            if (HasParameterTypes)
                totalCount += (uint) ParameterTypes.Count;
            if (IncludeSentinel && HasSentinelParameterTypes)
                totalCount += (uint) SentinelParameterTypes.Count;

            context.Writer.WriteCompressedUInt32(totalCount);

            // Return type
            ReturnType.Write(context);

            // Parameter types
            if (HasParameterTypes)
            {
                for (int i = 0; i < ParameterTypes.Count; i++)
                    ParameterTypes[i].Write(context);
            }

            // Sentinel parameter types.
            if (IncludeSentinel)
            {
                context.Writer.WriteByte((byte) ElementType.Sentinel);

                if (HasSentinelParameterTypes)
                {
                    for (int i = 0; i < SentinelParameterTypes.Count; i++)
                        SentinelParameterTypes[i].Write(context);
                }
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            string prefix = HasThis ? "instance " : string.Empty;
            string fullName = ReturnType.FullName;

            string genericsString = GenericParameterCount > 0
                ? $"<{StringShim.Join(", ", new string('?', GenericParameterCount))}>"
                : string.Empty;

            string parameterTypesString = StringShim.Join(", ", ParameterTypes);

            string sentinelSuffix;
            if (CallingConvention == CallingConventionAttributes.VarArg)
            {
                sentinelSuffix = ParameterTypes.Count > 0
                    ? ", ..."
                    : "...";
            }
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
        public MethodSignature ImportWith(ReferenceImporter importer) => importer.ImportMethodSignature(this);

        /// <inheritdoc />
        protected override CallingConventionSignature ImportWithInternal(ReferenceImporter importer) => ImportWith(importer);
    }
}
