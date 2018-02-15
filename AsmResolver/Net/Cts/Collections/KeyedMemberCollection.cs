using System;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts.Collections
{
    public abstract class KeyedMemberCollection<TOwner, TMember> : MemberCollection<TOwner, TMember>
        where TOwner : class, IMetadataMember
        where TMember : IMetadataMember
    {
        private readonly MetadataTokenType _table;
        private readonly IndexEncoder _indexEncoder;
        private readonly int _keyColumnIndex;

        internal KeyedMemberCollection(TOwner owner, MetadataTokenType table, CodedIndex codedIndex, int keyColumnIndex)
            : this (owner, table, null, keyColumnIndex)
        {
            if (Owner.Image != null)
                _indexEncoder = Owner.Image.Header.GetStream<TableStream>().GetIndexEncoder(codedIndex);
        }

        internal KeyedMemberCollection(TOwner owner, MetadataTokenType table, MetadataTokenType tokenType, int keyColumnIndex)
            : this(owner, table, null, keyColumnIndex)
        {
            if (Owner.Image != null)
                _indexEncoder = new IndexEncoder(Owner.Image.Header.GetStream<TableStream>(), tokenType);
        }

        internal KeyedMemberCollection(TOwner owner, MetadataTokenType table, IndexEncoder encoder, int keyColumnIndex)
            : base(owner)
        {
            _table = table;
            _indexEncoder = encoder;
            _keyColumnIndex = keyColumnIndex;
        }

        protected override void Initialize()
        {
            if (Owner.Image != null && _indexEncoder != null)
            {
                var stream = Owner.Image.Header.GetStream<TableStream>();
                var memberTable = stream.GetTable(_table);
                if (memberTable != null)
                {
                    uint key = _indexEncoder.EncodeToken(Owner.MetadataToken);
                    var row = memberTable.GetRowByKey(_keyColumnIndex, key);

                    if (row != null)
                    {
                        int startIndex = (int) row.MetadataToken.Rid - 1;
                        while (startIndex > 0 && Convert.ToUInt32(memberTable.GetRow(startIndex - 1).GetAllColumns()[_keyColumnIndex]) == key)
                            startIndex--;
                        
                        for (int index = startIndex; index < memberTable.Count; index++)
                        {
                            var item = memberTable.GetRow(index);
                            if (Convert.ToUInt32(item.GetAllColumns()[_keyColumnIndex]) != key)
                                break;
                            Items.Add((TMember) memberTable.GetMemberFromRow(Owner.Image, row));
                        }
                    }
                }
            }

            base.Initialize();
        }
    }

    public class CustomAttributeCollection : KeyedMemberCollection<IHasCustomAttribute, CustomAttribute>
    {
        public CustomAttributeCollection(IHasCustomAttribute owner)
            : base(owner, MetadataTokenType.CustomAttribute, CodedIndex.HasCustomAttribute, 0)
        {
        }
        
        protected override IHasCustomAttribute GetOwner(CustomAttribute item)
        {
            return item.Parent;
        }

        protected override void SetOwner(CustomAttribute item, IHasCustomAttribute owner)
        {
            item.Parent = owner;
        }
    }

    //public class MethodSemanticsCollection : MetadataMemberCollection<IHasSemantics, MethodSemantics>
    //{
    //    public MethodSemanticsCollection(IHasSemantics owner)
    //        : base(CodedIndex.HasSemantics, 2, owner)
    //    {
    //    }

    //    protected override IHasSemantics GetOwner(MethodSemantics item)
    //    {
    //        return item.Association;
    //    }

    //    protected override void SetOwner(MethodSemantics item, IHasSemantics owner)
    //    {
    //        item.Association = owner;
    //    }
    //}

    //public class NestedClassCollection : MetadataMemberCollection<TypeDefinition, NestedClass>
    //{
    //    public NestedClassCollection(TypeDefinition owner)
    //        : base(MetadataTokenType.TypeDef, 1, owner)
    //    {
    //    }

    //    protected override TypeDefinition GetOwner(NestedClass item)
    //    {
    //        return item.EnclosingClass;
    //    }

    //    protected override void SetOwner(NestedClass item, TypeDefinition owner)
    //    {
    //        item.EnclosingClass = owner;
    //    }
    //}

    public class SecurityDeclarationCollection : KeyedMemberCollection<IHasSecurityAttribute, SecurityDeclaration>
    {
        public SecurityDeclarationCollection(IHasSecurityAttribute owner)
            : base(owner, MetadataTokenType.DeclSecurity, CodedIndex.HasDeclSecurity, 1)
        {
        }

        protected override IHasSecurityAttribute GetOwner(SecurityDeclaration item)
        {
            return item.Parent;
        }

        protected override void SetOwner(SecurityDeclaration item, IHasSecurityAttribute owner)
        {
            item.Parent = owner;
        }
    }

    //public class GenericParameterCollection : MetadataMemberCollection<IGenericParameterProvider, GenericParameter>
    //{
    //    public GenericParameterCollection(IGenericParameterProvider owner)
    //        : base(CodedIndex.TypeOrMethodDef, 2, owner)
    //    {
    //    }

    //    protected override IGenericParameterProvider GetOwner(GenericParameter item)
    //    {
    //        return item.Owner;
    //    }

    //    protected override void SetOwner(GenericParameter item, IGenericParameterProvider owner)
    //    {
    //        item.Owner = owner;
    //    }
    //}

    //public class GenericParameterConstraintCollection : MetadataMemberCollection<GenericParameter, GenericParameterConstraint>
    //{
    //    public GenericParameterConstraintCollection(GenericParameter owner)
    //        : base(MetadataTokenType.GenericParam, 0, owner)
    //    {
    //    }

    //    protected override GenericParameter GetOwner(GenericParameterConstraint item)
    //    {
    //        return item.Owner;
    //    }

    //    protected override void SetOwner(GenericParameterConstraint item, GenericParameter owner)
    //    {
    //        item.Owner = owner;
    //    }
    //}

    //public class InterfaceImplementationCollection : MetadataMemberCollection<TypeDefinition, InterfaceImplementation>
    //{
    //    public InterfaceImplementationCollection(TypeDefinition owner)
    //        : base(MetadataTokenType.TypeDef, 0, owner)
    //    {
    //    }

    //    protected override TypeDefinition GetOwner(InterfaceImplementation item)
    //    {
    //        return item.Class;
    //    }

    //    protected override void SetOwner(InterfaceImplementation item, TypeDefinition owner)
    //    {
    //        item.Class = owner;
    //    }
    //}

    //public class MethodImplementationCollection : MetadataMemberCollection<TypeDefinition, MethodImplementation>
    //{
    //    public MethodImplementationCollection(TypeDefinition owner)
    //        : base(MetadataTokenType.TypeDef, 0, owner)
    //    {
    //    }

    //    protected override TypeDefinition GetOwner(MethodImplementation item)
    //    {
    //        return item.Class;
    //    }

    //    protected override void SetOwner(MethodImplementation item, TypeDefinition owner)
    //    {
    //        item.Class = owner;
    //    }
    //}
}
