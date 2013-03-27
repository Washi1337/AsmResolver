using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class MethodDefinition : MethodReference
    {
        ParameterDefinition[] paramdefs = null;
        MethodSignature signature = null;
        MethodSemantics semantics = null;
        MSIL.MethodBody body = null;
        TypeReference declaringtype;
        GenericParameter[] genericparams = null;

        public uint RVA
        {
            get { return Convert.ToUInt32(metadatarow.parts[0]); }
            set { metadatarow.parts[0] = value; }
        }

        public MethodImplAttributes ImplementationAttributes
        {
            get { return (MethodImplAttributes)metadatarow.parts[1]; }
            set { metadatarow.parts[1] = ((ushort)value); }
        }
        public MethodAttributes Attributes
        {
            get { 
                
                MethodAttributes attr = (MethodAttributes)metadatarow.parts[2];
                return attr;
            }
            set { metadatarow.parts[2] = ((ushort)value); }
        }

        public override TypeReference DeclaringType
        {
            get
            {
                
                if (declaringtype == null)
                {
                    uint relativetoken = this.metadatatoken - (6 << 0x18);

                    TypeDefinition lasttypeDef = null;
                    foreach (var member in netheader.TablesHeap.GetTable( MetaDataTableType.TypeDef).members)
                    {

                        TypeDefinition typeDef = ((TypeDefinition)member);
                        if (lasttypeDef != null)
                        {
                            if (typeDef.MethodList > relativetoken && lasttypeDef.MethodList <= relativetoken)
                            {
                                declaringtype = lasttypeDef;
                                return declaringtype;
                            }
                        }

                        lasttypeDef = typeDef;
                    }
                    if (lasttypeDef != null && lasttypeDef.MethodList <= relativetoken)
                        return lasttypeDef;
                }
                return declaringtype;

            }
        }
        public override string Name
        {
            get { return netheader.StringsHeap.GetStringByOffset(Convert.ToUInt32(metadatarow.parts[3])); }
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
                if (signature != null)
                    return signature;
                signature = (MethodSignature)netheader.BlobHeap.ReadMemberRefSignature(Convert.ToUInt32(metadatarow.parts[4]),this, DeclaringType);
                return signature;
                //return Convert.ToUInt32(metadatarow.parts[4]); 
            }
        }
        public override GenericParameter[] GenericParameters
        {
            get
            {
                if (genericparams == null && netheader.TablesHeap.HasTable(MetaDataTableType.GenericParam))
                {
                    try
                    {
                        List<GenericParameter> parameters = new List<GenericParameter>();

                        foreach (var member in netheader.TablesHeap.GetTable( MetaDataTableType.GenericParam).members)
                        {
                            GenericParameter param = (GenericParameter)member;
                            try
                            {
                                if (param.Owner != null && param.Owner.metadatatoken == this.metadatatoken)
                                    parameters.Add(param);
                            }
                            catch { }
                        }

                        genericparams = parameters.ToArray();
                        parameters.Clear();
                    }
                    catch { }
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
            get {
                if (this.paramdefs != null)
                    return this.paramdefs;
                if (netheader.TablesHeap.HasTable(MetaDataTableType.Param))
                {
                    int paramlist = Convert.ToInt32(metadatarow.parts[5]);

                    int nextparamlist = -1;

                    if (netheader.TablesHeap.GetTable( MetaDataTableType.Method).members.Last().metadatatoken != this.metadatatoken)
                        nextparamlist = Convert.ToInt32(netheader.TokenResolver.ResolveMember(this.MetaDataToken + 1).metadatarow.parts[5]);


                    MetaDataTable paramTable = netheader.TablesHeap.GetTable(MetaDataTableType.Param);
                    int length = -1;
                    if (nextparamlist != -1)
                        length = nextparamlist - paramlist;
                    else
                        length = paramTable.members.Count - (paramlist - 1);

                    ParameterDefinition[] paramdefs = null;

                    if (length > -1)
                    {
                        paramdefs = new ParameterDefinition[length];
                        if (length > paramTable.members.Count)
                            return null;
                        for (int i = 0; i < paramdefs.Length; i++)
                        {
                            paramdefs[i] = (ParameterDefinition)paramTable.members[paramlist + i - 1];
                        }

                    }
                    this.paramdefs = paramdefs;
                }
                return this.paramdefs;
                //return Convert.ToUInt32(metadatarow.parts[5]); 
            }
        }

        public MethodSemantics Semantics
        {
            get
            {
                if (semantics == null && netheader.TablesHeap.HasTable(MetaDataTableType.MethodSemantics))
                {
                    foreach (MetaDataMember member in netheader.TablesHeap.GetTable(MetaDataTableType.MethodSemantics).members)
                    {
                        MethodSemantics methodSem = member as MethodSemantics;
                        if (methodSem.Method.metadatatoken == this.metadatatoken)
                        {
                            semantics = methodSem;
                            break;
                        }
                    }
                }

                return semantics;
            }

        }

        public bool HasBody
        {
            get { return RVA != 0; }
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

       

    }
}
