using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Collections.Generic;

namespace AsmResolver.Net.Metadata
{
    public abstract class MetadataMemberCollection<TOwner, TItem> : Collection<TItem>
        where TOwner : class, IMetadataMember
        where TItem : MetadataMember
    {
        private readonly TOwner _owner;
        private readonly IndexEncoder _indexEncoder;
        private readonly int _columnIndex;

        internal MetadataMemberCollection(CodedIndex codedIndex, int columnIndex, TOwner owner)
            : this (null, columnIndex, owner)
        {
            if (_owner.Header != null)
            {
                var stream = _owner.Header.GetStream<TableStream>();
                _indexEncoder = stream.GetIndexEncoder(codedIndex);
            }
        }

        internal MetadataMemberCollection(MetadataTokenType tokenType, int columnIndex, TOwner owner)
            : this(null, columnIndex, owner)
        {
            if (_owner.Header != null)
            {
                var stream = _owner.Header.GetStream<TableStream>();
                _indexEncoder = new IndexEncoder(stream, tokenType);
            }
        }

        internal MetadataMemberCollection(IndexEncoder encoder, int columnIndex, TOwner owner)
        {
            _indexEncoder = encoder;
            _columnIndex = columnIndex;
            _owner = owner;
        }

        protected abstract TOwner GetOwner(TItem item);

        protected abstract void SetOwner(TItem item, TOwner owner);

        protected override void Initialize()
        {
            if (_owner.Header != null && _indexEncoder != null)
            {
                var stream = _owner.Header.GetStream<TableStream>();
                var attributeTable = stream.GetTable<TItem>();
                if (attributeTable != null)
                {
                    uint key = _indexEncoder.EncodeToken(_owner.MetadataToken);
                    var member = attributeTable.GetMemberByKey(_columnIndex, key);

                    if (member != null)
                    {
                        int startIndex = (int) member.MetadataToken.Rid - 1;
                        while (startIndex > 0 && Convert.ToUInt32(attributeTable[startIndex - 1].MetadataRow.GetAllColumns().ElementAt(_columnIndex)) == key)
                            startIndex--;
                        
                        for (int index = startIndex; index < attributeTable.Count; index++)
                        {
                            var item = attributeTable[index];
                            if (Convert.ToUInt32(item.MetadataRow.GetAllColumns().ElementAt(_columnIndex)) != key)
                                break;
                            Items.Add(item);
                        }
                    }
                }
            }

            base.Initialize();
        }

        private void AssertHasNoOwner(TItem item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            if (GetOwner(item) != null)
                throw new InvalidOperationException("Item is already added to another member.");
        }

        protected override void ClearItems()
        {
            foreach (var item in Items)
                SetOwner(item, null);
            base.ClearItems();
        }

        protected override void InsertItem(int index, TItem item)
        {
            AssertHasNoOwner(item);
            base.InsertItem(index, item);
            SetOwner(item, _owner);
        }

        protected override void RemoveItem(int index)
        {
            SetOwner(Items[index], null);
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, TItem item)
        {
            AssertHasNoOwner(item);
            SetOwner(Items[index], null);
            base.SetItem(index, item);
            SetOwner(item, _owner);
        }
    }

    public class CustomAttributeCollection : MetadataMemberCollection<IHasCustomAttribute, CustomAttribute>
    {
        public CustomAttributeCollection(IHasCustomAttribute owner)
            : base(CodedIndex.HasCustomAttribute, 0, owner)
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

    public class MethodSemanticsCollection : MetadataMemberCollection<IHasSemantics, MethodSemantics>
    {
        public MethodSemanticsCollection(IHasSemantics owner)
            : base(CodedIndex.HasSemantics, 2, owner)
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

    public class NestedClassCollection : MetadataMemberCollection<TypeDefinition, NestedClass>
    {
        public NestedClassCollection(TypeDefinition owner)
            : base(MetadataTokenType.TypeDef, 1, owner)
        {
        }

        protected override TypeDefinition GetOwner(NestedClass item)
        {
            return item.EnclosingClass;
        }

        protected override void SetOwner(NestedClass item, TypeDefinition owner)
        {
            item.EnclosingClass = owner;
        }
    }

    public class SecurityDeclarationCollection : MetadataMemberCollection<IHasSecurityAttribute, SecurityDeclaration>
    {
        public SecurityDeclarationCollection(IHasSecurityAttribute owner)
            : base(CodedIndex.HasDeclSecurity, 1, owner)
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

    public class GenericParameterCollection : MetadataMemberCollection<IGenericParameterProvider, GenericParameter>
    {
        public GenericParameterCollection(IGenericParameterProvider owner)
            : base(CodedIndex.TypeOrMethodDef, 2, owner)
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

    public class GenericParameterConstraintCollection : MetadataMemberCollection<GenericParameter, GenericParameterConstraint>
    {
        public GenericParameterConstraintCollection(GenericParameter owner)
            : base(MetadataTokenType.GenericParam, 0, owner)
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

    public class InterfaceImplementationCollection : MetadataMemberCollection<TypeDefinition, InterfaceImplementation>
    {
        public InterfaceImplementationCollection(TypeDefinition owner)
            : base(MetadataTokenType.TypeDef, 0, owner)
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

    public class MethodImplementationCollection : MetadataMemberCollection<TypeDefinition, MethodImplementation>
    {
        public MethodImplementationCollection(TypeDefinition owner)
            : base(MetadataTokenType.TypeDef, 0, owner)
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
