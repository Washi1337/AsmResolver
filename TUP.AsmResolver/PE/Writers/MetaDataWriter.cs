using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TUP.AsmResolver.NET;
using TUP.AsmResolver.NET.Specialized;

namespace TUP.AsmResolver.PE.Writers
{
    public class MetaDataWriter : RebuildingTask 
    {
        public MetaDataWriter(PEConstructor constructor)
            : base(constructor)
        {
        }

        public override void RunProcedure(Workspace workspace)
        {
            AppendHeaders(workspace);
            UpdateMetaDataRows(workspace);
            AppendMembers(workspace);
        }

        private void AppendHeaders(Workspace workspace)
        {
            TablesHeap tablesHeap = workspace.GetStream<TablesHeap>();

            foreach (var keypair in workspace.Members)
            {
                tablesHeap._binWriter.Write(keypair.Value.Length);
            }
        }

        private void AppendMembers(Workspace workspace)
        {
            TablesHeap tablesHeap = workspace.GetStream<TablesHeap>();
            foreach (var keypair in workspace.Members)
            {
                foreach (MetaDataMemberInfo memberInfo in keypair.Value)
                {
                    AppendMetaDataRow(tablesHeap, memberInfo.Instance.MetaDataRow);
                }
            }
        }

        private void AppendMetaDataRow(TablesHeap tablesHeap, MetaDataRow row)
        {
            foreach (var part in row.Parts)
            {
                if (Marshal.SizeOf(part) == sizeof(uint))
                    tablesHeap._binWriter.Write((uint)part);
                else if (Marshal.SizeOf(part) == sizeof(ushort))
                    tablesHeap._binWriter.Write((ushort)part);
                else if (Marshal.SizeOf(part) == sizeof(byte))
                    tablesHeap._binWriter.Write((byte)part);
                else
                    throw new ArgumentException("Invalid MetaData Row");
            }
        }

        private void UpdateMetaDataRows(Workspace workspace)
        {
            TablesHeap tablesHeap = workspace.GetStream<TablesHeap>();
            foreach (var table in workspace.Members)
            {
                foreach (MetaDataMemberInfo member in table.Value)
                {
                    switch (table.Key)
                    {
                        case MetaDataTableType.Assembly: UpdateAssemblyDef(workspace, member.Instance as AssemblyDefinition); break;
                        case MetaDataTableType.AssemblyRef: UpdateAssemblyRef(workspace, member.Instance as AssemblyReference); break;
                        case MetaDataTableType.ClassLayout: UpdateClassLayout(workspace, member.Instance as ClassLayout); break;
                        case MetaDataTableType.Constant: UpdateConstant(workspace, member.Instance as Constant); break;
                        case MetaDataTableType.CustomAttribute: UpdateCustomAttribute(workspace, member.Instance as CustomAttribute); break;
                        case MetaDataTableType.DeclSecurity: UpdateSecurityDecl(workspace, member.Instance as SecurityDeclaration); break;
                        case MetaDataTableType.EncLog: ; break;
                        case MetaDataTableType.EncMap: ; break;
                        case MetaDataTableType.Event: UpdateEventDef(workspace, member.Instance as EventDefinition); break;
                        case MetaDataTableType.EventMap: UpdateEventMap(workspace, member.Instance as EventMap); break;
                        case MetaDataTableType.EventPtr: ; break;
                        case MetaDataTableType.ExportedType: ; break;
                        case MetaDataTableType.Field: UpdateFieldDef(workspace, member.Instance as FieldDefinition); break;
                        case MetaDataTableType.FieldLayout: UpdateFieldLayout(workspace, member.Instance as FieldLayout); break;
                        case MetaDataTableType.FieldMarshal: UpdateFieldMarshal(workspace, member.Instance as FieldMarshal); break;
                        case MetaDataTableType.FieldPtr: ; break;
                        case MetaDataTableType.FieldRVA: UpdateFieldRva(workspace, member.Instance as FieldRVA); break;
                        case MetaDataTableType.File: ; break;
                        case MetaDataTableType.GenericParam: ; break;
                        case MetaDataTableType.GenericParamConstraint: ; break;
                        case MetaDataTableType.ImplMap: UpdatePInvokeImpl(workspace, member.Instance as PInvokeImplementation); break;
                        case MetaDataTableType.InterfaceImpl: UpdateInterfaceImpl(workspace, member.Instance as InterfaceImplementation); break;
                        case MetaDataTableType.ManifestResource: ; break;
                        case MetaDataTableType.MemberRef: UpdateMemberRef(workspace, member.Instance as MemberReference); break;
                        case MetaDataTableType.Method: UpdateMethodDef(workspace, member.Instance as MethodDefinition); break;
                        case MetaDataTableType.MethodImpl: UpdateMethodImpl(workspace, member.Instance as MethodImplementation); break;
                        case MetaDataTableType.MethodPtr: ; break;
                        case MetaDataTableType.MethodSemantics: UpdateMethodSemantics(workspace, member.Instance as MethodSemantics); break;
                        case MetaDataTableType.MethodSpec: UpdateMethodSpec(workspace, member.Instance as MethodSpecification); break;
                        case MetaDataTableType.Module: UpdateModule(workspace, member.Instance as ModuleDefinition); break;
                        case MetaDataTableType.ModuleRef: UpdateModuleRef(workspace, member.Instance as ModuleReference); break;
                        case MetaDataTableType.NestedClass: ; break;
                        case MetaDataTableType.Param: UpdateParamDef(workspace, member.Instance as ParameterDefinition); break;
                        case MetaDataTableType.ParamPtr: UpdateParamPtr(workspace, member.Instance as ParamPtr); break;
                        case MetaDataTableType.Property: UpdatePropertyDef(workspace, member.Instance as PropertyDefinition); break;
                        case MetaDataTableType.PropertyMap: UpdatePropertyMap(workspace, member.Instance as PropertyMap); break;
                        case MetaDataTableType.PropertyPtr: UpdatePropertyPtr(workspace, member.Instance as PropertyPtr); break;
                        case MetaDataTableType.StandAloneSig: UpdateStandAloneSig(workspace, member.Instance as StandAloneSignature); break;
                        case MetaDataTableType.TypeDef: UpdateTypeDef(workspace, member.Instance as TypeDefinition); break;
                        case MetaDataTableType.TypeRef: UpdateTypeRef(workspace, member.Instance as TypeReference); break;
                        case MetaDataTableType.TypeSpec: UpdateTypeSpec(workspace, member.Instance as TypeSpecification); break;

                    }

                }
            }
        }

