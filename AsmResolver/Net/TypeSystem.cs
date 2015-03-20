using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net
{
    public class TypeSystem 
    {
        private readonly MetadataHeader _header;
        private readonly bool _isMsCorLib;
        private MetadataTable<TypeDefinition> _typeDefinitions;

        private MsCorLibTypeSignature _boolean;
        private MsCorLibTypeSignature _byte;
        private MsCorLibTypeSignature _char;
        private MsCorLibTypeSignature _double;
        private MsCorLibTypeSignature _intPtr;
        private MsCorLibTypeSignature _int16;
        private MsCorLibTypeSignature _int32;
        private MsCorLibTypeSignature _int64;
        private MsCorLibTypeSignature _object;
        private MsCorLibTypeSignature _sbyte;
        private MsCorLibTypeSignature _single;
        private MsCorLibTypeSignature _string;
        private MsCorLibTypeSignature _type;
        private MsCorLibTypeSignature _typedReference;
        private MsCorLibTypeSignature _uIntPtr;
        private MsCorLibTypeSignature _uint16;
        private MsCorLibTypeSignature _uint32;
        private MsCorLibTypeSignature _uint64;
        private MsCorLibTypeSignature _void;

        public TypeSystem(MetadataHeader header, bool isMsCorLib)
        {
            _header = header;
            _isMsCorLib = isMsCorLib;
            MsCorLibReference = new AssemblyReference(new ReflectionAssemblyNameWrapper(typeof(object).Assembly.GetName()))
            {
                Header = header,
            }; // TODO, set correct version.
 
        }

        private MsCorLibTypeSignature CreateSignature(ElementType type, string name, bool isValueType)
        {
            if (!_isMsCorLib)
                return new MsCorLibTypeSignature(new TypeReference(MsCorLibReference, "System", name)
                {
                    Header = _header
                }, type, isValueType);

            if (_typeDefinitions == null)
                _typeDefinitions = _header.GetStream<TableStream>().GetTable<TypeDefinition>();
            return new MsCorLibTypeSignature(_typeDefinitions.First(x => x.Name == name), type, isValueType);
        }

        public AssemblyReference MsCorLibReference
        {
            get;
            private set;
        }

        public MsCorLibTypeSignature Boolean
        {
            get { return _boolean ?? (_boolean = CreateSignature(ElementType.Boolean, "Boolean", true)); }
        }

        public MsCorLibTypeSignature Byte
        {
            get { return _byte ?? (_byte = CreateSignature(ElementType.U1, "Byte", true)); }
        }

        public MsCorLibTypeSignature Char
        {
            get { return _char ?? (_char = CreateSignature(ElementType.Char, "Char", true)); }
        }

        public MsCorLibTypeSignature Double
        {
            get { return _double ?? (_double = CreateSignature(ElementType.R8, "Double", true)); }
        }

        public MsCorLibTypeSignature IntPtr
        {
            get { return _intPtr ?? (_intPtr = CreateSignature(ElementType.I, "IntPtr", true)); }
        }

        public MsCorLibTypeSignature Int16
        {
            get { return _int16 ?? (_int16 = CreateSignature(ElementType.I2, "Int16", true)); }
        }

        public MsCorLibTypeSignature Int32
        {
            get { return _int32 ?? (_int32 = CreateSignature(ElementType.I4, "Int32", true)); }
        }

        public MsCorLibTypeSignature Int64
        {
            get { return _int64 ?? (_int64 = CreateSignature(ElementType.I8, "Int64", true)); }
        }

        public MsCorLibTypeSignature Object
        {
            get { return _object ?? (_object = CreateSignature(ElementType.Object, "Object", false)); }
        }

        public MsCorLibTypeSignature SByte
        {
            get { return _sbyte ?? (_sbyte = CreateSignature(ElementType.I1, "SByte", true)); }
        }

        public MsCorLibTypeSignature Single
        {
            get { return _single ?? (_single = CreateSignature(ElementType.R4, "Single", true)); }
        }

        public MsCorLibTypeSignature String
        {
            get { return _string ?? (_string = CreateSignature(ElementType.String, "String", false)); }
        }

        public MsCorLibTypeSignature Type
        {
            get { return _type ?? (_type = CreateSignature(ElementType.Type, "BaseType", false)); }
        }

        public MsCorLibTypeSignature TypedReference
        {
            get { return _typedReference ?? (_typedReference = CreateSignature(ElementType.TypedByRef, "TypedReference", true)); }
        }

        public MsCorLibTypeSignature UIntPtr
        {
            get { return _uIntPtr ?? (_uIntPtr = CreateSignature(ElementType.U, "UIntPtr", true)); }
        }

        public MsCorLibTypeSignature UInt16
        {
            get { return _uint16 ?? (_uint16 = CreateSignature(ElementType.U2, "UInt16", true)); }
        }

        public MsCorLibTypeSignature UInt32
        {
            get { return _uint32 ?? (_uint32 = CreateSignature(ElementType.U4, "UInt32", true)); }
        }

        public MsCorLibTypeSignature UInt64
        {
            get { return _uint64 ?? (_uint64 = CreateSignature(ElementType.U8, "UInt64", true)); }
        }

        public MsCorLibTypeSignature Void
        {
            get { return _void ?? (_void = CreateSignature(ElementType.Void, "Void", true)); }
        }
    }
}
