using System;
using System.Collections.Generic;
using System.IO;

namespace AsmResolver.DotNet.Signatures.Types.Parsing
{
    internal readonly struct TypeNameBuilder : ITypeSignatureVisitor<object>
    {
        public static string GetAssemblyQualifiedName(TypeSignature signature)
        {
            var writer = new StringWriter();
            var builder = new TypeNameBuilder(writer);
            builder.WriteTypeAssemblyQualifiedName(signature);
            return writer.ToString();
        }

        private readonly TextWriter _writer;

        private TypeNameBuilder(TextWriter writer)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        }

        private void WriteTypeAssemblyQualifiedName(TypeSignature type)
        {
            type.AcceptVisitor(this);
            
            if (type.Scope != type.Module)
            {
                _writer.Write(", ");
                WriteAssemblySpec(type.Scope.GetAssembly());
            }
        }

        public object VisitArrayType(ArrayTypeSignature signature)
        {
            signature.BaseType.AcceptVisitor(this);
            _writer.Write('[');
            for (int i = 0; i < signature.Dimensions.Count; i++)
                _writer.Write(',');
            _writer.Write(']');
            return null;
        }

        public object VisitBoxedType(BoxedTypeSignature signature) => throw new NotSupportedException();

        public object VisitByReferenceType(ByReferenceTypeSignature signature)
        {
            signature.BaseType.AcceptVisitor(this);
            _writer.Write('&');
            return null;
        }

        public object VisitCorLibType(CorLibTypeSignature signature)
        {
            WriteIdentifier(signature.Namespace);
            _writer.Write('.');
            WriteIdentifier(signature.Name);
            return null;
        }

        public object VisitCustomModifierType(CustomModifierTypeSignature signature) => throw new NotSupportedException();

        public object VisitGenericInstanceType(GenericInstanceTypeSignature signature)
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

        public object VisitGenericParameter(GenericParameterSignature signature) => throw new NotSupportedException();

        public object VisitPinnedType(PinnedTypeSignature signature) => throw new NotSupportedException();

        public object VisitPointerType(PointerTypeSignature signature)
        {
            signature.BaseType.AcceptVisitor(this);
            _writer.Write('*');
            return null;
        }

        public object VisitSentinelType(SentinelTypeSignature signature) => throw new NotSupportedException();

        public object VisitSzArrayType(SzArrayTypeSignature signature)
        {
            signature.BaseType.AcceptVisitor(this);
            _writer.Write("[]");
            return null;
        }

        public object VisitTypeDefOrRef(TypeDefOrRefSignature signature)
        {
            WriteSimpleTypeName(signature.Type);
            return null;
        }

        private void WriteSimpleTypeName(ITypeDefOrRef type)
        {
            var ancestors = new List<ITypeDefOrRef>();
            while (type != null)
            {
                ancestors.Add(type);
                type = type.DeclaringType;
            }

            WriteIdentifier(ancestors[ancestors.Count - 1].Namespace);
            _writer.Write('.');
            WriteIdentifier(ancestors[ancestors.Count - 1].Name);
            
            for (int i = ancestors.Count - 2; i >= 0; i--)
            {
                _writer.Write('+');
                WriteIdentifier(ancestors[i].Name);
            }
        }

        private void WriteAssemblySpec(AssemblyDescriptor assembly)
        {
            WriteIdentifier(assembly.Name);
            _writer.Write(", Version=");
            _writer.Write(assembly.Version.ToString());
            _writer.Write(", PublicKeyToken=");
            
            var token = assembly.GetPublicKeyToken();
            if (token is null)
                _writer.Write("null");
            else
                WriteHexBlob(token);

            if (assembly.Culture != null)
            {
                _writer.Write(", Culture=");
                WriteIdentifier(assembly.Culture);
            }
        }

        private void WriteIdentifier(string identifier)
        {
            foreach (char c in identifier)
            {
                if (TypeNameLexer.ReservedChars.Contains(c))
                    _writer.Write('\\');
                _writer.Write(c);
            }
        }

        private void WriteHexBlob(byte[] token)
        {
            foreach (byte b in token)
                _writer.Write(b.ToString("X2"));
        }
    }
}