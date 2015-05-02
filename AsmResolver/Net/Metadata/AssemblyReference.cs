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
        private readonly LazyValue<string> _name;
        private readonly LazyValue<string> _culture;
        private string _fullName;
        private readonly LazyValue<DataBlobSignature> _publicKey;
        private readonly LazyValue<DataBlobSignature> _hashValue;

        public AssemblyReference(IAssemblyDescriptor info)
            : base(
                null, new MetadataToken(MetadataTokenType.AssemblyRef),
                new MetadataRow<ushort, ushort, ushort, ushort, uint, uint, uint, uint, uint>())
        {
            _name = new LazyValue<string>(info.Name);
            Version = info.Version;
            _culture = new LazyValue<string>(info.Culture);
            _publicKey = new LazyValue<DataBlobSignature>(info.PublicKeyToken == null ? null : new DataBlobSignature(info.PublicKeyToken));
            _hashValue = new LazyValue<DataBlobSignature>();
        }

        public AssemblyReference(string name, Version version)
            : base(
                null, new MetadataToken(MetadataTokenType.AssemblyRef),
                new MetadataRow<ushort, ushort, ushort, ushort, uint, uint, uint, uint, uint>())
        {
            _name = new LazyValue<string>(name);
            _version = version;
            _culture = new LazyValue<string>();
            _publicKey = new LazyValue<DataBlobSignature>();
            _hashValue = new LazyValue<DataBlobSignature>();
        }

        internal AssemblyReference(MetadataHeader header, MetadataToken token, MetadataRow<ushort, ushort, ushort, ushort, uint, uint, uint, uint, uint> row)
            : base(header, token, row)
        {
            var stringStream = header.GetStream<StringStream>();
            var blobStream = header.GetStream<BlobStream>();

            Version = new Version(row.Column1, row.Column2, row.Column3, row.Column4);
            Attributes = (AssemblyAttributes)row.Column5;
            _publicKey = new LazyValue<DataBlobSignature>(() => row.Column6 == 0 ? null : DataBlobSignature.FromReader(blobStream.CreateBlobReader(row.Column6)));
            _name = new LazyValue<string>(() => stringStream.GetStringByOffset(row.Column7));
            _culture = new LazyValue<string>(() => stringStream.GetStringByOffset(row.Column8));
            _hashValue = new LazyValue<DataBlobSignature>(() => row.Column9 == 0 ? null : DataBlobSignature.FromReader(blobStream.CreateBlobReader(row.Column9)));
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
            get { return _name.Value; }
            set
            {
                _name.Value = value;
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
            get { return _culture.Value; }
            set
            {
                _culture.Value = value;
                _fullName = null;
            }
        }

        public DataBlobSignature HashValue
        {
            get { return _hashValue.Value; }
            set { _hashValue.Value = value; }
        }

        public DataBlobSignature PublicKey
        {
            get { return _publicKey.Value; }
            set
            {
                _publicKey.Value = value;
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

        public AssemblyDefinition Resolve()
        {
            if (Header == null)
                throw new AssemblyResolutionException(this);
            return Header.MetadataResolver.AssemblyResolver.ResolveAssembly(this);
        }

        IMetadataMember IResolvable.Resolve()
        {
            return Resolve();
        }

        public override string ToString()
        {
            return FullName;
        }
    }
}
