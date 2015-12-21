using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Builder;

namespace AsmResolver
{
    public class ImageImportDirectory  : FileSegment
    {
        public static ImageImportDirectory FromReadingContext(ReadingContext context)
        {
            return new ImageImportDirectory()
            {
                _readingContext = context
            };
        }

        private List<ImageModuleImport> _imports;
        private ReadingContext _readingContext;

        public IList<ImageModuleImport> ModuleImports
        {
            get
            {
                if (_imports != null)
                    return _imports;
                _imports = new List<ImageModuleImport>();

                if (_readingContext == null)
                    return _imports;

                while (true)
                {
                    var directory = ImageModuleImport.FromReadingContext(_readingContext);
                    if (directory.IsEmpty)
                        break;
                    _imports.Add(directory);
                }

                return _imports;
            }
        }

        public override uint GetPhysicalLength()
        {
            return (uint)((ModuleImports.Count + 1) * 5 * sizeof (uint));
        }

        public override void Write(WritingContext context)
        {
            foreach (var moduleImport in ModuleImports)
                moduleImport.Write(context);
            new ImageModuleImport().Write(context);
        }
    }
}