        private ValueType GetHeapOffset(MetaDataStream stream, uint index)
        {
            if (stream.IndexSize == 4)
                return index;
            return (ushort)index;
        }

        private ValueType GetStringIndex(Workspace workspace, string str)
        {
            StringsHeap stringsHeap = workspace.GetStream<StringsHeap>();
            return GetHeapOffset(stringsHeap, stringsHeap.GetStringOffset(str));
        }

        private ValueType GetGuidIndex(Workspace workspace, Guid guid)
        {
            GuidHeap guidHeap = workspace.GetStream<GuidHeap>();
            return GetHeapOffset(guidHeap, guidHeap.GetGuidOffset(guid));
        }

        private ValueType GetMemberIndex(Workspace workspace, MetaDataMember member)
        {
            if (member.Table.IsLarge(0))
                return member.TableIndex;
            return (ushort)member.TableIndex;
        }

        private ValueType GetMemberIndex(Workspace workspace, MetaDataTableGroup group, MetaDataMember member)
        {
            if (group.IsLarge)
                return group.GetCodedIndex(member);
            return (ushort)group.GetCodedIndex(member);
        }

        private void UpdateModule(Workspace workspace, ModuleDefinition moduleDef)
        {
            moduleDef.MetaDataRow.Parts[1] = GetStringIndex(workspace, moduleDef.Name);
            moduleDef.MetaDataRow.Parts[2] = GetGuidIndex(workspace, moduleDef.Mvid);
        }

        private void UpdateTypeRef(Workspace workspace, TypeReference typeRef)
        {
            typeRef.MetaDataRow.Parts[0] = GetMemberIndex(workspace, typeRef._resolutionScope as MetaDataMember);
            typeRef.MetaDataRow.Parts[1] = GetStringIndex(workspace, typeRef.Namespace);
            typeRef.MetaDataRow.Parts[2] = GetStringIndex(workspace, typeRef.Name);
        }

        private void UpdateTypeDef(Workspace workspace, TypeDefinition typeDef)
        {
            typeDef.MetaDataRow.Parts[1] = GetStringIndex(workspace, typeDef.Namespace);
            typeDef.MetaDataRow.Parts[2] = GetStringIndex(workspace, typeDef.Name);
            typeDef.MetaDataRow.Parts[3] = GetMemberIndex(workspace, Constructor.OriginalAssembly.NETHeader.TablesHeap.TypeDefOrRef, typeDef);
            
            // method and field list updated by MetaDataBuilder class.
        }

