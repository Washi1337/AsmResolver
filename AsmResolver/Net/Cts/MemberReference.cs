using System;
using System.Runtime.Remoting.Messaging;
using AsmResolver.Net.Builder;
using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cts
{
    public class MemberReference : MetadataMember<MetadataRow<uint, uint, uint>>, ICustomAttributeType, ICallableMemberReference
    {
        private readonly LazyValue<string> _name;
        private readonly LazyValue<MemberSignature> _signature;
        private readonly LazyValue<IMemberRefParent> _parent;
        private string _fullName;

        public MemberReference(IMemberRefParent parent, string name, MemberSignature signature)
            : base(null, new MetadataToken(MetadataTokenType.MemberRef))
        {
            _parent = new LazyValue<IMemberRefParent>(parent);
            _name = new LazyValue<string>(name);
            _signature = new LazyValue<MemberSignature>(signature);
            CustomAttributes = new CustomAttributeCollection(this);
        }

        internal MemberReference(MetadataImage image, MetadataRow<uint, uint, uint> row)
            : base(image, row.MetadataToken)
        {
            var tableStream = image.Header.GetStream<TableStream>();

            _parent = new LazyValue<IMemberRefParent>(() =>
            {
                var parentToken = tableStream.GetIndexEncoder(CodedIndex.MemberRefParent).DecodeIndex(row.Column1);
                return parentToken.Rid != 0 ? (IMemberRefParent)image.ResolveMember(parentToken) : null;
            });

            _name = new LazyValue<string>(() => image.Header.GetStream<StringStream>().GetStringByOffset(row.Column2));

            _signature = new LazyValue<MemberSignature>(() => 
                CallingConventionSignature.FromReader(image, image.Header.GetStream<BlobStream>().CreateBlobReader(row.Column3)) as MemberSignature);
            
            CustomAttributes = new CustomAttributeCollection(this);
        }

        public IMemberRefParent Parent
        {
            get { return _parent.Value; }
            set { _parent.Value = value; }
        }

        public string Name
        {
            get { return _name.Value; }
            set
            {
                _name.Value = value;
                _fullName = null;
            }
        }

        public string FullName
        {
            get { return _fullName ?? (_fullName = this.GetFullName(Signature)); }
        }

        public ITypeDefOrRef DeclaringType
        {
            get
            {
                var declaringType = Parent as ITypeDefOrRef;
                if (declaringType != null)
                    return declaringType;

                var method = Parent as MethodDefinition;
                if (method != null)
                    return method.DeclaringType;
                
                // TODO: handle modulereference parent

                return null;
            }
        }

        public MemberSignature Signature
        {
            get { return _signature.Value; }
            set
            {
                _signature.Value = value;
                _fullName = null;
            }
        }

        CallingConventionSignature ICallableMemberReference.Signature
        {
            get { return Signature; }
        }

        public CustomAttributeCollection CustomAttributes
        {
            get;
            private set;
        }
        
        public override string ToString()
        {
            return FullName;
        }

        public IMetadataMember Resolve()
        {
            if (Image == null || Image.MetadataResolver == null || Signature == null)
                throw new MemberResolutionException(this);

            return Signature.IsMethod
                ? (IMetadataMember)Image.MetadataResolver.ResolveMethod(this)
                : Image.MetadataResolver.ResolveField(this);
        }

        public override void AddToBuffer(MetadataBuffer buffer)
        {
            var tableStream = buffer.TableStreamBuffer;
            tableStream.GetTable<MemberReferenceTable>().Add(new MetadataRow<uint, uint, uint>
            {
                Column1 = tableStream.GetIndexEncoder(CodedIndex.MemberRefParent).EncodeToken(Parent.MetadataToken),
                Column2 = buffer.StringStreamBuffer.GetStringOffset(Name),
                Column3 = buffer.BlobStreamBuffer.GetBlobOffset(Signature)
            });

            foreach (var attribute in CustomAttributes)
                attribute.AddToBuffer(buffer);
        }
    }
}
