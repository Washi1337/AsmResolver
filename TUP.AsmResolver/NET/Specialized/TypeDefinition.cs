using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class TypeDefinition : TypeReference
    {
        private MemberRange<MethodDefinition> methodRange = null;
        private MemberRange<FieldDefinition> fieldRange = null;
        private PropertyMap propertymap = null;
        private EventMap eventmap = null;
        private NestedClass[] nestedClasses = null;
        private InterfaceImplementation[] interfaces = null;
        private TypeDefinition decltype = null;
        private GenericParameter[] genericparams = null;
        private string fullname = string.Empty;
        private TypeReference baseType = null;
        private IResolutionScope resolutionScope = null;

        public TypeDefinition(MetaDataRow row)
            : base(row)
        {
        }

        public TypeDefinition(string @namespace, string name, TypeAttributes attributes, TypeReference baseType, uint fieldList, uint methodList)
            : base(new MetaDataRow((uint)attributes, 0U, 0U, 0U, fieldList, methodList))
        {
            this.@namespace = @namespace;
            this.name = name;
            this.baseType = baseType;
        }

        public TypeAttributes Attributes
        {
            get{return (TypeAttributes)metadatarow.parts[0];}
            set { metadatarow.parts[0] = (uint)value; }
        }

        public override string Name
        {
            get { 
                if (name == string.Empty)
                    name = netheader.StringsHeap.GetStringByOffset(Convert.ToUInt32(metadatarow.parts[1]));
                return name;
            }
        }

        public override string Namespace
        {
            get {
                if (@namespace == string.Empty)
                    @namespace = netheader.StringsHeap.GetStringByOffset(Convert.ToUInt32(metadatarow.parts[2]));
                return @namespace;
            }
        }

        public override IResolutionScope ResolutionScope
        {
            get
            {
                if (resolutionScope == null && HasImage)
                {
                    resolutionScope = NETHeader.TablesHeap.GetTable(MetaDataTableType.Assembly, false).Members[0] as IResolutionScope;
                }
                return resolutionScope;
            }
        }

        public override TypeReference DeclaringType
        {
            get
            {
                if (decltype == null && netheader.TablesHeap.HasTable(MetaDataTableType.NestedClass))
                {
                    MetaDataTable nestedClassTable = netheader.TablesHeap.GetTable(MetaDataTableType.NestedClass);
                    foreach (MetaDataMember member in nestedClassTable.members)
                    {
                        NestedClass nestedclass = (NestedClass)member;
                        if (nestedclass.Class != null && nestedclass.EnclosingClass != null)
                            if (nestedclass.Class.metadatatoken == this.metadatatoken)
                            {
                                decltype = nestedclass.EnclosingClass;
                                break;
                            }
                    }
                }
                return decltype;
            }
        }

        public override bool IsDefinition
        {
            get
            {
                return true;
            }
        }

        public TypeReference BaseType
        {
            get {
                if (baseType == null)
                {
                    if (Convert.ToInt32(metadatarow.parts[3]) == 0)
                        return null;
                    baseType = (TypeReference)netheader.TablesHeap.TypeDefOrRef.GetMember(Convert.ToInt32(metadatarow.parts[3]));
                }
                return baseType;
            }
        }

        public uint FieldList
        {
            get { return Convert.ToUInt32(metadatarow.parts[4]); }
        }

        public uint MethodList
        {
            get { return Convert.ToUInt32(metadatarow.parts[5]); }
        }

        public FieldDefinition[] Fields
        {
            get 
            {
                if (fieldRange == null)
                    fieldRange = MemberRange.CreateRange<FieldDefinition>(this, 4, NETHeader.TablesHeap.GetTable(MetaDataTableType.Field, false));
                return fieldRange.Members;
            }
        }

        public MethodDefinition[] Methods
        {
            get 
            {
                if (methodRange == null)
                    methodRange = MemberRange.CreateRange<MethodDefinition>(this, 5, NETHeader.TablesHeap.GetTable(MetaDataTableType.Method, false));
                return methodRange.Members;
            }
        }

        public InterfaceImplementation[] Interfaces
        {
            get
            {
                if (this.interfaces == null && netheader.TablesHeap.HasTable(MetaDataTableType.InterfaceImpl))
                {
                    List<InterfaceImplementation> interfaces = new List<InterfaceImplementation>();

                    foreach (MetaDataMember member in netheader.TablesHeap.GetTable(MetaDataTableType.InterfaceImpl).members)
                    {
                        InterfaceImplementation @interface = (InterfaceImplementation)member;

                        if (@interface.Class != null && @interface.Class.metadatatoken == this.metadatatoken)
                            interfaces.Add(@interface);

                    }

                    this.interfaces = interfaces.ToArray();
                }

                return this.interfaces;
            }
        }

        public PropertyMap PropertyMap
        {
            get
            {
                if (propertymap == null && netheader.TablesHeap.HasTable(MetaDataTableType.PropertyMap))
                {
                    foreach (MetaDataMember member in netheader.TablesHeap.GetTable(MetaDataTableType.PropertyMap).members)
                    {
                        PropertyMap prprtymap = (PropertyMap)member;
                        if (prprtymap.Parent != null && (prprtymap.Parent.metadatatoken == this.metadatatoken))
                        {
                            propertymap = prprtymap;
                            break;
                        }
                    }
                }

                return propertymap;

            }
        }

        public EventMap EventMap
        {
            get
            {
                if (eventmap == null && netheader.TablesHeap.HasTable(MetaDataTableType.EventMap))
                {
                    foreach (MetaDataMember member in netheader.TablesHeap.GetTable(MetaDataTableType.EventMap).members)
                    {
                        EventMap emap = (EventMap)member;
                        if (emap.Parent != null && (emap.Parent.metadatatoken == this.metadatatoken))
                        {
                            eventmap = emap;
                            break;
                        }
                    }
                }
                return eventmap;
            }
        }

        public NestedClass[] NestedClasses
        {
            get
            {
                if (nestedClasses == null && netheader.TablesHeap.HasTable(MetaDataTableType.NestedClass) )
                {
                    List<NestedClass> nestedclasses = new List<NestedClass>();

                    foreach (MetaDataMember member in netheader.TablesHeap.GetTable(MetaDataTableType.NestedClass).members)
                    {
                        NestedClass nestedType = (NestedClass)member;
                        if (nestedType.EnclosingClass != null && nestedType.EnclosingClass.metadatatoken == this.metadatatoken)
                            nestedclasses.Add(nestedType);
                    }

                    nestedClasses = nestedclasses.Count == 0 ? null : nestedclasses.ToArray();
                }
                return nestedClasses;
            }
        }

        public bool IsNested
        {
            get{
                return Attributes.HasFlag(TypeAttributes.NestedAssembly) || Attributes.HasFlag(TypeAttributes.NestedFamANDAssem) || Attributes.HasFlag(TypeAttributes.NestedFamily) || Attributes.HasFlag(TypeAttributes.NestedFamORAssem) || Attributes.HasFlag(TypeAttributes.NestedPrivate) || Attributes.HasFlag(TypeAttributes.NestedPublic);
            }
        }

        public override GenericParameter[] GenericParameters
        {
            get
            {
                if (genericparams == null && netheader.TablesHeap.HasTable(MetaDataTableType.GenericParam))
                {
                    List<GenericParameter> generics = new List<GenericParameter>();

                    foreach (MetaDataMember member in netheader.TablesHeap.GetTable(MetaDataTableType.GenericParam).members)
                    {
                        GenericParameter genericParam = member as GenericParameter;
                        if (genericParam.Owner != null && genericParam.Owner.metadatatoken == this.metadatatoken)
                        {
                            generics.Insert(genericParam.Index, genericParam);
                        }

                    }

                    genericparams = generics.ToArray();

                }
                return genericparams;
            }
        }

        public bool HasFields
        {
            get { return Fields != null && Fields.Length > 0; }
        }

        public bool HasMethods
        {
            get { return Methods != null && Methods.Length > 0; }
        }

        public bool HasNestedClasses
        {
            get { return NestedClasses != null && NestedClasses.Length > 0; }
        }

        public bool HasInterfaces
        {
            get { return Interfaces != null && Interfaces.Length > 0; }
        }

        public override string ToString()
        {
            return FullName;
        }

        public bool IsBasedOn(TypeReference typeRef)
        {
            return BaseType != null && BaseType.FullName == typeRef.FullName;
        }

        public bool IsBasedOn(string fullname)
        {
            return BaseType != null && BaseType.FullName == fullname;
        }
    }
}
