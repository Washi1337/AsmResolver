using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class FileReference : MetaDataMember , IStreamProvider
    {
        Stream _stream;
        string _name;

        public FileReference(MetaDataRow row)
            : base(row)
        {
        }

        public FileReference(string name, FileAttributes flags, uint hash)
            : base(new MetaDataRow((uint)flags, 0U, hash))
        {
            this._name = name;
        }

        public FileAttributes Flags { get { return (FileAttributes)Convert.ToUInt32(_metadatarow._parts[0]); } }

        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(_name))
                    _netheader.StringsHeap.TryGetStringByOffset(Convert.ToUInt32(_metadatarow._parts[1]), out _name);
                return _name;
            }
        }

        public uint Hash { get { return Convert.ToUInt32(_metadatarow._parts[2]); } }

        public Stream Stream
        {
            get {
                if (_stream == null)
                {
                    string fullpath = _netheader._assembly._path.Substring(0, _netheader._assembly._path.LastIndexOf('\\') + 1) + Name;
                    if (File.Exists(fullpath))
                        _stream = File.Open(fullpath, FileMode.Open, FileAccess.Read);
                }
                return _stream;
            }
        }

        public override void ClearCache()
        {
            if (_stream != null)
                _stream.Dispose();
            _stream = null;
            _name = null;
        }

        public override void LoadCache()
        {
            _stream = Stream;
            _name = Name;
        }
    }
}
