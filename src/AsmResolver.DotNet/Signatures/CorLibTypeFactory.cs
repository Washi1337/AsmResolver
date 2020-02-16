using System;
using System.Collections.Generic;
using System.Threading;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Provides a mechanism for obtaining commonly used element type signatures in various blob signatures, defined
    /// in the common object runtime library, such as mscorlib (for .NET framework) or System.Private.CorLib
    /// (for .NET Core). 
    /// </summary>
    public class CorLibTypeFactory
    {
        /// <summary>
        /// A collection of names of known implementations of the common runtime library.
        /// </summary>
        public static readonly ICollection<string> KnownCorLibNames = new HashSet<string>(new []
        {
            "mscorlib",
            "netstandard",
            "System.Runtime",
            "System.Private.CoreLib"
        });
        
        /// <summary>
        /// Creates a new type factory that references mscorlib 4.0.0.0.
        /// </summary>
        /// <returns>The factory.</returns>
        public static CorLibTypeFactory CreateMscorlib40TypeFactory() =>
            new CorLibTypeFactory(new AssemblyReference("mscorlib", new Version(4, 0, 0, 0),
                false,new byte[] {0xb7, 0x7a, 0x5c, 0x56, 0x19, 0x34, 0xe0, 0x89}));

        private CorLibTypeSignature _void;
        private CorLibTypeSignature _boolean;
        private CorLibTypeSignature _char;
        private CorLibTypeSignature _sbyte;
        private CorLibTypeSignature _byte;
        private CorLibTypeSignature _int16;
        private CorLibTypeSignature _uint16;
        private CorLibTypeSignature _int32;
        private CorLibTypeSignature _uint32;
        private CorLibTypeSignature _int64;
        private CorLibTypeSignature _uint64;
        private CorLibTypeSignature _single;
        private CorLibTypeSignature _double;
        private CorLibTypeSignature _string;
        private CorLibTypeSignature _intPtr;
        private CorLibTypeSignature _uintPtr;
        private CorLibTypeSignature _object;
        private CorLibTypeSignature _typedReference;
        
        /// <summary>
        /// Creates a new factory with the provided resolution scope referencing a common object runtime library.
        /// </summary>
        /// <param name="corLibScope">The reference to the common object runtime library.</param>
        public CorLibTypeFactory(IResolutionScope corLibScope)
        {
            CorLibScope = corLibScope ?? throw new ArgumentNullException(nameof(corLibScope));
        }

        /// <summary>
        /// Gets the resolution scope referencing the common object runtime (COR) library.
        /// </summary>
        public IResolutionScope CorLibScope
        {
            get;
        }

        /// <summary>
        /// Gets the element type signature for <see cref="System.Void"/>.
        /// </summary>
        public CorLibTypeSignature Void => GetOrCreateCorLibTypeSignature(ref _void, ElementType.Void, nameof(Void));
        
        /// <summary>
        /// Gets the element type signature for <see cref="System.Boolean"/>.
        /// </summary>
        public CorLibTypeSignature Boolean => GetOrCreateCorLibTypeSignature(ref _boolean, ElementType.Boolean, nameof(Boolean));

        /// <summary>
        /// Gets the element type signature for <see cref="System.Char"/>.
        /// </summary>
        public CorLibTypeSignature Char => GetOrCreateCorLibTypeSignature(ref _char, ElementType.Char, nameof(Char));

        /// <summary>
        /// Gets the element type signature for <see cref="System.SByte"/>.
        /// </summary>
        public CorLibTypeSignature SByte => GetOrCreateCorLibTypeSignature(ref _sbyte, ElementType.I1, nameof(SByte));
        
        /// <summary>
        /// Gets the element type signature for <see cref="System.Byte"/>.
        /// </summary>
        public CorLibTypeSignature Byte => GetOrCreateCorLibTypeSignature(ref _byte, ElementType.U1, nameof(Byte));
        
        /// <summary>
        /// Gets the element type signature for <see cref="System.Int16"/>.
        /// </summary>
        public CorLibTypeSignature Int16 => GetOrCreateCorLibTypeSignature(ref _int16, ElementType.I2, nameof(Int16));
        
        /// <summary>
        /// Gets the element type signature for <see cref="System.UInt16"/>.
        /// </summary>
        public CorLibTypeSignature UInt16 => GetOrCreateCorLibTypeSignature(ref _uint16, ElementType.U2, nameof(UInt16));
        
        /// <summary>
        /// Gets the element type signature for <see cref="System.Int32"/>.
        /// </summary>
        public CorLibTypeSignature Int32 => GetOrCreateCorLibTypeSignature(ref _int32, ElementType.I4, nameof(Int32));
        
        /// <summary>
        /// Gets the element type signature for <see cref="System.UInt32"/>.
        /// </summary>
        public CorLibTypeSignature UInt32 => GetOrCreateCorLibTypeSignature(ref _uint32, ElementType.U4, nameof(UInt32));
        
        /// <summary>
        /// Gets the element type signature for <see cref="System.Int64"/>.
        /// </summary>
        public CorLibTypeSignature Int64 => GetOrCreateCorLibTypeSignature(ref _int64, ElementType.I8, nameof(Int64));
        
        /// <summary>
        /// Gets the element type signature for <see cref="System.UInt64"/>.
        /// </summary>
        public CorLibTypeSignature UInt64 => GetOrCreateCorLibTypeSignature(ref _uint64, ElementType.U8, nameof(UInt64));

        /// <summary>
        /// Gets the element type signature for <see cref="System.Single"/>.
        /// </summary>
        public CorLibTypeSignature Single => GetOrCreateCorLibTypeSignature(ref _single, ElementType.R4, nameof(Single));

        /// <summary>
        /// Gets the element type signature for <see cref="System.Double"/>.
        /// </summary>
        public CorLibTypeSignature Double => GetOrCreateCorLibTypeSignature(ref _double, ElementType.R8, nameof(Double));

        /// <summary>
        /// Gets the element type signature for <see cref="System.String"/>.
        /// </summary>
        public CorLibTypeSignature String => GetOrCreateCorLibTypeSignature(ref _string, ElementType.String, nameof(String));
      
        /// <summary>
        /// Gets the element type signature for <see cref="System.IntPtr"/>.
        /// </summary>
        public CorLibTypeSignature IntPtr => GetOrCreateCorLibTypeSignature(ref _intPtr, ElementType.I, nameof(IntPtr));
        
        /// <summary>
        /// Gets the element type signature for <see cref="System.UIntPtr"/>.
        /// </summary>
        public CorLibTypeSignature UIntPtr => GetOrCreateCorLibTypeSignature(ref _uintPtr, ElementType.U, nameof(UIntPtr));

        /// <summary>
        /// Gets the element type signature for <see cref="System.TypedReference"/>.
        /// </summary>
        public CorLibTypeSignature TypedReference => GetOrCreateCorLibTypeSignature(ref _typedReference, ElementType.TypedByRef, nameof(TypedReference));

        /// <summary>
        /// Gets the element type signature for <see cref="System.Object"/>.
        /// </summary>
        public CorLibTypeSignature Object => GetOrCreateCorLibTypeSignature(ref _object, ElementType.Object, nameof(Object));

        /// <summary>
        /// Obtains the common object runtime type signature from its element type.
        /// </summary>
        /// <param name="elementType">The element type.</param>
        /// <returns>The type signature.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Occurs when <paramref name="elementType"/> does not specify a
        /// CorLib type signature.</exception>
        public CorLibTypeSignature FromElementType(ElementType elementType)
        {
            return elementType switch
            {
                ElementType.Void => Void,
                ElementType.Boolean => Boolean,
                ElementType.Char => Char,
                ElementType.I1 => SByte,
                ElementType.U1 => Byte,
                ElementType.I2 => Int16,
                ElementType.U2 => UInt16,
                ElementType.I4 => Int32,
                ElementType.U4 => UInt32,
                ElementType.I8 => Int64,
                ElementType.U8 => UInt64,
                ElementType.R4 => Single,
                ElementType.R8 => Double,
                ElementType.String => String,
                ElementType.I => IntPtr,
                ElementType.U => UIntPtr,
                ElementType.TypedByRef => TypedReference,
                ElementType.Object => Object,
                _ => throw new ArgumentOutOfRangeException(nameof(elementType))
            };
        }

        /// <summary>
        /// Obtains the common object runtime type signature by its full name.
        /// </summary>
        /// <param name="ns">The namespace.</param>
        /// <param name="name">The name.</param>
        /// <returns>The corlib type, or <c>null</c> if none was found.</returns>
        public CorLibTypeSignature FromName(string ns, string name)
        {
            if (ns == "System")
            {
                return name switch
                {
                    "Void" => Void,
                    "Boolean" => Boolean,
                    "Char" => Char,
                    "SByte" => SByte,
                    "Byte" => Byte,
                    "Int16" => Int16,
                    "UInt16" => UInt16,
                    "Int32" => Int32,
                    "UInt32" => UInt32,
                    "Int64" => Int64,
                    "UInt64" => UInt64,
                    "Single" => Single,
                    "Double" => Double,
                    "String" => String,
                    "IntPtr" => IntPtr,
                    "UIntPtr" => UIntPtr,
                    "TypedReference" => TypedReference,
                    "Object" => Object,
                    _ => null
                };
            }

            return null;
        }

        private CorLibTypeSignature GetOrCreateCorLibTypeSignature(ref CorLibTypeSignature cache, ElementType elementType, string name)
        {
            if (cache is null)
            {
                var signature = new CorLibTypeSignature(CorLibScope, elementType, name);
                Interlocked.CompareExchange(ref cache, signature, null);
            }

            return cache;
        }
    }
}