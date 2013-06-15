using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class FieldDefinition : FieldReference
    {
        Constant constant;

        public FieldDefinition(MetaDataRow row)
            : base(row)
        {
        }

        public FieldDefinition(string name, FieldAttributes attributes, uint signature)
            : base(new MetaDataRow((ushort)attributes, 0U, signature))
        {
            this.name = name;
        }

        public FieldAttributes Attributes
        {
            get { return (FieldAttributes)metadatarow.parts[0]; }
            set { metadatarow.parts[0] = (ushort)value; }
        }

        public override TypeReference DeclaringType
        {
            get
            {
                if (declaringType == null)
                {
                    foreach (var member in netheader.TablesHeap.GetTable(MetaDataTableType.TypeDef).Members)
                    {
                        TypeDefinition typeDef = member as TypeDefinition;
                        if (typeDef.Fields != null)
                            foreach (FieldDefinition field in typeDef.Fields)
                                if (field.metadatatoken == this.metadatatoken)
                                {
                                    declaringType = typeDef;
                                    break;
                                }
                    }
                }

                return declaringType;
            }
        }

        public Constant Constant
        {
            get
            {
                if (constant == null && Attributes.HasFlag(FieldAttributes.Literal) && netheader.TablesHeap.HasTable(MetaDataTableType.Constant))
                    foreach (var member in netheader.TablesHeap.GetTable(MetaDataTableType.Constant).Members)
                    {
                        Constant c = (Constant)member;
                        if (c.Parent != null && c.Parent.metadatatoken == this.metadatatoken)
                        {
                            constant = (Constant)member;
                            break;
                        }
                    }
                return constant;
            }
        }

        public override void LoadCache()
        {
            base.LoadCache();
            constant = Constant;
        }

    }
}
