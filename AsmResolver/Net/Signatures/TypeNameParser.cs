using System;
using System.Globalization;
using System.Text;
using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Signatures
{
    public static class TypeNameParser
    {
        public static TypeSignature ParseType(MetadataImage image, string name)
        {
            int position = 0;
            var defaultScope = image == null ? null : image.Assembly.Modules[0];
            var type = ReadTypeSignature(defaultScope, name, ref position);

            if (position >= name.Length)
                return type;

            position++;
            SkipSpaces(name, ref position);

            var elementType = ((TypeReference)type.GetElementType());
            while (elementType.DeclaringType != null)
                elementType = (TypeReference)elementType.DeclaringType;

            if (position >= name.Length)
                return type;

            elementType.ResolutionScope = ReadAssemblyReference(name, ref position);

            return type;
        }

        private static TypeSignature ReadTypeSignature(IResolutionScope scope, string name, ref int position)
        {
            TypeSignature type = null;
            while (position < name.Length && name[position] != ',')
            {
                type = ReadTypeSignature(ReadTypeDefOrRefSignature(scope, name, ref position), name, ref position);
            }
            return type;
        }

        private static TypeSignature ReadTypeSignature(TypeSignature elementType, string name, ref int position)
        {
            if (position < name.Length)
            {
                switch (name[position])
                {
                    case '*':
                        return new PointerTypeSignature(elementType);
                    case '&':
                        return new ByReferenceTypeSignature(elementType);
                    case '[':
                        position++;
                        if (name[position] == ']')
                        {
                            position++;
                            return new SzArrayTypeSignature(elementType);
                        }

                        // TODO: support generic types + generic instances.
                        break;
                }
            }
            return elementType;
        }

        private static TypeDefOrRefSignature ReadTypeDefOrRefSignature(IResolutionScope scope, string name, ref int position)
        {
            TypeReference type = null;

            while (position < name.Length)
            {
                var typeName = ReadTypeName(name, ref position);
                type = CreateTypeReference(type ?? scope, typeName);

                if (position < name.Length && name[position] == '+')
                    position++;
                else
                    break;
            }

            return new TypeDefOrRefSignature(type);
        }

        private static string ReadTypeName(string buffer, ref int position)
        {
            var builder = new StringBuilder();
            while (position < buffer.Length)
            {
                var current = buffer[position];
                switch (current)
                {
                    case ',':
                    case '+':
                    case '[':
                        return builder.ToString();
                    case '\\':
                        builder.Append(buffer[++position]);
                        break;
                    default:
                        builder.Append(current);
                        break;
                }
                position++;
            }
            return builder.ToString();
        }

        private static TypeReference CreateTypeReference(IResolutionScope scope, string fullName)
        {
            var dotIndex = fullName.LastIndexOf('.');
            var type = dotIndex == -1
                ? new TypeReference(scope, string.Empty, fullName)
                : new TypeReference(scope, fullName.Remove(dotIndex), fullName.Substring(dotIndex + 1));
            return type;
        }

        private static AssemblyReference ReadAssemblyReference(string name, ref int position)
        {
            var assemblyName = ReadPropertyValue(name, ref position);
            var version = new Version(0, 0, 0, 0);
            string culture = null;
            byte[] token = null;

            while (position < name.Length && name[position] == ',')
            {
                position++;
                SkipSpaces(name, ref position);
                var propertyName = ReadPropertyName(name, ref position);
                position++;
                switch (propertyName.ToLowerInvariant())
                {
                    case "version":
                        version = Version.Parse(ReadPropertyValue(name, ref position));
                        break;
                    case "culture":
                        culture = ReadPropertyValue(name, ref position);
                        break;
                    case "publickeytoken":
                        var value = ReadPropertyValue(name, ref position);
                        if (value != "null")
                            token = HexToByteArray(value);
                        break;
                    default:
                        throw new FormatException();
                }
            }

            return new AssemblyReference(assemblyName, version)
            {
                Culture = culture,
                PublicKey = token != null ? new DataBlobSignature(token) : null,
            };
        }

        private static byte[] HexToByteArray(string hexString)
        {
            if (hexString.Length % 2 != 0)
                throw new FormatException();

            var bytes = new byte[hexString.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = byte.Parse(hexString.Substring(i * 2, 2), NumberStyles.HexNumber);
            return bytes;
        }

        private static string ReadPropertyName(string name, ref int position)
        {
            var equalsIndex = name.IndexOf('=', position);
            if (equalsIndex == -1)
                throw new ArgumentException();
            var propertyName = name.Substring(position, equalsIndex - position);
            position += propertyName.Length;
            return propertyName;
        }

        private static string ReadPropertyValue(string name, ref int position)
        {
            var commaIndex = name.IndexOf(',', position);

            var value = commaIndex == -1
                ? name.Substring(position)
                : name.Substring(position, commaIndex - position);
            position += value.Length;

            return value;
        }

        private static void SkipSpaces(string name, ref int position)
        {
            while (char.IsWhiteSpace(name[position]))
                position++;
        }
    }
}
