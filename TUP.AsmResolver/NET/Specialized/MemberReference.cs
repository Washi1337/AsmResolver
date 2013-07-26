using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public abstract class MemberReference : MetaDataMember
    {
        PInvokeImplementation _pinvokeimpl = null;
        CustomAttribute[] _customAttributes = null;
        internal TypeReference _declaringType = null;

        public MemberReference(MetaDataRow row)
            : base(row)
        {
        }

        public abstract string Name { get; }
        public abstract string FullName { get; }
        public virtual TypeReference DeclaringType
        {
            get
            {
                if (_declaringType == null)
                {
                    _netheader.TablesHeap.MemberRefParent.TryGetMember(Convert.ToInt32(_metadatarow._parts[0]), out _declaringType);
                }
                return _declaringType;
            }
        }

        public override string ToString()
        {
            return FullName;
        }

        public virtual CustomAttribute[] CustomAttributes
        {
            get
            {
                if (_customAttributes == null && _netheader.TablesHeap.HasTable(MetaDataTableType.CustomAttribute))
                {
                    List<CustomAttribute> customattributes = new List<CustomAttribute>();

                    foreach (var member in _netheader.TablesHeap.GetTable(MetaDataTableType.CustomAttribute).Members)
                    {
                        CustomAttribute attribute = member as CustomAttribute;
                        if (attribute.Parent != null && attribute.Parent._metadatatoken == this._metadatatoken)
                            customattributes.Add(attribute);
                    }

                    _customAttributes = customattributes.ToArray();
                }
                return _customAttributes;
            }
        }

        public PInvokeImplementation PInvokeImplementation
        {
            get
            {
                if (_pinvokeimpl == null && _netheader.TablesHeap.HasTable(MetaDataTableType.ImplMap))
                {
                    foreach (var member in _netheader.TablesHeap.GetTable(MetaDataTableType.ImplMap).Members)
                    {
                        PInvokeImplementation implementation = member as PInvokeImplementation;
                        if (implementation.Member._metadatatoken == this._metadatatoken)
                        {
                            _pinvokeimpl = implementation;
                            break;
                        }
                    }
                }
                return _pinvokeimpl;
            }
        }

        public bool HasCustomAttributes
        {
            get
            {
                return CustomAttributes != null && CustomAttributes.Length > 0;
            }
        }

        public virtual bool IsDefinition
        {
            get { return false; }
        }

        public override void ClearCache()
        {
            _customAttributes = null;
            _declaringType = null;
            _pinvokeimpl = null;
        }

        public override void LoadCache()
        {
            _pinvokeimpl = PInvokeImplementation;
            _customAttributes = CustomAttributes;
            _declaringType = DeclaringType;
        }
    }
}
