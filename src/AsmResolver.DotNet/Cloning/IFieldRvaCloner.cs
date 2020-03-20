namespace AsmResolver.DotNet.Cloning
{
    /// <summary>
    /// Provides members for cloning initialization data (Field RVA data) of a field.
    /// </summary>
    public interface IFieldRvaCloner
    {
        /// <summary>
        /// Clones the contents of the <see cref="FieldDefinition.FieldRva"/> property of the provided field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>The cloned segment.</returns>
        ISegment CloneFieldRvaData(FieldDefinition field);
    }
}