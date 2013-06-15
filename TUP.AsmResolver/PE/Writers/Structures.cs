using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUP.AsmResolver.NET.Specialized;

namespace TUP.AsmResolver.PE.Writers
{
    public class Workspace
    {
        public Workspace(WritingParameters writingParameters)
        {
            WritingParameters = writingParameters;
        }

        public List<MetaDataMemberInfo> Members;
        public MethodBodyTable MethodBodyTable = new MethodBodyTable();
        public readonly WritingParameters WritingParameters;

    }

    public struct MetaDataMemberInfo
    {
    	 public uint OriginalToken;
    	 public uint NewToken;
    	 public string[] TempStrings;
	     public byte[][] TempBlobs; // <-- should be removed once all blobs are handled.
    	 public MetaDataMember Instance;
    }

    public class MethodBodyTable
    {
	    public List<MethodBodyInfo> MethodEntries = new List<MethodBodyInfo>();
	    
        Stream Stream;
        BinaryWriter Writer;

        public void AppendMethodBody(ref MethodBodyInfo methodBody)
        {
            methodBody.RelativeOffset = (uint)Stream.Position;
            Writer.Write(methodBody.Bytes);
        }
    }


    public struct MethodBodyInfo
    {
        public uint RelativeOffset;
        public byte[] Bytes;
    }
}
