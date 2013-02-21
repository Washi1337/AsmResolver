using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using TUP.AsmResolver.NET;
using TUP.AsmResolver.NET.Specialized;

namespace TUP.AsmResolver.PE
{
    internal class NETTableReader : IDisposable
    {
        MetaDataStream stream;
        BinaryReader reader;
        internal TablesHeap tableheap;

        internal MetaDataTableGroup TypeDefOrRef;
        internal MetaDataTableGroup HasConstant;
        internal MetaDataTableGroup HasCustomAttribute;
        internal MetaDataTableGroup HasFieldMarshall;
        internal MetaDataTableGroup HasDeclSecurity;
        internal MetaDataTableGroup MemberRefParent;
        internal MetaDataTableGroup HasSemantics;
        internal MetaDataTableGroup MethodDefOrRef;
        internal MetaDataTableGroup MemberForwarded;
        internal MetaDataTableGroup Implementation;
        internal MetaDataTableGroup CustomAttributeType;
        internal MetaDataTableGroup ResolutionScope;
        internal MetaDataTableGroup TypeOrMethod;

        internal NETTableReader(MetaDataStream stream)
        {
            this.stream = stream;
            reader = new BinaryReader(new MemoryStream(stream.Contents));
            tableheap = new TablesHeap();
            tableheap.header = ASMGlobals.ReadStructureFromReader<Structures.METADATA_TABLE_HEADER>(reader);
            tableheap.reader = stream.reader;
            tableheap.offset = stream.offset;
            tableheap.size = stream.size;
            tableheap.stream = stream;
            tableheap.streamoffset = stream.streamoffset;
            tableheap.headeroffset = stream.headeroffset;
            tableheap.netheader = stream.netheader;



            for (int i = 0; i < 45; i++)
                if (tableheap.HasTable((MetaDataTableType)i))
                {
                    tableheap.tablecount++;
                }
            ReadTableHeaders();
            tableheap.tablereader = this;
           // ReadTables();

        }
        internal MetaDataTable CreateTable(MetaDataTableType type, int amountofrows)
        {
            //customattribute[permission]?

            MetaDataTable table = new MetaDataTable();
            table.Type = type;
            table.AmountOfRows = amountofrows;
            switch (type)
            {
                case MetaDataTableType.Assembly:
                    HasCustomAttribute[14] = table;
                    HasDeclSecurity[2] = table;
                    break;
                case MetaDataTableType.AssemblyRef:
                    HasCustomAttribute[15] = table;
                    Implementation[1] = table;
                    ResolutionScope[2] = table;
                    break;
                case MetaDataTableType.Event:
                    HasCustomAttribute[10] = table;
                    HasSemantics[0] = table;
                    break;
                case MetaDataTableType.ExportedType:
                    HasCustomAttribute[17] = table;
                    Implementation[2] = table;
                    break;
                case MetaDataTableType.Field:
                    HasConstant[0] = table;
                    HasCustomAttribute[1] = table;
                    HasFieldMarshall[0] = table;
                    MemberForwarded[0] = table;
                    break;
                case MetaDataTableType.File:
                    HasCustomAttribute[16] = table;
                    Implementation[0] = table;
                    break;
                case MetaDataTableType.GenericParam:
                    HasCustomAttribute[19] = table;
                    break;
                case MetaDataTableType.GenericParamConstraint:
                    HasCustomAttribute[20] = table;
                    break;
                case MetaDataTableType.InterfaceImpl:
                    HasCustomAttribute[5] = table;
                    break;
                case MetaDataTableType.ManifestResource:
                    HasCustomAttribute[18] = table;
                    break;
                case MetaDataTableType.MemberRef:
                    HasCustomAttribute[6] = table;
                    MethodDefOrRef[1] = table;
                    CustomAttributeType[3] = table;
                    break;
                case MetaDataTableType.Method:
                    HasCustomAttribute[0] = table;
                    HasDeclSecurity[1] = table;
                    MemberRefParent[3] = table;
                    MethodDefOrRef[0] = table;
                    MemberForwarded[1] = table;
                    CustomAttributeType[2] = table;
                    TypeOrMethod[1] = table;
                    break;
                case MetaDataTableType.MethodSpec:
                    HasCustomAttribute[21] = table;
                    break;
                case MetaDataTableType.Module:
                    HasCustomAttribute[7] = table;
                    ResolutionScope[0] = table;
                    break;
                case MetaDataTableType.ModuleRef:
                    HasCustomAttribute[12] = table;
                    MemberRefParent[2] = table;
                    ResolutionScope[1] = table;
                    break;
                case MetaDataTableType.Param:
                    HasConstant[1] = table;
                    HasCustomAttribute[4] = table;
                    HasFieldMarshall[1] = table;
                    break;
                case MetaDataTableType.Property:
                    HasConstant[2] = table;
                    HasCustomAttribute[9] = table;
                    HasSemantics[1] = table;
                    break;
                case MetaDataTableType.StandAloneSig:
                    HasCustomAttribute[11] = table;
                    break;
                case MetaDataTableType.TypeDef:
                    TypeDefOrRef[0] = table;
                    HasCustomAttribute[3] = table;
                    HasDeclSecurity[0] = table;
                    MemberRefParent[0] = table;
                    TypeOrMethod[0] = table;
                    break;
                case MetaDataTableType.TypeRef:
                    TypeDefOrRef[1] = table;
                    HasCustomAttribute[2] = table;
                    MemberRefParent[1] = table;
                    ResolutionScope[3] = table;
                    break;
                case MetaDataTableType.TypeSpec:
                    TypeDefOrRef[2] = table;
                    HasCustomAttribute[13] = table;
                    MemberRefParent[4] = table;
                    break;
            }
            return table;
        }

