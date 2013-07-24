using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class ManifestResource : MetaDataMember, IStreamProvider
    {
        Stream _stream;
        string _name;
        MetaDataMember _implementation;

        public ManifestResource(MetaDataRow row)
            : base(row)
        {
        }

        public ManifestResource(string name, ManifestResourceAttributes attributes, MetaDataMember implementation, uint offset)
            : base(new MetaDataRow(offset, (uint)attributes, 0U, 0U))
        {
            this._name = name;
            this._implementation = implementation;
        }

        public uint Offset
        {
            get { return Convert.ToUInt32(_metadatarow._parts[0]); }
        }
        
        public Stream Stream
        {
            get
            {
                if (_stream == null )
                {
                       if (Implementation == null)
                       {
                           _netheader._assembly._peImage.SetOffset(_netheader.ResourcesDirectory.TargetOffset.FileOffset + Offset);
                           _stream = _netheader._assembly._peImage.ReadStream(_netheader._assembly._peImage.Reader.ReadInt32());
                       }
                       else
                       {
                           // TODO: assemblyref contents
                           if (Implementation is FileReference)
                           {
                               _stream = (Implementation as FileReference).Stream;
                   
                           }
                       }
                }
                
                return _stream;


            }
        }

        public ManifestResourceAttributes Attributes
        {
            get { return (ManifestResourceAttributes)Convert.ToUInt32(_metadatarow._parts[1]); }
        }

        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(_name))
                    _netheader.StringsHeap.TryGetStringByOffset(Convert.ToUInt32(_metadatarow._parts[2]), out _name);
                return _name;
            }
        }

        public MetaDataMember Implementation
        {
            get 
            {
                if (_implementation == null)
                {
                    int token = Convert.ToInt32(_metadatarow._parts[3]);
                    if (token == 0 || token == 1)
                        return null;

                    _netheader.TablesHeap.Implementation.TryGetMember(token, out _implementation);
                    
                }
                return _implementation;
            }
        }

        public override void ClearCache()
        {
            if (_stream != null)
                _stream.Dispose();
            _stream = null;
            _name = null;
            _implementation = null;
        }

        public override void LoadCache()
        {
            _stream = Stream;
            _name = Name;
            _implementation = Implementation;
        }
    }
}
