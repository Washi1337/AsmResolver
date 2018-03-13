using System;

namespace AsmResolver.Net.Metadata
{
    public struct MetadataToken : IEquatable<MetadataToken>
    {
        public static readonly MetadataToken Zero = new MetadataToken(0u);
        
        private readonly uint _token;

        public MetadataToken(uint token)
        {
            _token = token;
        }

        public MetadataToken(MetadataTokenType tokenType)
            : this(tokenType, 0)
        {
        }

        public MetadataToken(MetadataTokenType tokenType, uint rid)
            : this(rid | (uint)tokenType << 24)
        {
        }

        public MetadataTokenType TokenType
        {
            get { return (MetadataTokenType)(_token >> 24); }
        }

        public uint Rid
        {
            get { return _token & 0xFFFFFF; }
        }

        public uint ToUInt32()
        {
            return _token;
        }

        public int ToInt32()
        {
            return unchecked((int)_token);
        }

        public override bool Equals(object obj)
        {
            if (obj is MetadataToken)
                return Equals((MetadataToken)obj);
            return false;
        }

        public override int GetHashCode()
        {
            return (int)_token;
        }

        public bool Equals(MetadataToken other)
        {
            return _token == other._token;
        }

        public override string ToString()
        {
            return _token.ToString("X8");
        }

        public static bool operator ==(MetadataToken a, MetadataToken b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(MetadataToken a, MetadataToken b)
        {
            return !(a == b);
        }
    }
}
