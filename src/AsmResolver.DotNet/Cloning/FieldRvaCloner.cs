using System;

namespace AsmResolver.DotNet.Cloning
{
    /// <summary>
    /// Provides a default implementation of the <see cref="IFieldRvaCloner"/> interface.
    /// </summary>
    public class FieldRvaCloner : IFieldRvaCloner
    {
        /// <inheritdoc />
        public ISegment? CloneFieldRvaData(FieldDefinition field)
        {
            switch (field.FieldRva)
            {
                case null:
                    return null;

                case IReadableSegment readableSegment:
                    return new DataSegment(readableSegment.ToArray());

                case ICloneable cloneable:
                    return (ISegment) cloneable.Clone();

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
