using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Builder;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Metadata
{
    public class AssemblyDefinitionTable : MetadataTable<AssemblyDefinition>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.Assembly; }
        }

        public override uint GetElementByteCount()
        {
            return sizeof (uint) +
                   sizeof (ushort) +
                   sizeof (ushort) +
                   sizeof (ushort) +
                   sizeof (ushort) +
                   sizeof (uint) +
                   (uint)TableStream.BlobIndexSize +
                   (uint)TableStream.StringIndexSize +
                   (uint)TableStream.StringIndexSize;
        }

        protected override AssemblyDefinition ReadMember(MetadataToken token,
            ReadingContext context)
        {
            var reader = context.Reader;
            return new AssemblyDefinition(Header, token,
                new MetadataRow<uint, ushort, ushort, ushort, ushort, uint, uint, uint, uint>()
                {
                    Column1 = reader.ReadUInt32(),
                    Column2 = reader.ReadUInt16(),
                    Column3 = reader.ReadUInt16(),
                    Column4 = reader.ReadUInt16(),
                    Column5 = reader.ReadUInt16(),
                    Column6 = reader.ReadUInt32(),
                    Column7 = reader.ReadIndex(TableStream.BlobIndexSize),
                    Column8 = reader.ReadIndex(TableStream.StringIndexSize),
                    Column9 = reader.ReadIndex(TableStream.StringIndexSize),
                });
        }

        protected override void UpdateMember(NetBuildingContext context, AssemblyDefinition member)
        {
            var stringStream = context.GetStreamBuffer<StringStreamBuffer>();
            var blobStream = context.GetStreamBuffer<BlobStreamBuffer>();

            var row = member.MetadataRow;
            row.Column1 = (uint)member.HashAlgorithm;
            row.Column2 = (ushort)member.Version.Major;
            row.Column3 = (ushort)member.Version.Minor;
            row.Column4 = (ushort)member.Version.Build;
            row.Column5 = (ushort)member.Version.Revision;
            row.Column6 = (uint)member.Attributes;
            row.Column7 = blobStream.GetBlobOffset(member.PublicKey);
            row.Column8 = stringStream.GetStringOffset(member.Name);
            row.Column9 = stringStream.GetStringOffset(member.Culture);
        }

        protected override void WriteMember(WritingContext context, AssemblyDefinition member)
        {
            var writer = context.Writer;
            var row = member.MetadataRow;
            writer.WriteUInt32(row.Column1);
            writer.WriteUInt16(row.Column2);
            writer.WriteUInt16(row.Column3);
            writer.WriteUInt16(row.Column4);
            writer.WriteUInt16(row.Column5);
            writer.WriteUInt32(row.Column6);
            writer.WriteIndex(TableStream.BlobIndexSize, row.Column7);
            writer.WriteIndex(TableStream.StringIndexSize, row.Column8);
            writer.WriteIndex(TableStream.StringIndexSize, row.Column9);
        }
    }

    public class AssemblyDefinition : MetadataMember<MetadataRow<uint, ushort, ushort, ushort, ushort, uint, uint, uint, uint>>, IHasCustomAttribute, IHasSecurityAttribute, IAssemblyDescriptor
    {
        private CustomAttributeCollection _customAttributes;
        private SecurityDeclarationCollection _securityDeclarations;
        private string _name;
        private string _culture;
        private DataBlobSignature _publicKey;
        private Version _version;
        private string _fullName;
        private byte[] _publicKeyToken;

        public AssemblyDefinition(IAssemblyDescriptor info)
            : base(
                null, new MetadataToken(MetadataTokenType.Assembly),
                new MetadataRow<uint, ushort, ushort, ushort, ushort, uint, uint, uint, uint>())
        {
            Name = info.Name;
            Version = info.Version;
            Culture = info.Culture;
            PublicKey = info.PublicKeyToken == null ? null : new DataBlobSignature(info.PublicKeyToken);
        }

        public AssemblyDefinition(string name, Version version)
            : base(
                null, new MetadataToken(MetadataTokenType.Assembly),
                new MetadataRow<uint, ushort, ushort, ushort, ushort, uint, uint, uint, uint>())
        {
            Name = name;
            Version = version;
        }

        internal AssemblyDefinition(MetadataHeader header, MetadataToken token,
            MetadataRow<uint, ushort, ushort, ushort, ushort, uint, uint, uint, uint> row)
            : base(header, token, row)
        {
            var stringStream = header.GetStream<StringStream>();
            var blobStream = header.GetStream<BlobStream>();

            HashAlgorithm = (AssemblyHashAlgorithm)row.Column1;
            Version = new Version(row.Column2, row.Column3, row.Column4, row.Column5);
            Attributes = (AssemblyAttributes)row.Column6;
            PublicKey = row.Column7 == 0 ? null : DataBlobSignature.FromReader(blobStream.CreateBlobReader(row.Column7));
            Name = stringStream.GetStringByOffset(row.Column8);
            Culture = stringStream.GetStringByOffset(row.Column9);
        }

        public AssemblyHashAlgorithm HashAlgorithm
        {
            get;
            set;
        }

        public Version Version
        {
            get { return _version; }
            set
            {
                _version = value;
                _fullName = null;
            }
        }

        public AssemblyAttributes Attributes
        {
            get;
            set;
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                _fullName = null;
            }
        }

        public string FullName
        {
            get
            {
                if (_fullName != null)
                    return _fullName;
                return _fullName = this.GetFullName();
            }
        }

        public string Culture
        {
            get { return _culture; }
            set
            {
                _culture = value;
                _fullName = null;
            }
        }

        public DataBlobSignature PublicKey
        {
            get { return _publicKey; }
            set
            {
                _publicKey = value;
                _publicKeyToken = null;
                _fullName = null;
            }
        }

        public byte[] PublicKeyToken
        {
            get
            {
                if (_publicKeyToken != null)
                    return _publicKeyToken;
                if (PublicKey == null)
                    return null;
                return _publicKeyToken = ComputePublicKeyToken(PublicKey.Data, HashAlgorithm);
            }
        }

        public CustomAttributeCollection CustomAttributes
        {
            get { return _customAttributes ?? (_customAttributes = new CustomAttributeCollection(this)); }
        }

        public SecurityDeclarationCollection SecurityDeclarations
        {
            get { return _securityDeclarations ?? (_securityDeclarations = new SecurityDeclarationCollection(this)); }
        }

        public override string ToString()
        {
            return FullName;
        }

        private static byte[] ComputePublicKeyToken(byte[] key, AssemblyHashAlgorithm algorithm)
        {
            System.Security.Cryptography.HashAlgorithm hashAlgorithm;
            switch (algorithm)
            {
                case AssemblyHashAlgorithm.Md5:
                    hashAlgorithm = MD5.Create();
                    break;
                case AssemblyHashAlgorithm.Sha1:
                    hashAlgorithm = SHA1.Create();
                    break;
                case AssemblyHashAlgorithm.None:
                    return key;
                default:
                    throw new ArgumentException("algorithm");
            }
            using (hashAlgorithm)
            {
                var token = hashAlgorithm.ComputeHash(key);
                return token.Reverse().Take(8).ToArray();
            }    
        }
    }
}
