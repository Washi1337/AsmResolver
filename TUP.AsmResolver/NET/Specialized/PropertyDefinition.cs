using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class PropertyDefinition : MemberReference
    {
        private MethodDefinition _getMethod = null;
        private MethodDefinition _setMethod = null;
        private PropertySignature _propertySig = null;
        private TypeDefinition _declaringType = null;
        private string _name = null;

        public PropertyDefinition(MetaDataRow row)
            : base(row)
        {
        }

        public PropertyDefinition(string name, PropertyAttributes attributes, uint signature)
            : base(new MetaDataRow((uint)attributes, 0U, signature))
        {
            this._name = name;
        }

        public PropertyAttributes Attributes
        {
            get { return (PropertyAttributes)Convert.ToUInt16(_metadatarow._parts[0]); }
            set { _metadatarow._parts[0] = (ushort)value; }
        }

        public override string Name
        {
            get
            {
                if (string.IsNullOrEmpty(_name))
                    _netheader.StringsHeap.TryGetStringByOffset(Convert.ToUInt32(_metadatarow._parts[1]), out _name);
                return _name;
            }
        }

        public PropertySignature Signature
        {
            get
            {
                if (_propertySig == null)
                {
                    _propertySig = _netheader.BlobHeap.ReadPropertySignature(Convert.ToUInt32(_metadatarow._parts[2]), this);
                }
                return _propertySig;
            }
        }

        public MethodDefinition GetMethod
        {
            get
            {
                if (_getMethod == null)
                {
                    foreach (MetaDataMember member in _netheader.TablesHeap.GetTable(MetaDataTableType.MethodSemantics).Members)
                    {
                        MethodSemantics semantics = (MethodSemantics)member;
                        if (semantics.Association != null && semantics.Association._metadatatoken == this._metadatatoken && (semantics.Attributes & MethodSemanticsAttributes.Getter) == MethodSemanticsAttributes.Getter)
                        {
                            _getMethod = semantics.Method;
                            break;
                        }
                    }
                }
                return _getMethod;
            }
        }

        public MethodDefinition SetMethod
        {
            get
            {
                if (_setMethod == null)
                {
                    foreach (MetaDataMember member in _netheader.TablesHeap.GetTable(MetaDataTableType.MethodSemantics).Members)
                    {
                        MethodSemantics semantics = (MethodSemantics)member;
                        if (semantics.Association != null && semantics.Association._metadatatoken == this._metadatatoken && (semantics.Attributes & MethodSemanticsAttributes.Setter) == MethodSemanticsAttributes.Setter)
                        {
                            _setMethod = semantics.Method;
                            break;
                        }
                    }
                }
                return _setMethod;
            }
        }

        public override string ToString()
        {
            return Name;
        }       
        
        public override void ClearCache()
        {
            _getMethod = null;
            _setMethod = null;
            _name = null;
            _propertySig = null;
        }

        public override void LoadCache()
        {
            base.LoadCache();
            _getMethod = GetMethod;
            _setMethod = SetMethod;
            _name = Name;
            _propertySig = Signature;
        }

        public override string FullName
        {
            get
            {
                try
                {
                    if (DeclaringType is TypeReference)
                        return (Signature != null ? Signature.ReturnType.FullName + " " : "") + ((TypeReference)DeclaringType).FullName + "::" + Name;

                    return Name;
                }
                catch { return Name; }
            }
        }

        public override TypeReference DeclaringType
        {
            get 
            {
                if (_declaringType == null)
                {
                    MetaDataTable propertyMapTable = _netheader.TablesHeap.GetTable(MetaDataTableType.PropertyMap);
                    foreach (var member in propertyMapTable.Members)
                    {
                        PropertyMap propertyMap = member as PropertyMap;
                        if (propertyMap.Properties.Contains(this))
                        {
                            _declaringType = propertyMap.Parent;
                            break;
                        }
                    }
                }
                return _declaringType;
            }
        }

        
    }
}
