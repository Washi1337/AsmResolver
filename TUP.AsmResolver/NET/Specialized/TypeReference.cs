using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class TypeReference : MemberReference, IGenericParametersProvider, IGenericArgumentsProvider, IResolutionScope
    {
        internal ElementType elementType = ElementType.None;
        internal IResolutionScope resolutionScope;
        internal string name = string.Empty;
        internal string @namespace = string.Empty;
        internal string fullname = string.Empty;

        public TypeReference(MetaDataRow row)
            : base(row)
        {
        }

        public TypeReference(string @namespace, string name, IResolutionScope resolutionScope)
            : base(new MetaDataRow(0U, 0U, 0U))
        {
            this.name = name;
            this.@namespace = @namespace;
            this.resolutionScope = resolutionScope;
        }

        public override TypeReference DeclaringType
        {
            get { return null; }
        }

        public virtual IResolutionScope ResolutionScope
        {
            get
            {
                if (resolutionScope != null || !HasSavedMetaDataRow)
                    return resolutionScope;

                MetaDataMember member;
                netheader.TablesHeap.ResolutionScope.TryGetMember(Convert.ToInt32(metadatarow.parts[0]), out member);
                resolutionScope = member as IResolutionScope;

                return resolutionScope; 
            }
        }
        public override string Name
        {
            get
            {
                if (string.IsNullOrEmpty(this.name))
                    this.name = netheader.StringsHeap.GetStringByOffset(Convert.ToUInt32(metadatarow.parts[1]));
                return this.name;
            }
        }

        public virtual string Namespace
        {
            get
            {
                if (HasSavedMetaDataRow)
                    @namespace = netheader.StringsHeap.GetStringByOffset(Convert.ToUInt32(metadatarow.parts[2]));
                return @namespace;
            }
        }

        public override string FullName
        {
            get
            {
                if (string.IsNullOrEmpty(fullname))
                {

                    TypeReference declaringType = this.DeclaringType;
                    if (declaringType == null)
                    {
                        fullname = (string.IsNullOrEmpty(Namespace) ? string.Empty : Namespace + ".") + Name;
                    }
                    else
                    {
                        StringBuilder builder = new StringBuilder();
                        builder.Append(Name);
                        while (declaringType != null)
                        {
                            builder.Insert(0, declaringType.FullName + "/");

                            declaringType = declaringType.DeclaringType;
                        }
                        fullname = builder.ToString();
                    }
                }
                return fullname;
            }
        }

        public virtual bool IsArray { get; internal set; }
        public virtual bool IsPointer { get; internal set; }
        public virtual bool IsByReference { get; internal set; }
        public virtual bool IsPinned { get; internal set; }
        public virtual bool IsDefinition
        {
            get;
            internal set;
        }
        public virtual bool IsGenericInstance { get; internal set; }
        public virtual bool IsValueType
        {
            get;
            internal set;
        }
        public virtual bool IsElementType { get; internal set; }

        public virtual GenericParameter[] GenericParameters
        {
            get { return null; }
        }

        public virtual TypeReference[] GenericArguments { get { return null; } }

        public virtual TypeReference GetElementType()
        {
            return this;
        }

        public override void ClearCache()
        {
            resolutionScope = null;
            name = null;
            @namespace = null;
            fullname = null;
        }

        public override string ToString()
        {
            return FullName;
        }

    }
}
