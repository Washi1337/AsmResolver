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
                tablesheap._header = ASMGlobals.ReadStructureFromReader<Structures.METADATA_TABLE_HEADER>(reader);
                this.tablesHeap = tablesheap;

                for (int i = 0; i < 45; i++)
                    if (tablesHeap.HasTable((MetaDataTableType)i))
                    {
                        tablesHeap._tablecount++;
                    }

                tablesHeap._tablereader = this;
                if ((tablesHeap.HeapOffsetSizes & 1) == 1)
                    tablesHeap._netheader.StringsHeap._indexsize = 4;
                if ((tablesHeap.HeapOffsetSizes & 2) == 2)
                    tablesHeap._netheader.GuidHeap._indexsize  = 4;
                if ((tablesHeap.HeapOffsetSizes & 4) == 4)
                    tablesHeap._netheader.BlobHeap._indexsize  = 4;

                ReadTableHeaders(reader);
            }
        }

        internal MetaDataTable CreateTable(MetaDataTableType type, int rowAmount, long rowAmountOffset)
        {
            //customattribute[permission]?

            MetaDataTable table = new MetaDataTable(tablesHeap, false);
            table.Type = type;
            table._rowAmount = rowAmount;
            table._rowAmountOffset = rowAmountOffset;
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
                
                    tablesHeap._tables[i] = table;
                }
            }

            uint tableOffset = (uint)(tablesHeap.StreamOffset + Marshal.SizeOf(typeof(Structures.METADATA_TABLE_HEADER)) + (tablesHeap._tablecount) * 4);
            foreach (MetaDataTable table in tablesHeap._tables)
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

        internal MetaDataMember ReadMember(MetaDataTable table, int index)
        {
            using (MemoryStream stream = new MemoryStream(tablesHeap.Contents))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    stream.Position = table.TableOffset - tablesHeap.StreamOffset;
                    stream.Position += (table.CalculateRowSize() * (index - 1));
                    Type type;
                    MetaDataRow row;
                    if (GetRowAndType(reader, table, out row, out type))
                    {
                        return CreateMember(type, row, table.Type, index);
                    }
                }
            }
            return null;
        }

        private bool GetRowAndType(BinaryReader reader, MetaDataTable table, out MetaDataRow row, out Type type)
        {
            type = null;
            row = default(MetaDataRow);

            switch (table.Type)
            {
                case MetaDataTableType.Module:
                    row = ReadRow(reader, GetModuleSignature()); type = typeof(ModuleDefinition); break;
                case MetaDataTableType.TypeRef:
                    row = ReadRow(reader, GetTypeRefSignature()); type = typeof(TypeReference); break;
                case MetaDataTableType.TypeDef:
                    row = ReadRow(reader, GetTypeDefSignature()); type = typeof(TypeDefinition); break;
                case MetaDataTableType.FieldPtr:
                    row = ReadRow(reader, GetFieldPtrSignature()); type = typeof(FieldPtr); break;
                case MetaDataTableType.Field:
                    row = ReadRow(reader, GetFieldDefSignature()); type = typeof(FieldDefinition); break;
                case MetaDataTableType.MethodPtr:
                    row = ReadRow(reader, GetMethodPtrSignature()); type = typeof(MethodPtr); break;
                case MetaDataTableType.Method:
                    row = ReadRow(reader, GetMethodDefSignature()); type = typeof(MethodDefinition); break;
                case MetaDataTableType.ParamPtr:
                    row = ReadRow(reader, GetParamPtrSignature()); type = typeof(ParamPtr); break;
                case MetaDataTableType.Param:
                    row = ReadRow(reader, GetParamDefSignature()); type = typeof(ParameterDefinition); break;
                case MetaDataTableType.InterfaceImpl:
                    row = ReadRow(reader, GetInterfaceImplSignature()); type = typeof(InterfaceImplementation); break;
                case MetaDataTableType.MemberRef:
                    row = ReadRow(reader, GetMemberRefSignature());
                    tablesHeap._netheader.BlobHeap._mainStream.Seek(Convert.ToUInt32(row._parts[2]), SeekOrigin.Begin);
                    tablesHeap._netheader.BlobHeap._binReader.ReadByte();
                    byte sigtype = tablesHeap._netheader.BlobHeap._binReader.ReadByte();

                    if (sigtype == 0x6)
                        type = typeof(FieldReference);
                    else
                        type = typeof(MethodReference);

                    break;
                case MetaDataTableType.Constant:
                    row = ReadRow(reader, GetConstantSignature()); type = typeof(Constant); break;
                case MetaDataTableType.CustomAttribute:
                    row = ReadRow(reader, GetCustomAttributeSignature()); type = typeof(CustomAttribute); break;
                case MetaDataTableType.FieldMarshal:
                    row = ReadRow(reader, GetFieldMarshalSignature()); type = typeof(FieldMarshal); break;
                case MetaDataTableType.DeclSecurity:
                    row = ReadRow(reader, GetSecurityDeclSignature()); type = typeof(SecurityDeclaration); break;
                case MetaDataTableType.ClassLayout:
                    row = ReadRow(reader, GetClassLayoutSignature()); type = typeof(ClassLayout); break;
                case MetaDataTableType.FieldLayout:
                    row = ReadRow(reader, GetFieldLayoutSignature()); type = typeof(FieldLayout); break;
                case MetaDataTableType.StandAloneSig:
                    row = ReadRow(reader, GetStandAloneSigSignature()); type = typeof(StandAloneSignature); break;
                case MetaDataTableType.EventMap:
                    row = ReadRow(reader, GetEventMapSignature()); type = typeof(EventMap); break;
                case MetaDataTableType.Event:
                    row = ReadRow(reader, GetEventDefSignature()); type = typeof(EventDefinition); break;
                case MetaDataTableType.PropertyMap:
                    row = ReadRow(reader, GetPropertyMapSignature()); type = typeof(PropertyMap); break;
                case MetaDataTableType.PropertyPtr:
                    row = ReadRow(reader, GetPropertyPtrSignature()); type = typeof(PropertyPtr); break;
                case MetaDataTableType.Property:
                    row = ReadRow(reader, GetPropertyDefSignature()); type = typeof(PropertyDefinition); break;
                case MetaDataTableType.MethodSemantics:
                    row = ReadRow(reader, GetMethodSemanticsSignature()); type = typeof(MethodSemantics); break;
                case MetaDataTableType.MethodImpl:
                    row = ReadRow(reader, GetMethodImplSignature()); type = typeof(MethodImplementation); break;
                case MetaDataTableType.ModuleRef:
                    row = ReadRow(reader, GetModuleRefSignature()); type = typeof(ModuleReference); break;
                case MetaDataTableType.TypeSpec:
                    row = ReadRow(reader, GetTypeSpecSignature()); type = typeof(TypeSpecification); break;
                case MetaDataTableType.MethodSpec:
                    row = ReadRow(reader, GetMethodSpecSignature()); type = typeof(MethodSpecification); break;
                case MetaDataTableType.ImplMap:
                    row = ReadRow(reader, GetPInvokeImplSignature()); type = typeof(PInvokeImplementation); break;
                case MetaDataTableType.FieldRVA:
                    row = ReadRow(reader, GetFieldRVASignature()); type = typeof(FieldRVA); break;
                case MetaDataTableType.Assembly:
                    row = ReadRow(reader, GetAssemblyDefSignature()); type = typeof(AssemblyDefinition); break;
                case MetaDataTableType.AssemblyProcessor:
                    row = ReadRow(reader, GetAssemblyProcSignature()); type = typeof(AssemblyProcessor); break;
                case MetaDataTableType.AssemblyOS:
                    row = ReadRow(reader, GetAssemblyOSSignature()); type = typeof(AssemblyOS); break;
                case MetaDataTableType.AssemblyRef:
                    row = ReadRow(reader, GetAssemblyRefSignature()); type = typeof(AssemblyReference); break;
                case MetaDataTableType.AssemblyRefProcessor:
                    row = ReadRow(reader, GetAssemblyRefProcSignature()); type = typeof(AssemblyRefProcessor); break;
                case MetaDataTableType.AssemblyRefOS:
                    row = ReadRow(reader, GetAssemblyRefOSSignature()); type = typeof(AssemblyRefOS); break;
                case MetaDataTableType.File:
                    row = ReadRow(reader, GetFileReferenceSignature()); type = typeof(File); break;
                case MetaDataTableType.ExportedType:
                    row = ReadRow(reader, GetExportedTypeSignature()); type = typeof(ExportedType); break;
                case MetaDataTableType.ManifestResource:
                    row = ReadRow(reader, GetManifestResSignature()); type = typeof(ManifestResource); break;
                case MetaDataTableType.NestedClass:
                    row = ReadRow(reader, GetNestedClassSignature()); type = typeof(NestedClass); break;
                case MetaDataTableType.EncLog:
                    row = ReadRow(reader, GetEnCLogSignature()); type = typeof(EnCLog); break;
                case MetaDataTableType.EncMap:
                    row = ReadRow(reader, GetEnCMapSignature()); type = typeof(EnCMap); break;
                case MetaDataTableType.GenericParam:
                    row = ReadRow(reader, GetGenericParamSignature()); type = typeof(GenericParameter); break;
                case MetaDataTableType.GenericParamConstraint:
                    row = ReadRow(reader, GetGenericParamConstraintSignature()); type = typeof(GenericParamConstraint); break;

            }

            return type != null;
        }

        private byte GetIndexSize(MetaDataStream stream)
        {
            if (stream != null)
                return stream.IndexSize;
            return sizeof(ushort);
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
            foreach (MetaDataTable table in tablegroup._tables)
                if (table != null && table.IsLarge(tablegroup._bits))
                    return sizeof(uint);
            return sizeof(ushort);
        }

        internal MetaDataRow ReadRow(BinaryReader reader, byte[] signature)
        {
            MetaDataRow row = new MetaDataRow();
            row._parts = new ValueType[signature.Length];
            row.NETHeader = tablesHeap._netheader;

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

        internal MetaDataMember CreateMember(Type type, MetaDataRow row, MetaDataTableType table, int index)
        {
            MetaDataMember member = (MetaDataMember)Activator.CreateInstance(type, row);
            member._table = table;
            member._netheader = tablesHeap._netheader;
            member._metadatatoken = (uint)(((int)table) << 24 | index);
            return member;
        }

        internal byte[] GetModuleSignature()
        {
            byte[] parts = new byte[] {
                sizeof(ushort),
                GetIndexSize(tablesHeap._netheader.StringsHeap) ,
                GetIndexSize(tablesHeap._netheader.GuidHeap),
                GetIndexSize(tablesHeap._netheader.GuidHeap),
                GetIndexSize(tablesHeap._netheader.GuidHeap)
            };

            return parts;
        }

        internal byte[] GetTypeRefSignature()
        {
            byte[] parts = new byte[] { 
                GetIndexSize(tablesHeap.ResolutionScope),
                GetIndexSize(tablesHeap._netheader.StringsHeap),
                GetIndexSize(tablesHeap._netheader.StringsHeap),
            };

            return parts;
        }
        internal byte[] GetTypeDefSignature()
        {
            byte[] parts = new byte[] { 
                sizeof(uint),
                GetIndexSize(tablesHeap._netheader.StringsHeap),
                GetIndexSize(tablesHeap._netheader.StringsHeap),
                GetIndexSize(tablesHeap.TypeDefOrRef),
                GetIndexSize(MetaDataTableType.Field),
                GetIndexSize(MetaDataTableType.Method),
            };

            return parts;
        }
        internal byte[] GetFieldPtrSignature()
        {
            byte[] parts = new byte[]
            {
                GetIndexSize(MetaDataTableType.Field),
            };
            return parts;
        }
        internal byte[] GetFieldDefSignature()
        {
            byte[] parts = new byte[] { 
                sizeof(ushort),
                GetIndexSize(tablesHeap._netheader.StringsHeap),
                GetIndexSize(tablesHeap._netheader.BlobHeap),
     
            };

            return parts;
        }
        internal byte[] GetMethodPtrSignature()
        {
            byte[] parts = new byte[]
            {
                GetIndexSize(MetaDataTableType.Method),
            };
            return parts;
        }
        internal byte[] GetMethodDefSignature()
        {
            byte[] parts = new byte[] { 
                sizeof(uint),
                sizeof(ushort),
                sizeof(ushort),
                GetIndexSize(tablesHeap._netheader.StringsHeap),
                GetIndexSize(tablesHeap._netheader.BlobHeap),
                GetIndexSize(MetaDataTableType.Param),
     
            };

            return parts;
        }
        internal byte[] GetParamPtrSignature()
        {
            byte[] parts = new byte[]
            {
                 GetIndexSize(tablesHeap._netheader.BlobHeap)
            };
            return parts;
        }
        internal byte[] GetParamDefSignature()
        {
            byte[] parts = new byte[] { 
                sizeof(ushort),
                sizeof(ushort),
                GetIndexSize(tablesHeap._netheader.StringsHeap),
     
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
                GetIndexSize(tablesHeap._netheader.StringsHeap),
                GetIndexSize(tablesHeap._netheader.BlobHeap)
     
            };
            return parts;
           
        }
        internal byte[] GetConstantSignature()
        {
            byte[] parts = new byte[] { 
                sizeof(byte),
                sizeof(byte),
                GetIndexSize(tablesHeap.HasConstant),
                GetIndexSize(tablesHeap._netheader.BlobHeap),
     
            };

            return parts;
        }
        internal byte[] GetCustomAttributeSignature()
        {
            byte[] parts = new byte[] { 
                GetIndexSize(tablesHeap.HasCustomAttribute),
                GetIndexSize(tablesHeap.CustomAttributeType),
                GetIndexSize(tablesHeap._netheader.BlobHeap),
     
            };

            return parts;
        
        }
        internal byte[] GetFieldMarshalSignature()
        {
            byte[] parts = new byte[] { 
                GetIndexSize(tablesHeap.HasFieldMarshall),
                GetIndexSize(tablesHeap._netheader.BlobHeap),
     
            };

            return parts;

        }
        internal byte[] GetSecurityDeclSignature()
        {
            byte[] parts = new byte[] { 
                sizeof(ushort),
                GetIndexSize(tablesHeap.HasDeclSecurity),
                GetIndexSize(tablesHeap._netheader.BlobHeap),
     
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
                GetIndexSize(tablesHeap._netheader.BlobHeap),
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
                GetIndexSize(tablesHeap._netheader.StringsHeap),
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
                GetIndexSize(tablesHeap._netheader.StringsHeap),
                GetIndexSize(tablesHeap._netheader.BlobHeap),
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
                GetIndexSize(tablesHeap._netheader.StringsHeap),
            };

            return parts;
        }
        internal byte[] GetTypeSpecSignature()
        {
            byte[] parts = new byte[] { 
                GetIndexSize(tablesHeap._netheader.BlobHeap),
            };

            return parts;
        }
        internal byte[] GetMethodSpecSignature()
        {
            byte[] parts = new byte[] { 
                GetIndexSize(tablesHeap.MethodDefOrRef),
                GetIndexSize(tablesHeap._netheader.BlobHeap),
            };

            return parts;
       }
        internal byte[] GetPInvokeImplSignature()
        {
            byte[] parts = new byte[] { 
                sizeof(ushort),
                GetIndexSize(tablesHeap.MemberForwarded),
                GetIndexSize(tablesHeap._netheader.StringsHeap),
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
                GetIndexSize(tablesHeap._netheader.BlobHeap) ,
                GetIndexSize(tablesHeap._netheader.StringsHeap) ,
                GetIndexSize(tablesHeap._netheader.StringsHeap) ,
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
                GetIndexSize(tablesHeap._netheader.BlobHeap) ,
                GetIndexSize(tablesHeap._netheader.StringsHeap) ,
                GetIndexSize(tablesHeap._netheader.StringsHeap) ,
                GetIndexSize(tablesHeap._netheader.BlobHeap) ,
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
                GetIndexSize(tablesHeap._netheader.StringsHeap),
                GetIndexSize(tablesHeap._netheader.BlobHeap),
            };

            return parts;
       
        }
        internal byte[] GetExportedTypeSignature()
        {
            byte[] parts = new byte[] { 
                sizeof(uint),
                sizeof(uint),
                GetIndexSize(tablesHeap._netheader.StringsHeap),
                GetIndexSize(tablesHeap._netheader.StringsHeap),
                GetIndexSize(tablesHeap.Implementation),
            };

            return parts;
       
        }
        internal byte[] GetManifestResSignature()
        {
            byte[] parts = new byte[] { 
                sizeof(uint),
                sizeof(uint),
                GetIndexSize(tablesHeap._netheader.StringsHeap),
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
                GetIndexSize(tablesHeap._netheader.StringsHeap)};
            return parts;

        }
        internal byte[] GetGenericParamConstraintSignature()
        {
            byte[] parts = new byte[] { 
                GetIndexSize(MetaDataTableType.GenericParam),
                GetIndexSize(tablesHeap.TypeDefOrRef),};
            return parts;

        }

        public void Dispose()
        {
        }
    }
}
