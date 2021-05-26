using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Strings;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="EventDefinition"/>  that is read from a
    /// .NET metadata image.
    /// </summary>
    public class SerializedEventDefinition : EventDefinition
    {
        private readonly ModuleReaderContext _context;
        private readonly EventDefinitionRow _row;

        /// <summary>
        /// Creates a event definition from a event metadata row.
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="token">The token to initialize the event for.</param>
        /// <param name="row">The metadata table row to base the event definition on.</param>
        public SerializedEventDefinition(ModuleReaderContext context, MetadataToken token, in EventDefinitionRow row)
            : base(token)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _row = row;

            Attributes = row.Attributes;
        }

        /// <inheritdoc />
        protected override string GetName() => _context.Image.DotNetDirectory.Metadata
            .GetStream<StringsStream>()
            .GetStringByIndex(_row.Name);

        /// <inheritdoc />
        protected override ITypeDefOrRef GetEventType()
        {
            var encoder =  _context.Image.DotNetDirectory.Metadata
                .GetStream<TablesStream>()
                .GetIndexEncoder(CodedIndex.TypeDefOrRef);

            var eventTypeToken = encoder.DecodeIndex(_row.EventType);
            return _context.ParentModule.TryLookupMember(eventTypeToken, out var member)
                ? member as ITypeDefOrRef
                : _context.BadImageAndReturn<ITypeDefOrRef>($"Invalid event type referenced by event {MetadataToken.ToString()}.");
        }

        /// <inheritdoc />
        protected override TypeDefinition GetDeclaringType()
        {
            var module = _context.ParentModule;
            var declaringTypeToken = new MetadataToken(TableIndex.TypeDef, module.GetEventDeclaringType(MetadataToken.Rid));
            return module.TryLookupMember(declaringTypeToken, out var member)
                ? member as TypeDefinition
                : _context.BadImageAndReturn<TypeDefinition>(
                    $"Event {MetadataToken.ToString()} is not added to an event map of a declaring type.");
        }

        /// <inheritdoc />
        protected override IList<MethodSemantics> GetSemantics()
        {
            var result = new MethodSemanticsCollection(this);
            result.ValidateMembership = false;

            var module = _context.ParentModule;
            foreach (uint rid in module.GetMethodSemantics(MetadataToken))
            {
                var semanticsToken = new MetadataToken(TableIndex.MethodSemantics, rid);
                result.Add((MethodSemantics) module.LookupMember(semanticsToken));
            }

            result.ValidateMembership = true;
            return result;
        }

        /// <inheritdoc />
        protected override IList<CustomAttribute> GetCustomAttributes() =>
            _context.ParentModule.GetCustomAttributeCollection(this);
    }
}
