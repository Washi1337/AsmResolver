using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime;
using System.Runtime.InteropServices;
using TUP.AsmResolver.Exceptions;
namespace TUP.AsmResolver.NET.Specialized
{
    
    public class MemberResolver
    {
        public delegate Win32Assembly ResolveFailedEventHandler(object sender, ResolveFailedEventArgs e);
        public event ResolveFailedEventHandler ResolveFailed;

        string directory;
        Dictionary<string, Win32Assembly> ResolvedAssemblies;

        public MemberResolver(Win32Assembly parentAssembly)
        {
            if (parentAssembly.path != "")
                directory = parentAssembly.path.Substring(0, parentAssembly.path.LastIndexOf('\\'));

            ResolvedAssemblies = new Dictionary<string, Win32Assembly>();
        }

        protected virtual Win32Assembly OnResolveFailed(ResolveFailedEventArgs e)
        {
            if (ResolveFailed != null)
                return ResolveFailed(this, e);
            return null;

        }

        public Win32Assembly ResolveAssembly(AssemblyReference assemblyRef)
        {
            return ResolveAssembly(assemblyRef.Name);
        }
        public Win32Assembly ResolveAssembly(string name)
        {
            if (ResolvedAssemblies.ContainsKey(name))
                return ResolvedAssemblies[name];


            string path = "";
            if (File.Exists(directory + "\\" + name + ".dll"))
                path = directory + "\\" + name + ".dll";
            else if (File.Exists(Environment.CurrentDirectory + "\\" + name + ".dll"))
                path = Environment.CurrentDirectory + "\\" + name + ".dll";
            else if (File.Exists(RuntimeEnvironment.GetRuntimeDirectory() + "\\" + name + ".dll"))
                path = Environment.CurrentDirectory + "\\" + name + ".dll";

            Win32Assembly asm = null;
            if (path =="")
                try
                {
                    
                    asm = Win32Assembly.LoadFile(path);
                }
                catch (Exception ex)
                {
                    asm = OnResolveFailed(new ResolveFailedEventArgs(name, ex));
                }

            if (asm != null)
                ResolvedAssemblies.Add(name, asm);
            return OnResolveFailed(new ResolveFailedEventArgs(name, new ResolveException("Failed to resolve assembly " + name  + ".")));
        }


    }

    public class ResolveFailedEventArgs
    {
        public ResolveFailedEventArgs(string nameofasm, Exception exception)
        {
            NameOfAssembly = nameofasm;
            Exception = exception;
        }

        public string NameOfAssembly {get; private set;}
        public Exception Exception { get; private set; }
        
    }
}