        internal void ReadTableHeaders()
        {
            SetupCodedIndexes();

            for (int i = 0; i < 45; i++)
            {
                    
                if (tableheap.HasTable((MetaDataTableType)i))
                {
                    tableheap.tables[i] = (CreateTable((MetaDataTableType)i, reader.ReadInt32()));
                }
            }

        }
        internal void SetupCodedIndexes()
        {
            TypeDefOrRef = new MetaDataTableGroup(3, 2);
            HasConstant = new MetaDataTableGroup(3, 2);
            HasCustomAttribute = new MetaDataTableGroup(22, 5);
            HasFieldMarshall = new MetaDataTableGroup(2, 1);
            HasDeclSecurity = new MetaDataTableGroup(3, 2);
            MemberRefParent = new MetaDataTableGroup(5, 3);
            HasSemantics = new MetaDataTableGroup(2, 1);
            MethodDefOrRef = new MetaDataTableGroup(2, 1);
            MemberForwarded = new MetaDataTableGroup(2, 1);
            Implementation = new MetaDataTableGroup(3, 2);
            CustomAttributeType = new MetaDataTableGroup(5, 3);
            ResolutionScope = new MetaDataTableGroup(4, 2);
            TypeOrMethod = new MetaDataTableGroup(2, 1);
        }

