using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class TypeDefinition : TypeReference
    {
        private MemberRange<MethodDefinition> _methodRange = null;
        private MemberRange<FieldDefinition> _fieldRange = null;
        private PropertyMap _propertyMap = null;
        private EventMap _eventMap = null;
        private NestedClass[] _nestedClasses = null;
        private InterfaceImplementation[] _interfaces = null;
        private TypeDefinition _decltype = null;
        private GenericParameter[] _genericparams = null;
        private TypeReference _baseType = null;

        public TypeDefinition(MetaDataRow row)
            : base(row)
        {
        }

        public TypeDefinition(string @namespace, string name, TypeAttributes attributes, TypeReference baseType, uint fieldList, uint methodList)
            : base(new MetaDataRow((uint)attributes, 0U, 0U, 0U, fieldList, methodList))
        {
            this._namespace = @namespace;
            this._name = name;
            this._baseType = baseType;
        }

        public TypeAttributes Attributes
        {
            get{return (TypeAttributes)_metadatarow._parts[0];}
            set { _metadatarow._parts[0] = (uint)value; }
        }

        public override string Name
        {
            get { 
                if (string.IsNullOrEmpty(_name))
                    _netheader.StringsHeap.TryGetStringByOffset(Convert.ToUInt32(_metadatarow._parts[1]), out _name);
                return _name;
            }
        }

        public override string Namespace
        {
            get {
                if (string.IsNullOrEmpty(_namespace))
                    _netheader.StringsHeap.TryGetStringByOffset(Convert.ToUInt32(_metadatarow._parts[2]), out _namespace);
                return _namespace;
            }
        }

        public override IResolutionScope ResolutionScope
        {
            get
            {
                if (_resolutionScope == null && HasImage)
                {
                    _resolutionScope = NETHeader.TablesHeap.GetTable(MetaDataTableType.Assembly, false).Members[0] as IResolutionScope;
                }
                return _resolutionScope;
            }
        }

        public override TypeReference DeclaringType
        {
            get
            {
                if (_decltype == null && _netheader.TablesHeap.HasTable(MetaDataTableType.NestedClass))
                {
                    MetaDataTable nestedClassTable = _netheader.TablesHeap.GetTable(MetaDataTableType.NestedClass);
                    foreach (MetaDataMember member in nestedClassTable.Members)
                    {
                        NestedClass nestedclass = (NestedClass)member;
                        if (nestedclass.Class != null && nestedclass.EnclosingClass != null)
                            if (nestedclass.Class._metadatatoken == this._metadatatoken)
                            {
                                _decltype = nestedclass.EnclosingClass;
                                break;
                            }
                    }
                }
                return _decltype;
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
                if (_baseType == null)
                {
                    if (Convert.ToInt32(_metadatarow._parts[3]) == 0)
                        return null;
                    _netheader.TablesHeap.TypeDefOrRef.TryGetMember(Convert.ToInt32(_metadatarow._parts[3]), out _baseType);
                }
                return _baseType;
            }
        }

        public uint FieldList
        {
            get { return Convert.ToUInt32(_metadatarow._parts[4]); }
        }

        public uint MethodList
        {
            get { return Convert.ToUInt32(_metadatarow._parts[5]); }
        }

        public FieldDefinition[] Fields
        {
            get 
            {
                if (_fieldRange == null && _netheader.TablesHeap.HasTable(MetaDataTableType.Field))
                    _fieldRange = MemberRange.CreateRange<FieldDefinition>(this, 4, NETHeader.TablesHeap.GetTable(MetaDataTableType.Field, false));

                return _fieldRange != null ? _fieldRange.Members : null;
            }
        }

        public MethodDefinition[] Methods
        {
            get 
            {
                if (_methodRange == null && _netheader.TablesHeap.HasTable(MetaDataTableType.Method))
                    _methodRange = MemberRange.CreateRange<MethodDefinition>(this, 5, NETHeader.TablesHeap.GetTable(MetaDataTableType.Method, false));

                return _methodRange != null ? _methodRange.Members : null;
            }
        }

        public InterfaceImplementation[] Interfaces
        {
            get
            {
                if (this._interfaces == null && _netheader.TablesHeap.HasTable(MetaDataTableType.InterfaceImpl))
                {
                    List<InterfaceImplementation> interfaces = new List<InterfaceImplementation>();

                    foreach (MetaDataMember member in _netheader.TablesHeap.GetTable(MetaDataTableType.InterfaceImpl).Members)
                    {
                        InterfaceImplementation @interface = (InterfaceImplementation)member;

                        if (@interface.Class != null && @interface.Class._metadatatoken == this._metadatatoken)
                            interfaces.Add(@interface);

                    }

                    this._interfaces = interfaces.ToArray();
                }

                return this._interfaces;
            }
        }

        public PropertyMap PropertyMap
        {
            get
            {
                if (_propertyMap == null && _netheader.TablesHeap.HasTable(MetaDataTableType.PropertyMap))
                {
                    foreach (MetaDataMember member in _netheader.TablesHeap.GetTable(MetaDataTableType.PropertyMap).Members)
                    {
                        PropertyMap prprtymap = (PropertyMap)member;
                        if (prprtymap.Parent != null && (prprtymap.Parent._metadatatoken == this._metadatatoken))
                        {
                            _propertyMap = prprtymap;
                            break;
                        }
                    }
                }

                return _propertyMap;

            }
        }

        public EventMap EventMap
        {
            get
            {
                if (_eventMap == null && _netheader.TablesHeap.HasTable(MetaDataTableType.EventMap))
                {
                    foreach (MetaDataMember member in _netheader.TablesHeap.GetTable(MetaDataTableType.EventMap).Members)
                    {
                        EventMap emap = (EventMap)member;
                        if (emap.Parent != null && (emap.Parent._metadatatoken == this._metadatatoken))
                        {
                            _eventMap = emap;
                            break;
                        }
                    }
                }
                return _eventMap;
            }
        }

        public NestedClass[] NestedClasses
        {
            get
            {
                if (_nestedClasses == null && _netheader.TablesHeap.HasTable(MetaDataTableType.NestedClass) )
                {
                    List<NestedClass> nestedclasses = new List<NestedClass>();

                    foreach (MetaDataMember member in _netheader.TablesHeap.GetTable(MetaDataTableType.NestedClass).Members)
                    {
                        NestedClass nestedType = (NestedClass)member;
                        if (nestedType.EnclosingClass != null && nestedType.EnclosingClass._metadatatoken == this._metadatatoken)
                            nestedclasses.Add(nestedType);
                    }

                    _nestedClasses = nestedclasses.Count == 0 ? null : nestedclasses.ToArray();
                }
                return _nestedClasses;
            }
        }

        //public override bool IsNested
        //{
        //    get
        //    {
        //        return Attributes.HasFlag(TypeAttributes.NestedAssembly) || Attributes.HasFlag(TypeAttributes.NestedFamANDAssem) || Attributes.HasFlag(TypeAttributes.NestedFamily) || Attributes.HasFlag(TypeAttributes.NestedFamORAssem) || Attributes.HasFlag(TypeAttributes.NestedPrivate) || Attributes.HasFlag(TypeAttributes.NestedPublic);
        //    }
        //}

        public override GenericParameter[] GenericParameters
        {
            get
            {
                if (_genericparams == null && _netheader.TablesHeap.HasTable(MetaDataTableType.GenericParam))
                {
                    List<GenericParameter> generics = new List<GenericParameter>();

                    foreach (MetaDataMember member in _netheader.TablesHeap.GetTable(MetaDataTableType.GenericParam).Members)
                    {
                        GenericParameter genericParam = member as GenericParameter;
                        if (genericParam.Owner != null && genericParam.Owner.MetaDataToken == this._metadatatoken)
                        {
                            generics.Insert(genericParam.Index, genericParam);
                        }

                    }

                    _genericparams = generics.ToArray();

                }
                return _genericparams;
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

        public override TypeDefinition Resolve()
        {
            return this;
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

        public override void ClearCache()
        {
            base.ClearCache();
            _methodRange = null;
            _fieldRange = null;
            _propertyMap = null;
            _eventMap = null;
            _nestedClasses = null;
            _interfaces = null;
            _decltype = null;
            _genericparams = null;
            _baseType = null;
        }

        public override void LoadCache()
        {
            base.LoadCache();
            _methodRange = MemberRange.CreateRange<MethodDefinition>(this, 5, NETHeader.TablesHeap.GetTable(MetaDataTableType.Method, false));
            _methodRange.LoadCache();
            _fieldRange = MemberRange.CreateRange<FieldDefinition>(this, 4, NETHeader.TablesHeap.GetTable(MetaDataTableType.Field, false));
            _fieldRange.LoadCache();
            _propertyMap = PropertyMap;
            _eventMap = EventMap;
            _nestedClasses = NestedClasses;
            _interfaces = Interfaces;
            _decltype = DeclaringType as TypeDefinition;
            _genericparams = GenericParameters;
            _baseType = BaseType;
        }
    }
}
