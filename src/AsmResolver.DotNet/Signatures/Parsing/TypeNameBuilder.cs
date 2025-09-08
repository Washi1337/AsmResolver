using System;
using System.Collections.Generic;
using System.IO;
using AsmResolver.Shims;

namespace AsmResolver.DotNet.Signatures.Parsing
{
    // Type names must include the assembly spec for 'corelib' as well. E.g., this is what Roslyn emits:
    //
    // 'System.Collections.Generic.List`1[[System.Uri, System.Runtime, Version=10.0.0.0, Culture=neutral,
    //  PublicKeyToken=b03f5f7f11d50a3a]], System.Collections, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
    //
    // When parsing 'Type' attribute elements, if the assembly spec is not specified, types would otherwise default to 'corlib', but that
    // would not necessarily be correct. For instance, 'System.Uri' is not in 'System.Private.CoreLib.dll', it's in 'System.Private.Uri.dll'.
    // Emitting the assembly spec in all cases fixes this issue, and results in the same output as Roslyn.

    /// <summary>
    /// Provides a mechanism for building up a fully qualified type names, as they are stored in custom attribute signatures.
    /// </summary>
    public sealed class TypeNameBuilder : ITypeSignatureVisitor<object?>
    {
        private readonly TextWriter _writer;

        private TypeNameBuilder(TextWriter writer)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        }

        /// <summary>
        /// Builds up an assembly qualified type name.
        /// </summary>
        /// <param name="signature">The type to convert to a string.</param>
        /// <returns>The built up type name.</returns>
        public static string GetAssemblyQualifiedName(TypeSignature signature)
        {
            var writer = new StringWriter();
            var builder = new TypeNameBuilder(writer);
            builder.WriteTypeAssemblyQualifiedName(signature);
            return writer.ToString();
        }

        private void WriteTypeAssemblyQualifiedName(TypeSignature type)
        {
            type.AcceptVisitor(this);

            var assembly = type.Scope?.GetAssembly();
            if (assembly is not null && assembly != type.ContextModule?.Assembly)
            {
                _writer.Write(", ");
                WriteAssemblySpec(assembly);
            }
        }

        /// <inheritdoc />
        public object? VisitArrayType(ArrayTypeSignature signature)
        {
            signature.BaseType.AcceptVisitor(this);
            _writer.Write('[');
            for (int i = 0; i < signature.Dimensions.Count; i++)
                _writer.Write(',');
            _writer.Write(']');
            return null;
        }

        /// <inheritdoc />
        public object VisitBoxedType(BoxedTypeSignature signature) => throw new NotSupportedException();

        /// <inheritdoc />
        public object? VisitByReferenceType(ByReferenceTypeSignature signature)
        {
            signature.BaseType.AcceptVisitor(this);
            _writer.Write('&');
            return null;
        }

        /// <inheritdoc />
        public object? VisitCorLibType(CorLibTypeSignature signature)
        {
            WriteIdentifier(signature.Namespace);
            _writer.Write('.');
            WriteIdentifier(signature.Name);
            return null;
        }

        /// <inheritdoc />
        public object VisitCustomModifierType(CustomModifierTypeSignature signature) => throw new NotSupportedException();

        /// <inheritdoc />
        public object? VisitGenericInstanceType(GenericInstanceTypeSignature signature)
        {
            WriteSimpleTypeName(signature.GenericType);
            _writer.Write('[');
            for (int i = 0; i < signature.TypeArguments.Count; i++)
            {
                _writer.Write('[');
                WriteTypeAssemblyQualifiedName(signature.TypeArguments[i]);
                _writer.Write(']');

                if (i < signature.TypeArguments.Count - 1)
                    _writer.Write(',');
            }

            _writer.Write(']');
            return null;
        }

        /// <inheritdoc />
        public object VisitGenericParameter(GenericParameterSignature signature) => throw new NotSupportedException();

        /// <inheritdoc />
        public object VisitPinnedType(PinnedTypeSignature signature) => throw new NotSupportedException();

        /// <inheritdoc />
        public object? VisitPointerType(PointerTypeSignature signature)
        {
            signature.BaseType.AcceptVisitor(this);
            _writer.Write('*');
            return null;
        }

        /// <inheritdoc />
        public object VisitSentinelType(SentinelTypeSignature signature) => throw new NotSupportedException();

        /// <inheritdoc />
        public object? VisitSzArrayType(SzArrayTypeSignature signature)
        {
            signature.BaseType.AcceptVisitor(this);
            _writer.Write("[]");
            return null;
        }

        /// <inheritdoc />
        public object? VisitTypeDefOrRef(TypeDefOrRefSignature signature)
        {
            WriteSimpleTypeName(signature.Type);
            return null;
        }

        /// <inheritdoc />
        public object VisitFunctionPointerType(FunctionPointerTypeSignature signature) =>
            throw new NotSupportedException("Function pointer type signatures are not supported in type name building.");

        private void WriteSimpleTypeName(ITypeDefOrRef? type)
        {
            var ancestors = new List<ITypeDefOrRef>();
            while (type is not null)
            {
                ancestors.Add(type);
                type = type.DeclaringType;
            }

            string? ns = ancestors[ancestors.Count - 1].Namespace;
            if (!string.IsNullOrEmpty(ns))
            {
                WriteIdentifier(ns, true);
                _writer.Write('.');
            }

            WriteIdentifier(ancestors[ancestors.Count - 1].Name);

            for (int i = ancestors.Count - 2; i >= 0; i--)
            {
                _writer.Write('+');
                WriteIdentifier(ancestors[i].Name);
            }
        }

        private void WriteAssemblySpec(AssemblyDescriptor assembly)
        {
            // Order matters for Mono

            WriteIdentifier(assembly.Name, true);
            _writer.Write(", Version=");
            _writer.Write(assembly.Version.ToString());

            _writer.Write(", Culture=");
            WriteIdentifier(assembly.Culture ?? "neutral");

            _writer.Write(", PublicKeyToken=");

            var token = assembly.GetPublicKeyToken();
            if (token is null)
                _writer.Write("null");
            else
                WriteHexBlob(token);
        }

        private void WriteIdentifier(string? identifier, bool escapeDots = false)
        {
            if (string.IsNullOrEmpty(identifier))
                return;

            foreach (char c in identifier!)
            {
                if (TypeNameLexer.ReservedChars.Contains(c) && (c != '.' || !escapeDots))
                    _writer.Write('\\');

                _writer.Write(c);
            }
        }

        private void WriteHexBlob(byte[] token)
        {
            foreach (byte b in token)
                _writer.Write(b.ToString("x2"));
        }
    }
}
