using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cts
{
    public class TypeSystem
    {
        private readonly MetadataImage _image;
        private readonly bool _isMsCorLib;
        private readonly ICollection<TypeDefinition> _typeDefinitions;

        private readonly IDictionary<string, MsCorLibTypeSignature> _typesByName = new Dictionary<string, MsCorLibTypeSignature>();
        private readonly IDictionary<ElementType, MsCorLibTypeSignature> _typesByElementType = new Dictionary<ElementType, MsCorLibTypeSignature>();

        public TypeSystem(MetadataImage image, bool isMsCorLib)
        {
            if (image == null)
                throw new ArgumentNullException("image");
            _image = image;
            _isMsCorLib = isMsCorLib;

            if (isMsCorLib)
            {
                _typeDefinitions = image.Assembly.Modules[0].TopLevelTypes;
            }
            else
            {
                MsCorLibReference = image.Assembly.AssemblyReferences.FirstOrDefault(x => x.Name == "mscorlib");
                if (MsCorLibReference == null)
                {
                    MsCorLibReference = new AssemblyReference(new ReflectionAssemblyNameWrapper(typeof(object).Assembly.GetName()))
                    {
                        Referrer = image.Assembly,
                        Culture = "neutral",
                        Version = new Version(image.Header.VersionString[1] - 48, 0, 0, 0)
                    };
                }
            }

            Boolean = CreateSignature(ElementType.Boolean, "Boolean", true);
            SByte = CreateSignature(ElementType.I1, "SByte", true);
            Int16 = CreateSignature(ElementType.I2, "Int16", true);
            Int32 = CreateSignature(ElementType.I4, "Int32", true);
            Int64 = CreateSignature(ElementType.I8, "Int64", true);
            IntPtr = CreateSignature(ElementType.I, "IntPtr", true);
            Byte = CreateSignature(ElementType.U1, "Byte", true);
            UInt16 = CreateSignature(ElementType.U2, "UInt16", true);
            UInt32 = CreateSignature(ElementType.U4, "UInt32", true);
            UInt64 = CreateSignature(ElementType.U8, "UInt64", true);
            UIntPtr = CreateSignature(ElementType.U, "UIntPtr", true);
            Single = CreateSignature(ElementType.R4, "Single", true);
            Double = CreateSignature(ElementType.R8, "Double", true);
            Object = CreateSignature(ElementType.Object, "Object", false);
            Char = CreateSignature(ElementType.Char, "Char", true);
            String = CreateSignature(ElementType.String, "String", false);
            Type = CreateSignature(ElementType.Type, "Type", false);
            TypedReference = CreateSignature(ElementType.TypedByRef, "TypedReference", true);
            Void = CreateSignature(ElementType.Void, "Void", true);
        }

        public MsCorLibTypeSignature GetMscorlibType(ElementType elementType)
        {
            MsCorLibTypeSignature signature;
            _typesByElementType.TryGetValue(elementType, out signature);
            return signature;
        }

        public MsCorLibTypeSignature GetMscorlibType(string name)
        {
            MsCorLibTypeSignature signature;
            _typesByName.TryGetValue(name, out signature);
            return signature;
        }

        public MsCorLibTypeSignature GetMscorlibType(ITypeDescriptor type)
        {
            SignatureComparer comparer = new SignatureComparer();
            MsCorLibTypeSignature signature;

            if (!comparer.Equals(type.ResolutionScope.GetAssembly(), MsCorLibReference)
                || type.Namespace != "System"
                || !_typesByName.TryGetValue(type.Name, out signature))
            {
                return null;
            }

            return signature;
        }

        private MsCorLibTypeSignature CreateSignature(ElementType type, string name, bool isValueType)
        {
            MsCorLibTypeSignature signature;

            if (_isMsCorLib)
            {
                signature = new MsCorLibTypeSignature(_typeDefinitions.First(x => x.Name == name), type, isValueType);
            }
            else
            {
                signature = new MsCorLibTypeSignature(new TypeReference(MsCorLibReference, "System", name), type, isValueType);
            }

            _typesByName[name] = signature;
            _typesByElementType[type] = signature;
            return signature;
        }

        public AssemblyReference MsCorLibReference
        {
            get;
            private set;
        }

        public MsCorLibTypeSignature Boolean
        {
            get;
            private set;
        }

        public MsCorLibTypeSignature Byte
        {
            get;
            private set;
        }

        public MsCorLibTypeSignature Char
        {
            get;
            private set;
        }

        public MsCorLibTypeSignature Double
        {
            get;
            private set;
        }

        public MsCorLibTypeSignature IntPtr
        {
            get;
            private set;
        }

        public MsCorLibTypeSignature Int16
        {
            get;
            private set;
        }

        public MsCorLibTypeSignature Int32
        {
            get;
            private set;
        }

        public MsCorLibTypeSignature Int64
        {
            get;
            private set;
        }

        public MsCorLibTypeSignature Object
        {
            get;
            private set;
        }

        public MsCorLibTypeSignature SByte
        {
            get;
            private set;
        }

        public MsCorLibTypeSignature Single
        {
            get;
            private set;
        }

        public MsCorLibTypeSignature String
        {
            get;
            private set;
        }

        public MsCorLibTypeSignature Type
        {
            get;
            private set;
        }

        public MsCorLibTypeSignature TypedReference
        {
            get;
            private set;
        }

        public MsCorLibTypeSignature UIntPtr
        {
            get;
            private set;
        }

        public MsCorLibTypeSignature UInt16
        {
            get;
            private set;
        }

        public MsCorLibTypeSignature UInt32
        {
            get;
            private set;
        }

        public MsCorLibTypeSignature UInt64
        {
            get;
            private set;
        }

        public MsCorLibTypeSignature Void
        {
            get;
            private set;
        }
    }
}
