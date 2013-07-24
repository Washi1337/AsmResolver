using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class ModuleDefinition : ModuleReference
    {
        private string _name = null;
        private Guid _mvid = default(Guid);
        private CustomAttribute[] _customAttributes = null;

        public ModuleDefinition(MetaDataRow row)
            : base(row)
        {
        }

        public ModuleDefinition(string name, Guid mvid)
            : base(new MetaDataRow(0U, 0U, 0U, 0U, 0U))
        {
            this._name = name;
            this._mvid = mvid;
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

        public Guid Mvid
        {
            get
            {
                if (_mvid == default(Guid))
                    _mvid = _netheader.GuidHeap.GetGuidByOffset(Convert.ToUInt32(_metadatarow._parts[2]));
                return _mvid;
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public override void ClearCache()
        {
            _name = null;
            _mvid = default(Guid);
            _customAttributes = null;
        }

        public override void LoadCache()
        {
            base.LoadCache();
            _name = Name;
            _mvid = Mvid;
            _customAttributes = CustomAttributes;
        }
    }
}
