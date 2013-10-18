using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class MethodDefinition : MethodReference
    {
        MemberRange<ParameterDefinition> _paramRange = null;
        MethodSemantics _semantics = null;
        MSIL.MethodBody _body = null;
        bool _hasLoadedGenericParameters = false;
        
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
            get { return Convert.ToUInt32(_metadatarow._parts[0]); }
            set { _metadatarow._parts[0] = value; }
        }

        public MethodImplAttributes ImplementationAttributes
        {
            get { return (MethodImplAttributes)_metadatarow._parts[1]; }
            set { _metadatarow._parts[1] = ((ushort)value); }
        }

        public MethodAttributes Attributes
        {
            get { 
                
                MethodAttributes attr = (MethodAttributes)_metadatarow._parts[2];
                return attr;
            }
            set { _metadatarow._parts[2] = ((ushort)value); }
        }

        public override TypeReference DeclaringType
        {
            get
            {
                
                if (_declaringType == null)
                {
                    uint relativetoken = this._metadatatoken - (6 << 0x18);

                    TypeDefinition lasttypeDef = null;
                    foreach (var member in _netheader.TablesHeap.GetTable( MetaDataTableType.TypeDef).Members)
                    {

                        TypeDefinition typeDef = ((TypeDefinition)member);
                        if (lasttypeDef != null)
                        {
                            if (typeDef.MethodList > relativetoken && lasttypeDef.MethodList <= relativetoken)
                            {
                                _declaringType = lasttypeDef;
                                return _declaringType;
                            }
                        }

                        lasttypeDef = typeDef;
                    }
                    if (lasttypeDef != null && lasttypeDef.MethodList <= relativetoken)
                        return lasttypeDef;
                }
                return _declaringType;

            }
        }

        public override string Name
        {
            get 
            { 
                if (string.IsNullOrEmpty(_name))
                    _netheader.StringsHeap.TryGetStringByOffset(Convert.ToUInt32(_metadatarow._parts[3]), out _name);
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
                _signature = (MethodSignature)_netheader.BlobHeap.ReadMemberRefSignature(Convert.ToUInt32(_metadatarow._parts[4]), this);
                return _signature;
                //return Convert.ToUInt32(metadatarow.parts[4]); 
            }
        }

        public override GenericParameter[] GenericParameters
        {
            get
            {
                if (!_hasLoadedGenericParameters && _netheader.TablesHeap.HasTable(MetaDataTableType.GenericParam))
                {
                    List<GenericParameter> parameters = new List<GenericParameter>();
                    try
                    {

                        foreach (var member in _netheader.TablesHeap.GetTable( MetaDataTableType.GenericParam).Members)
                        {
                            GenericParameter param = (GenericParameter)member;
                            try
                            {
                                if (param.Owner != null && param.Owner.MetaDataToken == this._metadatatoken)
                                    parameters.Add(param);
                            }
                            catch { }
                        }

                    }
                    catch { }
                    _hasLoadedGenericParameters = true;
                    base._genericParameters = parameters.ToArray();
                    parameters.Clear();
                }
                return base._genericParameters;
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
                if (_paramRange == null)
                {
                    _paramRange = MemberRange.CreateRange<ParameterDefinition>(this, 5, NETHeader.TablesHeap.GetTable(MetaDataTableType.Param, false));
                }
                return _paramRange.Members;
            }
        }

        public MethodSemantics Semantics
        {
            get
            {
                if (_semantics == null && _netheader.TablesHeap.HasTable(MetaDataTableType.MethodSemantics))
                {
                    foreach (MetaDataMember member in _netheader.TablesHeap.GetTable(MetaDataTableType.MethodSemantics).Members)
                    {
                        MethodSemantics methodSem = member as MethodSemantics;
                        if (methodSem.Method != null && methodSem.Method._metadatatoken == this._metadatatoken)
                        {
                            _semantics = methodSem;
                            break;
                        }
                    }
                }

                return _semantics;
            }

        }

        public MSIL.MethodBody Body
        {
            get
            {
                if (_body == null && RVA != 0)
                    _body = MSIL.MethodBody.FromMethod(this);
                return _body;
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

        public override MethodDefinition Resolve()
        {
            return this;
        }

        public override void ClearCache()
        {
            base.ClearCache();
            _paramRange = null;
            _semantics = null;
            _body = null;
            _genericParameters = new GenericParameter[0];
            _hasLoadedGenericParameters = false;
        }

        public override void LoadCache()
        {
            base.LoadCache();
            _paramRange = MemberRange.CreateRange<ParameterDefinition>(this, 5, NETHeader.TablesHeap.GetTable(MetaDataTableType.Param, false));
            _semantics = Semantics;
            _body = Body;
            _genericParameters = GenericParameters;
            
        }
    }
}
