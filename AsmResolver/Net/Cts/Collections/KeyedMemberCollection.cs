using System;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts.Collections
{
    public abstract class KeyedMemberCollection<TOwner, TMember> : MemberCollection<TOwner, TMember>
        where TOwner : class, IMetadataMember
        where TMember : IMetadataMember
    {
        private readonly MetadataTokenType _itemTable;
        private readonly IndexEncoder _indexEncoder;
        private readonly int _keyColumnIndex;

        internal KeyedMemberCollection(TOwner owner, MetadataTokenType itemTable, CodedIndex codedIndex, int keyColumnIndex)
            : this (owner, itemTable, null, keyColumnIndex)
        {
            if (Owner.Image != null)
                _indexEncoder = Owner.Image.Header.GetStream<TableStream>().GetIndexEncoder(codedIndex);
        }

        internal KeyedMemberCollection(TOwner owner, MetadataTokenType itemTable, MetadataTokenType ownerTable, int keyColumnIndex)
            : this(owner, itemTable, null, keyColumnIndex)
        {
            if (Owner.Image != null)
                _indexEncoder = new IndexEncoder(Owner.Image.Header.GetStream<TableStream>(), ownerTable);
        }

        internal KeyedMemberCollection(TOwner owner, MetadataTokenType itemTable, IndexEncoder encoder, int keyColumnIndex)
            : base(owner)
        {
            _itemTable = itemTable;
            _indexEncoder = encoder;
            _keyColumnIndex = keyColumnIndex;
        }

        protected override void Initialize()
        {
            if (Owner.Image != null && _indexEncoder != null)
            {
                var stream = Owner.Image.Header.GetStream<TableStream>();
                var itemTable = stream.GetTable(_itemTable);
                if (itemTable != null)
                {
                    uint key = _indexEncoder.EncodeToken(Owner.MetadataToken);
                    var row = itemTable.GetRowByKey(_keyColumnIndex, key);

                    if (row != null)
                    {
                        int startIndex = (int) row.MetadataToken.Rid - 1;
                        while (startIndex > 0 && Convert.ToUInt32(itemTable.GetRow(startIndex - 1).GetAllColumns()[_keyColumnIndex]) == key)
                            startIndex--;
                        
                        for (int index = startIndex; index < itemTable.Count; index++)
                        {
                            var item = itemTable.GetRow(index);
                            if (Convert.ToUInt32(item.GetAllColumns()[_keyColumnIndex]) != key)
                                break;
                            var member = (TMember) itemTable.GetMemberFromRow(Owner.Image, item);
                            Items.Add(member);
                            SetOwner(member, Owner);
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

    public class MethodSemanticsCollection : KeyedMemberCollection<IHasSemantics, MethodSemantics>
    {
        public MethodSemanticsCollection(IHasSemantics owner)
            : base(owner, MetadataTokenType.MethodSemantics, CodedIndex.HasSemantics, 2)
        {
        }

        protected override IHasSemantics GetOwner(MethodSemantics item)
        {
            return item.Association;
        }

        protected override void SetOwner(MethodSemantics item, IHasSemantics owner)
        {
            item.Association = owner;
        }
    }

    public class NestedClassCollection : KeyedMemberCollection<TypeDefinition, NestedClass>
    {
        public NestedClassCollection(TypeDefinition owner)
            : base(owner, MetadataTokenType.NestedClass, MetadataTokenType.TypeDef, 1)
        {
        }

        protected override TypeDefinition GetOwner(NestedClass item)
        {
            return item.EnclosingClass;
        }

        protected override void SetOwner(NestedClass item, TypeDefinition owner)
        {
            item.EnclosingClass = owner;
            item.Class.Module = owner == null ? null : owner.Module;
        }
    }

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

    public class GenericParameterCollection : KeyedMemberCollection<IGenericParameterProvider, GenericParameter>
    {
        public GenericParameterCollection(IGenericParameterProvider owner)
            : base(owner, MetadataTokenType.GenericParam, CodedIndex.TypeOrMethodDef, 2)
        {
        }

        protected override IGenericParameterProvider GetOwner(GenericParameter item)
        {
            return item.Owner;
        }

        protected override void SetOwner(GenericParameter item, IGenericParameterProvider owner)
        {
            item.Owner = owner;
        }
    }

    public class GenericParameterConstraintCollection : KeyedMemberCollection<GenericParameter, GenericParameterConstraint>
    {
        public GenericParameterConstraintCollection(GenericParameter owner)
            : base(owner, MetadataTokenType.GenericParamConstraint, MetadataTokenType.GenericParam, 0)
        {
        }

        protected override GenericParameter GetOwner(GenericParameterConstraint item)
        {
            return item.Owner;
        }

        protected override void SetOwner(GenericParameterConstraint item, GenericParameter owner)
        {
            item.Owner = owner;
        }
    }

    public class InterfaceImplementationCollection : KeyedMemberCollection<TypeDefinition, InterfaceImplementation>
    {
        public InterfaceImplementationCollection(TypeDefinition owner)
            : base(owner, MetadataTokenType.InterfaceImpl, MetadataTokenType.TypeDef, 0)
        {
        }

        protected override TypeDefinition GetOwner(InterfaceImplementation item)
        {
            return item.Class;
        }

        protected override void SetOwner(InterfaceImplementation item, TypeDefinition owner)
        {
            item.Class = owner;
        }
    }

    public class MethodImplementationCollection : KeyedMemberCollection<TypeDefinition, MethodImplementation>
    {
        public MethodImplementationCollection(TypeDefinition owner)
            : base(owner, MetadataTokenType.MethodImpl, MetadataTokenType.TypeDef, 0)
        {
        }

        protected override TypeDefinition GetOwner(MethodImplementation item)
        {
            return item.Class;
        }

        protected override void SetOwner(MethodImplementation item, TypeDefinition owner)
        {
            item.Class = owner;
        }
    }
}
