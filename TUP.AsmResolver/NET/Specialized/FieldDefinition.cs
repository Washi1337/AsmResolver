using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class FieldDefinition : FieldReference
    {
        Constant _constant;

        public FieldDefinition(MetaDataRow row)
            : base(row)
        {
        }

        public FieldDefinition(string name, FieldAttributes attributes, uint signature)
            : base(new MetaDataRow((ushort)attributes, 0U, signature))
        {
            this._name = name;
        }

        public FieldAttributes Attributes
        {
            get { return (FieldAttributes)_metadatarow._parts[0]; }
            set { _metadatarow._parts[0] = (ushort)value; }
        }

        public override TypeReference DeclaringType
        {
            get
            {
                if (_declaringType == null)
                {
                    foreach (var member in _netheader.TablesHeap.GetTable(MetaDataTableType.TypeDef).Members)
                    {
                        TypeDefinition typeDef = member as TypeDefinition;
                        if (typeDef.Fields != null)
                            foreach (FieldDefinition field in typeDef.Fields)
                                if (field._metadatatoken == this._metadatatoken)
                                {
                                    _declaringType = typeDef;
                                    break;
                                }
                    }
                }

                return _declaringType;
            }
        }

        public Constant Constant
        {
            get
            {
                if (_constant == null && Attributes.HasFlag(FieldAttributes.Literal) && _netheader.TablesHeap.HasTable(MetaDataTableType.Constant))
                    foreach (var member in _netheader.TablesHeap.GetTable(MetaDataTableType.Constant).Members)
                    {
                        Constant c = (Constant)member;
                        if (c.Parent != null && c.Parent._metadatatoken == this._metadatatoken)
                        {
                            _constant = (Constant)member;
                            break;
                        }
                    }
                return _constant;
            }
        }

        public override bool IsDefinition
        {
            get
            {
                return true;
            }
        }

        public override FieldDefinition Resolve()
        {
            return this;
        }

        public override void LoadCache()
        {
            base.LoadCache();
            _constant = Constant;
        }

    }
}
