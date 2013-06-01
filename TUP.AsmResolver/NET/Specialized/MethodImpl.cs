using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class MethodImplementation : MetaDataMember 
    {
        TypeDefinition @class;
        MethodReference methodBody;
        MethodReference methodDeclaration;

        public MethodImplementation(MetaDataRow row)
            : base(row)
        {
        }

        public MethodImplementation(TypeDefinition @class, MethodReference methodDeclaration, MethodReference newMethod)
            : base(new MetaDataRow(@class.TableIndex, 0U, newMethod.TableIndex))
        {
            this.@class = @class;
            this.methodDeclaration = methodDeclaration;
            this.methodBody = newMethod;
        }

        public TypeDefinition Class
        {
            get
            {
                if (@class == null)
                {
                    int index = Convert.ToInt32(metadatarow.parts[0]);
                    MetaDataTable table = netheader.TablesHeap.GetTable(MetaDataTableType.TypeDef);
                    if (index > 0 && index <= table.Members.Length)
                        @class = table.Members[index - 1] as TypeDefinition;
                }
                return @class;
            }
        }
        public MethodReference MethodBody
        {
            get
            {
                if (methodBody == null)
                    netheader.TablesHeap.MethodDefOrRef.TryGetMember(Convert.ToInt32(metadatarow.parts[1]), out methodBody);
                return methodBody;
            }
        }
        public MethodReference MethodDeclaration
        {
            get
            {
                if (methodBody == null)
                    netheader.TablesHeap.MethodDefOrRef.TryGetMember(Convert.ToInt32(metadatarow.parts[2]), out methodDeclaration);
                return methodDeclaration;
            }
        }
        public override void ClearCache()
        {
            @class = null;
            methodBody = null;
            methodDeclaration = null;
        }
    }
}
