using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUP.AsmResolver.NET.Specialized
{
    public class CustomModifierType : TypeSpecification
    {
        public CustomModifierType(TypeReference modifierType, TypeReference baseType, bool isRequired)
            : base(baseType)
        {
            ModifierType = modifierType;
            IsRequired = isRequired;
        }

        public TypeReference ModifierType
        {
            get;
            set;
        }

        public bool IsRequired
        {
            get;
            set;
        }

        public override string Name
        {
            get
            {
                return string.Format("{0} {1}", base.Name, GetSuffix());
            }
        }

        public string GetSuffix()
        {
            return string.Format("{0}({1})", ModifierType.FullName, IsRequired ? "modreq" : "modopt");
        }
    }
}
