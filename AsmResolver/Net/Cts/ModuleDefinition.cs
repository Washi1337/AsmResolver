using System;
using AsmResolver.Collections.Generic;
using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts
{
    /// <summary>
    /// Represents a single module in a .NET assembly.
    /// </summary>
    public class ModuleDefinition : MetadataMember<MetadataRow<ushort, uint, uint, uint, uint>>, IHasCustomAttribute, IResolutionScope
    {
        private readonly LazyValue<string> _name;
        private readonly LazyValue<Guid> _mvid;
        private readonly LazyValue<Guid> _encId;
        private readonly LazyValue<Guid> _encBaseId;
        private CustomAttributeCollection _customAttributes;

        public ModuleDefinition(string name)
            : base(null, new MetadataToken(MetadataTokenType.Module))
        {
            _name = new LazyValue<string>(name);
            _mvid = new LazyValue<Guid>();
            _encId = new LazyValue<Guid>();
            _encBaseId = new LazyValue<Guid>();
            Types = new DelegatedMemberCollection<ModuleDefinition, TypeDefinition>(this, GetTypeOwner, SetTypeOwner);
        }

        internal ModuleDefinition(MetadataImage image, MetadataRow<ushort, uint, uint, uint, uint> row)
            : base(image, row.MetadataToken)
        {
            var header = image.Header;
            var tableStream = header.GetStream<TableStream>();
            var stringStream = header.GetStream<StringStream>();
            var guidStream = header.GetStream<GuidStream>();

            Generation = row.Column1;
           _name = new LazyValue<string>(() => stringStream.GetStringByOffset(row.Column2));
           _mvid = new LazyValue<Guid>(() => guidStream.GetGuidByOffset(row.Column3));
           _encId = new LazyValue<Guid>(() => guidStream.GetGuidByOffset(row.Column4));
           _encBaseId = new LazyValue<Guid>(() => guidStream.GetGuidByOffset(row.Column5));
            Types = new TableMemberCollection<ModuleDefinition, TypeDefinition>(this, tableStream.GetTable(MetadataTokenType.TypeDef), GetTypeOwner, SetTypeOwner);
        }

        public ushort Generation
        {
            get;
            set;
        }

        public string Name
        {
            get { return _name.Value; }
            set { _name.Value = value; }
        }

        public Guid Mvid
        {
            get { return _mvid.Value; }
            set { _mvid.Value = value; }
        }

        public Guid EncId
        {
            get { return _encId.Value; }
            set { _encId.Value = value; }
        }

        public Guid EncBaseId
        {
            get { return _encBaseId.Value; }
            set { _encBaseId.Value = value; }
        }

        public CustomAttributeCollection CustomAttributes
        {
            get
            {
                if (_customAttributes != null)
                    return _customAttributes;
                _customAttributes = new CustomAttributeCollection(this);
                return _customAttributes;
            }
        }

        public AssemblyDefinition Assembly
        {
            get;
            internal set;
        }

        public Collection<TypeDefinition> Types
        {
            get;
            private set;
        }

        private static ModuleDefinition GetTypeOwner(TypeDefinition type)
        {
            return type.Module;
        }

        private static void SetTypeOwner(TypeDefinition type, ModuleDefinition module)
        {
            type.Module = module;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
