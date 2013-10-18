using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class AssemblyDefinition : AssemblyReference
    {
        string _name;
        string _culture;

        public AssemblyDefinition(MetaDataRow row)
            : base(row)
        {
        }

        public AssemblyDefinition(string name, AssemblyAttributes attributes, Version version, AssemblyHashAlgorithm hashAlgorithm, uint publicKey, string culture)
            : base(new MetaDataRow(
                (uint)hashAlgorithm,
                (byte)version.Major,
                (byte)version.Minor,
                (byte)version.Build,
                (byte)version.Revision,
                (uint)attributes,
                publicKey,
                0U,
                0U))
        {
            this._name = name;
            this._culture = culture;
        }

        public override AssemblyHashAlgorithm HashAlgorithm
        {
            get { return (AssemblyHashAlgorithm)Convert.ToUInt32(_metadatarow._parts[0]); }
        }

        public override Version Version
        {
            get
            {
                return new Version(
                Convert.ToInt32(_metadatarow._parts[1]),
                Convert.ToInt32(_metadatarow._parts[2]),
                Convert.ToInt32(_metadatarow._parts[3]),
                Convert.ToInt32(_metadatarow._parts[4])
                );
            }
            set
            {
                _metadatarow._parts[1] = (ushort)value.Major;
                _metadatarow._parts[2] = (ushort)value.Minor;
                _metadatarow._parts[3] = (ushort)value.Build;
                _metadatarow._parts[4] = (ushort)value.Revision;
            }
        }

        public override AssemblyAttributes Attributes
        {
            get { return (AssemblyAttributes)Convert.ToUInt32(_metadatarow._parts[5]); }
        }

        public override uint PublicKeyOrToken
        {
            get { return Convert.ToUInt32(_metadatarow._parts[6]); }
        }

        public override string Name
        {
            get
            {
                if (string.IsNullOrEmpty(_name))
                    _netheader.StringsHeap.TryGetStringByOffset(Convert.ToUInt32(_metadatarow._parts[7]), out _name);
                return _name;
            }
        }

        public override string Culture
        {
            get
            { 
                if (_culture == null)
                    _netheader.StringsHeap.TryGetStringByOffset(Convert.ToUInt32(_metadatarow._parts[8]), out _name);
                return _culture;
            }
        }

        public override AssemblyDefinition Resolve()
        {
            return this;
        }

        public override string ToString()
        {
            return Name;
        }

        public override void ClearCache()
        {
            _name = null;
            _culture = null;
        }

    }
}
