using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class TypeDefinition : TypeReference
    {
        private MethodDefinition[] methods = null;
        private FieldDefinition[] fields = null;
        private PropertyMap propertymap = null;
        private EventMap eventmap = null;
        private NestedClass[] nestedClasses = null;
        private InterfaceImplementation[] interfaces = null;
        private TypeDefinition decltype = null;
        private GenericParameter[] genericparams = null;
        private string fullname = string.Empty;
        private TypeReference baseType = null;

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
        public override string FullName
        {
            get
            {
                if (fullname == string.Empty)
                {
                    
                    StringBuilder builder = new StringBuilder();
                    TypeReference last = this;
                    TypeReference declaringType = this.DeclaringType;
                    while (declaringType != null)
                    {
                        if (!declaringType.IsDefinition)
                            break;
                        builder.Insert(0, declaringType.Name + "/");
                        last = declaringType;
                        declaringType = ((TypeDefinition)declaringType).DeclaringType;
                    }
                    builder.Insert(0, last.Namespace + ".");
                    builder.Append(this.Name);
                    fullname = builder.ToString();
                    builder.Clear();
                }
                return fullname;
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

        public override MetaDataMember ResolutionScope
        {
            get
            {
                return null;
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
            get {

                if (this.fields != null || !netheader.TablesHeap.HasTable(MetaDataTableType.Field))
                    return this.fields;

                int fieldlist = Convert.ToInt32(metadatarow.parts[4]);

                int nextfieldlist = -1;

                if (netheader.TablesHeap.GetTable(MetaDataTableType.TypeDef).members.Last().metadatatoken != this.metadatatoken)
                    nextfieldlist = Convert.ToInt32(netheader.TokenResolver.ResolveMember(this.MetaDataToken + 1).metadatarow.parts[4]);


                MetaDataTable fieldTable = netheader.TablesHeap.GetTable(MetaDataTableType.Field);

                int length = -1;
                if (nextfieldlist != -1)
                    length = nextfieldlist - fieldlist;
                else
                    length = fieldTable.members.Count - (fieldlist - 1);

                if (length > 0)
                {

                    FieldDefinition[] fields = new FieldDefinition[length];
                    for (int i = 0; i < fields.Length; i++)
                    {
                        int index = fieldlist + i - 1;
                        if (index >= 0 && index < fieldTable.members.Count)
                            fields[i] = (FieldDefinition)fieldTable.members[fieldlist + i - 1];
                    }

                    this.fields = fields;
                    return fields;
                }

                return null;

                //return Convert.ToUInt32(metadatarow.parts[4]); 
            
            }
        }

        public MethodDefinition[] Methods
        {
            get 
            {

                if (this.methods != null || !netheader.TablesHeap.HasTable(MetaDataTableType.Method))
                    return this.methods;

                int methodlist = Convert.ToInt32(metadatarow.parts[5]);

                int nextmethodlist = -1;

                if (netheader.TablesHeap.GetTable(MetaDataTableType.TypeDef).members.Last().metadatatoken != this.metadatatoken)
                    nextmethodlist = Convert.ToInt32(netheader.TokenResolver.ResolveMember(this.MetaDataToken + 1).metadatarow.parts[5]);


                MetaDataTable methodTable = netheader.TablesHeap.GetTable(MetaDataTableType.Method);
                int length = -1;

                if (nextmethodlist != -1)
                    length = nextmethodlist - methodlist;
                else
                    length = methodTable.members.Count - (methodlist - 1);

                if (length > 0)
                {
                    MethodDefinition[] methods = new MethodDefinition[length];
                    for (int i = 0; i < methods.Length; i++)
                    {
                        int index = methodlist + i - 1;
                        if (index >= 0 && index < methodTable.members.Count )
                            methods[i] = (MethodDefinition)methodTable.members[methodlist + i - 1];
                        
                    }

                    this.methods = methods;
                    return methods;
                }
                return null;

                // return Convert.ToUInt32(metadatarow.parts[5]); 
            
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
