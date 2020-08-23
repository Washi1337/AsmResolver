using System;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Signatures.Types
{
    /// <summary>
    /// Represents a type signature that is annotated with a required or optional custom modifier type.
    /// </summary>
    public class CustomModifierTypeSignature : TypeSpecificationSignature
    {
        /// <summary>
        /// Creates a new type signature annotated with a modifier type.
        /// </summary>
        /// <param name="modifierType">The modifier type.</param>
        /// <param name="isRequired">Indicates whether the modifier is required or optional.</param>
        /// <param name="baseType">The type signature that was annotated.</param>
        public CustomModifierTypeSignature(ITypeDefOrRef modifierType, bool isRequired, TypeSignature baseType) 
            : base(baseType)
        {
            ModifierType = modifierType;
            IsRequired = isRequired;
        }

        /// <inheritdoc />
        public override ElementType ElementType => IsRequired ? ElementType.CModReqD : ElementType.CModOpt;

        /// <summary>
        /// Gets or sets a value indicating whether the custom modifier type is a required modifier.
        /// </summary>
        public bool IsRequired
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the custom modifier type is an optional modifier.
        /// </summary>
        public bool IsOptional
        {
            get => !IsRequired;
            set => IsRequired = !value;
        }

        /// <summary>
        /// Gets or sets the type representing the modifier that is added to the type.
        /// </summary>
        public ITypeDefOrRef ModifierType
        {
            get;
            set;
        }

        /// <inheritdoc />
        public override string Name
        {
            get
            {
                string modifierString = IsRequired ? "modreq(" : "modopt(";

                string baseType = BaseType?.Name ?? NullTypeToString;
                string modifier = ModifierType?.FullName ?? NullTypeToString;
                return $"{baseType} {modifierString}{modifier})";
            }
        }

        /// <inheritdoc />
        public override bool IsValueType => BaseType.IsValueType;
        
        /// <inheritdoc />
        public override TResult AcceptVisitor<TResult>(ITypeSignatureVisitor<TResult> visitor) => 
            visitor.VisitCustomModifierType(this);

        /// <inheritdoc />
        protected override void WriteContents(BlobSerializationContext context)
        {
            context.Writer.WriteByte((byte) ElementType);
            WriteTypeDefOrRef(context, ModifierType, "Modifier type");
            WriteBaseType(context);
        }
    }
}