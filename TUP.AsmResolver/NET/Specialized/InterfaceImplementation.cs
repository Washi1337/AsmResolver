using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class InterfaceImplementation : MetaDataMember
    {
        TypeDefinition _class = null;
        TypeReference _interface = null;

        public InterfaceImplementation(MetaDataRow row)
            : base(row)
        {
        }

        public InterfaceImplementation(TypeDefinition @class, TypeReference @interface)
            : base(new MetaDataRow(@class.TableIndex, 0U))
        {
            this._class = @class;
            this._interface = @interface;
        }
    
        public TypeDefinition Class
        {
            get
            {
                if (_class == null)
                {
                    int token = Convert.ToInt32(_metadatarow._parts[0]);
                    _netheader.TablesHeap.GetTable(MetaDataTableType.TypeDef).TryGetMember(token, out _class);
                }
                return _class;
            }
        }

        public TypeReference Interface
        {
            get
            {
                if (_interface == null)
                    _netheader.TablesHeap.TypeDefOrRef.TryGetMember(Convert.ToInt32(_metadatarow._parts[1]), out _interface);
                return _interface;
            }
        }

        public override string ToString()
        {
            return Interface.ToString();
        }

        public override void ClearCache()
        {
            _class = null;
            _interface = null;
        }

        public override void LoadCache()
        {
            _class = Class;
            _interface = Interface;
        }
    }
}
