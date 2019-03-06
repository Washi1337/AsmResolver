using System;
using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Signatures
{
    public abstract class CallingConventionSignature : ExtendableBlobSignature
    {
        private const CallingConventionAttributes SignatureTypeMask = (CallingConventionAttributes)0xF;

        public static CallingConventionSignature FromReader(MetadataImage image, IBinaryStreamReader reader,
            bool readToEnd = false)
        {
            return FromReader(image, reader, readToEnd, new RecursionProtection());
        }
        
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

        public CallingConventionAttributes Attributes
        {
            get;
            set;
        }

        public bool IsMethod => (int)(Attributes & SignatureTypeMask) <= 0x5;

        public bool IsField => (Attributes & SignatureTypeMask) == CallingConventionAttributes.Field;

        public bool IsLocal => (Attributes & SignatureTypeMask) == CallingConventionAttributes.Local;

        public bool IsGenericInstance => (Attributes & SignatureTypeMask) == CallingConventionAttributes.GenericInstance;

        public bool IsGeneric
        {
            get => Attributes.HasFlag(CallingConventionAttributes.Generic);
            set => Attributes = Attributes.SetFlag(CallingConventionAttributes.Generic, value);
        }

        public bool HasThis
        {
            get => Attributes.HasFlag(CallingConventionAttributes.HasThis);
            set => Attributes = Attributes.SetFlag(CallingConventionAttributes.HasThis, value);
        }

        public bool ExplicitThis
        {
            get => Attributes.HasFlag(CallingConventionAttributes.ExplicitThis);
            set => Attributes = Attributes.SetFlag(CallingConventionAttributes.ExplicitThis, value);
        }

        public bool IsSentinel
        {
            get => Attributes.HasFlag(CallingConventionAttributes.Sentinel);
            set => Attributes = Attributes.SetFlag(CallingConventionAttributes.Sentinel, value);
        }
    }
}
