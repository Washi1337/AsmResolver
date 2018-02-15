using System;
using System.Collections.Generic;
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
        private CustomAttributeCollection _customAttributes;
        private SecurityDeclarationCollection _securityDeclarations;
        private readonly LazyValue<string> _name;
        private readonly LazyValue<string> _culture;
        private readonly LazyValue<DataBlobSignature> _publicKey;
        private Version _version;
        private string _fullName;
        private byte[] _publicKeyToken;

        public AssemblyDefinition(IAssemblyDescriptor info)
            : base(null, new MetadataToken(MetadataTokenType.Assembly))
        {
            _name = new LazyValue<string>(info.Name);
            Version = info.Version;
            _culture = new LazyValue<string>(info.Culture);
            _publicKey = new LazyValue<DataBlobSignature>(info.PublicKeyToken == null ? null : new DataBlobSignature(info.PublicKeyToken));
            Modules = new DelegatedMemberCollection<AssemblyDefinition, ModuleDefinition>(this, GetModuleOwner, SetModuleOwner);
        }

        public AssemblyDefinition(string name, Version version)
            : base(null, new MetadataToken(MetadataTokenType.Assembly))
        {
            _name = new LazyValue<string>(name);
            _version = version;
            _culture = new LazyValue<string>();
            _publicKey = new LazyValue<DataBlobSignature>();
            Modules = new DelegatedMemberCollection<AssemblyDefinition, ModuleDefinition>(this, GetModuleOwner, SetModuleOwner);
        }

        internal AssemblyDefinition(MetadataImage image, MetadataRow<AssemblyHashAlgorithm, ushort, ushort, ushort, ushort, AssemblyAttributes, uint, uint, uint> row)
            : base(image, row.MetadataToken)
        {
            var tableStream = image.Header.GetStream<TableStream>();
            var stringStream = image.Header.GetStream<StringStream>();
            var blobStream = image.Header.GetStream<BlobStream>();

            HashAlgorithm = row.Column1;
            Version = new Version(row.Column2, row.Column3, row.Column4, row.Column5);
            Attributes = row.Column6;
            _publicKey = new LazyValue<DataBlobSignature>(() => row.Column7 == 0 ? null : DataBlobSignature.FromReader(blobStream.CreateBlobReader(row.Column7)));
            _name = new LazyValue<string>(() => stringStream.GetStringByOffset(row.Column8));
            _culture = new LazyValue<string>(() => stringStream.GetStringByOffset(row.Column9));
            Modules = new TableMemberCollection<AssemblyDefinition, ModuleDefinition>(this, tableStream.GetTable(MetadataTokenType.Module), GetModuleOwner, SetModuleOwner);
        }

        /// <summary>
        /// Gets or sets the hashing algorithm that is used for signing the assembly.
        /// </summary>
        public AssemblyHashAlgorithm HashAlgorithm
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the version number of the assembly.
        /// </summary>
        public Version Version
        {
            get { return _version; }
            set
            {
                _version = value;
                _fullName = null;
            }
        }

        /// <summary>
        /// Gets or sets the attributes for the assembly.
        /// </summary>
        public AssemblyAttributes Attributes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the assembly.
        /// </summary>
        public string Name
        {
            get { return _name.Value; }
            set
            {
                _name.Value = value;
                _fullName = null;
            }
        }

        /// <summary>
        /// Gets the fully qualified name of the assembly. This name is compatible with reflection.
        /// </summary>
        public string FullName
        {
            get
            {
                if (_fullName != null)
                    return _fullName;
                return _fullName = this.GetFullName();
            }
        }

        /// <summary>
        /// Gets or sets the culture string of the assembly.
        /// </summary>
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

        /// <summary>
        /// Gets the public key token derived from <see cref="PublicKey"/> for this assembly.
        /// </summary>
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

        /// <summary>
        /// Gets a collection of modules that this assembly defines.
        /// </summary>
        public Collection<ModuleDefinition> Modules
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

        private static AssemblyDefinition GetModuleOwner(ModuleDefinition module)
        {
            return module.Assembly;
        }

        private static void SetModuleOwner(ModuleDefinition module, AssemblyDefinition assembly)
        {
            module.Assembly = assembly;
        }
    }
}
