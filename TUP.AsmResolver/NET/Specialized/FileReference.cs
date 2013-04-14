using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class FileReference : MetaDataMember , IStreamProvider
    {
        Stream stream;
        string name;

        public FileReference(MetaDataRow row)
            : base(row)
        {
        }

        public FileReference(string name, FileAttributes flags, uint hash)
            : base(new MetaDataRow((uint)flags, 0U, hash))
        {
            this.name = name;
        }

        public FileAttributes Flags { get { return (FileAttributes)Convert.ToUInt32(metadatarow.parts[0]); } }
        public string Name
        {
            get
            {
                if (name == null)
                    name = netheader.StringsHeap.GetStringByOffset(Convert.ToUInt32(metadatarow.parts[1]));
                return name;
            }
        }
        public uint Hash { get { return Convert.ToUInt32(metadatarow.parts[2]); } }

        public Stream Stream
        {
            get {
                if (stream == null)
                {
                    string fullpath = netheader.assembly.path.Substring(0, netheader.assembly.path.LastIndexOf('\\') + 1) + Name;
                    if (File.Exists(fullpath))
                        stream = File.Open(fullpath, FileMode.Open, FileAccess.Read);
                }
                return stream;
            }
        }
        public override void ClearCache()
        {
            if (stream != null)
                stream.Dispose();
            stream = null;
            name = null;
        }

    }
}
