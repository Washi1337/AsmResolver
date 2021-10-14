namespace AsmResolver.DotNet.Resources
{
    /// <summary>
    /// Represents a single element in a resource set.
    /// </summary>
    public class ResourceSetEntry
    {
        private readonly LazyVariable<object?> _data;

        /// <summary>
        /// Creates a new empty resource set entry.
        /// </summary>
        /// <param name="name">The name of the resource.</param>
        /// <param name="typeCode">The type of the element's value.</param>
        public ResourceSetEntry(string name, ResourceTypeCode typeCode)
        {
            Name = name;
            Type = IntrinsicResourceType.Get(typeCode);
            _data = new LazyVariable<object?>(GetData);
        }

        /// <summary>
        /// Creates a new resource set entry.
        /// </summary>
        /// <param name="name">The name of the resource.</param>
        /// <param name="typeCode">The type of the element's value.</param>
        /// <param name="data">The value of the element.</param>
        public ResourceSetEntry(string name, ResourceTypeCode typeCode, object? data)
        {
            Name = name;
            Type = IntrinsicResourceType.Get(typeCode);
            _data = new LazyVariable<object?>(data);
        }

        /// <summary>
        /// Gets the name of the entry.
        /// </summary>
        public string Name
        {
            get;
        }

        /// <summary>
        /// Gets the type code associated to the element.
        /// </summary>
        public ResourceType Type
        {
            get;
        }

        /// <summary>
        /// Gets the value of this resource entry.
        /// </summary>
        public object? Data
        {
            get => _data.Value;
            set => _data.Value = value;
        }

        /// <summary>
        /// Obtains the value of the resource entry.
        /// </summary>
        /// <returns>The value.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Data"/> property.
        /// </remarks>
        protected virtual object? GetData() => null;
    }
}
