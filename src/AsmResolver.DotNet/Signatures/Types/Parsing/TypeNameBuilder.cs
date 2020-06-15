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
                _writer.Write(type.Scope.GetAssembly().FullName);
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
            _writer.Write(signature.Namespace);
            _writer.Write('.');
            _writer.Write(signature.Name);
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

            _writer.Write(ancestors[ancestors.Count - 1].Namespace);
            _writer.Write('.');
            _writer.Write(ancestors[ancestors.Count - 1].Name);
            
            for (int i = ancestors.Count - 2; i >= 0; i--)
            {
                _writer.Write('+');
                _writer.Write(ancestors[i].Name);
            }
        }
        
    }
}