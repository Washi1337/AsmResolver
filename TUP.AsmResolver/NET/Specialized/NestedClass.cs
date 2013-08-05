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
            get 
            {
                if (_class == null)
                {
                    _netheader.TablesHeap.GetTable(MetaDataTableType.TypeDef).TryGetMember(Convert.ToInt32(_metadatarow._parts[0]), out _class);
                }
                return _class;
            }
        }

        public TypeDefinition EnclosingClass
        {
            get
            {
                if (_enclosingClass == null)
                {
                    _netheader.TablesHeap.GetTable(MetaDataTableType.TypeDef).TryGetMember(Convert.ToInt32(_metadatarow._parts[1]), out _enclosingClass);
                }
                return _enclosingClass;
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
