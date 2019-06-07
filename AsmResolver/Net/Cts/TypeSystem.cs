using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cts
{
    /// <summary>
    /// Provides a collection of commonly used (element) type signatures.
    /// </summary>
    public class TypeSystem
    {
        private readonly MetadataImage _image;
        private readonly bool _isMsCorLib;
        private readonly ICollection<TypeDefinition> _typeDefinitions;

        private readonly IDictionary<string, MsCorLibTypeSignature> _typesByName = new Dictionary<string, MsCorLibTypeSignature>();
        private readonly IDictionary<ElementType, MsCorLibTypeSignature> _typesByElementType = new Dictionary<ElementType, MsCorLibTypeSignature>();

        public TypeSystem(MetadataImage image, bool isMsCorLib)
        {
            _image = image ?? throw new ArgumentNullException(nameof(image));
            _isMsCorLib = isMsCorLib;

            if (isMsCorLib)
            {
                _typeDefinitions = image.Assembly.Modules[0].TopLevelTypes;
            }
            else
            {
                MsCorLibReference = image.Assembly.AssemblyReferences.FirstOrDefault(x => x.Name != typeof(object).Assembly.GetName().Name);
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
            TypedReference = CreateSignature(ElementType.TypedByRef, "TypedReference", true);
            Void = CreateSignature(ElementType.Void, "Void", true);
        }

        /// <summary>
        /// Resolves the provided element type to a type signature.
        /// </summary>
        /// <param name="elementType">The element type to resolve.</param>
        /// <returns>The resolved signature, or null if none exists.</returns>
        public MsCorLibTypeSignature GetMscorlibType(ElementType elementType)
        {
            _typesByElementType.TryGetValue(elementType, out var signature);
            return signature;
        }

        /// <summary>
        /// Resolves the full type name of an element type to a type signature.
        /// </summary>
        /// <param name="name">The full name of the element type to resolve.</param>
        /// <returns>The resolved signature, or null if none exists.</returns>
        public MsCorLibTypeSignature GetMscorlibType(string name)
        {
            _typesByName.TryGetValue(name, out var signature);
            return signature;
        }

        /// <summary>
        /// Resolves the provided type descriptor to a type signature.
        /// </summary>
        /// <param name="type">The type descriptor to resolve.</param>
        /// <returns>The resolved signature, or null if none exists.</returns>
        public MsCorLibTypeSignature GetMscorlibType(ITypeDescriptor type)
        {
            var comparer = new SignatureComparer();

            if (!comparer.Equals(type.ResolutionScope.GetAssembly(), MsCorLibReference)
                || type.Namespace != "System"
                || !_typesByName.TryGetValue(type.Name, out var signature))
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

        /// <summary>
        /// Gets the assembly reference to the standard library (corlib).
        /// </summary>
        public AssemblyReference MsCorLibReference
        {
            get;
        }

        /// <summary>
        /// Gets the System.Boolean element type signature.
        /// </summary>
        public MsCorLibTypeSignature Boolean
        {
            get;
        }

        /// <summary>
        /// Gets the System.Byte element type signature.
        /// </summary>
        public MsCorLibTypeSignature Byte
        {
            get;
        }

        /// <summary>
        /// Gets the System.Char element type signature.
        /// </summary>
        public MsCorLibTypeSignature Char
        {
            get;
        }

        /// <summary>
        /// Gets the System.Double element type signature.
        /// </summary>
        public MsCorLibTypeSignature Double
        {
            get;
        }

        /// <summary>
        /// Gets the System.IntPtr element type signature.
        /// </summary>
        public MsCorLibTypeSignature IntPtr
        {
            get;
        }

        /// <summary>
        /// Gets the System.Int16 element type signature.
        /// </summary>
        public MsCorLibTypeSignature Int16
        {
            get;
        }

        /// <summary>
        /// Gets the System.Int32 element type signature.
        /// </summary>
        public MsCorLibTypeSignature Int32
        {
            get;
        }

        /// <summary>
        /// Gets the System.Int64 element type signature.
        /// </summary>
        public MsCorLibTypeSignature Int64
        {
            get;
        }

        /// <summary>
        /// Gets the System.Object element type signature.
        /// </summary>
        public MsCorLibTypeSignature Object
        {
            get;
        }

        /// <summary>
        /// Gets the System.SByte element type signature.
        /// </summary>
        public MsCorLibTypeSignature SByte
        {
            get;
        }

        /// <summary>
        /// Gets the System.Single element type signature.
        /// </summary>
        public MsCorLibTypeSignature Single
        {
            get;
        }

        /// <summary>
        /// Gets the System.String element type signature.
        /// </summary>
        public MsCorLibTypeSignature String
        {
            get;
        }

        /// <summary>
        /// Gets the System.TypedReference element type signature.
        /// </summary>
        public MsCorLibTypeSignature TypedReference
        {
            get;
        }

        /// <summary>
        /// Gets the System.UIntPtr element type signature.
        /// </summary>
        public MsCorLibTypeSignature UIntPtr
        {
            get;
        }

        /// <summary>
        /// Gets the System.UInt16 element type signature.
        /// </summary>
        public MsCorLibTypeSignature UInt16
        {
            get;
        }

        /// <summary>
        /// Gets the System.UInt32 element type signature.
        /// </summary>
        public MsCorLibTypeSignature UInt32
        {
            get;
        }

        /// <summary>
        /// Gets the System.UInt64 element type signature.
        /// </summary>
        public MsCorLibTypeSignature UInt64
        {
            get;
        }

        /// <summary>
        /// Gets the System.Void element type signature.
        /// </summary>
        public MsCorLibTypeSignature Void
        {
            get;
        }
    }
}
