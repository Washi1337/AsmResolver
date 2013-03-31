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
        BinaryReader reader;
        internal TablesHeap tablesHeap;



        internal NETTableReader(TablesHeap tablesheap)
        {
            reader = new BinaryReader(new MemoryStream(tablesheap.Contents));
            tablesheap.header = ASMGlobals.ReadStructureFromReader<Structures.METADATA_TABLE_HEADER>(reader);
            this.tablesHeap = tablesheap;


            for (int i = 0; i < 45; i++)
                if (tablesHeap.HasTable((MetaDataTableType)i))
                {
                    tablesHeap.tablecount++;
                }
            ReadTableHeaders();
            tablesHeap.tablereader = this;
           // ReadTables();

        }
        internal MetaDataTable CreateTable(MetaDataTableType type, int rowAmount, long rowAmountOffset)
        {
            //customattribute[permission]?

            MetaDataTable table = new MetaDataTable(tablesHeap);
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

        internal void ReadTableHeaders()
        {
            SetupCodedIndexes();

            for (int i = 0; i < 45; i++)
            {
                    
                if (tablesHeap.HasTable((MetaDataTableType)i))
                {
                    long offset = reader.BaseStream.Position + tablesHeap.StreamOffset;
                    tablesHeap.tables[i] = (CreateTable((MetaDataTableType)i, reader.ReadInt32(), offset));
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

        internal void ReadTables()
        {
            if ((tablesHeap.HeapOffsetSizes & 1) == 1)
                tablesHeap.netheader.StringsHeap.indexsize = 4;
            if ((tablesHeap.HeapOffsetSizes & 2) == 2)
                tablesHeap.netheader.GuidHeap.indexsize = 4;
            if ((tablesHeap.HeapOffsetSizes & 4) == 4)
                tablesHeap.netheader.BlobHeap.indexsize = 4;

            foreach (MetaDataTable table in tablesHeap.tables)
            {
                if (table != null)
                {
                    table.TableOffset = (uint)(tablesHeap.StreamOffset + reader.BaseStream.Position);
                    table.members = new MetaDataMember[table.AmountOfRows];
                    for (uint i = 0; i < table.AmountOfRows; i++)
                    {
                        switch (table.Type)
                        {
                            case MetaDataTableType.Module:
                                table.members[i] = ReadModule();
                                break;
                            case MetaDataTableType.TypeRef:
                                table.members[i] = ReadTypeRef();
                                break;
                            case MetaDataTableType.TypeDef:
                                table.members[i] = ReadTypeDef();
                                break;
                            case MetaDataTableType.Field:
                                table.members[i] = ReadFieldDef();
                                break;
                            case MetaDataTableType.Method:
                                table.members[i] = ReadMethodDef();
                                break;
                            case MetaDataTableType.ParamPtr:
                                table.members[i] = ReadParamPtr();
                                break;
                            case MetaDataTableType.Param:
                                table.members[i] = ReadParamDef();
                                break;
                            case MetaDataTableType.InterfaceImpl:
                                table.members[i] = ReadInterfaceImpl();
                                break;
                            case MetaDataTableType.MemberRef:
                                table.members[i] = ReadMemberRef();
                                break;
                            case MetaDataTableType.Constant:
                                table.members[i] = ReadConstant();
                                break;
                            case MetaDataTableType.CustomAttribute:
                                table.members[i] = ReadCustomAttribute();
                                break;
                            case MetaDataTableType.FieldMarshal:
                                table.members[i] = ReadFieldMarshal();
                                break;
                            case MetaDataTableType.DeclSecurity:
                                table.members[i] = ReadSecurityDecl();
                                break;
                            case MetaDataTableType.ClassLayout:
                                table.members[i] = ReadClassLayout();
                                break;
                            case MetaDataTableType.FieldLayout:
                                table.members[i] = ReadFieldLayout();
                                break;
                            case MetaDataTableType.StandAloneSig:
                                table.members[i] = ReadStandAloneSig();
                                break;
                            case MetaDataTableType.EventMap:
                                table.members[i] = ReadEventMap();
                                break;
                            case MetaDataTableType.Event:
                                table.members[i] = ReadEventDef();
                                break;
                            case MetaDataTableType.PropertyMap:
                                table.members[i] = ReadPropertyMap();
                                break;
                            case MetaDataTableType.Property:
                                table.members[i] = ReadPropertyDef();
                                break;
                            case MetaDataTableType.MethodSemantics:
                                table.members[i] = ReadMethodSemantics();
                                break;
                            case MetaDataTableType.MethodImpl:
                                table.members[i] = ReadMethodImpl();
                                break;
                            case MetaDataTableType.ModuleRef:
                                table.members[i] = ReadModuleRef();
                                break;
                            case MetaDataTableType.TypeSpec:
                                table.members[i] = ReadTypeSpec();
                                break;
                            case MetaDataTableType.MethodSpec:
                                table.members[i] = ReadMethodSpec();
                                break;
                            case MetaDataTableType.ImplMap:
                                table.members[i] = ReadPInvokeImpl();
                                break;
                            case MetaDataTableType.FieldRVA:
                                table.members[i] = ReadFieldRVA();
                                break;
                            case MetaDataTableType.Assembly:
                                table.members[i] = ReadAssemblyDef();
                                break;
                            case MetaDataTableType.AssemblyRef:
                                table.members[i] = ReadAssemblyRef();
                                break;
                            case MetaDataTableType.File:
                                table.members[i] = ReadFileReference();
                                break;
                            case MetaDataTableType.ExportedType:
                                table.members[i] = ReadExportedType();
                                break;
                            case MetaDataTableType.ManifestResource:
                                table.members[i] = ReadManifestRes();
                                break;
                            case MetaDataTableType.NestedClass:
                                table.members[i] = ReadNestedClass();
                                break;
                            case MetaDataTableType.EncLog:
                                table.members[i] = ReadEnCLog();
                                break;
                            case MetaDataTableType.EncMap:
                                table.members[i] = ReadEnCMap();
                                break;
                            case MetaDataTableType.GenericParam:
                                table.members[i] = ReadGenericParam();
                                break;
                            case MetaDataTableType.GenericParamConstraint:
                                table.members[i] = ReadGenericParamConstraint();
                                break;
                                
                        }
                        if (table.members.Length > 0)
                        {
                            table.members[i].metadatatoken = ConstructMetaDataToken(table.type, i);
                        }
                    }
                }
            }
            reader.BaseStream.Close();
            reader.BaseStream.Dispose();
        }




        private byte GetDefaultIndex(MetaDataTableType type)
        {
            for (int i = 0;i< tablesHeap.tables.Length;i++)
                if (tablesHeap.tables[i] != null)
                    if (tablesHeap.tables[i].type == type && tablesHeap.tables[i].IsLarge(0))
                        return sizeof(uint);
            return sizeof(ushort);
        }
        private byte GetDefaultIndex(MetaDataTableGroup tablegroup)
        {
            foreach (MetaDataTable table in tablegroup.tables)
                if (table != null && table.IsLarge(tablegroup.bits))
                    return sizeof(uint);
            return sizeof(ushort);
        }

        internal MetaDataRow ReadRow(byte[] parts)
        {
            MetaDataRow row = new MetaDataRow();
            row.parts = new object[parts.Length];

            row.offset = (uint)(reader.BaseStream.Position + tablesHeap.StreamOffset);
            
            for (int i = 0; i< parts.Length;i++)
            {
                if (parts[i] == sizeof(uint))
                    row.parts[i] = reader.ReadUInt32();
                else if (parts[i] == sizeof(ushort))
                    row.parts[i] = reader.ReadUInt16();
                else if (parts[i] == sizeof(byte))
                    row.parts[i] = reader.ReadByte();
            }
            
            return row;
        }

        internal ModuleDefinition ReadModule()
        {
            byte[] parts = new byte[] {
                sizeof(ushort),
                tablesHeap.netheader.StringsHeap.indexsize,
                tablesHeap.netheader.GuidHeap.indexsize,
                tablesHeap.netheader.GuidHeap.indexsize,
                tablesHeap.netheader.GuidHeap.indexsize
            };

            return new ModuleDefinition() { table = MetaDataTableType.Module , netheader = tablesHeap.netheader,  metadatarow = ReadRow(parts) };
        }
        internal TypeReference ReadTypeRef()
        {
            byte[] parts = new byte[] { 
                GetDefaultIndex(tablesHeap.ResolutionScope),
                tablesHeap.netheader.StringsHeap.indexsize,
                tablesHeap.netheader.StringsHeap.indexsize,
            };

            return new TypeReference() { table = MetaDataTableType.TypeRef , netheader = tablesHeap.netheader, metadatarow = ReadRow(parts) };
        }
        internal TypeDefinition ReadTypeDef()
        {
            byte[] parts = new byte[] { 
                sizeof(uint),
                tablesHeap.netheader.StringsHeap.indexsize,
                tablesHeap.netheader.StringsHeap.indexsize,
                GetDefaultIndex(tablesHeap.TypeDefOrRef),
                GetDefaultIndex(MetaDataTableType.Field),
                GetDefaultIndex(MetaDataTableType.Method),
            };

            return new TypeDefinition() { table = MetaDataTableType.TypeDef ,  netheader = tablesHeap.netheader, metadatarow = ReadRow(parts) };
        }
        internal FieldDefinition ReadFieldDef()
        {
            byte[] parts = new byte[] { 
                sizeof(ushort),
                tablesHeap.netheader.StringsHeap.indexsize,
                tablesHeap.netheader.BlobHeap.indexsize,
     
            };

            return new FieldDefinition() { table = MetaDataTableType.Field ,  netheader = tablesHeap.netheader, metadatarow = ReadRow(parts) };
        }
        internal MethodDefinition ReadMethodDef()
        {
            byte[] parts = new byte[] { 
                sizeof(uint),
                sizeof(ushort),
                sizeof(ushort),
                tablesHeap.netheader.StringsHeap.indexsize,
                tablesHeap.netheader.BlobHeap.indexsize,
                GetDefaultIndex(MetaDataTableType.Param),
     
            };

            return new MethodDefinition() { table = MetaDataTableType.Method ,  netheader = tablesHeap.netheader, metadatarow = ReadRow(parts) };
        }
        internal ParamPtr ReadParamPtr()
        {
            byte[] parts = new byte[]
            {
                 tablesHeap.netheader.BlobHeap.indexsize
            };
            return new ParamPtr() { table = MetaDataTableType.ParamPtr,  netheader = tablesHeap.netheader, metadatarow = ReadRow(parts) };
        }
        internal ParameterDefinition ReadParamDef()
        {
            byte[] parts = new byte[] { 
                sizeof(ushort),
                sizeof(ushort),
                tablesHeap.netheader.StringsHeap.indexsize,
     
            };

            return new ParameterDefinition() { table = MetaDataTableType.Param,  netheader = tablesHeap.netheader, metadatarow = ReadRow(parts) };
        }
        internal InterfaceImplementation ReadInterfaceImpl()
        {
            byte[] parts = new byte[] { 
                GetDefaultIndex(MetaDataTableType.TypeDef),
                GetDefaultIndex(tablesHeap.TypeDefOrRef),
     
            };

            return new InterfaceImplementation() { table = MetaDataTableType.InterfaceImpl,  netheader = tablesHeap.netheader, metadatarow = ReadRow(parts) };

        }
        internal MemberReference ReadMemberRef()
        {
            byte[] parts = new byte[] { 
                GetDefaultIndex(tablesHeap.MemberRefParent),
                tablesHeap.netheader.StringsHeap.indexsize,
                tablesHeap.netheader.BlobHeap.indexsize
     
            };
            MetaDataRow row = ReadRow(parts);
            tablesHeap.netheader.BlobHeap.mainStream.Seek(Convert.ToUInt32(row.parts[2]), SeekOrigin.Begin);
            tablesHeap.netheader.BlobHeap.binReader.ReadByte();
            byte sigtype = tablesHeap.netheader.BlobHeap.binReader.ReadByte();
            //IMemberSignature sig = tableheap.netheader.blobheap.ReadMemberRefSignature(Convert.ToUInt32(row.parts[2]));
            if (sigtype == 0x6)
                return new FieldReference() { table = MetaDataTableType.MemberRef,  netheader = tablesHeap.netheader, metadatarow = row };
            else
                return new NET.Specialized.MethodReference() { table = MetaDataTableType.MemberRef, netheader = tablesHeap.netheader, metadatarow = row };
        }
        internal Constant ReadConstant()
        {
            byte[] parts = new byte[] { 
                sizeof(byte),
                sizeof(byte),
                GetDefaultIndex(tablesHeap.HasConstant),
                tablesHeap.netheader.BlobHeap.indexsize,
     
            };

            return new Constant() { table = MetaDataTableType.Constant ,  netheader = tablesHeap.netheader, metadatarow = ReadRow(parts) };
        }
        internal CustomAttribute ReadCustomAttribute()
        {
            byte[] parts = new byte[] { 
                GetDefaultIndex(tablesHeap.HasCustomAttribute),
                GetDefaultIndex(tablesHeap.CustomAttributeType),
                tablesHeap.netheader.BlobHeap.indexsize,
     
            };

            return new CustomAttribute() { table = MetaDataTableType.CustomAttribute ,  netheader = tablesHeap.netheader, metadatarow = ReadRow(parts) };
        
        }
        internal FieldMarshal ReadFieldMarshal()
        {
            byte[] parts = new byte[] { 
                GetDefaultIndex(tablesHeap.HasFieldMarshall),
                tablesHeap.netheader.BlobHeap.indexsize,
     
            };

            return new FieldMarshal() { table = MetaDataTableType.FieldMarshal ,  netheader = tablesHeap.netheader, metadatarow = ReadRow(parts) };

        }
        internal SecurityDeclaration ReadSecurityDecl()
        {
            byte[] parts = new byte[] { 
                sizeof(ushort),
                GetDefaultIndex(tablesHeap.HasDeclSecurity),
                tablesHeap.netheader.BlobHeap.indexsize,
     
            };

            return new SecurityDeclaration() { table = MetaDataTableType.DeclSecurity ,  netheader = tablesHeap.netheader, metadatarow = ReadRow(parts) };            
        }
        internal ClassLayout ReadClassLayout()
        {
            byte[] parts = new byte[] { 
                sizeof(ushort),
                sizeof(uint),
                GetDefaultIndex(MetaDataTableType.TypeDef),
     
            };

            return new ClassLayout() { table = MetaDataTableType.ClassLayout ,  netheader = tablesHeap.netheader, metadatarow = ReadRow(parts) };      
        }
        internal FieldLayout ReadFieldLayout()
        {
            byte[] parts = new byte[] { 
                sizeof(uint),
                GetDefaultIndex(MetaDataTableType.Field),
     
            };

            return new FieldLayout() { table = MetaDataTableType.FieldLayout ,  netheader = tablesHeap.netheader, metadatarow = ReadRow(parts) }; 
        }
        internal StandAloneSignature ReadStandAloneSig()
        {
            byte[] parts = new byte[] { 
                tablesHeap.netheader.BlobHeap.indexsize,
            };

            return new StandAloneSignature() { table = MetaDataTableType.StandAloneSig ,  netheader = tablesHeap.netheader, metadatarow = ReadRow(parts) }; 

        }
        internal EventMap ReadEventMap()
        {
            byte[] parts = new byte[] { 
                GetDefaultIndex(MetaDataTableType.TypeDef),
                GetDefaultIndex(MetaDataTableType.Event),
            };

            return new EventMap() { table = MetaDataTableType.EventMap,  netheader = tablesHeap.netheader, metadatarow = ReadRow(parts) };
        }
        internal EventDefinition ReadEventDef()
        {
            byte[] parts = new byte[] { 
                sizeof(ushort),
                tablesHeap.netheader.StringsHeap.indexsize,
                GetDefaultIndex(tablesHeap.TypeDefOrRef),
            };

            return new EventDefinition() { table = MetaDataTableType.Event,  netheader = tablesHeap.netheader, metadatarow = ReadRow(parts) };
        }
        internal PropertyMap ReadPropertyMap()
        {
            byte[] parts = new byte[] { 
                GetDefaultIndex(MetaDataTableType.TypeDef),
                GetDefaultIndex(MetaDataTableType.Property),
            };

            return new PropertyMap() { table = MetaDataTableType.PropertyMap,  netheader = tablesHeap.netheader, metadatarow = ReadRow(parts) };

        }
        internal PropertyDefinition ReadPropertyDef()
        {
            byte[] parts = new byte[] { 
                sizeof(ushort),
                tablesHeap.netheader.StringsHeap.indexsize,
                tablesHeap.netheader.BlobHeap.indexsize,
            };

            return new PropertyDefinition() { table = MetaDataTableType.Property,  netheader = tablesHeap.netheader, metadatarow = ReadRow(parts) };

        }
        internal MethodSemantics ReadMethodSemantics()
        {
            byte[] parts = new byte[] { 
                sizeof(ushort),
                GetDefaultIndex(MetaDataTableType.Method),
                GetDefaultIndex(tablesHeap.HasSemantics),
            };

            return new MethodSemantics() { table = MetaDataTableType.MethodSemantics,  netheader = tablesHeap.netheader, metadatarow = ReadRow(parts) };
        }
        internal MethodImplementation ReadMethodImpl()
        {
            byte[] parts = new byte[] { 
                GetDefaultIndex(MetaDataTableType.TypeDef),
                GetDefaultIndex(tablesHeap.MethodDefOrRef),
                GetDefaultIndex(tablesHeap.MethodDefOrRef),
            };

            return new MethodImplementation() { table = MetaDataTableType.MethodImpl,  netheader = tablesHeap.netheader, metadatarow = ReadRow(parts) };
        

        }
        internal ModuleReference ReadModuleRef()
        {
            byte[] parts = new byte[] { 
                tablesHeap.netheader.StringsHeap.indexsize,
            };

            return new ModuleReference() { table = MetaDataTableType.ModuleRef,  netheader = tablesHeap.netheader, metadatarow = ReadRow(parts) };
        }
        internal TypeSpecification ReadTypeSpec()
        {
            byte[] parts = new byte[] { 
                tablesHeap.netheader.BlobHeap.indexsize,
            };

            return new TypeSpecification() { table = MetaDataTableType.TypeSpec,  netheader = tablesHeap.netheader, metadatarow = ReadRow(parts) };
        }
        internal MethodSpecification ReadMethodSpec()
        {
            byte[] parts = new byte[] { 
                GetDefaultIndex(tablesHeap.MethodDefOrRef),
                tablesHeap.netheader.BlobHeap.indexsize,
            };

            return new MethodSpecification() { table = MetaDataTableType.MethodSpec,  netheader = tablesHeap.netheader, metadatarow = ReadRow(parts) };
       }
        internal PInvokeImplementation ReadPInvokeImpl()
        {
            byte[] parts = new byte[] { 
                sizeof(ushort),
                GetDefaultIndex(tablesHeap.MemberForwarded),
                tablesHeap.netheader.StringsHeap.indexsize,
                GetDefaultIndex(MetaDataTableType.ModuleRef),
            };

            return new PInvokeImplementation() { table = MetaDataTableType.ImplMap,  netheader = tablesHeap.netheader, metadatarow = ReadRow(parts) };
        }
        internal FieldRVA ReadFieldRVA()
        {
            byte[] parts = new byte[] { 
                sizeof(uint),
                GetDefaultIndex(MetaDataTableType.Field),
            };

            return new FieldRVA() { table = MetaDataTableType.FieldRVA,  netheader = tablesHeap.netheader, metadatarow = ReadRow(parts) };
        }
        internal AssemblyDefinition ReadAssemblyDef()
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

            return new AssemblyDefinition() { table = MetaDataTableType.Assembly,  netheader = tablesHeap.netheader, metadatarow = ReadRow(parts) };
       
        }
        internal AssemblyReference ReadAssemblyRef()
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

            return new AssemblyReference() { table = MetaDataTableType.AssemblyRef,  netheader = tablesHeap.netheader, metadatarow = ReadRow(parts) };
       

        }
        internal FileReference ReadFileReference()
        {
            byte[] parts = new byte[] { 
                sizeof(uint),
                tablesHeap.netheader.StringsHeap.indexsize,
                tablesHeap.netheader.BlobHeap.indexsize,
            };

            return new FileReference() { table = MetaDataTableType.File,  netheader = tablesHeap.netheader, metadatarow = ReadRow(parts) };
       
        }
        internal ExportedType ReadExportedType()
        {
            byte[] parts = new byte[] { 
                sizeof(uint),
                sizeof(uint),
                tablesHeap.netheader.StringsHeap.indexsize,
                tablesHeap.netheader.StringsHeap.indexsize,
                GetDefaultIndex(tablesHeap.Implementation),
            };

            return new ExportedType() { table = MetaDataTableType.ExportedType,  netheader = tablesHeap.netheader, metadatarow = ReadRow(parts) };
       
        }
        internal ManifestResource ReadManifestRes()
        {
            byte[] parts = new byte[] { 
                sizeof(uint),
                sizeof(uint),
                tablesHeap.netheader.StringsHeap.indexsize,
                GetDefaultIndex(tablesHeap.Implementation),
            };

            return new ManifestResource() { table = MetaDataTableType.ManifestResource,  netheader = tablesHeap.netheader, metadatarow = ReadRow(parts) };
       

        }
        internal NestedClass ReadNestedClass()
        {
            byte[] parts = new byte[] { 
                GetDefaultIndex(MetaDataTableType.TypeDef),
                GetDefaultIndex(MetaDataTableType.TypeDef),
            };

            return new NestedClass() { table = MetaDataTableType.NestedClass,  netheader = tablesHeap.netheader, metadatarow = ReadRow(parts) };
       
            
        }
        internal EnCLog ReadEnCLog()
        {
            byte[] parts = new byte[] { sizeof(uint),sizeof(uint) };
            return new EnCLog() { table = MetaDataTableType.EncLog,  netheader = tablesHeap.netheader, metadatarow = ReadRow(parts) };
        }
        internal EnCMap ReadEnCMap()
        {
            byte[] parts = new byte[] { sizeof(uint) };
            return new EnCMap() { table = MetaDataTableType.EncMap,  netheader = tablesHeap.netheader, metadatarow = ReadRow(parts) };

        }
        internal GenericParameter ReadGenericParam()
        {
            byte[] parts = new byte[] { 
                sizeof(ushort),
                sizeof(ushort),
                GetDefaultIndex(tablesHeap.TypeOrMethod),
                tablesHeap.netheader.StringsHeap.indexsize};
            return new GenericParameter() { table = MetaDataTableType.GenericParam,  netheader = tablesHeap.netheader, metadatarow = ReadRow(parts) };

        }
        internal GenericParamConstraint ReadGenericParamConstraint()
        {
            byte[] parts = new byte[] { 
                GetDefaultIndex(MetaDataTableType.GenericParam),
                GetDefaultIndex(tablesHeap.TypeDefOrRef),};
            return new GenericParamConstraint() { table = MetaDataTableType.GenericParamConstraint,  netheader = tablesHeap.netheader, metadatarow = ReadRow(parts) };

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
            reader.BaseStream.Close();
            reader.BaseStream.Dispose();
            reader.Close();
            reader.Dispose();
        }
    }
}
