using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TUP.AsmResolver.NET;
using TUP.AsmResolver.NET.Specialized;
namespace TUP.AsmResolver.PE.Writers
{
    internal class NETTableReconstructor : IReconstructionTask
    {
        TablesHeap tablesHeap;
        NETHeader netHeader;
        MemoryStream stream;
        BinaryWriter writer;

        internal NETTableReconstructor(TablesHeap tablesHeap)
        {
            this.tablesHeap = tablesHeap;
            this.netHeader = tablesHeap.netheader;
            stream = new MemoryStream();
            writer = new BinaryWriter(stream);
            stream.Seek(0, SeekOrigin.Begin);
        }

        public void Reconstruct()
        {

            GenerateMetadataRows();

            WriteTablesHeapHeader();
        }

        internal void UpdateTablesHeapHeader()
        {
            tablesHeap.HeapOffsetSizes = 0;
            foreach (MetaDataStream stream in netHeader.MetaDataStreams)
            {
                if (stream.StreamSize >= 0xFFFF)
                {
                    stream.indexsize = 4;

                    if (stream is StringsHeap)
                        tablesHeap.HeapOffsetSizes |= 1;
                    if (stream is GuidHeap)
                        tablesHeap.HeapOffsetSizes |= 2;
                    if (stream is BlobHeap)
                        tablesHeap.HeapOffsetSizes |= 4;
                }
            }


        }

        internal void GenerateMetadataRows()
        {
            for (int i = 0; i < tablesHeap.tables.Length; i++)
            {
                if (tablesHeap.HasTable((MetaDataTableType)i))
                {
                    MetaDataTable table = tablesHeap.GetTable((MetaDataTableType)i);
                    foreach (MetaDataMember member in table.Members)
                    {
                        if (!member.HasMetaDataRow)
                        {
                            switch (table.Type)
                            {
                                case MetaDataTableType.Module:
                                    member.metadatarow = CreateModuleRow((ModuleDefinition)member);
                                    break;
                                case MetaDataTableType.TypeRef:
                                    member.metadatarow = CreateModuleRow((ModuleDefinition)member);
                                    break;
                                case MetaDataTableType.TypeDef:
                                    member.metadatarow = CreateModuleRow((ModuleDefinition)member);
                                    break;
                                case MetaDataTableType.Field:
                                    member.metadatarow = CreateModuleRow((ModuleDefinition)member);
                                    break;
                                case MetaDataTableType.Method:
                                    member.metadatarow = CreateModuleRow((ModuleDefinition)member);
                                    break;
                                case MetaDataTableType.Param:
                                    member.metadatarow = CreateModuleRow((ModuleDefinition)member);
                                    break;
                                case MetaDataTableType.InterfaceImpl:
                                    member.metadatarow = CreateModuleRow((ModuleDefinition)member);
                                    break;
                                case MetaDataTableType.MemberRef:
                                    member.metadatarow = CreateModuleRow((ModuleDefinition)member);
                                    break;
                                case MetaDataTableType.Constant:
                                    member.metadatarow = CreateModuleRow((ModuleDefinition)member);
                                    break;
                                case MetaDataTableType.CustomAttribute:
                                    member.metadatarow = CreateModuleRow((ModuleDefinition)member);
                                    break;
                                case MetaDataTableType.FieldMarshal:
                                    member.metadatarow = CreateModuleRow((ModuleDefinition)member);
                                    break;
                                case MetaDataTableType.DeclSecurity:
                                    member.metadatarow = CreateModuleRow((ModuleDefinition)member);
                                    break;
                                case MetaDataTableType.ClassLayout:
                                    member.metadatarow = CreateModuleRow((ModuleDefinition)member);
                                    break;
                                case MetaDataTableType.FieldLayout:
                                    member.metadatarow = CreateModuleRow((ModuleDefinition)member);
                                    break;
                                case MetaDataTableType.StandAloneSig:
                                    member.metadatarow = CreateModuleRow((ModuleDefinition)member);
                                    break;
                                case MetaDataTableType.EventMap:
                                    member.metadatarow = CreateModuleRow((ModuleDefinition)member);
                                    break;
                                case MetaDataTableType.Event:
                                    member.metadatarow = CreateModuleRow((ModuleDefinition)member);
                                    break;
                                case MetaDataTableType.PropertyMap:
                                    member.metadatarow = CreateModuleRow((ModuleDefinition)member);
                                    break;
                                case MetaDataTableType.Property:
                                    member.metadatarow = CreateModuleRow((ModuleDefinition)member);
                                    break;
                                case MetaDataTableType.MethodSemantics:
                                    member.metadatarow = CreateModuleRow((ModuleDefinition)member);
                                    break;
                                case MetaDataTableType.MethodImpl:
                                    member.metadatarow = CreateModuleRow((ModuleDefinition)member);
                                    break;
                                case MetaDataTableType.ModuleRef:
                                    member.metadatarow = CreateModuleRow((ModuleDefinition)member);
                                    break;
                                case MetaDataTableType.TypeSpec:
                                    member.metadatarow = CreateModuleRow((ModuleDefinition)member);
                                    break;
                                case MetaDataTableType.MethodSpec:
                                    member.metadatarow = CreateModuleRow((ModuleDefinition)member);
                                    break;
                                case MetaDataTableType.ImplMap:
                                    member.metadatarow = CreateModuleRow((ModuleDefinition)member);
                                    break;
                                case MetaDataTableType.FieldRVA:
                                    member.metadatarow = CreateModuleRow((ModuleDefinition)member);
                                    break;
                                case MetaDataTableType.Assembly:
                                    member.metadatarow = CreateModuleRow((ModuleDefinition)member);
                                    break;
                                case MetaDataTableType.AssemblyRef:
                                    member.metadatarow = CreateModuleRow((ModuleDefinition)member);
                                    break;
                                case MetaDataTableType.File:
                                    member.metadatarow = CreateModuleRow((ModuleDefinition)member);
                                    break;
                                case MetaDataTableType.ExportedType:
                                    member.metadatarow = CreateModuleRow((ModuleDefinition)member);
                                    break;
                                case MetaDataTableType.ManifestResource:
                                    member.metadatarow = CreateModuleRow((ModuleDefinition)member);
                                    break;
                                case MetaDataTableType.NestedClass:
                                    member.metadatarow = CreateModuleRow((ModuleDefinition)member);
                                    break;
                                case MetaDataTableType.EncLog:
                                    member.metadatarow = CreateModuleRow((ModuleDefinition)member);
                                    break;
                                case MetaDataTableType.EncMap:
                                    member.metadatarow = CreateModuleRow((ModuleDefinition)member);
                                    break;
                                case MetaDataTableType.GenericParam:
                                    member.metadatarow = CreateModuleRow((ModuleDefinition)member);
                                    break;
                                case MetaDataTableType.GenericParamConstraint:
                                    member.metadatarow = CreateModuleRow((ModuleDefinition)member);
                                    break;
                            }
                        }
                    }
                }
            }
        }

        internal object GetHeapOffset(MetaDataStream stream, uint offset)
        {
            if (stream.indexsize == 4)
                return offset;
            else
                return (ushort)offset;
        }

        internal MetaDataRow CreateModuleRow(ModuleDefinition moduleDef)
        {
            object[] parts = new object[]
            {
                (ushort)0,
                GetHeapOffset(netHeader.StringsHeap, netHeader.StringsHeap.GetStringOffset(moduleDef.Name)),
                
            };
            return null;
        }


        internal void WriteTablesHeapHeader()
        {

        }
    }
}
