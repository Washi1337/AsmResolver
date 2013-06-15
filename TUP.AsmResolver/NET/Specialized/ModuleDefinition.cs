using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class ModuleDefinition : ModuleReference
    {
        string name = null;
        Guid mvid = default(Guid);
        CustomAttribute[] customAttributes = null;

        public ModuleDefinition(MetaDataRow row)
            : base(row)
        {
        }

        public ModuleDefinition(string name, Guid mvid)
            : base(new MetaDataRow(0U, 0U, 0U, 0U, 0U))
        {
            this.name = name;
            this.mvid = mvid;
        }

        public override string Name
        {
            get
            {
                if (string.IsNullOrEmpty(name))
                    netheader.StringsHeap.TryGetStringByOffset(Convert.ToUInt32(metadatarow.parts[1]), out name);
                return name;
            }
        }

        public virtual CustomAttribute[] CustomAttributes
        {
            get
            {
                if (customAttributes == null && netheader.TablesHeap.HasTable(MetaDataTableType.CustomAttribute))
                {
                    List<CustomAttribute> customattributes = new List<CustomAttribute>();

                    foreach (var member in netheader.TablesHeap.GetTable(MetaDataTableType.CustomAttribute).Members)
                    {
                        CustomAttribute attribute = member as CustomAttribute;
                        if (attribute.Parent != null && attribute.Parent.metadatatoken == this.metadatatoken)
                            customattributes.Add(attribute);
                    }

                    customAttributes = customattributes.ToArray();
                }
                return customAttributes;
            }
        }

        public Guid Mvid
        {
            get
            {
                if (mvid == default(Guid))
                    mvid = netheader.GuidHeap.GetGuidByOffset(Convert.ToUInt32(metadatarow.parts[2]));
                return mvid;
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public override void ClearCache()
        {
            name = null;
            mvid = default(Guid);
            customAttributes = null;
        }

        public override void LoadCache()
        {
            base.LoadCache();
            name = Name;
            mvid = Mvid;
            customAttributes = CustomAttributes;
        }
    }
}
