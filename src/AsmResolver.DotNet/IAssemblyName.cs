using System;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet
{
    public interface IAssemblyName
    {
        string Name
        {
            get;
        }

        Version Version
        {
            get;
        }

        AssemblyAttributes Attributes
        {
            get;
        }

        string Culture
        {
            get;
        }
        
        byte[] GetPublicKeyToken();
    }
}