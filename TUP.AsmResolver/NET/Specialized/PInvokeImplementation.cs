using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class PInvokeImplementation : MetaDataMember 
    {
        MetaDataMember member=null;
        string entrypoint = null;
        ModuleReference importScope = null;

        public PInvokeImplementation(MetaDataRow row)
            : base(row)
        {
        }

        public PInvokeImplementation(MetaDataMember member, PInvokeImplAttributes attributes, ModuleReference moduleRef, string entrypoint)
            : base(new MetaDataRow((uint)attributes, (uint)0, (uint)0, moduleRef.TableIndex))
        {
            this.member = member;
            this.importScope = moduleRef;
            this.entrypoint = entrypoint;
        }

        public PInvokeImplAttributes Attributes
        {
            get { return (PInvokeImplAttributes)Convert.ToUInt32(metadatarow.parts[0]); }
        }

        public MetaDataMember Member
        {
            get
            {
                if (member == null)
                    netheader.TablesHeap.MemberForwarded.TryGetMember(Convert.ToInt32(metadatarow.parts[1]), out member);
                return member;
            }
        }

        public string Entrypoint
        {
            get
            {
                if (string.IsNullOrEmpty(entrypoint))
                    netheader.StringsHeap.TryGetStringByOffset(Convert.ToUInt32(metadatarow.parts[2]), out entrypoint);
                return entrypoint;
            }
        }

        public ModuleReference ImportScope
        {
            get 
            {
                if (importScope == null)
                {
                    MetaDataTable table = netheader.TablesHeap.GetTable(MetaDataTableType.ModuleRef);

                    importScope = (ModuleReference)table.Members[Convert.ToInt32(metadatarow.parts[3]) - 1];
                }
                return importScope; 
            }
        }

        public override string ToString()
        {
            return Entrypoint;
        }

        public override void ClearCache()
        {
            member = null;
            entrypoint = null;
            importScope = null;
        }

        public override void LoadCache()
        {
            member = Member;
            entrypoint = Entrypoint;
            importScope = ImportScope;
        }
    }
}