        private void UpdateFieldDef(Workspace workspace, FieldDefinition fieldDef)
        {
            fieldDef.MetaDataRow.Parts[1] = GetStringIndex(workspace, fieldDef.Name);

            // TODO: serialize blob.
        }

        private void UpdateMethodDef(Workspace workspace, MethodDefinition methodDef)
        {
            // rva updated later in another task.
            methodDef.MetaDataRow.Parts[3] = GetStringIndex(workspace, methodDef.Name);
            // TODO: blob.
            // param list updated by MetaDataBuilder class.
        }

        private void UpdateParamPtr(Workspace workspace, ParamPtr paramPtr)
        {
            // TODO: serialize blob.
        }

        private void UpdateParamDef(Workspace workspace, ParameterDefinition paramDef)
        {
            paramDef.MetaDataRow.Parts[2] = GetStringIndex(workspace, paramDef.Name);
        }

        private void UpdateInterfaceImpl(Workspace workspace, InterfaceImplementation interfaceImpl)
        {
            interfaceImpl.MetaDataRow.Parts[0] = GetMemberIndex(workspace, interfaceImpl.Class);
            interfaceImpl.MetaDataRow.Parts[1] = GetMemberIndex(workspace, Constructor.OriginalAssembly.NETHeader.TablesHeap.TypeDefOrRef, interfaceImpl.Interface);
        }

        private void UpdateMemberRef(Workspace workspace, MemberReference memberRef)
        {
            memberRef.MetaDataRow.Parts[0] = GetMemberIndex(workspace, Constructor.OriginalAssembly.NETHeader.TablesHeap.MemberRefParent, memberRef.DeclaringType);
            memberRef.MetaDataRow.Parts[1] = GetStringIndex(workspace, memberRef.Name);
            // TODO: serialize blob.
        }

        private void UpdateConstant(Workspace workspace, Constant constant)
        {
            constant.MetaDataRow.Parts[2] = GetMemberIndex(workspace, Constructor.OriginalAssembly.NETHeader.TablesHeap.HasConstant, constant.Parent);
            // TODO: serialize blob.
        }

        private void UpdateCustomAttribute(Workspace workspace, CustomAttribute customAttribute)
        {
            customAttribute.MetaDataRow.Parts[0] = GetMemberIndex(workspace, Constructor.OriginalAssembly.NETHeader.TablesHeap.HasCustomAttribute, customAttribute.Parent);
            customAttribute.MetaDataRow.Parts[1] = GetMemberIndex(workspace, Constructor.OriginalAssembly.NETHeader.TablesHeap.CustomAttributeType, customAttribute.Constructor);
            // TODO: serialize blob.
        }

        private void UpdateFieldMarshal(Workspace workspace, FieldMarshal marshal)
        {
            marshal.MetaDataRow.Parts[0] = GetMemberIndex(workspace, Constructor.OriginalAssembly.NETHeader.TablesHeap.HasFieldMarshall, marshal.Parent);
            // TODO: serialize blob.
        }

        private void UpdateSecurityDecl(Workspace workspace, SecurityDeclaration securityDecl)
        {
            securityDecl.MetaDataRow.Parts[1] = GetMemberIndex(workspace, Constructor.OriginalAssembly.NETHeader.TablesHeap.HasDeclSecurity, securityDecl.Parent);
            // TODO: serialize blob.
        }

        private void UpdateClassLayout(Workspace workspace, ClassLayout classLayout)
        {
            classLayout.MetaDataRow.Parts[2] = GetMemberIndex(workspace, classLayout.Parent);
        }

        private void UpdateFieldLayout(Workspace workspace, FieldLayout fieldLayout)
        {
            fieldLayout.MetaDataRow.Parts[1] = GetMemberIndex(workspace, fieldLayout.Field);
        }

        private void UpdateStandAloneSig(Workspace workspace, StandAloneSignature signature)
        {
            // TODO: serialize blob.
        }

        private void UpdateEventMap(Workspace workspace, EventMap map)
        {
            map.MetaDataRow.Parts[0] = GetMemberIndex(workspace, map.Parent);
            // event list updated by MetaDataBuilder class
        }

