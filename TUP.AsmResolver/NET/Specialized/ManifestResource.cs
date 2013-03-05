using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class ManifestResource : MetaDataMember, IStreamProvider
    {
        Stream stream;
        string name;
        MetaDataMember implementation;

        public uint Offset
        {
            get { return Convert.ToUInt32(metadatarow.parts[0]); }
        }
        
        public Stream Stream
        {
            get
            {
                if (stream == null )
                {
                       if (Implementation == null)
                       {
                           netheader.assembly.peImage.SetOffset(netheader.ResourcesDirectory.TargetOffset.FileOffset + Offset);
                           stream = netheader.assembly.peImage.ReadStream(netheader.assembly.peImage.reader.ReadInt32());
                       }
                       else
                       {
                           // TODO: assemblyref contents
                           if (Implementation is FileReference)
                           {
                               stream = (Implementation as FileReference).Stream;
                   
                           }
                       }
                }
                
                return stream;


            }
        }

        public ManifestResourceAttributes Attributes
        {
            get { return (ManifestResourceAttributes)Convert.ToUInt32(metadatarow.parts[1]); }
        }
        public string Name
        {
            get
            {
                if (name == null)
                    name = netheader.StringsHeap.GetStringByOffset(Convert.ToUInt32(metadatarow.parts[2]));
                return name;
            }
        }
        public MetaDataMember Implementation
        {
            get {
                if (implementation == null)
                {
                    int token = Convert.ToInt32(metadatarow.parts[3]);
                    if (token == 0 || token == 1)
                        return null;
                    implementation = netheader.TablesHeap.Implementation.GetMember(token);
                }
                return implementation;
            }
        }
        public override void ClearCache()
        {
            if (stream != null)
                stream.Dispose();
            stream = null;
            name = null;
            implementation = null;
        }
    }
}
