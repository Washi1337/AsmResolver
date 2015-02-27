using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmResolver
{
    public class ImageRelocationDirectory : FileSegment
    {
        public static ImageRelocationDirectory FromReadingContext(ReadingContext context)
        {
            var directory = new ImageRelocationDirectory();
            var relocDirectory =
                context.Assembly.NtHeaders.OptionalHeader.DataDirectories[
                    ImageDataDirectory.BaseRelocationDirectoryIndex];

            while (context.Reader.Position < context.Reader.StartPosition + relocDirectory.Size)
            {
                var block = BaseRelocationBlock.FromReadingContext(context);
                directory.Blocks.Add(block);
                context.Reader.Position += block.BlockSize - 2 * sizeof (uint);
            }

            return directory;
        }

        public ImageRelocationDirectory()
        {
            Blocks = new List<BaseRelocationBlock>();
        }

        public IList<BaseRelocationBlock> Blocks
        {
            get;
            private set;
        }

        public override uint GetPhysicalLength()
        {
            return (uint)Blocks.Sum(x => x.GetPhysicalLength());
        }

        public override void Write(WritingContext context)
        {
            foreach (var block in Blocks)
                block.Write(context);
        }
    }
}
