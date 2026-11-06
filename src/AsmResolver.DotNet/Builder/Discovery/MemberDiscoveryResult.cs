using System.Collections.Generic;

namespace AsmResolver.DotNet.Builder.Discovery
{
    /// <summary>
    /// Provides a collection of members that were discovered during a traversal of a module.
    /// </summary>
    public class MemberDiscoveryResult
    {
        /// <summary>
        /// Creates a new empty discovery result.
        /// </summary>
        public MemberDiscoveryResult()
            : this([], [], [], [], [], [])
        {
        }

        internal MemberDiscoveryResult(
            List<TypeDefinition> types,
            List<FieldDefinition> fields,
            List<MethodDefinition> methods,
            List<ParameterDefinition> parameters,
            List<PropertyDefinition> properties,
            List<EventDefinition> events
        )
        {
            Types = types;
            Fields = fields;
            Methods = methods;
            Parameters = parameters;
            Properties = properties;
            Events = events;
        }

        /// <summary>
        /// Gets a list of types that are discovered during the traversal of the module.
        /// </summary>
        public List<TypeDefinition> Types
        {
            get;
        }

        /// <summary>
        /// Gets a list of fields that are discovered during the traversal of the module.
        /// </summary>
        public List<FieldDefinition> Fields
        {
            get;
        }

        /// <summary>
        /// Gets a list of modules that are discovered during the traversal of the module.
        /// </summary>
        public List<MethodDefinition> Methods
        {
            get;
        }

        /// <summary>
        /// Gets a list of parameters that are discovered during the traversal of the module.
        /// </summary>
        public List<ParameterDefinition> Parameters
        {
            get;
        }

        /// <summary>
        /// Gets a list of properties that are discovered during the traversal of the module.
        /// </summary>
        public List<PropertyDefinition> Properties
        {
            get;
        }

        /// <summary>
        /// Gets a list of events that are discovered during the traversal of the module.
        /// </summary>
        public List<EventDefinition> Events
        {
            get;
        }
    }
}
