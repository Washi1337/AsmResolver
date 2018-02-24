using AsmResolver.Net.Builder;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts
{
    public class MethodSemantics : MetadataMember<MetadataRow<MethodSemanticsAttributes, uint, uint>>
    {
        private readonly LazyValue<MethodDefinition> _method;
        private readonly LazyValue<IHasSemantics> _association;

        public MethodSemantics(MethodDefinition method, IHasSemantics association, MethodSemanticsAttributes attributes) 
            : base(null, new MetadataToken(MetadataTokenType.MethodSemantics))
        {
            _method = new LazyValue<MethodDefinition>(method);
            _association = new LazyValue<IHasSemantics>(association);
            Attributes = attributes;
        }
        
        internal MethodSemantics(MetadataImage image, MetadataRow<MethodSemanticsAttributes, uint, uint> row)
            : base(image, row.MetadataToken)
        {
            var tableStream = image.Header.GetStream<TableStream>();

            Attributes = row.Column1;
            
            _method = new LazyValue<MethodDefinition>(() =>
            {
                var methodTable = tableStream.GetTable(MetadataTokenType.Method);
                var methodRow = methodTable.GetRow((int) (row.Column2 - 1));
                return (MethodDefinition) methodTable.GetMemberFromRow(image, methodRow);
            });

            _association = new LazyValue<IHasSemantics>(() =>
            {
                var associationToken = tableStream.GetIndexEncoder(CodedIndex.HasSemantics).DecodeIndex(row.Column3);
                IMetadataMember member;
                return image.TryResolveMember(associationToken, out member) ? (IHasSemantics) member : null;
            });
        }

        public MethodSemanticsAttributes Attributes
        {
            get;
            set;
        }

        public MethodDefinition Method
        {
            get { return _method.Value; }
            set { _method.Value = value; }
        }

        public IHasSemantics Association
        {
            get { return _association.Value; }
            set { _association.Value = value; }
        }

        public override void AddToBuffer(MetadataBuffer buffer)
        {
            var tableStream = buffer.TableStreamBuffer;
            tableStream.GetTable<MethodSemanticsTable>().Add(new MetadataRow<MethodSemanticsAttributes, uint, uint>
            {
                Column1 = Attributes,
                Column2 = Method.MetadataToken.Rid,
                Column3 = tableStream.GetIndexEncoder(CodedIndex.HasSemantics).EncodeToken(Association.MetadataToken)
            });
        }
    }
    
}