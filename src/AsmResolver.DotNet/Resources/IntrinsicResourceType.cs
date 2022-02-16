using System;
using System.Collections.Concurrent;

namespace AsmResolver.DotNet.Resources
{
    /// <summary>
    /// Represents an intrinsic resource data type.
    /// </summary>
    public class IntrinsicResourceType : ResourceType
    {
        private static readonly ConcurrentDictionary<ResourceTypeCode, IntrinsicResourceType> Instances = new();

        /// <summary>
        /// Gets the instance of an intrinsic resource data type.
        /// </summary>
        /// <param name="code">The type code to get the resource type from.</param>
        /// <returns>The resource type.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Occurs when the provided type code is not an intrinsic type.</exception>
        public static IntrinsicResourceType Get(ResourceTypeCode code)
        {
            if (code >= ResourceTypeCode.StartOfUserTypes)
                throw new ArgumentOutOfRangeException(nameof(code));

            IntrinsicResourceType? type;

            while (!Instances.TryGetValue(code, out type))
            {
                type = new IntrinsicResourceType(code);
                Instances.TryAdd(code, type);
            }

            return type;
        }

        private IntrinsicResourceType(ResourceTypeCode typeCode)
        {
            TypeCode = typeCode;
        }

        /// <summary>
        /// Gets the type code associated to the type.
        /// </summary>
        public ResourceTypeCode TypeCode
        {
            get;
        }

        /// <inheritdoc />
        public override string FullName => TypeCode switch
        {
            ResourceTypeCode.Null => "null",
            ResourceTypeCode.String => "System.String",
            ResourceTypeCode.Boolean => "System.Boolean",
            ResourceTypeCode.Char => "System.Char",
            ResourceTypeCode.Byte => "System.Byte",
            ResourceTypeCode.SByte => "System.SByte",
            ResourceTypeCode.Int16 => "System.Int16",
            ResourceTypeCode.UInt16 => "System.UInt16",
            ResourceTypeCode.Int32 => "System.Int32",
            ResourceTypeCode.UInt32 => "System.UInt32",
            ResourceTypeCode.Int64 => "System.Int64",
            ResourceTypeCode.UInt64 => "System.UInt64",
            ResourceTypeCode.Single => "System.Single",
            ResourceTypeCode.Double => "System.Double",
            ResourceTypeCode.Decimal => "System.Decimal",
            ResourceTypeCode.DateTime => "System.DateTime",
            ResourceTypeCode.TimeSpan => "System.TimeSpan",
            ResourceTypeCode.ByteArray => "System.Byte[]",
            ResourceTypeCode.Stream => "System.IO.Stream",
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
