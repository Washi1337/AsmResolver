using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class PInvokeImplementation : MetaDataMember 
    {
        private MetaDataMember _member= null;
        private string _entrypoint = null;
        private ModuleReference _importScope = null;

        public PInvokeImplementation(MetaDataRow row)
            : base(row)
        {
        }

        public PInvokeImplementation(MetaDataMember member, PInvokeImplAttributes attributes, ModuleReference moduleRef, string entrypoint)
            : base(new MetaDataRow((uint)attributes, (uint)0, (uint)0, moduleRef.TableIndex))
        {
            this._member = member;
            this._importScope = moduleRef;
            this._entrypoint = entrypoint;
        }

        public PInvokeImplAttributes Attributes
        {
            get { return (PInvokeImplAttributes)Convert.ToUInt32(_metadatarow._parts[0]); }
        }

        public MetaDataMember Member
        {
            get
            {
                if (_member == null)
                    _netheader.TablesHeap.MemberForwarded.TryGetMember(Convert.ToInt32(_metadatarow._parts[1]), out _member);
                return _member;
            }
        }

        public string Entrypoint
        {
            get
            {
                if (string.IsNullOrEmpty(_entrypoint))
                    _netheader.StringsHeap.TryGetStringByOffset(Convert.ToUInt32(_metadatarow._parts[2]), out _entrypoint);
                return _entrypoint;
            }
        }

        public ModuleReference ImportScope
        {
            get 
            {
                if (_importScope == null)
                {
                    MetaDataTable table = _netheader.TablesHeap.GetTable(MetaDataTableType.ModuleRef);

                    _importScope = (ModuleReference)table.Members[Convert.ToInt32(_metadatarow._parts[3]) - 1];
                }
                return _importScope; 
            }
        }

        public override string ToString()
        {
            return Entrypoint;
        }

        public override void ClearCache()
        {
            _member = null;
            _entrypoint = null;
            _importScope = null;
        }

        public override void LoadCache()
        {
            _member = Member;
            _entrypoint = Entrypoint;
            _importScope = ImportScope;
        }
    }
}
