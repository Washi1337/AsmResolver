using System;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.IO;

namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Represents a signature of a field defined in a .NET executable file.
    /// </summary>
    public class FieldSignature : MemberSignature
    {
        /// <summary>
        /// Creates a new field signature for a static field.
        /// </summary>
        /// <param name="fieldType">The value type of the field.</param>
        /// <returns>The signature.</returns>
        [Obsolete("The HasThis bit in field signatures is ignored by the CLR. Use the constructor instead,"
                  + " or when this call is used in an argument of a FieldDefinition constructor, use the overload"
                  + " taking TypeSignature instead.")]
        public static FieldSignature CreateStatic(TypeSignature fieldType)
            => new(CallingConventionAttributes.Field, fieldType);

        /// <summary>
        /// Creates a new field signature for a static field.
        /// </summary>
        /// <param name="fieldType">The value type of the field.</param>
        /// <returns>The signature.</returns>
        [Obsolete("The HasThis bit in field signatures is ignored by the CLR. Use the constructor instead,"
                  + " or when this call is used in an argument of a FieldDefinition constructor, use the overload"
                  + " taking TypeSignature instead.")]
        public static FieldSignature CreateInstance(TypeSignature fieldType)
            => new(CallingConventionAttributes.Field | CallingConventionAttributes.HasThis, fieldType);

        /// <summary>
        /// Reads a single field signature from an input stream.
        /// </summary>
        /// <param name="context">The blob reader context.</param>
        /// <param name="reader">The blob input stream.</param>
        /// <returns>The field signature.</returns>
        public static FieldSignature FromReader(in BlobReadContext context, ref BinaryStreamReader reader)
        {
            return new(
                (CallingConventionAttributes) reader.ReadByte(),
                TypeSignature.FromReader(context, ref reader));
        }

        /// <summary>
        /// Creates a new field signature with the provided field type.
        /// </summary>
        /// <param name="fieldType">The field type.</param>
        public FieldSignature(TypeSignature fieldType)
            : this(CallingConventionAttributes.Field, fieldType)
        {
        }

        /// <summary>
        /// Creates a new field signature with the provided field type.
        /// </summary>
        /// <param name="attributes">The attributes of the field.</param>
        /// <param name="fieldType">The field type.</param>
        /// <remarks>
        /// This constructor automatically sets the <see cref="CallingConventionAttributes.Field"/> bit.
        /// </remarks>
        public FieldSignature(CallingConventionAttributes attributes, TypeSignature fieldType)
            : base(attributes | CallingConventionAttributes.Field, fieldType)
        {
        }

        /// <summary>
        /// Gets the type of the value that the field contains.
        /// </summary>
        public TypeSignature FieldType
        {
            get => MemberReturnType;
            set => MemberReturnType = value;
        }

        /// <summary>
        /// Substitutes any generic type parameter in the field signature with the parameters provided by
        /// the generic context.
        /// </summary>
        /// <param name="context">The generic context.</param>
        /// <returns>The instantiated field signature.</returns>
        /// <remarks>
        /// When the type signature does not contain any generic parameter, this method might return the current
        /// instance of the field signature.
        /// </remarks>
        public FieldSignature InstantiateGenericTypes(GenericContext context) =>
            GenericTypeActivator.Instance.InstantiateFieldSignature(this, context);

        /// <inheritdoc />
        protected override void WriteContents(BlobSerializationContext context)
        {
            context.Writer.WriteByte((byte) Attributes);
            FieldType.Write(context);
        }
        /// <inheritdoc />
        public override string ToString() => FieldType.FullName;
    }
}
