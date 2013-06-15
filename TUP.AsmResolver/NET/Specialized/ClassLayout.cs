using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class ClassLayout : MetaDataMember
    {
        TypeDefinition parent;

        public ClassLayout(MetaDataRow row)
            : base(row)
        {
        }

        public ClassLayout(TypeDefinition parent, uint classSize, ushort packingSize)
            :base(new MetaDataRow(packingSize, classSize, parent.TableIndex))
        {
            this.parent = parent;
        }

        public ushort PackingSize
        {
            get { return Convert.ToUInt16(metadatarow.parts[0]); }
        }

        public uint ClassSize
        {
            get { return Convert.ToUInt32(metadatarow.parts[1]); }
        }

        public TypeDefinition Parent
        {
            get 
            {
                if (parent == null)
                    parent = netheader.TablesHeap.GetTable(MetaDataTableType.TypeDef,false).Members[Convert.ToInt32(metadatarow.parts[2])] as TypeDefinition;
                return parent;
            }
        }

        public override void ClearCache()
        {
            this.parent = null;
        }

        public override void LoadCache()
        {
            this.parent = Parent;
        }
    }
}
