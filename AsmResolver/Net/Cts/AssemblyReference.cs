using System;
using AsmResolver.Collections.Generic;
using AsmResolver.Net.Builder;
using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cts
{
    public class AssemblyReference : MetadataMember<MetadataRow<ushort,ushort,ushort,ushort,AssemblyAttributes,uint,uint,uint,uint>>, IImplementation, IHasCustomAttribute, IResolutionScope, IAssemblyDescriptor
    {
        private Version _version;
        private readonly LazyValue<string> _name;
        private readonly LazyValue<string> _culture;
        private string _fullName;
        private readonly LazyValue<DataBlobSignature> _publicKey;
        private readonly LazyValue<DataBlobSignature> _hashValue;

        public AssemblyReference(IAssemblyDescriptor info)
            : base(null, new MetadataToken(MetadataTokenType.AssemblyRef))
        {
            _name = new LazyValue<string>(info.Name);
            Version = info.Version;
            _culture = new LazyValue<string>(info.Culture);
            _publicKey = new LazyValue<DataBlobSignature>(info.PublicKeyToken == null ? null : new DataBlobSignature(info.PublicKeyToken));
            _hashValue = new LazyValue<DataBlobSignature>();
         
            CustomAttributes = new CustomAttributeCollection(this);
            OperatingSystems = new DelegatedMemberCollection<AssemblyReference, AssemblyRefOs>(this, GetOsOwner, SetOsOwner);
            Processors = new DelegatedMemberCollection<AssemblyReference, AssemblyRefProcessor>(this, GetProcessorOwner, SetProcessorOwner);
        }

        public AssemblyReference(string name, Version version)
            : base(null, new MetadataToken(MetadataTokenType.AssemblyRef))
        {
            _name = new LazyValue<string>(name);
            _version = version;
            _culture = new LazyValue<string>();
            _publicKey = new LazyValue<DataBlobSignature>();
            _hashValue = new LazyValue<DataBlobSignature>();
            CustomAttributes = new CustomAttributeCollection(this);
            
            OperatingSystems = new Collection<AssemblyRefOs>();
            OperatingSystems = new DelegatedMemberCollection<AssemblyReference, AssemblyRefOs>(this, GetOsOwner, SetOsOwner);
            Processors = new DelegatedMemberCollection<AssemblyReference, AssemblyRefProcessor>(this, GetProcessorOwner, SetProcessorOwner);
        }

        internal AssemblyReference(MetadataImage image, MetadataRow<ushort, ushort, ushort, ushort, AssemblyAttributes, uint, uint, uint, uint> row)
            : base(image, row.MetadataToken)
        {
            var stringStream = image.Header.GetStream<StringStream>();
            var blobStream = image.Header.GetStream<BlobStream>();

            Version = new Version(row.Column1, row.Column2, row.Column3, row.Column4);
            Attributes = row.Column5;
            _publicKey = new LazyValue<DataBlobSignature>(() => row.Column6 == 0 ? null : DataBlobSignature.FromReader(blobStream.CreateBlobReader(row.Column6)));
            _name = new LazyValue<string>(() => stringStream.GetStringByOffset(row.Column7));
            _culture = new LazyValue<string>(() => stringStream.GetStringByOffset(row.Column8));
            _hashValue = new LazyValue<DataBlobSignature>(() => row.Column9 == 0 ? null : DataBlobSignature.FromReader(blobStream.CreateBlobReader(row.Column9)));
            
            CustomAttributes = new CustomAttributeCollection(this);
            OperatingSystems = new TableMemberCollection<AssemblyReference, AssemblyRefOs>(this, MetadataTokenType.AssemblyRefOs, GetOsOwner, SetOsOwner);
            Processors = new TableMemberCollection<AssemblyReference, AssemblyRefProcessor>(this, MetadataTokenType.AssemblyRefProcessor, GetProcessorOwner, SetProcessorOwner);
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
            get;
            private set;
        }

        /// <summary>
        /// Gets a collection of operating systems supported by this referenced assembly.  
        /// </summary>
        /// <remarks>This collection is no longer used in the newer versions of the .NET framework.</remarks>
        public Collection<AssemblyRefOs> OperatingSystems
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a collection of processors supported by this referenced assembly.
        /// </summary>
        /// <remarks>This collection is no longer used in the newer versions of the .NET framework.</remarks>
        public Collection<AssemblyRefProcessor> Processors
        {
            get;
            private set;
        }

        public AssemblyDefinition Resolve()
        {
            if (Image == null)
                throw new AssemblyResolutionException(this);
            return Image.MetadataResolver.AssemblyResolver.ResolveAssembly(this);
        }

        IMetadataMember IResolvable.Resolve()
        {
            return Resolve();
        }

        public override string ToString()
        {
            return FullName;
        }

        private static AssemblyReference GetProcessorOwner(AssemblyRefProcessor processor)
        {
            return processor.Reference;
        }

        private static void SetProcessorOwner(AssemblyRefProcessor processor, AssemblyReference reference)
        {
            processor.Reference = reference;
        }

        private static AssemblyReference GetOsOwner(AssemblyRefOs os)
        {
            return os.Reference;
        }

        private static void SetOsOwner(AssemblyRefOs os, AssemblyReference reference)
        {
            os.Reference = reference;
        }
    }
}
