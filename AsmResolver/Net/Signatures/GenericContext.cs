using System;
using System.Linq;

namespace AsmResolver.Net.Signatures
{
    public interface IGenericContext
    {
        IGenericArgumentsProvider Type
        {
            get;
        }

        IGenericArgumentsProvider Method
        {
            get;
        }
    }

    public class GenericContext : IGenericContext
    {
        public GenericContext(IGenericArgumentsProvider type, IGenericArgumentsProvider method)
        {
            Type = type;
            Method = method;
        }        
        
        public IGenericArgumentsProvider Type
        {
            get;
        }

        public IGenericArgumentsProvider Method
        {
            get;
        }
        
        public override string ToString()
        {
            return $"{nameof(Type)}: {Type}, {nameof(Method)}: {Method}";
        }
    
    }
}