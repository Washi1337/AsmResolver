using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class NestedClass : MetaDataMember
    {
        private TypeDefinition _class;
        private TypeDefinition _enclosingClass;

        public NestedClass(MetaDataRow row)
            : base(row)
        {
        }

        public NestedClass(TypeDefinition nestedClass, TypeDefinition enclosingClass)
            : base(new MetaDataRow(nestedClass.TableIndex, enclosingClass.TableIndex))
        {
            this._class = nestedClass;
            this._enclosingClass = enclosingClass;
        }

        public TypeDefinition Class
        {
            get {
                if (_class == null)
                {
                    MetaDataTable table = _netheader.TablesHeap.GetTable( MetaDataTableType.TypeDef);
                    int index = Convert.ToInt32(_metadatarow._parts[0]) - 1;
                    if (index >= 0 && index < table.Members.Length)
                        _class = (TypeDefinition)table.Members[index];
                }
                return _class;
            }
            set {
                _metadatarow._parts[0] = ProcessPartType(0, Array.IndexOf(_netheader.TablesHeap.GetTable( MetaDataTableType.TypeDef).Members, value));
                _class = value; 
            }
        }

        public TypeDefinition EnclosingClass
        {
            get
            {
                if (_enclosingClass == null)
                {
                    MetaDataTable table = _netheader.TablesHeap.GetTable( MetaDataTableType.TypeDef);
                    int index = Convert.ToInt32(_metadatarow._parts[1]) - 1;
                    if (index >= 0 && index < table.Members.Length)
                        _enclosingClass = (TypeDefinition)table.Members[index];
                }
                return _enclosingClass;
            }
            set { 
                _metadatarow._parts[1] = ProcessPartType(1, Array.IndexOf(_netheader.TablesHeap.GetTable( MetaDataTableType.TypeDef).Members, value));
                _enclosingClass = value;
            }
       
        }

        public override string ToString()
        {
            return Class.Name + " -> " + EnclosingClass.FullName;
        }

        public override void ClearCache()
        {
            _class = null;
            _enclosingClass = null;
        }

        public override void LoadCache()
        {
            _class = Class;
            _enclosingClass = EnclosingClass;
        }
    }
}
