using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class PropertyMap : MetaDataMember
    {
        MemberRange<PropertyDefinition> propertyRange;
        TypeDefinition parent;

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
                if (parent == null)
                {
                    MetaDataTable table = netheader.TablesHeap.GetTable(MetaDataTableType.TypeDef);
                    int index = Convert.ToInt32(metadatarow.parts[0]) - 1;
                    if (index >= 0 && index < table.Members.Length)
                        parent = table.Members[index] as TypeDefinition;
                }
                return parent;
            }
        }
        public PropertyDefinition[] Properties
        {
            get
            {
                if (propertyRange == null)
                {
                    propertyRange = MemberRange.CreateRange<PropertyDefinition>(this, 1, NETHeader.TablesHeap.GetTable(MetaDataTableType.Property, false));

                }
                return propertyRange.Members;

                //return Convert.ToUInt32(metadatarow.parts[1]); 
            }
        }

        public bool HasProperties
        {
            get { return Properties != null && Properties.Length > 0; }
        }

        public override void ClearCache()
        {
            propertyRange = null;
            parent = null;
        }

        public override void LoadCache()
        {
            propertyRange = MemberRange.CreateRange<PropertyDefinition>(this, 1, NETHeader.TablesHeap.GetTable(MetaDataTableType.Property, false));
            parent = Parent;
        }
    }
}