        private void UpdateEventDef(Workspace workspace, EventDefinition eventDef)
        {
            eventDef.MetaDataRow.Parts[1] = GetStringIndex(workspace, eventDef.Name);
            eventDef.MetaDataRow.Parts[2] = GetMemberIndex(workspace, Constructor.OriginalAssembly.NETHeader.TablesHeap.TypeDefOrRef, eventDef.EventType);
        }

        private void UpdatePropertyMap(Workspace workspace, PropertyMap map)
        {
            map.MetaDataRow.Parts[0] = GetMemberIndex(workspace, map.Parent);
            // event list updated by MetaDataBuilder class
        }

        private void UpdatePropertyPtr(Workspace workspace, PropertyPtr propertyPtr)
        {
            // TODO: serialize blob.
        }

        private void UpdatePropertyDef(Workspace workspace, PropertyDefinition propertyDef)
        {
            propertyDef.MetaDataRow.Parts[1] = GetStringIndex(workspace, propertyDef.Name);
            // TODO: serialize blob.
        }

        private void UpdateMethodSemantics(Workspace workspace, MethodSemantics semantics)
        {
            semantics.MetaDataRow.Parts[1] = GetMemberIndex(workspace, semantics.Method);
            semantics.MetaDataRow.Parts[2] = GetMemberIndex(workspace, Constructor.OriginalAssembly.NETHeader.TablesHeap.HasSemantics, semantics.Association);
        }

        private void UpdateMethodImpl(Workspace workspace, MethodImplementation methodImpl)
        {
            methodImpl.MetaDataRow.Parts[0] = GetMemberIndex(workspace, methodImpl.Class);
            methodImpl.MetaDataRow.Parts[1] = GetMemberIndex(workspace, Constructor.OriginalAssembly.NETHeader.TablesHeap.MethodDefOrRef, methodImpl.MethodBody);
            methodImpl.MetaDataRow.Parts[1] = GetMemberIndex(workspace, Constructor.OriginalAssembly.NETHeader.TablesHeap.MethodDefOrRef, methodImpl.MethodDeclaration);
        }

        private void UpdateModuleRef(Workspace workspace, ModuleReference moduleRef)
        {
            moduleRef.MetaDataRow.Parts[0] = GetStringIndex(workspace, moduleRef.Name);
        }

        private void UpdateTypeSpec(Workspace workspace, TypeSpecification typeSpec)
        {
            // TODO: serialize blob.
        }

        private void UpdateMethodSpec(Workspace workspace, MethodSpecification methodSpec)
        {
            methodSpec.MetaDataRow.Parts[0] = GetMemberIndex(workspace, Constructor.OriginalAssembly.NETHeader.TablesHeap.MethodDefOrRef, methodSpec.OriginalMethod);
            // TODO: serialize blob.
        }

        private void UpdatePInvokeImpl(Workspace workspace, PInvokeImplementation pinvokeImpl)
        {
            pinvokeImpl.MetaDataRow.Parts[1] = GetMemberIndex(workspace, Constructor.OriginalAssembly.NETHeader.TablesHeap.MemberForwarded, pinvokeImpl.Member);
            pinvokeImpl.MetaDataRow.Parts[2] = GetStringIndex(workspace, pinvokeImpl.Entrypoint);
            pinvokeImpl.MetaDataRow.Parts[3] = GetMemberIndex(workspace, pinvokeImpl.ImportScope);
        }

        private void UpdateFieldRva(Workspace workspace, FieldRVA fieldRva)
        {
            // TODO: update rva
            fieldRva.MetaDataRow.Parts[1] = GetMemberIndex(workspace, fieldRva.Field);
        }

        private void UpdateAssemblyDef(Workspace workspace, AssemblyDefinition asmDef)
        {
            // TODO: update public key token (blob)
            asmDef.MetaDataRow.Parts[7] = GetStringIndex(workspace, asmDef.Name);
            asmDef.MetaDataRow.Parts[8] = GetStringIndex(workspace, asmDef.Culture);
        }

        private void UpdateAssemblyRef(Workspace workspace, AssemblyReference asmRef)
        {
            // TODO: update public key token (blob)
            asmRef.MetaDataRow.Parts[6] = GetStringIndex(workspace, asmRef.Name);
            asmRef.MetaDataRow.Parts[7] = GetStringIndex(workspace, asmRef.Culture );
            // TODO: serialize blob (hash value).
        }

        // TODO: Rest of the tables.
    }
}
