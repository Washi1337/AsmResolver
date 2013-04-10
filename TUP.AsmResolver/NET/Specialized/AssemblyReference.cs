using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class AssemblyReference : MetaDataMember , IResolutionScope
    {
        internal string name;
        Version version;

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
            get {
                if (!string.IsNullOrEmpty(name))
                    return name;
                name = netheader.StringsHeap.GetStringByOffset(Convert.ToUInt32(metadatarow.parts[6]));
                return name;
            }
        }
        public virtual string Culture
        {
            get { return netheader.StringsHeap.GetStringByOffset(Convert.ToUInt32(metadatarow.parts[7])); }
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
        }
    }
}
