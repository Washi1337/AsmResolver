using System;
using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Signatures
{
    /// <summary>
    /// Provides a base for all signature that deal with a calling convention. This includes most member signatures,
    /// such as method and field signatures.
    /// </summary>
    public abstract class CallingConventionSignature : ExtendableBlobSignature
    {
        private const CallingConventionAttributes SignatureTypeMask = (CallingConventionAttributes)0xF;

        /// <summary>
        /// Reads a single calling convention signature at the current position of the provided stream reader.
        /// </summary>
        /// <param name="image">The image the signature is defined in.</param>
        /// <param name="reader">The reader to use.</param>
        /// <param name="readToEnd">Determines whether any extra data after the signature should be read and
        /// put into the <see cref="ExtendableBlobSignature.ExtraData"/> property.</param>
        /// <returns>The read signature.</returns>
        public static CallingConventionSignature FromReader(MetadataImage image, IBinaryStreamReader reader,
            bool readToEnd = false)
        {
            return FromReader(image, reader, readToEnd, new RecursionProtection());
        }

        /// <summary>
        /// Reads a single calling convention signature at the current position of the provided stream reader.
        /// </summary>
        /// <param name="image">The image the signature is defined in.</param>
        /// <param name="reader">The reader to use.</param>
        /// <param name="readToEnd">Determines whether any extra data after the signature should be read and
        /// put into the <see cref="ExtendableBlobSignature.ExtraData"/> property.</param>
        /// <param name="protection">The recursion protection that is used to detect malicious loops in the metadata.</param>
        /// <returns>The read signature.</returns>
        public static CallingConventionSignature FromReader(
            MetadataImage image, 
            IBinaryStreamReader reader, 
            bool readToEnd,
            RecursionProtection protection)
        {
            var signature = ReadSignature(image, reader, protection);
            if (readToEnd)
                signature.ExtraData = reader.ReadToEnd();
            return signature;
        }

        private static CallingConventionSignature ReadSignature(
            MetadataImage image, 
            IBinaryStreamReader reader, 
            RecursionProtection protection)
        {
            var flag = reader.ReadByte();
            reader.Position--;

            switch ((CallingConventionAttributes) flag & SignatureTypeMask)
            {
                case CallingConventionAttributes.Default:
                case CallingConventionAttributes.C:
                case CallingConventionAttributes.ExplicitThis:
                case CallingConventionAttributes.FastCall:
                case CallingConventionAttributes.StdCall:
                case CallingConventionAttributes.ThisCall:
                case CallingConventionAttributes.VarArg:
                    return MethodSignature.FromReader(image, reader, false, protection);
                case CallingConventionAttributes.Property:
                    return PropertySignature.FromReader(image, reader, false, protection);
                case CallingConventionAttributes.Local:
                    return LocalVariableSignature.FromReader(image, reader, false,protection);
                case CallingConventionAttributes.GenericInstance:
                    return GenericInstanceMethodSignature.FromReader(image, reader, protection);
                case CallingConventionAttributes.Field:
                    return FieldSignature.FromReader(image, reader, false, protection);
            }

            throw new NotSupportedException();
        }

        protected CallingConventionSignature()
        {
        }

        protected CallingConventionSignature(CallingConventionAttributes attributes)
        {
            Attributes = attributes;
        }

        /// <summary>
        /// Gets or sets the attributes of the signature.
        /// </summary>
        public CallingConventionAttributes Attributes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether the signature describes a method.
        /// </summary>
        public bool IsMethod => (int)(Attributes & SignatureTypeMask) <= 0x5;

        /// <summary>
        /// Gets a value indicating whether the signature describes a field
        /// </summary>
        public bool IsField => (Attributes & SignatureTypeMask) == CallingConventionAttributes.Field;

        /// <summary>
        /// Gets a value indicating whether the signature describes a local variable.
        /// </summary>
        public bool IsLocal => (Attributes & SignatureTypeMask) == CallingConventionAttributes.Local;

        /// <summary>
        /// Gets a value indicating whether the signature describes a generic instance of a method.
        /// </summary>
        public bool IsGenericInstance => (Attributes & SignatureTypeMask) == CallingConventionAttributes.GenericInstance;

        /// <summary>
        /// Gets or sets a value indicating whether the member using this signature is a generic member and defines
        /// generic parameters.
        /// </summary>
        public bool IsGeneric
        {
            get => Attributes.HasFlag(CallingConventionAttributes.Generic);
            set => Attributes = Attributes.SetFlag(CallingConventionAttributes.Generic, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the member using this signature is an instance member or not.
        /// </summary>
        public bool HasThis
        {
            get => Attributes.HasFlag(CallingConventionAttributes.HasThis);
            set => Attributes = Attributes.SetFlag(CallingConventionAttributes.HasThis, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the this parameter is explicitly specified in the parameter list.
        /// That is, determines whether the first parameter is used for the current instance object.
        /// </summary>
        public bool ExplicitThis
        {
            get => Attributes.HasFlag(CallingConventionAttributes.ExplicitThis);
            set => Attributes = Attributes.SetFlag(CallingConventionAttributes.ExplicitThis, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the signature is part of a vararg method signature.
        /// </summary>
        public bool IsSentinel
        {
            get => Attributes.HasFlag(CallingConventionAttributes.Sentinel);
            set => Attributes = Attributes.SetFlag(CallingConventionAttributes.Sentinel, value);
        }
    }
}
