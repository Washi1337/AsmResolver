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
        public TypeDefinition Class
        {
            get {
                if (@class == null)
                {
                    MetaDataTable table = netheader.TablesHeap.GetTable( MetaDataTableType.TypeDef);
                    int index = Convert.ToInt32(metadatarow.parts[0]) - 1;
                    if (index >= 0 && index < table.members.Count)
                        @class = (TypeDefinition)table.members[index];
                }
                return @class;
            }
            set {
                metadatarow.parts[0] = ProcessPartType(0, netheader.TablesHeap.GetTable( MetaDataTableType.TypeDef).members.IndexOf(value));
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
                    if (index >= 0 && index < table.members.Count)
                        enclosingClass = (TypeDefinition)table.members[index];
                }
                return enclosingClass;
            }
            set { 
                metadatarow.parts[1] = ProcessPartType(1, netheader.TablesHeap.GetTable( MetaDataTableType.TypeDef).members.IndexOf(value));
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
