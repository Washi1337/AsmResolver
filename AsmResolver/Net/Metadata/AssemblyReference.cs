using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Builder;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Metadata
{
    public class AssemblyReferenceTable : MetadataTable<AssemblyReference>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.AssemblyRef; }
        }

        public override uint GetElementByteCount()
        {
            return sizeof (ushort) +
                   sizeof (ushort) +
                   sizeof (ushort) +
                   sizeof (ushort) +
                   sizeof (uint) +
                   (uint)TableStream.BlobIndexSize +
                   (uint)TableStream.StringIndexSize +
                   (uint)TableStream.StringIndexSize +
                   (uint)TableStream.BlobIndexSize;
        }

        protected override AssemblyReference ReadMember(MetadataToken token, ReadingContext context)
        {
            var reader = context.Reader;
            return new AssemblyReference(Header, token,
                new MetadataRow<ushort, ushort, ushort, ushort, uint, uint, uint, uint, uint>()
                {
                    Column1 = reader.ReadUInt16(),
                    Column2 = reader.ReadUInt16(),
                    Column3 = reader.ReadUInt16(),
                    Column4 = reader.ReadUInt16(),
                    Column5 = reader.ReadUInt32(),
                    Column6 = reader.ReadIndex(TableStream.BlobIndexSize),
                    Column7 = reader.ReadIndex(TableStream.StringIndexSize),
                    Column8 = reader.ReadIndex(TableStream.StringIndexSize),
                    Column9 = reader.ReadIndex(TableStream.BlobIndexSize),
                });
        }

        protected override void UpdateMember(NetBuildingContext context, AssemblyReference member)
        {
            var stringStream = context.GetStreamBuffer<StringStreamBuffer>();
            var blobStream = context.GetStreamBuffer<BlobStreamBuffer>();

            var row = member.MetadataRow;
            row.Column1 = (ushort)member.Version.Major;
            row.Column2 = (ushort)member.Version.Minor;
            row.Column3 = (ushort)member.Version.Build;
            row.Column4 = (ushort)member.Version.Revision;
            row.Column5 = (uint)member.Attributes;
            row.Column6 = blobStream.GetBlobOffset(member.PublicKey);
            row.Column7 = stringStream.GetStringOffset(member.Name);
            row.Column8 = stringStream.GetStringOffset(member.Culture);
            row.Column9 = blobStream.GetBlobOffset(member.HashValue);
        }

        protected override void WriteMember(WritingContext context, AssemblyReference member)
        {
            var writer = context.Writer;
            var row = member.MetadataRow;
            writer.WriteUInt16(row.Column1);
            writer.WriteUInt16(row.Column2);
            writer.WriteUInt16(row.Column3);
            writer.WriteUInt16(row.Column4);
            writer.WriteUInt32(row.Column5);
            writer.WriteIndex(TableStream.BlobIndexSize, row.Column6);
            writer.WriteIndex(TableStream.StringIndexSize, row.Column7);
            writer.WriteIndex(TableStream.StringIndexSize, row.Column8);
            writer.WriteIndex(TableStream.BlobIndexSize, row.Column9);
        }
    }

    public class AssemblyReference : MetadataMember<MetadataRow<ushort,ushort,ushort,ushort,uint,uint,uint,uint,uint>>, IImplementation, IHasCustomAttribute, IResolutionScope, IAssemblyDescriptor
    {
        private CustomAttributeCollection _customAttributes;
        private Version _version;
        private string _name;
        private string _culture;
        private string _fullName;
        private DataBlobSignature _publicKey;

        public AssemblyReference(IAssemblyDescriptor info)
            : base(
                null, new MetadataToken(MetadataTokenType.AssemblyRef),
                new MetadataRow<ushort, ushort, ushort, ushort, uint, uint, uint, uint, uint>())
        {
            Name = info.Name;
            Version = info.Version;
            Culture = info.Culture;
            PublicKey = info.PublicKeyToken == null ? null : new DataBlobSignature(info.PublicKeyToken);
        }

        public AssemblyReference(string name, Version version)
            : base(
                null, new MetadataToken(MetadataTokenType.AssemblyRef),
                new MetadataRow<ushort, ushort, ushort, ushort, uint, uint, uint, uint, uint>())
        {
            Name = name;
            Version = version;
        }

        internal AssemblyReference(MetadataHeader header, MetadataToken token, MetadataRow<ushort, ushort, ushort, ushort, uint, uint, uint, uint, uint> row)
            : base(header, token, row)
        {
            var stringStream = header.GetStream<StringStream>();
            var blobStream = header.GetStream<BlobStream>();

            Version = new Version(row.Column1, row.Column2, row.Column3, row.Column4);
            Attributes = (AssemblyAttributes)row.Column5;
            PublicKey = row.Column6 == 0 ? null : DataBlobSignature.FromReader(blobStream.CreateBlobReader(row.Column6));
            Name = stringStream.GetStringByOffset(row.Column7);
            Culture = stringStream.GetStringByOffset(row.Column8);
            HashValue = row.Column9 == 0 ? null : DataBlobSignature.FromReader(blobStream.CreateBlobReader(row.Column9));
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

        public DataBlobSignature HashValue
        {
            get;
            set;
        }

        public DataBlobSignature PublicKey
        {
            get { return _publicKey; }
            set
            {
                _publicKey = value;
                _fullName = null;
            }
        }

        byte[] IAssemblyDescriptor.PublicKeyToken
        {
            get { return PublicKey != null ? PublicKey.Data : null; }
        }

        public CustomAttributeCollection CustomAttributes
        {
            get
            {
                if (_customAttributes != null)
                    return _customAttributes;
                return _customAttributes = new CustomAttributeCollection(this);
            }
        }

        public override string ToString()
        {
            return FullName;
        }
    }
}
