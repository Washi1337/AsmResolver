using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class AssemblyReference : MetaDataMember , IResolutionScope
    {
        internal string name;
        internal string culture;
        Version version;
        CustomAttribute[] customAttributes = null;

        public AssemblyReference(MetaDataRow row)
            : base(row)
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
            this.name = name;
            this.culture = culture;
        }

        public virtual CustomAttribute[] CustomAttributes
        {
            get
            {
                if (customAttributes == null && netheader.TablesHeap.HasTable(MetaDataTableType.CustomAttribute))
                {
                    List<CustomAttribute> customattributes = new List<CustomAttribute>();

                    foreach (var member in netheader.TablesHeap.GetTable(MetaDataTableType.CustomAttribute).Members)
                    {
                        CustomAttribute attribute = member as CustomAttribute;
                        if (attribute.Parent != null && attribute.Parent.metadatatoken == this.metadatatoken)
                            customattributes.Add(attribute);
                    }

                    customAttributes = customattributes.ToArray();
                }
                return customAttributes;
            }
        }

        public virtual Version Version
        {
            get
            {
                if (version == null)
                    version = new Version(
                    Convert.ToInt32(metadatarow.parts[0]),
                    Convert.ToInt32(metadatarow.parts[1]),
                    Convert.ToInt32(metadatarow.parts[2]),
                    Convert.ToInt32(metadatarow.parts[3])
                    );
                return version;
            }
        }

        public virtual AssemblyAttributes Attributes
        {
            get { return (AssemblyAttributes)Convert.ToUInt32(metadatarow.parts[4]); }
        }

        public virtual uint PublicKeyOrToken
        {
            get { return Convert.ToUInt32(metadatarow.parts[5]); }
        }

        public virtual string Name
        {
            get
            {
                if (string.IsNullOrEmpty(name))
                    netheader.StringsHeap.TryGetStringByOffset(Convert.ToUInt32(metadatarow.parts[6]), out name);
                return name;
            }
        }

        public virtual string Culture
        {
            get
            {
                if (string.IsNullOrEmpty(culture))
                    culture = netheader.StringsHeap.GetStringByOffset(Convert.ToUInt32(metadatarow.parts[7]));
                return culture;
            }
        }

        public virtual AssemblyHashAlgorithm HashAlgorithm
        {
            get { return (AssemblyHashAlgorithm)Convert.ToUInt32(metadatarow.parts[8]); }
        }

        public override string ToString()
        {
            return Name;
        }


        public override void ClearCache()
        {
            version = null;
            name = null;
            customAttributes = null;
        }
    }
}
