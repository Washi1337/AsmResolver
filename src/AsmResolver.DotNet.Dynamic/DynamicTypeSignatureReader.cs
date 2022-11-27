using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.DotNet.Dynamic
{
    /// <summary>
    /// Provides an implementation for the <see cref="ITypeSignatureResolver"/> that resolves metadata tokens from
    /// the underlying module's tables stream, and is able to transform addresses referencing method tables in the
    /// current process to type signatures.
    /// </summary>
    public class DynamicTypeSignatureResolver : PhysicalTypeSignatureResolver
    {
        private static readonly MethodInfo? GetTypeFromHandleUnsafeMethod;

        static DynamicTypeSignatureResolver()
        {
            // We need to use reflection for this to stay compatible with .netstandard 2.0.
            GetTypeFromHandleUnsafeMethod = typeof(Type)
                .GetMethod("GetTypeFromHandleUnsafe",
                    (BindingFlags) (-1),
                    null,
                    new[] {typeof(IntPtr)},
                    null);
        }

        /// <summary>
        /// Gets the singleton instance of the <see cref="DynamicTypeSignatureResolver"/> class.
        /// </summary>
        public new static DynamicTypeSignatureResolver Instance
        {
            get;
        } = new();

        /// <summary>
        /// Gets a value indicating whether dynamic resolution of method tables is supported.
        /// </summary>
        [MemberNotNullWhen(true, nameof(GetTypeFromHandleUnsafeMethod))]
        public static bool IsSupported => GetTypeFromHandleUnsafeMethod is not null;

        /// <inheritdoc />
        public override TypeSignature ResolveRuntimeType(ref BlobReadContext context, nint address)
        {
            if (!IsSupported)
                throw new PlatformNotSupportedException("The current platform does not support the translation of raw type handles to System.Type instances.");

            // Let the runtime translate the address to a type and import it.
            var clrType = (Type?) GetTypeFromHandleUnsafeMethod.Invoke(null, new object[] { address });

            var type = clrType is not null
                ? new ReferenceImporter(context.ReaderContext.ParentModule).ImportType(clrType)
                : InvalidTypeDefOrRef.Get(InvalidTypeSignatureError.IllegalTypeSpec);

            return new TypeDefOrRefSignature(type);
        }
    }
}
