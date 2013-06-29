using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUP.AsmResolver.NET;
using TUP.AsmResolver.NET.Specialized;

namespace TUP.AsmResolver.PE.Writers
{
    public class Workspace
    {
        public Workspace(WritingParameters writingParameters)
        {
            WritingParameters = writingParameters;
            Members = new Dictionary<MetaDataTableType, MetaDataMemberInfo[]>();
            MethodBodyTable = new MethodBodyTable();
            NewDataDirectories = new DataDirectory[16];
            NewNetHeader = new NETHeader();
        }

        public Dictionary<MetaDataTableType, MetaDataMemberInfo[]> Members;
        public MethodBodyTable MethodBodyTable;
        public readonly WritingParameters WritingParameters;
        public DataDirectory[] NewDataDirectories;
        public NETHeader NewNetHeader;

        public T GetStream<T>() where T : MetaDataStream
        {
            return NewNetHeader.MetaDataStreams.FirstOrDefault(s => s is T) as T;
        }

        public T[] GetMembers<T>(MetaDataTableType table) where T : MetaDataMember
        {
            
            T[] output = new T[Members[table].Length];
            for (int i = 0; i < output.Length; i++)
                output[i] = Members[table][i].Instance as T;

            return output;
        }

        public int GetIndexOfMember<T>(T member) where T : MetaDataMember
        {
            return Array.FindIndex(Members[member.TableType], m => m.Instance == member);
        }
    }

    public struct MetaDataMemberInfo
    {
        public MetaDataMemberInfo(MetaDataMember member)
        {
            member.LoadCache();
            Instance = member;
            OriginalToken = member.MetaDataToken;
        }

        public readonly uint OriginalToken;
        public MetaDataMember Instance;

        public override string ToString()
        {
            if (Instance != null)
                return string.Format("[{0:X}]{1}", OriginalToken, Instance);
            return  string.Format("[{0:X}]",OriginalToken);
        }
    }

    public class MethodBodyTable : IDisposable
    {
        public MethodBodyTable()
        {
            Stream = new MemoryStream();
            Writer = new BinaryWriter(Stream);
        }
	    public List<MethodBodyInfo> MethodEntries = new List<MethodBodyInfo>();

        public MemoryStream Stream;
        public BinaryWriter Writer;

        public void AppendMethodBody(MethodBodyInfo methodBody)
        {
            methodBody.RelativeOffset = (uint)Stream.Position;
            Writer.Write(methodBody.Bytes);
            MethodEntries.Add(methodBody);
        }

        public void Dispose()
        {
            Writer.Dispose();
            Stream.Dispose();
        }
    }


    public struct MethodBodyInfo
    {
        public uint RelativeOffset;
        public byte[] Bytes;
    }
}
