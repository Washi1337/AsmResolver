using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class NestedClass : MetaDataMember
    {
        TypeDefinition @class;
        TypeDefinition enclosingClass;

        public NestedClass(MetaDataRow row)
            : base(row)
        {
        }

        public NestedClass(TypeDefinition nestedClass, TypeDefinition enclosingClass)
            : base(new MetaDataRow(nestedClass.TableIndex, enclosingClass.TableIndex))
        {
            this.@class = nestedClass;
            this.enclosingClass = enclosingClass;
        }

        public TypeDefinition Class
        {
            get {
                if (@class == null)
                {
                    MetaDataTable table = netheader.TablesHeap.GetTable( MetaDataTableType.TypeDef);
                    int index = Convert.ToInt32(metadatarow.parts[0]) - 1;
                    if (index >= 0 && index < table.members.Length)
                        @class = (TypeDefinition)table.members[index];
                }
                return @class;
            }
            set {
                metadatarow.parts[0] = ProcessPartType(0, Array.IndexOf(netheader.TablesHeap.GetTable( MetaDataTableType.TypeDef).members, value));
                @class = value; 
            }
        }

        public TypeDefinition EnclosingClass
        {
            get
            {
                if (enclosingClass == null)
                {
                    MetaDataTable table = netheader.TablesHeap.GetTable( MetaDataTableType.TypeDef);
                    int index = Convert.ToInt32(metadatarow.parts[1]) - 1;
                    if (index >= 0 && index < table.members.Length)
                        enclosingClass = (TypeDefinition)table.members[index];
                }
                return enclosingClass;
            }
            set { 
                metadatarow.parts[1] = ProcessPartType(1, Array.IndexOf(netheader.TablesHeap.GetTable( MetaDataTableType.TypeDef).members, value));
                enclosingClass = value;
            }
       
        }

        public override string ToString()
        {
            return Class.Name + " -> " + EnclosingClass.FullName;
        }
        public override void ClearCache()
        {
            @class = null;
            enclosingClass = null;
        }
    }
}
