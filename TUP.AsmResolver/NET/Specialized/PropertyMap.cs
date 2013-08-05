using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class PropertyMap : MetaDataMember
    {
        private MemberRange<PropertyDefinition> _propertyRange;
        private TypeDefinition _parent;

        public PropertyMap(MetaDataRow row)
            : base(row)
        {
        }

        public PropertyMap(TypeDefinition parentType, uint startingIndex)
            : base(new MetaDataRow(parentType.TableIndex, startingIndex))
        {
        }

        public TypeDefinition Parent
        {
            get
            {
                if (_parent == null)
                {
                    _netheader.TablesHeap.GetTable(MetaDataTableType.TypeDef).TryGetMember(Convert.ToInt32(_metadatarow._parts[0]), out _parent);
                }
                return _parent;
            }
        }
        public PropertyDefinition[] Properties
        {
            get
            {
                if (_propertyRange == null)
                {
                    _propertyRange = MemberRange.CreateRange<PropertyDefinition>(this, 1, NETHeader.TablesHeap.GetTable(MetaDataTableType.Property, false));

                }
                return _propertyRange.Members;

                //return Convert.ToUInt32(metadatarow.parts[1]); 
            }
        }

        public bool HasProperties
        {
            get { return Properties != null && Properties.Length > 0; }
        }

        public override void ClearCache()
        {
            _propertyRange = null;
            _parent = null;
        }

        public override void LoadCache()
        {
            _propertyRange = MemberRange.CreateRange<PropertyDefinition>(this, 1, NETHeader.TablesHeap.GetTable(MetaDataTableType.Property, false));
            _propertyRange.LoadCache();
            _parent = Parent;
        }
    }
}
