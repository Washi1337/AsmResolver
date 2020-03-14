namespace AsmResolver.DotNet.Cloning
{
    public class CloneContextAwareReferenceImporter : ReferenceImporter
    {
        public CloneContextAwareReferenceImporter(MetadataCloneContext context)
            : base(context.Module)
        {
        }
        
    }
}