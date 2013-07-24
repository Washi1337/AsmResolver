using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using TUP.AsmResolver.NET;
using TUP.AsmResolver.NET.Specialized;

namespace TUP.AsmResolver.PE.Readers
{
    internal class NETTableReader : IDisposable
    {
        internal TablesHeap tablesHeap;
        uint tablesOffset = 0;


        internal NETTableReader(TablesHeap tablesheap)
        {
            using (BinaryReader reader = new BinaryReader(new MemoryStream(tablesheap.Contents)))
            {
                tablesheap.header = ASMGlobals.ReadStructureFromReader<Structures.METADATA_TABLE_HEADER>(reader);
                this.tablesHeap = tablesheap;

                for (int i = 0; i < 45; i++)
                    if (tablesHeap.HasTable((MetaDataTableType)i))
                    {
                        tablesHeap.tablecount++;
                    }

                tablesHeap.tablereader = this;
                if ((tablesHeap.HeapOffsetSizes & 1) == 1)
                    tablesHeap.netheader.StringsHeap.indexsize = 4;
                if ((tablesHeap.HeapOffsetSizes & 2) == 2)
                    tablesHeap.netheader.GuidHeap.indexsize = 4;
                if ((tablesHeap.HeapOffsetSizes & 4) == 4)
                    tablesHeap.netheader.BlobHeap.indexsize = 4;

                ReadTableHeaders(reader);
            }
        }

        internal MetaDataTable CreateTable(MetaDataTableType type, int rowAmount, long rowAmountOffset)
        {
            //customattribute[permission]?

            MetaDataTable table = new MetaDataTable(tablesHeap, false);
            table.Type = type;
            table.rowAmount = rowAmount;
            table.rowAmountOffset = rowAmountOffset;
            switch (type)
            {
                case MetaDataTableType.Assembly:
                    tablesHeap.HasCustomAttribute[14] = table;
                    tablesHeap.HasDeclSecurity[2] = table;
                    break;
                case MetaDataTableType.AssemblyRef:
                    tablesHeap.HasCustomAttribute[15] = table;
                    tablesHeap.Implementation[1] = table;
                    tablesHeap.ResolutionScope[2] = table;
                    break;
                case MetaDataTableType.Event:
                    tablesHeap.HasCustomAttribute[10] = table;
                    tablesHeap.HasSemantics[0] = table;
                    break;
                case MetaDataTableType.ExportedType:
                    tablesHeap.HasCustomAttribute[17] = table;
                    tablesHeap.Implementation[2] = table;
                    break;
                case MetaDataTableType.Field:
                    tablesHeap.HasConstant[0] = table;
                    tablesHeap.HasCustomAttribute[1] = table;
                    tablesHeap.HasFieldMarshall[0] = table;
                    tablesHeap.MemberForwarded[0] = table;
                    break;
                case MetaDataTableType.File:
                    tablesHeap.HasCustomAttribute[16] = table;
                    tablesHeap.Implementation[0] = table;
                    break;
                case MetaDataTableType.GenericParam:
                    tablesHeap.HasCustomAttribute[19] = table;
                    break;
                case MetaDataTableType.GenericParamConstraint:
                    tablesHeap.HasCustomAttribute[20] = table;
                    break;
                case MetaDataTableType.InterfaceImpl:
                    tablesHeap.HasCustomAttribute[5] = table;
                    break;
                case MetaDataTableType.ManifestResource:
                    tablesHeap.HasCustomAttribute[18] = table;
                    break;
                case MetaDataTableType.MemberRef:
                    tablesHeap.HasCustomAttribute[6] = table;
                    tablesHeap.MethodDefOrRef[1] = table;
                    tablesHeap.CustomAttributeType[3] = table;
                    break;
                case MetaDataTableType.Method:
                    tablesHeap.HasCustomAttribute[0] = table;
                    tablesHeap.HasDeclSecurity[1] = table;
                    tablesHeap.MemberRefParent[3] = table;
                    tablesHeap.MethodDefOrRef[0] = table;
                    tablesHeap.MemberForwarded[1] = table;
                    tablesHeap.CustomAttributeType[2] = table;
                    tablesHeap.TypeOrMethod[1] = table;
                    break;
                case MetaDataTableType.MethodSpec:
                    tablesHeap.HasCustomAttribute[21] = table;
                    break;
                case MetaDataTableType.Module:
                    tablesHeap.HasCustomAttribute[7] = table;
                    tablesHeap.ResolutionScope[0] = table;
                    break;
                case MetaDataTableType.ModuleRef:
                    tablesHeap.HasCustomAttribute[12] = table;
                    tablesHeap.MemberRefParent[2] = table;
                    tablesHeap.ResolutionScope[1] = table;
                    break;
                case MetaDataTableType.Param:
                    tablesHeap.HasConstant[1] = table;
                    tablesHeap.HasCustomAttribute[4] = table;
                    tablesHeap.HasFieldMarshall[1] = table;
                    break;
                case MetaDataTableType.Property:
                    tablesHeap.HasConstant[2] = table;
                    tablesHeap.HasCustomAttribute[9] = table;
                    tablesHeap.HasSemantics[1] = table;
                    break;
                case MetaDataTableType.StandAloneSig:
                    tablesHeap.HasCustomAttribute[11] = table;
                    break;
                case MetaDataTableType.TypeDef:
                    tablesHeap.TypeDefOrRef[0] = table;
                    tablesHeap.HasCustomAttribute[3] = table;
                    tablesHeap.HasDeclSecurity[0] = table;
                    tablesHeap.MemberRefParent[0] = table;
                    tablesHeap.TypeOrMethod[0] = table;
                    break;
                case MetaDataTableType.TypeRef:
                    tablesHeap.TypeDefOrRef[1] = table;
                    tablesHeap.HasCustomAttribute[2] = table;
                    tablesHeap.MemberRefParent[1] = table;
                    tablesHeap.ResolutionScope[3] = table;
                    break;
                case MetaDataTableType.TypeSpec:
                    tablesHeap.TypeDefOrRef[2] = table;
                    tablesHeap.HasCustomAttribute[13] = table;
                    tablesHeap.MemberRefParent[4] = table;
                    break;
            }
            return table;
        }

