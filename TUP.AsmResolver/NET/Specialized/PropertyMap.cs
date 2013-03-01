using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class PropertyMap : MetaDataMember
    {
        PropertyDefinition[] properties;
        TypeDefinition parent;
        public TypeDefinition Parent
        {
            get
            {
                if (parent == null)
                {
                    MetaDataTable table = netheader.TablesHeap.GetTable(MetaDataTableType.TypeDef);
                    int index = Convert.ToInt32(metadatarow.parts[0]) - 1;
                    if (index > 0 && index < table.members.Count)
                        parent = table.members[index] as TypeDefinition;
                }
                return parent;
            }
        }
        public PropertyDefinition[] Properties
        {
            get
            {
                if (properties == null)
                {
                    int propertylist = Convert.ToInt32(metadatarow.parts[1]);

                    int nextpropertylist = -1;

                    if (netheader.TablesHeap.GetTable( MetaDataTableType.PropertyMap).members.Last().metadatatoken != this.metadatatoken)
                        nextpropertylist = Convert.ToInt32(netheader.TokenResolver.ResolveMember(this.MetaDataToken + 1).metadatarow.parts[1]);


                    MetaDataTable propertyTable = netheader.TablesHeap.GetTable(MetaDataTableType.Property);
                    int length = -1;
                    if (nextpropertylist != -1)
                        length = nextpropertylist - propertylist;
                    else
                        length = propertyTable.members.Count - propertylist + 1;


                    if (length > -1)
                    {
                        PropertyDefinition[] propertydefs = new PropertyDefinition[length];
                        for (int i = 0; i < propertydefs.Length; i++)
                        {
                            int index = propertylist + i - 1;
                            if (index >= 0 && index < propertyTable.members.Count)
                                propertydefs[i] = (PropertyDefinition)propertyTable.members[propertylist + i - 1];
                        }
                       // int actualsize = 0;
                       // for (int i = length-1; i > 0; i--)
                       // {
                       //     if (propertydefs[i] != null)
                       //     {
                       //         actualsize = i;
                       //         break;
                       //     }
                       // }
                        // Array.Resize<PropertyDefinition>(ref propertydefs, actualsize);
                        this.properties = propertydefs;
                    }

                }
                return properties;

                //return Convert.ToUInt32(metadatarow.parts[1]); 
            }
        }

        public override void ClearCache()
        {
            properties = null;
            parent = null;
            
        }
    }
}
