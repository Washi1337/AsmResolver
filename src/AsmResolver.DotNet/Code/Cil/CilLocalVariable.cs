using AsmResolver.DotNet.Blob;

namespace AsmResolver.DotNet.Code.Cil
{
    /// <summary>
    /// Represents a local variable defined in a body of a CIL method body.
    /// </summary>
    public class CilLocalVariable
    {
        /// <summary>
        /// Creates a new local variable.
        /// </summary>
        /// <param name="variableType">The variable type.</param>
        public CilLocalVariable(TypeSignature variableType)
        {
            VariableType = variableType;
        }
        
        /// <summary>
        /// Gets the index of the variable.
        /// </summary>
        public int Index
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets or sets the type of values this variable stores.
        /// </summary>
        public TypeSignature VariableType
        {
            get;
            set;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return "V_" + Index;
        }
        
    }
}