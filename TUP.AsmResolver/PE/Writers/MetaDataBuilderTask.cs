using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUP.AsmResolver.NET;
using TUP.AsmResolver.NET.Specialized;

namespace TUP.AsmResolver.PE.Writers
{
    public class MetaDataBuilderTask : RebuildingTask 
    {
        public MetaDataBuilderTask(PEConstructor constructor)
            : base(constructor)
        {
        }

        public override void RunProcedure(Workspace workspace)
        {
            ReorderRanges(workspace);
            SetNewMetaDataTokens(workspace);
        }

        private void ReorderRanges(Workspace workspace)
        {
            ReorderRange<TypeDefinition, FieldDefinition>(workspace, MetaDataTableType.TypeDef, MetaDataTableType.Field, 4, t => t.Fields);
            ReorderRange<TypeDefinition, MethodDefinition>(workspace, MetaDataTableType.TypeDef, MetaDataTableType.Method, 5, t => t.Methods);
            ReorderRange<PropertyMap, PropertyDefinition>(workspace, MetaDataTableType.PropertyMap, MetaDataTableType.Property, 1, m => m.Properties);
            ReorderRange<EventMap, EventDefinition>(workspace, MetaDataTableType.EventMap, MetaDataTableType.Event, 1, m => m.Events);
            ReorderRange<MethodDefinition, ParameterDefinition>(workspace, MetaDataTableType.Method, MetaDataTableType.Param, 1, m => m.Parameters);
        }

        private void ReorderRange<TParent, TMember>(Workspace workspace, MetaDataTableType parentTable, MetaDataTableType memberTable, int memberListIndex, Func<TParent, TMember[]> getMembersFunc)
            where TParent : MetaDataMember
            where TMember : MetaDataMember 
        {
            if (workspace.Members.ContainsKey(parentTable) && workspace.Members.ContainsKey(memberTable))
            {
                MetaDataMemberInfo[] newRange = new MetaDataMemberInfo[workspace.Members[memberTable].Length];
                TParent[] parentMembers = workspace.GetMembers<TParent>(parentTable);
                MetaDataTable targetTable = workspace.Members[memberTable][0].Instance.Table;

                uint currentIndex = 0;

                for (int i = 0; i < parentMembers.Length; i++)
                {
                    TMember[] members = getMembersFunc(parentMembers[i]);

                    if (members.Length > 0)
                    {
                        int startIndex = workspace.GetIndexOfMember(members[0]);
                        int endIndex = workspace.GetIndexOfMember(members[members.Length - 1]);

                        Array.Copy(workspace.Members[memberTable], startIndex, newRange, currentIndex, endIndex - startIndex + 1);
                    }

                    parentMembers[i].MetaDataRow.Parts[memberListIndex] = ConvertToIndex(targetTable, currentIndex + 1);

                    currentIndex += (uint)members.Length;

                }

                workspace.Members[memberTable] = newRange;
            }
        }

        private ValueType ConvertToIndex(MetaDataTable table, uint index)
        {
            if (table.IsLarge(0))
                return index;
            return (ushort)index;
        }

        private void SetNewMetaDataTokens(Workspace workspace)
        {
            foreach (var keyPair in workspace.Members)
            {
                uint root = (uint)keyPair.Key << 24;

                for (uint i = 0; i < keyPair.Value.Length; i++)
                {
                    keyPair.Value[i].Instance._metadatatoken = root + (i + 1);
                }
            }
        }
    }
}
