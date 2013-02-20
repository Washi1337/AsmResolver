using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class AssemblyReference : MetaDataMember 
    {
        internal string name;
        Version version;

        public Version Version
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
        public AssemblyAttributes Attributes
        {
            get { return (AssemblyAttributes)Convert.ToUInt32(metadatarow.parts[4]); }
        }
        public uint PublicKeyOrToken
        {
            get { return Convert.ToUInt32(metadatarow.parts[5]); }
        }
        public string Name
        {
            get {
                if (!string.IsNullOrEmpty(name))
                    return name;
                name = netheader.StringsHeap.GetStringByOffset(Convert.ToUInt32(metadatarow.parts[6]));
                return name;
            }
        }
        public string Culture
        {
            get { return netheader.StringsHeap.GetStringByOffset(Convert.ToUInt32(metadatarow.parts[7])); }
        }
        public uint HashAlgorithm
        {
            get { return Convert.ToUInt32(metadatarow.parts[8]); }
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
