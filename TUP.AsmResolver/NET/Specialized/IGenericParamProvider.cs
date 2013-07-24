using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUP.AsmResolver.NET.Specialized
{
    public interface IGenericParamProvider : IMetaDataMember 
    {
        GenericParameter[] GenericParameters { get; }
        bool HasGenericParameters { get; }
        TypeReference DeclaringType { get; }
        bool IsDefinition { get; }
        GenericParamType ParamType { get; }
        void AddGenericParameter(GenericParameter parameter);
    }

    public enum GenericParamType
    {
        Type,
        Method,
    }

}
