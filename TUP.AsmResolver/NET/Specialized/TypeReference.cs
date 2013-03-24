using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class TypeReference : MemberReference, IGenericParametersProvider, IGenericArgumentsProvider
    {
        internal ElementType elementType = ElementType.None;

        internal MetaDataMember resolutionScope;
        internal string name = string.Empty;
        internal string @namespace = string.Empty;
        public override TypeReference DeclaringType
        {
            get { return null; }
        }
        public virtual MetaDataMember ResolutionScope
        {
            get
            {
                if (resolutionScope != null || !HasSavedMetaDataRow)
                    return resolutionScope;

                netheader.TablesHeap.ResolutionScope.TryGetMember(Convert.ToInt32(metadatarow.parts[0]), out resolutionScope);
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
                //if (Name.Contains("Checked"))
                //    System.Diagnostics.Debugger.Break();

                TypeReference declaringType = DeclaringType;
                if (declaringType == null)
                    return (Namespace == "" ? "" : Namespace + ".") + Name;
                
                StringBuilder builder = new StringBuilder();
                builder.Append(Name);
                while (declaringType != null)
                {
                    builder.Insert(0,DeclaringType.FullName + ".");

                    declaringType = declaringType.DeclaringType;
                }
                return builder.ToString();
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

        public override string ToString()
        {
            return FullName;
        }



        public virtual GenericParameter[] GenericParameters
        {
            get { return null; }
        }

        public virtual TypeReference[] GenericArguments { get { return null; } }

        public override void ClearCache()
        {
            resolutionScope = null;
            name = null;
            @namespace = null;
        }
    }
}
