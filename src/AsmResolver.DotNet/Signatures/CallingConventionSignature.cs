using System;
using AsmResolver.IO;

namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Provides a base for all signature that deal with a calling convention. This includes most member signatures,
    /// such as method and field signatures.
    /// </summary>
    public abstract class CallingConventionSignature : ExtendableBlobSignature, IImportable
    {
        private const CallingConventionAttributes SignatureTypeMask = (CallingConventionAttributes)0xF;

        /// <summary>
        /// Reads a single calling convention signature at the current position of the provided stream reader.
        /// </summary>
        /// <param name="context">The blob reader context.</param>
        /// <param name="reader">The reader to use.</param>
        /// <param name="readToEnd">Determines whether any extra data after the signature should be read and
        /// put into the <see cref="ExtendableBlobSignature.ExtraData"/> property.</param>
        /// <returns>The read signature.</returns>
        public static CallingConventionSignature? FromReader(
            ref BlobReaderContext context,
            ref BinaryStreamReader reader,
            bool readToEnd = true)
        {
            var signature = ReadSignature(ref context, ref reader);
            if (readToEnd)
            {
                byte[] extraData = reader.ReadToEnd();
                if (signature is not null)
                    signature.ExtraData = extraData;
            }

            return signature;
        }

        private static CallingConventionSignature? ReadSignature(ref BlobReaderContext context, ref BinaryStreamReader reader)
        {
            byte flag = reader.ReadByte();
            reader.Offset--;

            switch ((CallingConventionAttributes) flag & SignatureTypeMask)
            {
                case CallingConventionAttributes.Default:
                case CallingConventionAttributes.C:
                case CallingConventionAttributes.ExplicitThis:
                case CallingConventionAttributes.FastCall:
                case CallingConventionAttributes.StdCall:
                case CallingConventionAttributes.ThisCall:
                case CallingConventionAttributes.VarArg:
                case CallingConventionAttributes.Unmanaged:
                case CallingConventionAttributes.NativeVarArg:
                    return MethodSignature.FromReader(ref context, ref reader);

                case CallingConventionAttributes.Property:
                    return PropertySignature.FromReader(ref context, ref reader);

                case CallingConventionAttributes.Local:
                    return LocalVariablesSignature.FromReader(ref context, ref reader);

                case CallingConventionAttributes.GenericInstance:
                    return GenericInstanceMethodSignature.FromReader(ref context, ref reader);

                case CallingConventionAttributes.Field:
                    return FieldSignature.FromReader(ref context, ref reader);
            }

            throw new NotSupportedException($"Invalid or unsupported calling convention signature header {flag:X2}.");
        }

        /// <summary>
        /// Creates a new calling convention signature.
        /// </summary>
        /// <param name="attributes">The attributes associated to the signature.</param>
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
        /// When this signature references a method signature, gets or sets the calling convention that is used.
        /// </summary>
        public CallingConventionAttributes CallingConvention
        {
            get => Attributes & SignatureTypeMask;
            set => Attributes = (Attributes & ~SignatureTypeMask) | value;
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
            get => (Attributes & CallingConventionAttributes.Generic) != 0;
            set => Attributes = (Attributes & ~CallingConventionAttributes.Generic)
                                | (value ? CallingConventionAttributes.Generic : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the member is an instance member and an additional argument is
        /// required to use this member.
        /// </summary>
        public bool HasThis
        {
            get => (Attributes & CallingConventionAttributes.HasThis) != 0;
            set => Attributes = (Attributes & ~CallingConventionAttributes.HasThis)
                                | (value ? CallingConventionAttributes.HasThis : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the this parameter is explicitly specified in the parameter list.
        /// That is, determines whether the first parameter is used for the current instance object.
        /// </summary>
        public bool ExplicitThis
        {
            get => (Attributes & CallingConventionAttributes.ExplicitThis) != 0;
            set => Attributes = (Attributes & ~CallingConventionAttributes.ExplicitThis)
                                | (value ? CallingConventionAttributes.ExplicitThis : 0);
        }

        /// <inheritdoc />
        public abstract bool IsImportedInModule(ModuleDefinition module);

        /// <summary>
        /// Imports the signature using the provided reference importer object.
        /// </summary>
        /// <param name="importer">The reference importer to us.</param>
        /// <returns>The imported signature.</returns>
        protected abstract CallingConventionSignature ImportWithInternal(ReferenceImporter importer);

        /// <inheritdoc />
        IImportable IImportable.ImportWith(ReferenceImporter importer) => ImportWithInternal(importer);
    }
}