        internal void ReadTables()
        {
            if ((tableheap.HeapOffsetSizes & 1) == 1)
                tableheap.netheader.StringsHeap.indexsize = 4;
            if ((tableheap.HeapOffsetSizes & 2) == 2)
                tableheap.netheader.GuidHeap.indexsize = 4;
            if ((tableheap.HeapOffsetSizes & 4) == 4)
                tableheap.netheader.BlobHeap.indexsize = 4;

            foreach (MetaDataTable table in tableheap.tables)
            {
                if (table != null)
                {
                    for (int i = 0; i < table.AmountOfRows; i++)
                    {
                        switch (table.Type)
                        {
                            case MetaDataTableType.Module:
                                table.members.Add(ReadModule());
                                break;
                            case MetaDataTableType.TypeRef:
                                table.members.Add(ReadTypeRef());
                                break;
                            case MetaDataTableType.TypeDef:
                                table.members.Add(ReadTypeDef());
                                break;
                            case MetaDataTableType.Field:
                                table.members.Add(ReadFieldDef());
                                break;
                            case MetaDataTableType.Method:
                                table.members.Add(ReadMethodDef());
                                break;
                            case MetaDataTableType.Param:
                                table.members.Add(ReadParamDef());
                                break;
                            case MetaDataTableType.InterfaceImpl:
                                table.members.Add(ReadInterfaceImpl());
                                break;
                            case MetaDataTableType.MemberRef:
                                table.members.Add(ReadMemberRef());
                                break;
                            case MetaDataTableType.Constant:
                                table.members.Add(ReadConstant());
                                break;
                            case MetaDataTableType.CustomAttribute:
                                table.members.Add(ReadCustomAttribute());
                                break;
                            case MetaDataTableType.FieldMarshal:
                                table.members.Add(ReadFieldMarshal());
                                break;
                            case MetaDataTableType.DeclSecurity:
                                table.members.Add(ReadSecurityDecl());
                                break;
                            case MetaDataTableType.ClassLayout:
                                table.members.Add(ReadClassLayout());
                                break;
                            case MetaDataTableType.FieldLayout:
                                table.members.Add(ReadFieldLayout());
                                break;
                            case MetaDataTableType.StandAloneSig:
                                table.members.Add(ReadStandAloneSig());
                                break;
                            case MetaDataTableType.EventMap:
                                table.members.Add(ReadEventMap());
                                break;
                            case MetaDataTableType.Event:
                                table.members.Add(ReadEventDef());
                                break;
                            case MetaDataTableType.PropertyMap:
                                table.members.Add(ReadPropertyMap());
                                break;
                            case MetaDataTableType.Property:
                                table.members.Add(ReadPropertyDef());
                                break;
                            case MetaDataTableType.MethodSemantics:
                                table.members.Add(ReadMethodSemantics());
                                break;
                            case MetaDataTableType.MethodImpl:
                                table.members.Add(ReadMethodImpl());
                                break;
                            case MetaDataTableType.ModuleRef:
                                table.members.Add(ReadModuleRef());
                                break;
                            case MetaDataTableType.TypeSpec:
                                table.members.Add(ReadTypeSpec());
                                break;
                            case MetaDataTableType.MethodSpec:
                                table.members.Add(ReadMethodSpec());
                                break;
                            case MetaDataTableType.ImplMap:
                                table.members.Add(ReadPInvokeImpl());
                                break;
                            case MetaDataTableType.FieldRVA:
                                table.members.Add(ReadFieldRVA());
                                break;
                            case MetaDataTableType.Assembly:
                                table.members.Add(ReadAssemblyDef());
                                break;
                            case MetaDataTableType.AssemblyRef:
                                table.members.Add(ReadAssemblyRef());
                                break;
                            case MetaDataTableType.File:
                                table.members.Add(ReadFileReference());
                                break;
                            case MetaDataTableType.ExportedType:
                                table.members.Add(ReadExportedType());
                                break;
                            case MetaDataTableType.ManifestResource:
                                table.members.Add(ReadManifestRes());
                                break;
                            case MetaDataTableType.NestedClass:
                                table.members.Add(ReadNestedClass());
                                break;
                            case MetaDataTableType.EncLog:
                                table.members.Add(ReadEnCLog());
                                break;
                            case MetaDataTableType.EncMap:
                                table.members.Add(ReadEnCMap());
                                break;
                            case MetaDataTableType.GenericParam:
                                table.members.Add(ReadGenericParam());
                                break;
                        }
                        if (table.members.Count > 0)
                        {
                            table.members.Last().metadatatoken = ConstructMetaDataToken(table.type, i);
                        }
                    }
                }
            }
            reader.BaseStream.Close();
            reader.BaseStream.Dispose();
        }




        private byte GetDefaultIndex(MetaDataTableType type)
        {
            for (int i = 0;i< tableheap.tables.Length;i++)
                if (tableheap.tables[i] != null)
                    if (tableheap.tables[i].type == type && tableheap.tables[i].IsLarge(0))
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

            row.offset = (uint)(reader.BaseStream.Position + stream.streamoffset);
            
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
                tableheap.netheader.StringsHeap.indexsize,
                tableheap.netheader.GuidHeap.indexsize,
                tableheap.netheader.GuidHeap.indexsize,
                tableheap.netheader.GuidHeap.indexsize
            };

