using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class PropertyDefinition : MemberReference
    {
        MethodDefinition getmethod = null;
        MethodDefinition setmethod = null;
        PropertySignature propertySig = null;
        TypeDefinition declaringType = null;
        string name = null;

        public PropertyDefinition(MetaDataRow row)
            : base(row)
        {
        }

        public PropertyDefinition(string name, PropertyAttributes attributes, uint signature)
            : base(new MetaDataRow((uint)attributes, 0U, signature))
        {
            this.name = name;
        }

        public PropertyAttributes Attributes
        {
            get { return (PropertyAttributes)Convert.ToUInt16(metadatarow.parts[0]); }
            set { metadatarow.parts[0] = (ushort)value; }
        }

        public override string Name
        {
            get
            {
                if (name == null)
                    netheader.StringsHeap.TryGetStringByOffset(Convert.ToUInt32(metadatarow.parts[1]), out name);
                return name;
            }
        }

        public PropertySignature Signature
        {
            get
            {
                if (propertySig == null)
                {
                    propertySig = netheader.BlobHeap.ReadPropertySignature(Convert.ToUInt32(metadatarow.parts[2]), this);
                }
                return propertySig;
            }
        }

        public MethodDefinition GetMethod
        {
            get
            {
                if (getmethod == null)
                {
                    foreach (MetaDataMember member in netheader.TablesHeap.GetTable(MetaDataTableType.MethodSemantics).Members)
                    {
                        MethodSemantics semantics = (MethodSemantics)member;
                        if (semantics.Association.metadatatoken == this.metadatatoken && (semantics.Attributes & MethodSemanticsAttributes.Getter ) == MethodSemanticsAttributes.Getter)
                        {
                            getmethod = semantics.Method;
                            break;
                        }
                    }
                }
                return getmethod;
            }
        }

        public MethodDefinition SetMethod
        {
            get
            {
                if (setmethod == null)
                {
                    foreach (MetaDataMember member in netheader.TablesHeap.GetTable(MetaDataTableType.MethodSemantics).Members)
                    {
                        MethodSemantics semantics = (MethodSemantics)member;
                        if (semantics.Association.metadatatoken == this.metadatatoken && (semantics.Attributes & MethodSemanticsAttributes.Setter) == MethodSemanticsAttributes.Setter)
                        {
                            setmethod = semantics.Method;
                            break;
                        }
                    }
                }
                return setmethod;
            }
        }

        public override string ToString()
        {
            return Name;
        }       
        
        public override void ClearCache()
        {
            getmethod = null;
            setmethod = null;
            name = null;
            propertySig = null;
        }

        public override string FullName
        {
            get
            {
                try
                {
                    if (DeclaringType is TypeReference)
                        return (Signature != null ? Signature.ReturnType.FullName + " " : "") + ((TypeReference)DeclaringType).FullName + "::" + Name;

                    return Name;
                }
                catch { return Name; }
            }
        }

        public override TypeReference DeclaringType
        {
            get {
                if (declaringType == null)
                {
                    MetaDataTable propertyMapTable = netheader.TablesHeap.GetTable(MetaDataTableType.PropertyMap);
                    foreach (var member in propertyMapTable.Members)
                    {
                        PropertyMap propertyMap = member as PropertyMap;
                        if (propertyMap.Properties.Contains(this))
                        {
                            declaringType = propertyMap.Parent;
                            break;
                        }
                    }
                }
                return declaringType;
            }
        }

        
    }
}
