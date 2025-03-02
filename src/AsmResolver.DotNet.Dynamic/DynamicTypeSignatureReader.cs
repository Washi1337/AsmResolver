using System;
#if !NET8_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
#endif
using AsmResolver.DotNet.Signatures;

namespace AsmResolver.DotNet.Dynamic
{
    /// <summary>
    /// Provides an implementation for the <see cref="ITypeSignatureResolver"/> that resolves metadata tokens from
    /// the underlying module's tables stream, and is able to transform addresses referencing method tables in the
    /// current process to type signatures.
    /// </summary>
    public class DynamicTypeSignatureResolver : PhysicalTypeSignatureResolver
    {
#if !NET8_0_OR_GREATER
        // We need to use reflection for this to stay compatible with .netstandard 2.0.
        private static readonly MethodInfo? GetTypeFromHandleUnsafeMethod = typeof(Type)
                .GetMethod("GetTypeFromHandleUnsafe",
                    (BindingFlags) (-1),
                    null,
                    new[] {typeof(IntPtr)},
                    null);
#endif

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
#if NET8_0_OR_GREATER
        public static bool IsSupported => true;
#else
        [MemberNotNullWhen(true, nameof(GetTypeFromHandleUnsafeMethod))]
        public static bool IsSupported => GetTypeFromHandleUnsafeMethod is not null;
#endif

        /// <inheritdoc />
        public override TypeSignature ResolveRuntimeType(ref BlobReaderContext context, nint address)
        {
#if NET8_0_OR_GREATER
            var clrType = Type.GetTypeFromHandle(RuntimeTypeHandle.FromIntPtr(address));
#else
            if (!IsSupported)
                throw new PlatformNotSupportedException("The current platform does not support the translation of raw type handles to System.Type instances.");

            // Let the runtime translate the address to a type and import it.
            var clrType = (Type?) GetTypeFromHandleUnsafeMethod.Invoke(null, new object[] { address });
#endif

            var type = clrType is not null
                ? new ReferenceImporter(context.ReaderContext.ParentModule).ImportType(clrType)
                : InvalidTypeDefOrRef.Get(InvalidTypeSignatureError.IllegalTypeSpec);

            return new TypeDefOrRefSignature(type);
        }
    }
}