            return new ModuleDefinition() { tablereader = this, netheader = tableheap.netheader,  metadatarow = ReadRow(parts) };
        }
        internal TypeReference ReadTypeRef()
        {
            byte[] parts = new byte[] { 
                GetDefaultIndex(ResolutionScope),
                tableheap.netheader.StringsHeap.indexsize,
                tableheap.netheader.StringsHeap.indexsize,
            };

            return new TypeReference() {tablereader = this, netheader = tableheap.netheader, metadatarow = ReadRow(parts) };
        }
        internal TypeDefinition ReadTypeDef()
        {
            byte[] parts = new byte[] { 
                sizeof(uint),
                tableheap.netheader.StringsHeap.indexsize,
                tableheap.netheader.StringsHeap.indexsize,
                GetDefaultIndex(TypeDefOrRef),
                GetDefaultIndex(MetaDataTableType.Field),
                GetDefaultIndex(MetaDataTableType.Method),
            };

            return new TypeDefinition() { tablereader = this, netheader = tableheap.netheader, metadatarow = ReadRow(parts) };
        }
        internal FieldDefinition ReadFieldDef()
        {
            byte[] parts = new byte[] { 
                sizeof(ushort),
                tableheap.netheader.StringsHeap.indexsize,
                tableheap.netheader.BlobHeap.indexsize,
     
            };

            return new FieldDefinition() { tablereader = this, netheader = tableheap.netheader, metadatarow = ReadRow(parts) };
        }
        internal MethodDefinition ReadMethodDef()
        {
            byte[] parts = new byte[] { 
                sizeof(uint),
                sizeof(ushort),
                sizeof(ushort),
                tableheap.netheader.StringsHeap.indexsize,
                tableheap.netheader.BlobHeap.indexsize,
                GetDefaultIndex(MetaDataTableType.Param),
     
            };

            return new MethodDefinition() { tablereader = this, netheader = tableheap.netheader, metadatarow = ReadRow(parts) };
        }
        internal ParameterDefinition ReadParamDef()
        {
            byte[] parts = new byte[] { 
                sizeof(ushort),
                sizeof(ushort),
                tableheap.netheader.StringsHeap.indexsize,
     
            };

            return new ParameterDefinition() { tablereader = this, netheader = tableheap.netheader, metadatarow = ReadRow(parts) };
        }
        internal InterfaceImplementation ReadInterfaceImpl()
        {
            byte[] parts = new byte[] { 
                GetDefaultIndex(MetaDataTableType.TypeDef),
                GetDefaultIndex(TypeDefOrRef),
     
            };

            return new InterfaceImplementation() { tablereader = this, netheader = tableheap.netheader, metadatarow = ReadRow(parts) };

        }
        internal MemberReference ReadMemberRef()
        {
            byte[] parts = new byte[] { 
                GetDefaultIndex(MemberRefParent),
                tableheap.netheader.StringsHeap.indexsize,
                tableheap.netheader.BlobHeap.indexsize
     
            };
            MetaDataRow row = ReadRow(parts);
            tableheap.netheader.blobheap.stream.Seek(Convert.ToUInt32(row.parts[2]), SeekOrigin.Begin);
            tableheap.netheader.blobheap.reader.ReadByte();
            byte sigtype = tableheap.netheader.blobheap.reader.ReadByte();
            //IMemberSignature sig = tableheap.netheader.blobheap.ReadMemberRefSignature(Convert.ToUInt32(row.parts[2]));
            if (sigtype == 0x6)
                return new FieldReference() { tablereader = this, netheader = tableheap.netheader, metadatarow = row };
            else
                return new NET.Specialized.MethodReference() { tablereader = this, netheader = tableheap.netheader,metadatarow = row };
        }
        internal Constant ReadConstant()
        {
            byte[] parts = new byte[] { 
                sizeof(byte),
                sizeof(byte),
                GetDefaultIndex(HasConstant),
                tableheap.netheader.BlobHeap.indexsize,
     
            };

            return new Constant() { tablereader = this, netheader = tableheap.netheader, metadatarow = ReadRow(parts) };
        }
        internal CustomAttribute ReadCustomAttribute()
        {
            byte[] parts = new byte[] { 
                GetDefaultIndex(HasCustomAttribute),
                GetDefaultIndex(CustomAttributeType),
                tableheap.netheader.BlobHeap.indexsize,
     
            };

            return new CustomAttribute() { tablereader = this, netheader = tableheap.netheader, metadatarow = ReadRow(parts) };
        
        }
        internal FieldMarshal ReadFieldMarshal()
        {
            byte[] parts = new byte[] { 
                GetDefaultIndex(HasFieldMarshall),
                tableheap.netheader.BlobHeap.indexsize,
     
            };

            return new FieldMarshal() { tablereader = this, netheader = tableheap.netheader, metadatarow = ReadRow(parts) };

        }
        internal SecurityDeclaration ReadSecurityDecl()
        {
            byte[] parts = new byte[] { 
                sizeof(ushort),
                GetDefaultIndex(HasDeclSecurity),
                tableheap.netheader.BlobHeap.indexsize,
     
            };

            return new SecurityDeclaration() { tablereader = this, netheader = tableheap.netheader, metadatarow = ReadRow(parts) };            
        }
        internal ClassLayout ReadClassLayout()
        {
            byte[] parts = new byte[] { 
                sizeof(ushort),
                sizeof(uint),
                GetDefaultIndex(MetaDataTableType.TypeDef),
     
            };

            return new ClassLayout() { tablereader = this, netheader = tableheap.netheader, metadatarow = ReadRow(parts) };      
        }
        internal FieldLayout ReadFieldLayout()
        {
            byte[] parts = new byte[] { 
                sizeof(uint),
                GetDefaultIndex(MetaDataTableType.Field),
     
            };

            return new FieldLayout() { tablereader = this, netheader = tableheap.netheader, metadatarow = ReadRow(parts) }; 
        }
        internal StandAloneSignature ReadStandAloneSig()
        {
            byte[] parts = new byte[] { 
                tableheap.netheader.BlobHeap.indexsize,
            };

            return new StandAloneSignature() { tablereader = this, netheader = tableheap.netheader, metadatarow = ReadRow(parts) }; 

        }
        internal EventMap ReadEventMap()
        {
            byte[] parts = new byte[] { 
                GetDefaultIndex(MetaDataTableType.TypeDef),
                GetDefaultIndex(MetaDataTableType.Event),
            };

            return new EventMap() { tablereader = this, netheader = tableheap.netheader, metadatarow = ReadRow(parts) };
        }
        internal EventDefinition ReadEventDef()
        {
            byte[] parts = new byte[] { 
                sizeof(ushort),
                tableheap.netheader.StringsHeap.indexsize,
                GetDefaultIndex(TypeDefOrRef),
            };

            return new EventDefinition() { tablereader = this, netheader = tableheap.netheader, metadatarow = ReadRow(parts) };
        }
        internal PropertyMap ReadPropertyMap()
        {
            byte[] parts = new byte[] { 
                GetDefaultIndex(MetaDataTableType.TypeDef),
                GetDefaultIndex(MetaDataTableType.Property),
            };

            return new PropertyMap() { tablereader = this, netheader = tableheap.netheader, metadatarow = ReadRow(parts) };

        }
        internal PropertyDefinition ReadPropertyDef()
        {
            byte[] parts = new byte[] { 
                sizeof(ushort),
                tableheap.netheader.StringsHeap.indexsize,
                tableheap.netheader.BlobHeap.indexsize,
            };

            return new PropertyDefinition() { tablereader = this, netheader = tableheap.netheader, metadatarow = ReadRow(parts) };

        }
        internal MethodSemantics ReadMethodSemantics()
        {
            byte[] parts = new byte[] { 
                sizeof(ushort),
                GetDefaultIndex(MetaDataTableType.Method),
                GetDefaultIndex(HasSemantics),
            };

            return new MethodSemantics() { tablereader = this, netheader = tableheap.netheader, metadatarow = ReadRow(parts) };
        }
        internal MethodImplementation ReadMethodImpl()
        {
            byte[] parts = new byte[] { 
                GetDefaultIndex(MetaDataTableType.TypeDef),
                GetDefaultIndex(MethodDefOrRef),
                GetDefaultIndex(MethodDefOrRef),
            };

            return new MethodImplementation() { tablereader = this, netheader = tableheap.netheader, metadatarow = ReadRow(parts) };
        

        }
        internal ModuleReference ReadModuleRef()
        {
            byte[] parts = new byte[] { 
                tableheap.netheader.StringsHeap.indexsize,
            };

            return new ModuleReference() { tablereader = this, netheader = tableheap.netheader, metadatarow = ReadRow(parts) };
        }
        internal TypeSpecification ReadTypeSpec()
        {
            byte[] parts = new byte[] { 
                tableheap.netheader.BlobHeap.indexsize,
            };

            return new TypeSpecification() { tablereader = this, netheader = tableheap.netheader, metadatarow = ReadRow(parts) };
        }
        internal MethodSpecification ReadMethodSpec()
        {
            byte[] parts = new byte[] { 
                GetDefaultIndex(MethodDefOrRef),
                tableheap.netheader.BlobHeap.indexsize,
            };

            return new MethodSpecification() { tablereader = this, netheader = tableheap.netheader, metadatarow = ReadRow(parts) };
       }
        internal PInvokeImplementation ReadPInvokeImpl()
        {
            byte[] parts = new byte[] { 
                sizeof(ushort),
                GetDefaultIndex(MemberForwarded),
                tableheap.netheader.StringsHeap.indexsize,
                GetDefaultIndex(MetaDataTableType.ModuleRef),
            };

            return new PInvokeImplementation() { tablereader = this, netheader = tableheap.netheader, metadatarow = ReadRow(parts) };
        }
        internal FieldRVA ReadFieldRVA()
        {
            byte[] parts = new byte[] { 
                sizeof(uint),
                GetDefaultIndex(MetaDataTableType.Field),
            };

            return new FieldRVA() { tablereader = this, netheader = tableheap.netheader, metadatarow = ReadRow(parts) };
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
                tableheap.netheader.BlobHeap.indexsize ,
                tableheap.netheader.StringsHeap.indexsize ,
                tableheap.netheader.StringsHeap.indexsize ,
            };

