using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class MethodSemantics : MetaDataMember 
    {
        MethodDefinition method;
        MetaDataMember association;

        public MethodSemantics(MetaDataRow row)
            : base(row)
        {
        }

        public MethodSemantics(MethodDefinition method, MetaDataMember association, MethodSemanticsAttributes attributes)
            : base(new MetaDataRow((uint)attributes, method.TableIndex, (uint)0))
        {
            this.method = method;
            this.association = association;
        }

        public MethodSemanticsAttributes Attributes
        {
            get { return (MethodSemanticsAttributes)Convert.ToUInt16(metadatarow._parts[0]); }
        }

        public MethodDefinition Method
        {
            get 
            {
                if (method == null)
                {
                    MetaDataTable methodtable = netheader.TablesHeap.GetTable(MetaDataTableType.Method);
                    int index = Convert.ToInt32(metadatarow._parts[1]) - 1;
                    if (index >= 0 && index < methodtable.Members.Length)
                        method = (MethodDefinition)methodtable.Members[index];
                }
                return method;
            }
        }

        public MetaDataMember Association
        {
            get
            {
                if (association == null)
                    netheader.TablesHeap.HasSemantics.TryGetMember(Convert.ToInt32(metadatarow._parts[2]), out association);
                return association;
            }
        }

        public override void ClearCache()
        {
            method = null;
            association = null;
        }

        public override void LoadCache()
        {
            method = Method;
            association = Association;
        }

    }
}
