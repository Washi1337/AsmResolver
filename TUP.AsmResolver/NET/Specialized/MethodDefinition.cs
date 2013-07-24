using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class MethodDefinition : MethodReference
    {
        MemberRange<ParameterDefinition> paramRange = null;
        MethodSemantics semantics = null;
        MSIL.MethodBody body = null;
        GenericParameter[] genericparams = null;
        
        public MethodDefinition(MetaDataRow row)
            : base(row)
        {
        }

        public MethodDefinition(string name, MethodAttributes attributes, uint rva, uint signature, uint paramlist)
            : base(new MetaDataRow(rva, (ushort)0, (ushort)attributes, 0U, signature, paramlist))
        {
            this._name = name;
        }

        public uint RVA
        {
            get { return Convert.ToUInt32(metadatarow._parts[0]); }
            set { metadatarow._parts[0] = value; }
        }

        public MethodImplAttributes ImplementationAttributes
        {
            get { return (MethodImplAttributes)metadatarow._parts[1]; }
            set { metadatarow._parts[1] = ((ushort)value); }
        }

        public MethodAttributes Attributes
        {
            get { 
                
                MethodAttributes attr = (MethodAttributes)metadatarow._parts[2];
                return attr;
            }
            set { metadatarow._parts[2] = ((ushort)value); }
        }

        public override TypeReference DeclaringType
        {
            get
            {
                
                if (declaringType == null)
                {
                    uint relativetoken = this.metadatatoken - (6 << 0x18);

                    TypeDefinition lasttypeDef = null;
                    foreach (var member in netheader.TablesHeap.GetTable( MetaDataTableType.TypeDef).Members)
                    {

                        TypeDefinition typeDef = ((TypeDefinition)member);
                        if (lasttypeDef != null)
                        {
                            if (typeDef.MethodList > relativetoken && lasttypeDef.MethodList <= relativetoken)
                            {
                                declaringType = lasttypeDef;
                                return declaringType;
                            }
                        }

                        lasttypeDef = typeDef;
                    }
                    if (lasttypeDef != null && lasttypeDef.MethodList <= relativetoken)
                        return lasttypeDef;
                }
                return declaringType;

            }
        }

        public override string Name
        {
            get 
            { 
                if (string.IsNullOrEmpty(_name))
                    netheader.StringsHeap.TryGetStringByOffset(Convert.ToUInt32(metadatarow._parts[3]), out _name);
                return _name;
            }
        }

        public override bool IsDefinition
        {
            get
            {
                return true;
            }
        }

        public override MethodSignature Signature
        {
            get
            {
                if (_signature != null)
                    return _signature;
                _signature = (MethodSignature)netheader.BlobHeap.ReadMemberRefSignature(Convert.ToUInt32(metadatarow._parts[4]), this);
                return _signature;
                //return Convert.ToUInt32(metadatarow.parts[4]); 
            }
        }

        public override GenericParameter[] GenericParameters
        {
            get
            {
                if (genericparams == null && netheader.TablesHeap.HasTable(MetaDataTableType.GenericParam))
                {
                    List<GenericParameter> parameters = new List<GenericParameter>();
                    try
                    {

                        foreach (var member in netheader.TablesHeap.GetTable( MetaDataTableType.GenericParam).Members)
                        {
                            GenericParameter param = (GenericParameter)member;
                            try
                            {
                                if (param.Owner != null && param.Owner.MetaDataToken == this.metadatatoken)
                                    parameters.Add(param);
                            }
                            catch { }
                        }

                        parameters.Clear();
                    }
                    catch { }
                    genericparams = parameters.ToArray();
                }
                return genericparams;
            }
        }

        public bool IsConstructor
        {
            get
            {
                return (Attributes.HasFlag(MethodAttributes.SpecialName | MethodAttributes.RTSpecialName) && (Name == ".ctor" || Name == ".cctor"));
            }

        }

        public ParameterDefinition[] Parameters
        {
            get 
            {
                if (paramRange == null)
                {
                    paramRange = MemberRange.CreateRange<ParameterDefinition>(this, 5, NETHeader.TablesHeap.GetTable(MetaDataTableType.Param, false));
                }
                return paramRange.Members;
            }
        }

        public MethodSemantics Semantics
        {
            get
            {
                if (semantics == null && netheader.TablesHeap.HasTable(MetaDataTableType.MethodSemantics))
                {
                    foreach (MetaDataMember member in netheader.TablesHeap.GetTable(MetaDataTableType.MethodSemantics).Members)
                    {
                        MethodSemantics methodSem = member as MethodSemantics;
                        if (methodSem.Method != null && methodSem.Method.metadatatoken == this.metadatatoken)
                        {
                            semantics = methodSem;
                            break;
                        }
                    }
                }

                return semantics;
            }

        }

        public MSIL.MethodBody Body
        {
            get
            {
                if (body == null && RVA != 0)
                    body = MSIL.MethodBody.FromMethod(this);
                return body;
            }
        }
        
        public bool HasBody
        {
            get { return RVA != 0; }
        }

        public bool HasParameters
        {
            get { return Parameters != null && Parameters.Length > 0; }
        }

        public override void ClearCache()
        {
            base.ClearCache();
            paramRange = null;
            semantics = null;
            body = null;
            genericparams = null;
        }

        public override void LoadCache()
        {
            base.LoadCache();
            paramRange = MemberRange.CreateRange<ParameterDefinition>(this, 5, NETHeader.TablesHeap.GetTable(MetaDataTableType.Param, false));
            semantics = Semantics;
            body = Body;
            genericparams = GenericParameters;
        }
    }
}
