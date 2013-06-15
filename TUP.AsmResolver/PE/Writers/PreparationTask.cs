using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUP.AsmResolver.NET;
using TUP.AsmResolver.NET.Specialized;
using TUP.AsmResolver.NET.Specialized.MSIL;

namespace TUP.AsmResolver.PE.Writers
{
    public class PreparationTask : RebuildingTask
    {
        public PreparationTask(PEConstructor constructor)
            : base(constructor)
        {
        }

        public override void RunProcedure(Workspace workspace)
        {
            NETHeader netHeader = Constructor.OriginalAssembly.NETHeader;
            
            // Tasks:
            CacheMethodBodies(netHeader.TablesHeap);
            CacheBlobs(netHeader.BlobHeap);
            CollectMembers(netHeader.TablesHeap, workspace);
            ClearAllStreams(netHeader);
        }

        private void CacheMethodBodies(TablesHeap tablesHeap)
        {
            Constructor.Log("Caching .NET method bodies.");
            foreach (MethodDefinition methodDef in tablesHeap.GetTable(MetaDataTableType.Method).Members)
            {
                if (methodDef.HasBody)
                {
                    // check if valid rva.
                    if (!Constructor.OriginalAssembly.Image.ContainsOffset(Offset.FromRva(methodDef.RVA, Constructor.OriginalAssembly).FileOffset))
                        Constructor.Log("Failed to read method body.", LogMessageType.Error, new ArgumentException("Invalid method RVA."));

                    // cache instructions.
                    MSILInstruction[] instructions = methodDef.Body.Instructions;
                }
            }
        }

        private void CacheBlobs(BlobHeap blobHeap)
        {
            Constructor.Log("Caching .NET blob signatures.");
            blobHeap.ReadAllBlobs();
        }

        private void CollectMembers(TablesHeap tablesHeap, Workspace workspace)
        {
            Constructor.Log("Collecting .NET metadata members.");
            for (int i = 0; i < tablesHeap.tables.Length; i++)
            {
                MetaDataTableType tableType = (MetaDataTableType)i;
                if (tablesHeap.HasTable(tableType))
                {
                    MetaDataTable table = tablesHeap.GetTable(tableType);

                    if (table.Members.Length != 0)
                    {
                        MetaDataMemberInfo[] subMembers = new MetaDataMemberInfo[table.Members.Length];

                        for (int j = 0; i < subMembers.Length; j++)
                        {
                            subMembers[j] = new MetaDataMemberInfo()
                            {
                                Instance = table.Members[j],
                                OriginalToken = table.Members[j].MetaDataToken,
                            };

                            // TODO: Cache all strings (maybe an extra method in MetaDataMember)
                        }

                        workspace.Members.AddRange(subMembers);
                    }
                }
            }
        }

        private void ClearAllStreams(NETHeader netHeader)
        {
            Constructor.Log("Clearing .NET metadata streams.");
            for (int i = 0; i < netHeader.MetaDataStreams.Count; i++)
            {
                netHeader.MetaDataStreams[i].MakeEmpty();
            }
        }

    }
}
