using AsmResolver.DotNet.Code.Cil;

namespace AsmResolver.DotNet.Code
{
    /// <summary>
    /// Provides an implementation for the <see cref="IMethodBodySerializer"/> that multiplexes two instances of
    /// the <see cref="IMethodBodySerializer"/> interface together, and uses one for serializing managed method bodies,
    /// and the other for unmanaged method bodies. 
    /// </summary>
    public class MultiMethodBodySerializer : IMethodBodySerializer
    {
        /// <summary>
        /// Creates a new instance of the <see cref="MultiMethodBodySerializer"/> class.
        /// </summary>
        /// <param name="managedSerializer">The serializer responsible for serializing managed CIL method bodies.</param>
        /// <param name="unmanagedSerializer">The serializer responsible for serializing unmanaged CIL method bodies.</param>
        public MultiMethodBodySerializer(IMethodBodySerializer managedSerializer, IMethodBodySerializer unmanagedSerializer)
        {
            ManagedSerializer = managedSerializer;
            UnmanagedSerializer = unmanagedSerializer;
        }
        
        /// <summary>
        /// Gets or sets the method body serializer responsible for serializing managed CIL method bodies.
        /// </summary>
        public IMethodBodySerializer ManagedSerializer
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the method body serializer responsible for serializing unmanaged method bodies.
        /// </summary>
        public IMethodBodySerializer UnmanagedSerializer
        {
            get;
            set;
        }

        /// <inheritdoc />
        public ISegmentReference SerializeMethodBody(MethodBodySerializationContext context, MethodDefinition method)
        {
            var serializer = method.MethodBody is CilMethodBody
                ? ManagedSerializer
                : UnmanagedSerializer;
            
            return serializer.SerializeMethodBody(context, method);
        }
    }
}