using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class AssemblyReference : MetaDataMember , IResolutionScope
    {
        internal string _name;
        internal string _culture;
        Version _version;
        CustomAttribute[] _customAttributes = null;

        public AssemblyReference(MetaDataRow row)
            : base(row)
        {
        }

        public AssemblyReference(string name, Version version)
            : this(name, AssemblyAttributes.None, version, AssemblyHashAlgorithm.None, 0, string.Empty)
        {
        }

        public AssemblyReference(string name, AssemblyAttributes attributes, Version version, AssemblyHashAlgorithm hashAlgorithm, uint publicKey, string culture)
            : base(new MetaDataRow(
                (byte)version.Major, (byte)version.Minor, (byte)version.Build, (byte)version.Revision,
                (uint)attributes,
                publicKey,
                0U,
                0U,
                (uint)hashAlgorithm))
        {
            this._name = name;
            this._culture = culture;
        }

        public virtual CustomAttribute[] CustomAttributes
        {
            get
            {
                if (_customAttributes == null && _netheader.TablesHeap.HasTable(MetaDataTableType.CustomAttribute))
                {
                    List<CustomAttribute> customattributes = new List<CustomAttribute>();

                    foreach (var member in _netheader.TablesHeap.GetTable(MetaDataTableType.CustomAttribute).Members)
                    {
                        CustomAttribute attribute = member as CustomAttribute;
                        if (attribute.Parent != null && attribute.Parent._metadatatoken == this._metadatatoken)
                            customattributes.Add(attribute);
                    }

                    _customAttributes = customattributes.ToArray();
                }
                return _customAttributes;
            }
        }

        public bool HasCustomAttributes
        {
            get
            {
                return CustomAttributes != null && CustomAttributes.Length > 0;
            }
        }
        
        public virtual Version Version
        {
            get
            {
                if (_version == null)
                    _version = new Version(
                    Convert.ToInt32(_metadatarow._parts[0]),
                    Convert.ToInt32(_metadatarow._parts[1]),
                    Convert.ToInt32(_metadatarow._parts[2]),
                    Convert.ToInt32(_metadatarow._parts[3])
                    );
                return _version;
            }
            set
            {
                _metadatarow._parts[0] = (ushort)value.Major;
                _metadatarow._parts[1] = (ushort)value.Minor;
                _metadatarow._parts[2] = (ushort)value.Build;
                _metadatarow._parts[3] = (ushort)value.Revision;
            }
        }

        public virtual AssemblyAttributes Attributes
        {
            get { return (AssemblyAttributes)Convert.ToUInt32(_metadatarow._parts[4]); }
        }

        public virtual uint PublicKeyOrToken
        {
            get { return Convert.ToUInt32(_metadatarow._parts[5]); }
        }

        public virtual string Name
        {
            get
            {
                if (string.IsNullOrEmpty(_name))
                    _netheader.StringsHeap.TryGetStringByOffset(Convert.ToUInt32(_metadatarow._parts[6]), out _name);
                return _name;
            }
        }

        public virtual string Culture
        {
            get
            {
                if (string.IsNullOrEmpty(_culture))
                    _culture = _netheader.StringsHeap.GetStringByOffset(Convert.ToUInt32(_metadatarow._parts[7]));
                return _culture;
            }
        }

        public virtual AssemblyHashAlgorithm HashAlgorithm
        {
            get { return (AssemblyHashAlgorithm)Convert.ToUInt32(_metadatarow._parts[8]); }
        }

        public virtual AssemblyDefinition Resolve()
        {
            if (NETHeader == null)
                return null;
            var assembly = NETHeader.MetaDataResolver.AssemblyResolver.Resolve(this);
            if (assembly != null)
            {
                return assembly.NETHeader.TablesHeap.GetTable(MetaDataTableType.Assembly).Members[0] as AssemblyDefinition;
            }
            return null;
        }

        public override string ToString()
        {
            return Name;
        }

        public override void ClearCache()
        {
            _version = null;
            _name = null;
            _customAttributes = null;
        }

        public override void LoadCache()
        {
            this._name = Name;
            this._culture = Culture;
            this._customAttributes = CustomAttributes;
        }
    }
}