        internal void ReadTableHeaders(BinaryReader reader)
        {
            SetupCodedIndexes();

            for (int i = 0; i < 45; i++)
            {
                if (tablesHeap.HasTable((MetaDataTableType)i))
                {
                    long headerOffset = reader.BaseStream.Position + tablesHeap.StreamOffset;
                    MetaDataTable table = CreateTable((MetaDataTableType)i, reader.ReadInt32(), headerOffset);
                
                    tablesHeap.tables[i] = table;
                }
            }

            uint tableOffset = (uint)(tablesHeap.StreamOffset + Marshal.SizeOf(typeof(Structures.METADATA_TABLE_HEADER)) + (tablesHeap.tablecount) * 4);
            foreach (MetaDataTable table in tablesHeap.tables)
            {
                if (table != null)
                {
                    table.TableOffset = tableOffset;
                    tableOffset += (uint)table.PhysicalSize;
                }
            }

        }

        internal void SetupCodedIndexes()
        {
            tablesHeap.TypeDefOrRef = new MetaDataTableGroup(3, 2);
            tablesHeap.HasConstant = new MetaDataTableGroup(3, 2);
            tablesHeap.HasCustomAttribute = new MetaDataTableGroup(22, 5);
            tablesHeap.HasFieldMarshall = new MetaDataTableGroup(2, 1);
            tablesHeap.HasDeclSecurity = new MetaDataTableGroup(3, 2);
            tablesHeap.MemberRefParent = new MetaDataTableGroup(5, 3);
            tablesHeap.HasSemantics = new MetaDataTableGroup(2, 1);
            tablesHeap.MethodDefOrRef = new MetaDataTableGroup(2, 1);
            tablesHeap.MemberForwarded = new MetaDataTableGroup(2, 1);
            tablesHeap.Implementation = new MetaDataTableGroup(3, 2);
            tablesHeap.CustomAttributeType = new MetaDataTableGroup(5, 3);
            tablesHeap.ResolutionScope = new MetaDataTableGroup(4, 2);
            tablesHeap.TypeOrMethod = new MetaDataTableGroup(2, 1);
        }

