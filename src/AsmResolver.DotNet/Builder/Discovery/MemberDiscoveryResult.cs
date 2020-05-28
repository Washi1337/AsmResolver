using System.Collections.Generic;

namespace AsmResolver.DotNet.Builder.Discovery
{
    public class MemberDiscoveryResult
    {
        public List<TypeDefinition> Types
        {
            get;
        } = new List<TypeDefinition>();

        public List<FieldDefinition> Fields
        {
            get;
        } = new List<FieldDefinition>();

        public List<MethodDefinition> Methods
        {
            get;
        } = new List<MethodDefinition>();

        public List<ParameterDefinition> Parameters
        {
            get;
        } = new List<ParameterDefinition>();

        public List<PropertyDefinition> Properties
        {
            get;
        } = new List<PropertyDefinition>();
        
        public List<EventDefinition> Events
        {
            get;
        } = new List<EventDefinition>();
    }
}