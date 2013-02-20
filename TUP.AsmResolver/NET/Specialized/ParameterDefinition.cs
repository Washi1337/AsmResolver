using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class ParameterDefinition : MetaDataMember
    {
        string name = null;
        public ParameterAttributes Attributes
        {
            get{return (ParameterAttributes)metadatarow.parts[0];}
        }
        public ushort Sequence
        {
            get { return Convert.ToUInt16(metadatarow.parts[1]); }
        }
        public string Name
        {
            get
            {
                if (name == null)
                    name = netheader.StringsHeap.GetStringByOffset(Convert.ToUInt32(metadatarow.parts[2]));
                return name;
            }
        }
        public override string ToString()
        {
            return Name;
        }

        public ElementType ParameterType
        {
            get;
            internal set;
        }
        public override void ClearCache()
        {
            name = null;
        }
    }
}
