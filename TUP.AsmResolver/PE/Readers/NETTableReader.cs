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
            MetaDataTable table = tablesHeap.Tables[(int)type];
            if (table != null && table.IsLarge(0))
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

            return new ModuleDefinition(ReadRow(parts)) { table = MetaDataTableType.Module , netheader = tablesHeap.netheader };
        }
        internal TypeReference ReadTypeRef()
        {
            byte[] parts = new byte[] { 
                GetDefaultIndex(tablesHeap.ResolutionScope),
                tablesHeap.netheader.StringsHeap.indexsize,
                tablesHeap.netheader.StringsHeap.indexsize,
            };

            return new TypeReference(ReadRow(parts)) { table = MetaDataTableType.TypeRef , netheader = tablesHeap.netheader };
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

            return new TypeDefinition(ReadRow(parts)) { table = MetaDataTableType.TypeDef, netheader = tablesHeap.netheader };
        }
        internal FieldDefinition ReadFieldDef()
        {
            byte[] parts = new byte[] { 
                sizeof(ushort),
                tablesHeap.netheader.StringsHeap.indexsize,
                tablesHeap.netheader.BlobHeap.indexsize,
     
            };

            return new FieldDefinition(ReadRow(parts)) { table = MetaDataTableType.Field, netheader = tablesHeap.netheader };
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

            return new MethodDefinition(ReadRow(parts)) { table = MetaDataTableType.Method, netheader = tablesHeap.netheader };
        }
        internal ParamPtr ReadParamPtr()
        {
            byte[] parts = new byte[]
            {
                 tablesHeap.netheader.BlobHeap.indexsize
            };
            return new ParamPtr(ReadRow(parts)) { table = MetaDataTableType.ParamPtr, netheader = tablesHeap.netheader };
        }
        internal ParameterDefinition ReadParamDef()
        {
            byte[] parts = new byte[] { 
                sizeof(ushort),
                sizeof(ushort),
                tablesHeap.netheader.StringsHeap.indexsize,
     
            };

            return new ParameterDefinition(ReadRow(parts)) { table = MetaDataTableType.Param, netheader = tablesHeap.netheader };
        }
        internal InterfaceImplementation ReadInterfaceImpl()
        {
            byte[] parts = new byte[] { 
                GetDefaultIndex(MetaDataTableType.TypeDef),
                GetDefaultIndex(tablesHeap.TypeDefOrRef),
     
            };

            return new InterfaceImplementation(ReadRow(parts)) { table = MetaDataTableType.InterfaceImpl, netheader = tablesHeap.netheader };

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
                return new FieldReference(row) { table = MetaDataTableType.MemberRef, netheader = tablesHeap.netheader, metadatarow = row };
            else
                return new NET.Specialized.MethodReference(row) { table = MetaDataTableType.MemberRef, netheader = tablesHeap.netheader, metadatarow = row };
        }
        internal Constant ReadConstant()
        {
            byte[] parts = new byte[] { 
                sizeof(byte),
                sizeof(byte),
                GetDefaultIndex(tablesHeap.HasConstant),
                tablesHeap.netheader.BlobHeap.indexsize,
     
            };

            return new Constant(ReadRow(parts)) { table = MetaDataTableType.Constant, netheader = tablesHeap.netheader };
        }
        internal CustomAttribute ReadCustomAttribute()
        {
            byte[] parts = new byte[] { 
                GetDefaultIndex(tablesHeap.HasCustomAttribute),
                GetDefaultIndex(tablesHeap.CustomAttributeType),
                tablesHeap.netheader.BlobHeap.indexsize,
     
            };

            return new CustomAttribute(ReadRow(parts)) { table = MetaDataTableType.CustomAttribute, netheader = tablesHeap.netheader };
        
        }
        internal FieldMarshal ReadFieldMarshal()
        {
            byte[] parts = new byte[] { 
                GetDefaultIndex(tablesHeap.HasFieldMarshall),
                tablesHeap.netheader.BlobHeap.indexsize,
     
            };

            return new FieldMarshal(ReadRow(parts)) { table = MetaDataTableType.FieldMarshal, netheader = tablesHeap.netheader };

        }
        internal SecurityDeclaration ReadSecurityDecl()
        {
            byte[] parts = new byte[] { 
                sizeof(ushort),
                GetDefaultIndex(tablesHeap.HasDeclSecurity),
                tablesHeap.netheader.BlobHeap.indexsize,
     
            };

            return new SecurityDeclaration(ReadRow(parts)) { table = MetaDataTableType.DeclSecurity, netheader = tablesHeap.netheader };            
        }
        internal ClassLayout ReadClassLayout()
        {
            byte[] parts = new byte[] { 
                sizeof(ushort),
                sizeof(uint),
                GetDefaultIndex(MetaDataTableType.TypeDef),
     
            };

            return new ClassLayout(ReadRow(parts)) { table = MetaDataTableType.ClassLayout, netheader = tablesHeap.netheader };      
        }
        internal FieldLayout ReadFieldLayout()
        {
            byte[] parts = new byte[] { 
                sizeof(uint),
                GetDefaultIndex(MetaDataTableType.Field),
     
            };

            return new FieldLayout(ReadRow(parts)) { table = MetaDataTableType.FieldLayout, netheader = tablesHeap.netheader }; 
        }
        internal StandAloneSignature ReadStandAloneSig()
        {
            byte[] parts = new byte[] { 
                tablesHeap.netheader.BlobHeap.indexsize,
            };

            return new StandAloneSignature(ReadRow(parts)) { table = MetaDataTableType.StandAloneSig, netheader = tablesHeap.netheader }; 

        }
        internal EventMap ReadEventMap()
        {
            byte[] parts = new byte[] { 
                GetDefaultIndex(MetaDataTableType.TypeDef),
                GetDefaultIndex(MetaDataTableType.Event),
            };

            return new EventMap(ReadRow(parts)) { table = MetaDataTableType.EventMap, netheader = tablesHeap.netheader };
        }
        internal EventDefinition ReadEventDef()
        {
            byte[] parts = new byte[] { 
                sizeof(ushort),
                tablesHeap.netheader.StringsHeap.indexsize,
                GetDefaultIndex(tablesHeap.TypeDefOrRef),
            };

            return new EventDefinition(ReadRow(parts)) { table = MetaDataTableType.Event, netheader = tablesHeap.netheader };
        }
        internal PropertyMap ReadPropertyMap()
        {
            byte[] parts = new byte[] { 
                GetDefaultIndex(MetaDataTableType.TypeDef),
                GetDefaultIndex(MetaDataTableType.Property),
            };

            return new PropertyMap(ReadRow(parts)) { table = MetaDataTableType.PropertyMap, netheader = tablesHeap.netheader };

        }
        internal PropertyDefinition ReadPropertyDef()
        {
            byte[] parts = new byte[] { 
                sizeof(ushort),
                tablesHeap.netheader.StringsHeap.indexsize,
                tablesHeap.netheader.BlobHeap.indexsize,
            };

            return new PropertyDefinition(ReadRow(parts)) { table = MetaDataTableType.Property, netheader = tablesHeap.netheader };

        }
        internal MethodSemantics ReadMethodSemantics()
        {
            byte[] parts = new byte[] { 
                sizeof(ushort),
                GetDefaultIndex(MetaDataTableType.Method),
                GetDefaultIndex(tablesHeap.HasSemantics),
            };

            return new MethodSemantics(ReadRow(parts)) { table = MetaDataTableType.MethodSemantics, netheader = tablesHeap.netheader };
        }
        internal MethodImplementation ReadMethodImpl()
        {
            byte[] parts = new byte[] { 
                GetDefaultIndex(MetaDataTableType.TypeDef),
                GetDefaultIndex(tablesHeap.MethodDefOrRef),
                GetDefaultIndex(tablesHeap.MethodDefOrRef),
            };

            return new MethodImplementation(ReadRow(parts)) { table = MetaDataTableType.MethodImpl,  netheader = tablesHeap.netheader };
        

        }
        internal ModuleReference ReadModuleRef()
        {
            byte[] parts = new byte[] { 
                tablesHeap.netheader.StringsHeap.indexsize,
            };

            return new ModuleReference(ReadRow(parts)) { table = MetaDataTableType.ModuleRef, netheader = tablesHeap.netheader };
        }
        internal TypeSpecification ReadTypeSpec()
        {
            byte[] parts = new byte[] { 
                tablesHeap.netheader.BlobHeap.indexsize,
            };

            return new TypeSpecification(ReadRow(parts)) { table = MetaDataTableType.TypeSpec, netheader = tablesHeap.netheader };
        }
        internal MethodSpecification ReadMethodSpec()
        {
            byte[] parts = new byte[] { 
                GetDefaultIndex(tablesHeap.MethodDefOrRef),
                tablesHeap.netheader.BlobHeap.indexsize,
            };

            return new MethodSpecification(ReadRow(parts)) { table = MetaDataTableType.MethodSpec, netheader = tablesHeap.netheader };
       }
        internal PInvokeImplementation ReadPInvokeImpl()
        {
            byte[] parts = new byte[] { 
                sizeof(ushort),
                GetDefaultIndex(tablesHeap.MemberForwarded),
                tablesHeap.netheader.StringsHeap.indexsize,
                GetDefaultIndex(MetaDataTableType.ModuleRef),
            };

            return new PInvokeImplementation(ReadRow(parts)) { table = MetaDataTableType.ImplMap, netheader = tablesHeap.netheader };
        }
        internal FieldRVA ReadFieldRVA()
        {
            byte[] parts = new byte[] { 
                sizeof(uint),
                GetDefaultIndex(MetaDataTableType.Field),
            };

            return new FieldRVA(ReadRow(parts)) { table = MetaDataTableType.FieldRVA, netheader = tablesHeap.netheader };
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

            return new AssemblyDefinition(ReadRow(parts)) { table = MetaDataTableType.Assembly, netheader = tablesHeap.netheader };
       
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

            return new AssemblyReference(ReadRow(parts)) { table = MetaDataTableType.AssemblyRef, netheader = tablesHeap.netheader };
       

        }
        internal FileReference ReadFileReference()
        {
            byte[] parts = new byte[] { 
                sizeof(uint),
                tablesHeap.netheader.StringsHeap.indexsize,
                tablesHeap.netheader.BlobHeap.indexsize,
            };

            return new FileReference(ReadRow(parts)) { table = MetaDataTableType.File, netheader = tablesHeap.netheader };
       
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

            return new ExportedType(ReadRow(parts)) { table = MetaDataTableType.ExportedType, netheader = tablesHeap.netheader };
       
        }
        internal ManifestResource ReadManifestRes()
        {
            byte[] parts = new byte[] { 
                sizeof(uint),
                sizeof(uint),
                tablesHeap.netheader.StringsHeap.indexsize,
                GetDefaultIndex(tablesHeap.Implementation),
            };

            return new ManifestResource(ReadRow(parts)) { table = MetaDataTableType.ManifestResource, netheader = tablesHeap.netheader };
       

        }
        internal NestedClass ReadNestedClass()
        {
            byte[] parts = new byte[] { 
                GetDefaultIndex(MetaDataTableType.TypeDef),
                GetDefaultIndex(MetaDataTableType.TypeDef),
            };

            return new NestedClass(ReadRow(parts)) { table = MetaDataTableType.NestedClass, netheader = tablesHeap.netheader };
       
            
        }
        internal EnCLog ReadEnCLog()
        {
            byte[] parts = new byte[] { sizeof(uint),sizeof(uint) };
            return new EnCLog(ReadRow(parts)) { table = MetaDataTableType.EncLog, netheader = tablesHeap.netheader };
        }
        internal EnCMap ReadEnCMap()
        {
            byte[] parts = new byte[] { sizeof(uint) };
            return new EnCMap(ReadRow(parts)) { table = MetaDataTableType.EncMap, netheader = tablesHeap.netheader };

        }
        internal GenericParameter ReadGenericParam()
        {
            byte[] parts = new byte[] { 
                sizeof(ushort),
                sizeof(ushort),
                GetDefaultIndex(tablesHeap.TypeOrMethod),
                tablesHeap.netheader.StringsHeap.indexsize};
            return new GenericParameter(ReadRow(parts)) { table = MetaDataTableType.GenericParam, netheader = tablesHeap.netheader };

        }
        internal GenericParamConstraint ReadGenericParamConstraint()
        {
            byte[] parts = new byte[] { 
                GetDefaultIndex(MetaDataTableType.GenericParam),
                GetDefaultIndex(tablesHeap.TypeDefOrRef),};
            return new GenericParamConstraint(ReadRow(parts)) { table = MetaDataTableType.GenericParamConstraint, netheader = tablesHeap.netheader };

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
