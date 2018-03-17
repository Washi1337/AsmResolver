using System;
using System.Linq;
using System.Security.Cryptography;
using AsmResolver.Collections.Generic;
using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cts
{
    /// <summary>
    /// Represents an assembly definition as defined in a .NET assembly image.
    /// </summary>
    public class AssemblyDefinition : MetadataMember<MetadataRow<AssemblyHashAlgorithm, ushort, ushort, ushort, ushort, AssemblyAttributes, uint, uint, uint>>, IHasCustomAttribute, IHasSecurityAttribute, IAssemblyDescriptor
    {
        private readonly LazyValue<string> _name;
        private readonly LazyValue<string> _culture;
        private readonly LazyValue<DataBlobSignature> _publicKey;
        private Version _version;
        private string _fullName;
        private byte[] _publicKeyToken;
        private MetadataImage _image;

        public AssemblyDefinition(IAssemblyDescriptor info)
            : base(new MetadataToken(MetadataTokenType.Assembly))
        {
            _name = new LazyValue<string>(info.Name);
            Version = info.Version;
            _culture = new LazyValue<string>(info.Culture);
            _publicKey = new LazyValue<DataBlobSignature>(info.PublicKeyToken == null ? null : new DataBlobSignature(info.PublicKeyToken));
            Modules = new DelegatedMemberCollection<AssemblyDefinition, ModuleDefinition>(this, GetModuleOwner, SetModuleOwner);
            AssemblyReferences = new Collection<AssemblyReference>();
            ModuleReferences = new Collection<ModuleReference>();
            SecurityDeclarations = new SecurityDeclarationCollection(this);
            Resources = new Collection<ManifestResource>();
            Files = new Collection<FileReference>();
            OperatingSystems = new DelegatedMemberCollection<AssemblyDefinition,AssemblyOs>(this, GetOsOwner, SetOsOwner);
            Processors = new DelegatedMemberCollection<AssemblyDefinition,AssemblyProcessor>(this, GetProcessorOwner, SetProcessorOwner);
        }

        public AssemblyDefinition(string name, Version version)
            : base(new MetadataToken(MetadataTokenType.Assembly))
        {
            _name = new LazyValue<string>(name);
            _version = version;
            _culture = new LazyValue<string>();
            _publicKey = new LazyValue<DataBlobSignature>();
            Modules = new DelegatedMemberCollection<AssemblyDefinition, ModuleDefinition>(this, GetModuleOwner, SetModuleOwner);
            AssemblyReferences = new Collection<AssemblyReference>();
            ModuleReferences = new Collection<ModuleReference>();
            CustomAttributes = new CustomAttributeCollection(this);
            SecurityDeclarations = new SecurityDeclarationCollection(this);
            Resources = new Collection<ManifestResource>();
            Files = new Collection<FileReference>();
            OperatingSystems = new DelegatedMemberCollection<AssemblyDefinition,AssemblyOs>(this, GetOsOwner, SetOsOwner);
            Processors = new DelegatedMemberCollection<AssemblyDefinition,AssemblyProcessor>(this, GetProcessorOwner, SetProcessorOwner);
        }

        internal AssemblyDefinition(MetadataImage image, MetadataRow<AssemblyHashAlgorithm, ushort, ushort, ushort, ushort, AssemblyAttributes, uint, uint, uint> row)
            : base(row.MetadataToken)
        {
            _image = image;
            var tableStream = image.Header.GetStream<TableStream>();
            var stringStream = image.Header.GetStream<StringStream>();
            var blobStream = image.Header.GetStream<BlobStream>();

            HashAlgorithm = row.Column1;
            Version = new Version(row.Column2, row.Column3, row.Column4, row.Column5);
            Attributes = row.Column6;
            
            _publicKey = new LazyValue<DataBlobSignature>(() => 
                row.Column7 == 0 ? null : DataBlobSignature.FromReader(blobStream.CreateBlobReader(row.Column7)));
            
            _name = new LazyValue<string>(() => 
                stringStream.GetStringByOffset(row.Column8));
            
            _culture = new LazyValue<string>(() => 
                stringStream.GetStringByOffset(row.Column9));
           
            Modules = new TableMemberCollection<AssemblyDefinition, ModuleDefinition>(
                this, tableStream.GetTable(MetadataTokenType.Module), GetModuleOwner, SetModuleOwner);
            
            AssemblyReferences = new TableMemberCollection<AssemblyDefinition, AssemblyReference>(
                this, tableStream.GetTable(MetadataTokenType.AssemblyRef), GetReferenceOwner, SetReferenceOwner);
            
            ModuleReferences = new TableMemberCollection<AssemblyDefinition, ModuleReference>(
                this, tableStream.GetTable(MetadataTokenType.ModuleRef), GetReferenceOwner, SetReferenceOwner);
            
            Resources = new TableMemberCollection<AssemblyDefinition, ManifestResource>(
                this, tableStream.GetTable(MetadataTokenType.ManifestResource), GetResourceOwner, SetResourceOwner);
            
            Files = new TableMemberCollection<AssemblyDefinition, FileReference>(
                this, tableStream.GetTable(MetadataTokenType.File), GetFileOwner, SetFileOwner);
            
            CustomAttributes = new CustomAttributeCollection(this);
            SecurityDeclarations = new SecurityDeclarationCollection(this);
            
            OperatingSystems = new TableMemberCollection<AssemblyDefinition, AssemblyOs>(
                this, tableStream.GetTable(MetadataTokenType.AssemblyOs), GetOsOwner, SetOsOwner);
            
            Processors = new TableMemberCollection<AssemblyDefinition, AssemblyProcessor>(
                this, tableStream.GetTable(MetadataTokenType.AssemblyProcessor), GetProcessorOwner, SetProcessorOwner);
        }

        /// <inheritdoc />
        public override MetadataImage Image
        {
            get { return _image; }
        }

        /// <summary>
        /// Gets or sets the hashing algorithm that is used for signing the assembly.
        /// </summary>
        public AssemblyHashAlgorithm HashAlgorithm
        {
            get;
            set;
        }

        /// <inheritdoc />
        public Version Version
        {
            get { return _version; }
            set
            {
                _version = value;
                _fullName = null;
            }
        }

        /// <inheritdoc />
        public AssemblyAttributes Attributes
        {
            get;
            set;
        }

        /// <inheritdoc />
        public string Name
        {
            get { return _name.Value; }
            set
            {
                _name.Value = value;
                _fullName = null;
            }
        }

        /// <inheritdoc />
        public string FullName
        {
            get
            {
                if (_fullName != null)
                    return _fullName;
                return _fullName = this.GetFullName();
            }
        }

        /// <inheritdoc />
        public string Culture
        {
            get { return _culture.Value; }
            set
            {
                _culture.Value = value;
                _fullName = null;
            }
        }

        /// <summary>
        /// Gets or sets the public key data blob of this assembly (if available).
        /// </summary>
        public DataBlobSignature PublicKey
        {
            get { return _publicKey.Value; }
            set
            {
                _publicKey.Value = value;
                _publicKeyToken = null;
                _fullName = null;
            }
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public CustomAttributeCollection CustomAttributes
        {
            get;
            private set;
        }

        /// <inheritdoc />
        public SecurityDeclarationCollection SecurityDeclarations
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a collection of modules that this assembly defines.
        /// </summary>
        public Collection<ModuleDefinition> Modules
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a collection of dependency assemblies that this assembly references.
        /// </summary>
        public Collection<AssemblyReference> AssemblyReferences
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a collection of dependency modules that this assembly references.
        /// </summary>
        public Collection<ModuleReference> ModuleReferences
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a collection of manifest resources that this assembly embeds.
        /// </summary>
        public Collection<ManifestResource> Resources
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a collection of extra files that this assembly consists of.
        /// </summary>
        public Collection<FileReference> Files
        {
            get;
            private set;
        }
        
        /// <summary>
        /// Gets a collection of operating systems supported by this assembly.
        /// </summary>
        /// <remarks>This collection is no longer used in the newer versions of the .NET framework.</remarks>
        public Collection<AssemblyOs> OperatingSystems
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a collection of processors supported by this assembly.  
        /// </summary>
        /// <remarks>This collection is no longer used in the newer versions of the .NET framework.</remarks>
        public Collection<AssemblyProcessor> Processors
        {
            get;
            private set;
        }

        public override string ToString()
        {
            return FullName;
        }

        IMetadataMember IResolvable.Resolve()
        {
            return this;
        }
        
        private static byte[] ComputePublicKeyToken(byte[] key, AssemblyHashAlgorithm algorithm)
        {
            HashAlgorithm hashAlgorithm;
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

        private static AssemblyDefinition GetModuleOwner(ModuleDefinition module)
        {
            return module.Assembly;
        }

        private static void SetModuleOwner(ModuleDefinition module, AssemblyDefinition assembly)
        {
            module.Assembly = assembly;
        }

        private static AssemblyDefinition GetProcessorOwner(AssemblyProcessor processor)
        {
            return processor.Assembly;
        }

        private static void SetProcessorOwner(AssemblyProcessor processor, AssemblyDefinition definition)
        {
            processor.Assembly = definition;
        }

        private static AssemblyDefinition GetOsOwner(AssemblyOs os)
        {
            return os.Assembly;
        }

        private static void SetOsOwner(AssemblyOs os, AssemblyDefinition definition)
        {
            os.Assembly = definition;
        }

        private static void SetFileOwner(FileReference file, AssemblyDefinition definition)
        {
            file.Referrer = definition;
        }

        private static AssemblyDefinition GetFileOwner(FileReference file)
        {
            return file.Referrer;
        }

        private static void SetResourceOwner(ManifestResource resource, AssemblyDefinition definition)
        {
            resource.Owner = definition;
        }

        private static AssemblyDefinition GetResourceOwner(ManifestResource arg)
        {
            return arg.Owner;
        }

        private static void SetReferenceOwner(ModuleReference module, AssemblyDefinition definition)
        {
            module.Referrer = definition;
        }

        private static AssemblyDefinition GetReferenceOwner(ModuleReference arg)
        {
            return arg.Referrer;
        }

        private static void SetReferenceOwner(AssemblyReference reference, AssemblyDefinition definition)
        {
            reference.Referrer = definition;
        }

        private static AssemblyDefinition GetReferenceOwner(AssemblyReference arg)
        {
            return arg.Referrer;
        }
    }
}
