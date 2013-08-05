using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class MethodImplementation : MetaDataMember 
    {
        TypeDefinition _class;
        MethodReference _methodBody;
        MethodReference _methodDeclaration;

        public MethodImplementation(MetaDataRow row)
            : base(row)
        {
        }

        public MethodImplementation(TypeDefinition @class, MethodReference methodDeclaration, MethodReference newMethod)
            : base(new MetaDataRow(@class.TableIndex, 0U, newMethod.TableIndex))
        {
            this._class = @class;
            this._methodDeclaration = methodDeclaration;
            this._methodBody = newMethod;
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
        public MethodReference MethodBody
        {
            get
            {
                if (_methodBody == null)
                    _netheader.TablesHeap.MethodDefOrRef.TryGetMember(Convert.ToInt32(_metadatarow._parts[1]), out _methodBody);
                return _methodBody;
            }
        }
        public MethodReference MethodDeclaration
        {
            get
            {
                if (_methodBody == null)
                    _netheader.TablesHeap.MethodDefOrRef.TryGetMember(Convert.ToInt32(_metadatarow._parts[2]), out _methodDeclaration);
                return _methodDeclaration;
            }
        }

        public override void ClearCache()
        {
            _class = null;
            _methodBody = null;
            _methodDeclaration = null;
        }

        public override void LoadCache()
        {
            _class = Class;
            _methodBody = MethodBody;
            _methodDeclaration = MethodDeclaration;
        }
    }
}
