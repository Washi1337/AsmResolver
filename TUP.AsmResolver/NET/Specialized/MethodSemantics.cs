using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class MethodSemantics : MetaDataMember 
    {
        MethodDefinition _method;
        MetaDataMember _association;

        public MethodSemantics(MetaDataRow row)
            : base(row)
        {
        }

        public MethodSemantics(MethodDefinition method, MetaDataMember association, MethodSemanticsAttributes attributes)
            : base(new MetaDataRow((uint)attributes, method.TableIndex, (uint)0))
        {
            this._method = method;
            this._association = association;
        }

        public MethodSemanticsAttributes Attributes
        {
            get { return (MethodSemanticsAttributes)Convert.ToUInt16(_metadatarow._parts[0]); }
        }

        public MethodDefinition Method
        {
            get 
            {
                if (_method == null)
                {
                    _netheader.TablesHeap.GetTable(MetaDataTableType.Method).TryGetMember(Convert.ToInt32(_metadatarow._parts[1]), out _method);
                }
                return _method;
            }
        }

        public MetaDataMember Association
        {
            get
            {
                if (_association == null)
                    _netheader.TablesHeap.HasSemantics.TryGetMember(Convert.ToInt32(_metadatarow._parts[2]), out _association);
                return _association;
            }
        }

        public override void ClearCache()
        {
            _method = null;
            _association = null;
        }

        public override void LoadCache()
        {
            _method = Method;
            _association = Association;
        }

    }
}
