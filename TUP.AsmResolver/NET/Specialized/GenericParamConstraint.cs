using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class GenericParamConstraint : MetaDataMember
    {
        private GenericParameter owner;
        private TypeReference constraint;

        public GenericParamConstraint(MetaDataRow row)
            : base(row)
        {
        }

        public GenericParamConstraint(GenericParameter owner, TypeReference constraint)
            : base(new MetaDataRow(owner.TableIndex, 0U))
        {
            this.owner = owner;
            this.constraint = constraint;
        }

        public GenericParameter Owner
        {
            get
            {
                if (owner == null)
                {
                    MetaDataTable table = netheader.TablesHeap.GetTable(MetaDataTableType.GenericParam);
                    int index = Convert.ToInt32(metadatarow.parts[0]) - 1;
                    if (index > 0 || index < table.Members.Length)
                        owner = table.Members[index] as GenericParameter;
                }
                return owner;
            }
        }
        public TypeReference Constraint
        {
            get
            {
                if (constraint == null)
                {
                    netheader.TablesHeap.TypeDefOrRef.TryGetMember(Convert.ToInt32(metadatarow.parts[1]), out constraint);

                }
                return constraint;
            }
        }

        public override void ClearCache()
        {
            owner = null;
            constraint = null;
        }

        public override void LoadCache()
        {
            owner = Owner;
            constraint = Constraint;
        }
    }
}
