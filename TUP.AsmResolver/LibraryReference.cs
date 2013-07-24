using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TUP.AsmResolver.PE;
namespace TUP.AsmResolver
{
    /// <summary>
    /// Represents a library reference containing imported methods that the corresponding portable executable uses.
    /// </summary>
    public class LibraryReference
    {
        private PeImage _image;
        private uint _offset;
        internal Structures.IMAGE_IMPORT_DESCRIPTOR _rawDescriptor;

        internal LibraryReference(PeImage image, uint offset, Structures.IMAGE_IMPORT_DESCRIPTOR rawDescriptor, string libraryName, ImportMethod[] importMethods)
        {
            this._image = image;
            this._offset = offset;
            this._rawDescriptor = rawDescriptor;
            this.LibraryName = libraryName;
            this.ImportMethods = importMethods;
        }

        /// <summary>
        /// Gets the name of the library.
        /// </summary>
        public string LibraryName
        {
            get;
            private set;
        }
        /// <summary>
        /// Gets a list of methods the corresponding portable executable uses.
        /// </summary>
        public ImportMethod[] ImportMethods
        {
            get;
            private set;
        }
      //  /// <summary>
      //  /// Gets the address of the library reference in the portable executable file.
      //  /// </summary>
      //  public uint Address
      //  {
      //      get { return rawDescriptor.; }
      //  }

        /// <summary>
        /// Resolves the asembly by checking the directory of the assembly, the system directories and the current directory.
        /// </summary>
        /// <param name="parentAssembly">The parent assembly to search from. You can fill in a null value, but it can influent the result.</param>
        /// <param name="disableWOW64Redirection">Disables WOW64 layer redirections to get access 64 bit directories. Default value is true.</param>
        /// <returns></returns>
        public Win32Assembly Resolve(Win32Assembly parentAssembly, bool disableWOW64Redirection = true)
        {
            Win32Assembly assembly;
            if (disableWOW64Redirection)
                ASMGlobals.Wow64EnableWow64FsRedirection(false);
            try
            {
                string actualpath = "";
                if (parentAssembly != null)
                {
                    string path = parentAssembly._path.Substring(0, parentAssembly._path.LastIndexOf("\\"));
                    if (File.Exists(path + "\\" + LibraryName))
                    {
                        actualpath = path + "\\" + LibraryName;
                        goto things;
                    }
                }
                if (parentAssembly._ntHeader.OptionalHeader.Is32Bit)
                {
                    if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86) + "\\" + LibraryName))
                        actualpath = Environment.GetFolderPath(Environment.SpecialFolder.SystemX86) + "\\" + LibraryName;
                }
                else if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.System) + "\\" + LibraryName))
                        actualpath = Environment.GetFolderPath(Environment.SpecialFolder.System) + "\\" + LibraryName;


                if (actualpath == "" & File.Exists(Environment.CurrentDirectory + "\\" + LibraryName))
                    actualpath = Environment.CurrentDirectory + "\\" + LibraryName;

            things:
                if (actualpath == "")
                    throw new Exceptions.ResolveException(new FileNotFoundException("The target application can not be found."));


                try
                {
                    assembly = Win32Assembly.LoadFile(actualpath);
                }
                catch (Exception ex)
                {
                    throw new Exceptions.ResolveException(ex);
                }
            }
            catch
            {
                if (disableWOW64Redirection)
                    ASMGlobals.Wow64EnableWow64FsRedirection(true);
                throw;
            }
            finally
            {
                if (disableWOW64Redirection)
                    ASMGlobals.Wow64EnableWow64FsRedirection(true);
            }
            return assembly;
       
        }





    }
}