        internal MetaDataMember[] ReadMembers(MetaDataTable table)
        {
            MetaDataMember[] members = null;
            if (table != null)
            {
                using (MemoryStream stream = new MemoryStream(tablesHeap.Contents))
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        stream.Position = table.TableOffset - tablesHeap.StreamOffset;
                        members = new MetaDataMember[table.AmountOfRows];
                        for (uint i = 0; i < table.AmountOfRows; i++)
                        {
                            switch (table.Type)
                            {
                                case MetaDataTableType.Module:
                                    members[i] = CreateMember<ModuleDefinition>(reader, GetModuleSignature(), table.type);
                                    break;
                                case MetaDataTableType.TypeRef:
                                    members[i] = CreateMember<TypeReference>(reader, GetTypeRefSignature(), table.type);
                                    break;
                                case MetaDataTableType.TypeDef:
                                    members[i] = CreateMember<TypeDefinition>(reader, GetTypeDefSignature(), table.type);
                                    break;
                                case MetaDataTableType.Field:
                                    members[i] = CreateMember<FieldDefinition>(reader, GetFieldDefSignature(), table.type);
                                    break;
                                case MetaDataTableType.Method:
                                    members[i] = CreateMember<MethodDefinition>(reader, GetMethodDefSignature(), table.type);
                                    break;
                                case MetaDataTableType.ParamPtr:
                                    members[i] = CreateMember<ParamPtr>(reader, GetParamPtrSignature(), table.type);
                                    break;
                                case MetaDataTableType.Param:
                                    members[i] = CreateMember<ParameterDefinition>(reader, GetParamDefSignature(), table.type);
                                    break;
                                case MetaDataTableType.InterfaceImpl:
                                    members[i] = CreateMember<InterfaceImplementation>(reader, GetInterfaceImplSignature(), table.type);
                                    break;
                                case MetaDataTableType.MemberRef:
                                    MetaDataRow row = ReadRow(reader, GetMemberRefSignature());
                                    tablesHeap.netheader.BlobHeap.mainStream.Seek(Convert.ToUInt32(row._parts[2]), SeekOrigin.Begin);
                                    tablesHeap.netheader.BlobHeap.binReader.ReadByte();
                                    byte sigtype = tablesHeap.netheader.BlobHeap.binReader.ReadByte();

                                    if (sigtype == 0x6)
                                        members[i] = new FieldReference(row) { table = MetaDataTableType.MemberRef, netheader = tablesHeap.netheader, metadatarow = row };
                                    else
                                        members[i] = new NET.Specialized.MethodReference(row) { table = MetaDataTableType.MemberRef, netheader = tablesHeap.netheader, metadatarow = row };

                                    break;
                                case MetaDataTableType.Constant:
                                    members[i] = CreateMember<Constant>(reader, GetConstantSignature(), table.type);
                                    break;
                                case MetaDataTableType.CustomAttribute:
                                    members[i] = CreateMember<CustomAttribute>(reader, GetCustomAttributeSignature(), table.type);
                                    break;
                                case MetaDataTableType.FieldMarshal:
                                    members[i] = CreateMember<FieldMarshal>(reader, GetFieldMarshalSignature(), table.type);
                                    break;
                                case MetaDataTableType.DeclSecurity:
                                    members[i] = CreateMember<SecurityDeclaration>(reader, GetSecurityDeclSignature(), table.type);
                                    break;
                                case MetaDataTableType.ClassLayout:
                                    members[i] = CreateMember<ClassLayout>(reader, GetClassLayoutSignature(), table.type);
                                    break;
                                case MetaDataTableType.FieldLayout:
                                    members[i] = CreateMember<FieldLayout>(reader, GetFieldLayoutSignature(), table.type);
                                    break;
                                case MetaDataTableType.StandAloneSig:
                                    members[i] = CreateMember<StandAloneSignature>(reader, GetStandAloneSigSignature(), table.type);
                                    break;
                                case MetaDataTableType.EventMap:
                                    members[i] = CreateMember<EventMap>(reader, GetEventMapSignature(), table.type);
                                    break;
                                case MetaDataTableType.Event:
                                    members[i] = CreateMember<EventDefinition>(reader, GetEventDefSignature(), table.type);
                                    break;
                                case MetaDataTableType.PropertyMap:
                                    members[i] = CreateMember<PropertyMap>(reader, GetPropertyMapSignature(), table.type);
                                    break;
                                case MetaDataTableType.PropertyPtr:
                                    members[i] = CreateMember<PropertyPtr>(reader, GetPropertyPtrSignature(), table.type);
                                    break;
                                case MetaDataTableType.Property:
                                    members[i] = CreateMember<PropertyDefinition>(reader, GetPropertyDefSignature(), table.type);
                                    break;
                                case MetaDataTableType.MethodSemantics:
                                    members[i] = CreateMember<MethodSemantics>(reader, GetMethodSemanticsSignature(), table.type);
                                    break;
                                case MetaDataTableType.MethodImpl:
                                    members[i] = CreateMember<MethodImplementation>(reader, GetMethodImplSignature(), table.type);
                                    break;
                                case MetaDataTableType.ModuleRef:
                                    members[i] = CreateMember<ModuleReference>(reader, GetModuleRefSignature(), table.type);
                                    break;
                                case MetaDataTableType.TypeSpec:
                                    members[i] = CreateMember<TypeSpecification>(reader, GetTypeSpecSignature(), table.type);
                                    break;
                                case MetaDataTableType.MethodSpec:
                                    members[i] = CreateMember<MethodSpecification>(reader, GetMethodSpecSignature(), table.type);
                                    break;
                                case MetaDataTableType.ImplMap:
                                    members[i] = CreateMember<PInvokeImplementation>(reader, GetPInvokeImplSignature(), table.type);
                                    break;
                                case MetaDataTableType.FieldRVA:
                                    members[i] = CreateMember<FieldRVA>(reader, GetFieldRVASignature(), table.type);
                                    break;
                                case MetaDataTableType.Assembly:
                                    members[i] = CreateMember<AssemblyDefinition>(reader, GetAssemblyDefSignature(), table.type);
                                    break;
                                case MetaDataTableType.AssemblyProcessor:
                                    members[i] = CreateMember<AssemblyProcessor>(reader, GetAssemblyProcSignature(), table.type);
                                    break;
                                case MetaDataTableType.AssemblyOS:
                                    members[i] = CreateMember<AssemblyOS>(reader, GetAssemblyOSSignature(), table.type);
                                    break;
                                case MetaDataTableType.AssemblyRef:
                                    members[i] = CreateMember<AssemblyReference>(reader, GetAssemblyRefSignature(), table.type);
                                    break;
                                case MetaDataTableType.AssemblyRefProcessor:
                                    members[i] = CreateMember<AssemblyRefProcessor>(reader, GetAssemblyRefProcSignature(), table.type);
                                    break;
                                case MetaDataTableType.AssemblyRefOS:
                                    members[i] = CreateMember<AssemblyRefOS>(reader, GetAssemblyRefOSSignature(), table.type);
                                    break;
                                case MetaDataTableType.File:
                                    members[i] = CreateMember<FileReference>(reader, GetFileReferenceSignature(), table.type);
                                    break;
                                case MetaDataTableType.ExportedType:
                                    members[i] = CreateMember<ExportedType>(reader, GetExportedTypeSignature(), table.type);
                                    break;
                                case MetaDataTableType.ManifestResource:
                                    members[i] = CreateMember<ManifestResource>(reader, GetManifestResSignature(), table.type);
                                    break;
                                case MetaDataTableType.NestedClass:
                                    members[i] = CreateMember<NestedClass>(reader, GetNestedClassSignature(), table.type);
                                    break;
                                case MetaDataTableType.EncLog:
                                    members[i] = CreateMember<EnCLog>(reader, GetEnCLogSignature(), table.type);
                                    break;
                                case MetaDataTableType.EncMap:
                                    members[i] = CreateMember<EnCMap>(reader, GetEnCMapSignature(), table.type);
                                    break;
                                case MetaDataTableType.GenericParam:
                                    members[i] = CreateMember<GenericParameter>(reader, GetGenericParamSignature(), table.type);
                                    break;
                                case MetaDataTableType.GenericParamConstraint:
                                    members[i] = CreateMember<GenericParamConstraint>(reader, GetGenericParamConstraintSignature(), table.type);
                                    break;

                            }
                            if (members.Length > 0)
                            {
                                members[i].metadatatoken = ConstructMetaDataToken(table.type, i);
                            }
                        }
                    }
                }
            }
            return members;
        }
        
        private byte GetIndexSize(MetaDataTableType type)
        {
            MetaDataTable table = tablesHeap.Tables[(int)type];
            if (table != null && table.IsLarge(0))
                return sizeof(uint);
            return sizeof(ushort);
        }

        private byte GetIndexSize(MetaDataTableGroup tablegroup)
        {
            foreach (MetaDataTable table in tablegroup.tables)
                if (table != null && table.IsLarge(tablegroup.bits))
                    return sizeof(uint);
            return sizeof(ushort);
        }

        internal MetaDataRow ReadRow(BinaryReader reader, byte[] signature)
        {
            MetaDataRow row = new MetaDataRow();
            row._parts = new ValueType[signature.Length];
            row.NETHeader = tablesHeap.netheader;

            row._offset = (uint)(reader.BaseStream.Position + tablesHeap.StreamOffset);
            
            for (int i = 0; i< signature.Length;i++)
            {
                if (signature[i] == sizeof(uint))
                    row._parts[i] = reader.ReadUInt32();
                else if (signature[i] == sizeof(ushort))
                    row._parts[i] = reader.ReadUInt16();
                else if (signature[i] == sizeof(byte))
                    row._parts[i] = reader.ReadByte();
            }
            
            return row;
        }

        internal T CreateMember<T>(BinaryReader reader, byte[] rowSignature, MetaDataTableType table) where T : MetaDataMember
        {
            T member = (T)Activator.CreateInstance(typeof(T), ReadRow(reader, rowSignature));
            member.table = table;
            member.netheader = tablesHeap.netheader;
            return member;
        }

        internal byte[] GetModuleSignature()
        {
            byte[] parts = new byte[] {
                sizeof(ushort),
                tablesHeap.netheader.StringsHeap.indexsize,
                tablesHeap.netheader.GuidHeap.indexsize,
                tablesHeap.netheader.GuidHeap.indexsize,
                tablesHeap.netheader.GuidHeap.indexsize
            };

            return parts;
        }

        internal byte[] GetTypeRefSignature()
        {
            byte[] parts = new byte[] { 
                GetIndexSize(tablesHeap.ResolutionScope),
                tablesHeap.netheader.StringsHeap.indexsize,
                tablesHeap.netheader.StringsHeap.indexsize,
            };

            return parts;
        }
        internal byte[] GetTypeDefSignature()
        {
            byte[] parts = new byte[] { 
                sizeof(uint),
                tablesHeap.netheader.StringsHeap.indexsize,
                tablesHeap.netheader.StringsHeap.indexsize,
                GetIndexSize(tablesHeap.TypeDefOrRef),
                GetIndexSize(MetaDataTableType.Field),
                GetIndexSize(MetaDataTableType.Method),
            };

            return parts;
        }
        internal byte[] GetFieldDefSignature()
        {
            byte[] parts = new byte[] { 
                sizeof(ushort),
                tablesHeap.netheader.StringsHeap.indexsize,
                tablesHeap.netheader.BlobHeap.indexsize,
     
            };

            return parts;
        }
        internal byte[] GetMethodDefSignature()
        {
            byte[] parts = new byte[] { 
                sizeof(uint),
                sizeof(ushort),
                sizeof(ushort),
                tablesHeap.netheader.StringsHeap.indexsize,
                tablesHeap.netheader.BlobHeap.indexsize,
                GetIndexSize(MetaDataTableType.Param),
     
            };

            return parts;
        }
        internal byte[] GetParamPtrSignature()
        {
            byte[] parts = new byte[]
            {
                 tablesHeap.netheader.BlobHeap.indexsize
            };
            return parts;
        }
        internal byte[] GetParamDefSignature()
        {
            byte[] parts = new byte[] { 
                sizeof(ushort),
                sizeof(ushort),
                tablesHeap.netheader.StringsHeap.indexsize,
     
            };

            return parts;
        }
        internal byte[] GetInterfaceImplSignature()
        {
            byte[] parts = new byte[] { 
                GetIndexSize(MetaDataTableType.TypeDef),
                GetIndexSize(tablesHeap.TypeDefOrRef),
     
            };

            return parts;

        }
        internal byte[] GetMemberRefSignature()
        {
            byte[] parts = new byte[] { 
                GetIndexSize(tablesHeap.MemberRefParent),
                tablesHeap.netheader.StringsHeap.indexsize,
                tablesHeap.netheader.BlobHeap.indexsize
     
            };
            return parts;
           
        }
        internal byte[] GetConstantSignature()
        {
            byte[] parts = new byte[] { 
                sizeof(byte),
                sizeof(byte),
                GetIndexSize(tablesHeap.HasConstant),
                tablesHeap.netheader.BlobHeap.indexsize,
     
            };

            return parts;
        }
        internal byte[] GetCustomAttributeSignature()
        {
            byte[] parts = new byte[] { 
                GetIndexSize(tablesHeap.HasCustomAttribute),
                GetIndexSize(tablesHeap.CustomAttributeType),
                tablesHeap.netheader.BlobHeap.indexsize,
     
            };

            return parts;
        
        }
        internal byte[] GetFieldMarshalSignature()
        {
            byte[] parts = new byte[] { 
                GetIndexSize(tablesHeap.HasFieldMarshall),
                tablesHeap.netheader.BlobHeap.indexsize,
     
            };

            return parts;

        }
        internal byte[] GetSecurityDeclSignature()
        {
            byte[] parts = new byte[] { 
                sizeof(ushort),
                GetIndexSize(tablesHeap.HasDeclSecurity),
                tablesHeap.netheader.BlobHeap.indexsize,
     
            };

            return parts;
        }
        internal byte[] GetClassLayoutSignature()
        {
            byte[] parts = new byte[] { 
                sizeof(ushort),
                sizeof(uint),
                GetIndexSize(MetaDataTableType.TypeDef),
     
            };

            return parts;
        }
        internal byte[] GetFieldLayoutSignature()
        {
            byte[] parts = new byte[] { 
                sizeof(uint),
                GetIndexSize(MetaDataTableType.Field),
     
            };

            return parts;
        }
        internal byte[] GetStandAloneSigSignature()
        {
            byte[] parts = new byte[] { 
                tablesHeap.netheader.BlobHeap.indexsize,
            };

            return parts;

        }
        internal byte[] GetEventMapSignature()
        {
            byte[] parts = new byte[] { 
                GetIndexSize(MetaDataTableType.TypeDef),
                GetIndexSize(MetaDataTableType.Event),
            };

            return parts;
        }
        internal byte[] GetEventDefSignature()
        {
            byte[] parts = new byte[] { 
                sizeof(ushort),
                tablesHeap.netheader.StringsHeap.indexsize,
                GetIndexSize(tablesHeap.TypeDefOrRef),
            };

            return parts;
        }
        internal byte[] GetPropertyMapSignature()
        {
            byte[] parts = new byte[] { 
                GetIndexSize(MetaDataTableType.TypeDef),
                GetIndexSize(MetaDataTableType.Property),
            };

            return parts;

        }
        internal byte[] GetPropertyPtrSignature()
        {
            byte[] parts = new byte[]
            {
                GetIndexSize(MetaDataTableType.Property),
            };

            return parts;
        }
        internal byte[] GetPropertyDefSignature()
        {
            byte[] parts = new byte[] { 
                sizeof(ushort),
                tablesHeap.netheader.StringsHeap.indexsize,
                tablesHeap.netheader.BlobHeap.indexsize,
            };

            return parts;

        }
        internal byte[] GetMethodSemanticsSignature()
        {
            byte[] parts = new byte[] { 
                sizeof(ushort),
                GetIndexSize(MetaDataTableType.Method),
                GetIndexSize(tablesHeap.HasSemantics),
            };

            return parts;
        }
        internal byte[] GetMethodImplSignature()
        {
            byte[] parts = new byte[] { 
                GetIndexSize(MetaDataTableType.TypeDef),
                GetIndexSize(tablesHeap.MethodDefOrRef),
                GetIndexSize(tablesHeap.MethodDefOrRef),
            };

            return parts;
        

        }
        internal byte[] GetModuleRefSignature()
        {
            byte[] parts = new byte[] { 
                tablesHeap.netheader.StringsHeap.indexsize,
            };

            return parts;
        }
        internal byte[] GetTypeSpecSignature()
        {
            byte[] parts = new byte[] { 
                tablesHeap.netheader.BlobHeap.indexsize,
            };

            return parts;
        }
        internal byte[] GetMethodSpecSignature()
        {
            byte[] parts = new byte[] { 
                GetIndexSize(tablesHeap.MethodDefOrRef),
                tablesHeap.netheader.BlobHeap.indexsize,
            };

            return parts;
       }
        internal byte[] GetPInvokeImplSignature()
        {
            byte[] parts = new byte[] { 
                sizeof(ushort),
                GetIndexSize(tablesHeap.MemberForwarded),
                tablesHeap.netheader.StringsHeap.indexsize,
                GetIndexSize(MetaDataTableType.ModuleRef),
            };

            return parts;
        }
        internal byte[] GetFieldRVASignature()
        {
            byte[] parts = new byte[] { 
                sizeof(uint),
                GetIndexSize(MetaDataTableType.Field),
            };

            return parts;
        }
        internal byte[] GetAssemblyDefSignature()
        {
            byte[] parts = new byte[] { 
                sizeof(uint),
                sizeof(ushort),
                sizeof(ushort),
                sizeof(ushort),
                sizeof(ushort),
                sizeof(uint),
                tablesHeap.netheader.BlobHeap.indexsize ,
                tablesHeap.netheader.StringsHeap.indexsize ,
                tablesHeap.netheader.StringsHeap.indexsize ,
            };

            return parts;
       
        }
        internal byte[] GetAssemblyProcSignature()
        {
            return new byte[] { sizeof(uint) };
        }
        internal byte[] GetAssemblyOSSignature()
        {
            return new byte[]
            {
                sizeof(uint),
                sizeof(uint),
                sizeof(uint),
            };
        }
        internal byte[] GetAssemblyRefSignature()
        {
            byte[] parts = new byte[] { 
                sizeof(ushort),
                sizeof(ushort),
                sizeof(ushort),
                sizeof(ushort),
                sizeof(uint),
                tablesHeap.netheader.BlobHeap.indexsize ,
                tablesHeap.netheader.StringsHeap.indexsize ,
                tablesHeap.netheader.StringsHeap.indexsize ,
                tablesHeap.netheader.BlobHeap.indexsize ,
            };

            return parts;


        }
        internal byte[] GetAssemblyRefProcSignature()
        {
            return new byte[] 
            { 
                sizeof(uint), 
                GetIndexSize(MetaDataTableType.AssemblyRef),
            };
        }
        internal byte[] GetAssemblyRefOSSignature()
        {
            return new byte[]
            {
                sizeof(uint),
                sizeof(uint),
                sizeof(uint),
                GetIndexSize(MetaDataTableType.AssemblyRef),
            };
        }
        internal byte[] GetFileReferenceSignature()
        {
            byte[] parts = new byte[] { 
                sizeof(uint),
                tablesHeap.netheader.StringsHeap.indexsize,
                tablesHeap.netheader.BlobHeap.indexsize,
            };

            return parts;
       
        }
        internal byte[] GetExportedTypeSignature()
        {
            byte[] parts = new byte[] { 
                sizeof(uint),
                sizeof(uint),
                tablesHeap.netheader.StringsHeap.indexsize,
                tablesHeap.netheader.StringsHeap.indexsize,
                GetIndexSize(tablesHeap.Implementation),
            };

            return parts;
       
        }
        internal byte[] GetManifestResSignature()
        {
            byte[] parts = new byte[] { 
                sizeof(uint),
                sizeof(uint),
                tablesHeap.netheader.StringsHeap.indexsize,
                GetIndexSize(tablesHeap.Implementation),
            };

            return parts;
       

        }
        internal byte[] GetNestedClassSignature()
        {
            byte[] parts = new byte[] { 
                GetIndexSize(MetaDataTableType.TypeDef),
                GetIndexSize(MetaDataTableType.TypeDef),
            };
            return parts;
            
        }
        internal byte[] GetEnCLogSignature()
        {
            byte[] parts = new byte[] { sizeof(uint),sizeof(uint) };
            return parts;
        }
        internal byte[] GetEnCMapSignature()
        {
            byte[] parts = new byte[] { sizeof(uint) };
            return parts;

        }
        internal byte[] GetGenericParamSignature()
        {
            byte[] parts = new byte[] { 
                sizeof(ushort),
                sizeof(ushort),
                GetIndexSize(tablesHeap.TypeOrMethod),
                tablesHeap.netheader.StringsHeap.indexsize};
            return parts;

        }
        internal byte[] GetGenericParamConstraintSignature()
        {
            byte[] parts = new byte[] { 
                GetIndexSize(MetaDataTableType.GenericParam),
                GetIndexSize(tablesHeap.TypeDefOrRef),};
            return parts;

        }
        internal uint ConstructMetaDataToken(MetaDataTableType type, uint index)
        {
            uint token = ((uint)type);
            token = token << 0x18;
            token += (index + 1);
            return token;
        }


        public void Dispose()
        {
        }
    }
}