            return new AssemblyDefinition() { tablereader = this, netheader = tableheap.netheader, metadatarow = ReadRow(parts) };
       
        }
        internal AssemblyReference ReadAssemblyRef()
        {
            byte[] parts = new byte[] { 
                sizeof(ushort),
                sizeof(ushort),
                sizeof(ushort),
                sizeof(ushort),
                sizeof(uint),
                tableheap.netheader.BlobHeap.indexsize ,
                tableheap.netheader.StringsHeap.indexsize ,
                tableheap.netheader.StringsHeap.indexsize ,
                tableheap.netheader.BlobHeap.indexsize ,
            };

            return new AssemblyReference() { tablereader = this, netheader = tableheap.netheader, metadatarow = ReadRow(parts) };
       

        }
        internal FileReference ReadFileReference()
        {
            byte[] parts = new byte[] { 
                sizeof(uint),
                tableheap.netheader.StringsHeap.indexsize,
                tableheap.netheader.BlobHeap.indexsize,
            };

            return new FileReference() { tablereader = this, netheader = tableheap.netheader, metadatarow = ReadRow(parts) };
       
        }
        internal ExportedType ReadExportedType()
        {
            byte[] parts = new byte[] { 
                sizeof(uint),
                sizeof(uint),
                tableheap.netheader.StringsHeap.indexsize,
                tableheap.netheader.StringsHeap.indexsize,
                GetDefaultIndex(Implementation),
            };

            return new ExportedType() { tablereader = this, netheader = tableheap.netheader, metadatarow = ReadRow(parts) };
       
        }
        internal ManifestResource ReadManifestRes()
        {
            byte[] parts = new byte[] { 
                sizeof(uint),
                sizeof(uint),
                tableheap.netheader.StringsHeap.indexsize,
                GetDefaultIndex(Implementation),
            };

            return new ManifestResource() { tablereader = this, netheader = tableheap.netheader, metadatarow = ReadRow(parts) };
       

        }
        internal NestedClass ReadNestedClass()
        {
            byte[] parts = new byte[] { 
                GetDefaultIndex(MetaDataTableType.TypeDef),
                GetDefaultIndex(MetaDataTableType.TypeDef),
            };

            return new NestedClass() { tablereader = this, netheader = tableheap.netheader, metadatarow = ReadRow(parts) };
       
            
        }
        internal EnCLog ReadEnCLog()
        {
            byte[] parts = new byte[] { sizeof(uint),sizeof(uint) };
            return new EnCLog() { tablereader = this, netheader = tableheap.netheader, metadatarow = ReadRow(parts) };
        }
        internal EnCMap ReadEnCMap()
        {
            byte[] parts = new byte[] { sizeof(uint) };
            return new EnCMap() { tablereader = this, netheader = tableheap.netheader, metadatarow = ReadRow(parts) };

        }
        internal GenericParameter ReadGenericParam()
        {
            byte[] parts = new byte[] { 
                sizeof(ushort),
                sizeof(ushort),
                GetDefaultIndex(TypeOrMethod),
                tableheap.netheader.StringsHeap.indexsize};
            return new GenericParameter() { tablereader = this, netheader = tableheap.netheader, metadatarow = ReadRow(parts) };

        }

        internal int ConstructMetaDataToken(MetaDataTableType type, int index)
        {
            int token = ((int)type);
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
