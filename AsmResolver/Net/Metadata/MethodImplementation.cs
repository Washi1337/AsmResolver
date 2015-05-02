using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Builder;

namespace AsmResolver.Net.Metadata
{
    public class MethodImplementationTable : MetadataTable<MethodImplementation>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.MethodImpl; }
        }

        public override uint GetElementByteCount()
        {
            var encoder = TableStream.GetIndexEncoder(CodedIndex.MethodDefOrRef);
            return (uint)TableStream.GetTable<TypeDefinition>().IndexSize +
                   (uint)encoder.IndexSize +
                   (uint)encoder.IndexSize;
        }

        protected override MethodImplementation ReadMember(MetadataToken token, ReadingContext context)
        {
            var reader = context.Reader;
            var encoder = TableStream.GetIndexEncoder(CodedIndex.MethodDefOrRef);
            return new MethodImplementation(Header, token, new MetadataRow<uint, uint, uint>()
            {
                Column1 = reader.ReadIndex(TableStream.GetTable<TypeDefinition>().IndexSize),
                Column2 = reader.ReadIndex(encoder.IndexSize),
                Column3 = reader.ReadIndex(encoder.IndexSize),
            });
        }

        protected override void UpdateMember(NetBuildingContext context, MethodImplementation member)
        {
            var encoder = TableStream.GetIndexEncoder(CodedIndex.MethodDefOrRef);
            var row = member.MetadataRow;
            row.Column1 = member.Class.MetadataToken.Rid;
            row.Column2 = encoder.EncodeToken(member.MethodBody.MetadataToken);
            row.Column3 = encoder.EncodeToken(member.MethodDeclaration.MetadataToken);
        }

        protected override void WriteMember(WritingContext context, MethodImplementation member)
        {
            var writer = context.Writer;
            var row = member.MetadataRow;

            var encoder = TableStream.GetIndexEncoder(CodedIndex.MethodDefOrRef);
            writer.WriteIndex(TableStream.GetTable<TypeDefinition>().IndexSize, row.Column1);
            writer.WriteIndex(encoder.IndexSize, row.Column2);
            writer.WriteIndex(encoder.IndexSize, row.Column3);
        }
    }

    public class MethodImplementation : MetadataMember<MetadataRow<uint, uint, uint>>
    {
        private readonly LazyValue<TypeDefinition> _class;
        private readonly LazyValue<IMethodDefOrRef> _methodBody;
        private readonly LazyValue<IMethodDefOrRef> _methodDeclaration;

        internal MethodImplementation(MetadataHeader header, MetadataToken token, MetadataRow<uint, uint, uint> row)
            : base(header, token, row)
        {
            var tableStream = header.GetStream<TableStream>();
            var encoder = tableStream.GetIndexEncoder(CodedIndex.MethodDefOrRef);

            _class = new LazyValue<TypeDefinition>(() => tableStream.GetTable<TypeDefinition>()[(int)row.Column1 - 1]);

            _methodBody = new LazyValue<IMethodDefOrRef>(() =>
            {
                var methodBodyToken = encoder.DecodeIndex(row.Column2);
                return methodBodyToken.Rid != 0 ? (IMethodDefOrRef)tableStream.ResolveMember(methodBodyToken) : null;
            });

            _methodDeclaration = new LazyValue<IMethodDefOrRef>(() =>
            {
                var methodDeclarationToken = encoder.DecodeIndex(row.Column3);
                return methodDeclarationToken.Rid != 0
                    ? (IMethodDefOrRef)tableStream.ResolveMember(methodDeclarationToken)
                    : null;
            });
        }

        public TypeDefinition Class
        {
            get { return _class.Value; }
            set { _class.Value = value; }
        }

        public IMethodDefOrRef MethodBody
        {
            get { return _methodBody.Value; }
            set { _methodBody.Value = value; }
        }

        public IMethodDefOrRef MethodDeclaration
        {
            get { return _methodDeclaration.Value; }
            set { _methodDeclaration.Value = value; }
        }
    }
}
