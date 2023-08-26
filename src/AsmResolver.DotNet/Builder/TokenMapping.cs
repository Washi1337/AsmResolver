using System;
using System.Collections.Generic;
using AsmResolver.Collections;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Builder
{
    /// <summary>
    /// Provides a default implementation of the <see cref="ITokenMapping"/> interface.
    /// </summary>
    public class TokenMapping : ITokenMapping
    {
        private readonly OneToOneRelation<TypeDefinition, MetadataToken> _typeDefTokens = new();
        private readonly Dictionary<FieldDefinition, MetadataToken> _fieldTokens = new();
        private readonly OneToOneRelation<MethodDefinition, MetadataToken> _methodTokens = new();
        private readonly Dictionary<ParameterDefinition, MetadataToken> _parameterTokens = new();
        private readonly Dictionary<PropertyDefinition, MetadataToken> _propertyTokens = new();
        private readonly Dictionary<EventDefinition, MetadataToken> _eventTokens = new();
        private readonly Dictionary<IMetadataMember, MetadataToken> _remainingTokens = new();

        /// <inheritdoc />
        public MetadataToken this[IMetadataMember member] => member.MetadataToken.Table switch
        {
            TableIndex.TypeDef => _typeDefTokens.GetValue((TypeDefinition) member),
            TableIndex.Field => _fieldTokens[(FieldDefinition) member],
            TableIndex.Method => _methodTokens.GetValue((MethodDefinition) member),
            TableIndex.Param => _parameterTokens[(ParameterDefinition) member],
            TableIndex.Event => _eventTokens[(EventDefinition) member],
            TableIndex.Property => _propertyTokens[(PropertyDefinition) member],
            _ => _remainingTokens[member]
        };

        /// <inheritdoc />
        public bool TryGetNewToken(IMetadataMember member, out MetadataToken token)
        {
            switch (member.MetadataToken.Table)
            {
                case TableIndex.TypeDef:
                    token = _typeDefTokens.GetValue((TypeDefinition) member);
                    return token.Rid != 0;
                case TableIndex.Field:
                    return _fieldTokens.TryGetValue((FieldDefinition) member, out token);
                case TableIndex.Method:
                    token = _methodTokens.GetValue((MethodDefinition) member);
                    return token.Rid != 0;
                case TableIndex.Param:
                    return _parameterTokens.TryGetValue((ParameterDefinition) member, out token);
                case TableIndex.Event:
                    return _eventTokens.TryGetValue((EventDefinition) member, out token);
                case TableIndex.Property:
                    return _propertyTokens.TryGetValue((PropertyDefinition) member, out token);
                default:
                    return _remainingTokens.TryGetValue(member, out token);
            }
        }

        /// <summary>
        /// Maps a single member to a new metadata token.
        /// </summary>
        /// <param name="member">The member to assign a token to.</param>
        /// <param name="newToken">The new token.</param>
        public void Register(IMetadataMember member, MetadataToken newToken)
        {
            if (member.MetadataToken.Table != newToken.Table)
                throw new ArgumentException($"Cannot assign a {newToken.Table} metadata token to a {member.MetadataToken.Table}.");

            // We allow for members to be duplicated in the mapping, but we will only keep track of the first token that
            // was registered for this member. This may result in a slightly different binary if token preservation is
            // enabled (e.g., a member may originally have referenced a duplicated member as opposed to the first one,
            // and this would always make it reference the first one), but since both members are semantically equivalent,
            // referencing one or the other should also be semantics-preserving. Note that both duplicated members will
            // still be present in the final binary with their original RIDs.
            if (TryGetNewToken(member, out _))
                return;

            switch (member.MetadataToken.Table)
            {
                case TableIndex.TypeDef:
                    _typeDefTokens.Add((TypeDefinition) member, newToken);
                    break;
                case TableIndex.Field:
                    _fieldTokens.Add((FieldDefinition) member, newToken);
                    break;
                case TableIndex.Method:
                    _methodTokens.Add((MethodDefinition) member, newToken);
                    break;
                case TableIndex.Param:
                    _parameterTokens.Add((ParameterDefinition) member, newToken);
                    break;
                case TableIndex.Event:
                    _eventTokens.Add((EventDefinition) member, newToken);
                    break;
                case TableIndex.Property:
                    _propertyTokens.Add((PropertyDefinition) member, newToken);
                    break;
                default:
                    _remainingTokens.Add(member, newToken);
                    break;
            }
        }

        /// <summary>
        /// Gets the type assigned to the provided metadata token.
        /// </summary>
        /// <param name="newToken">The new token.</param>
        /// <returns>The type, or <c>null</c> if no type is assigned to the provided token.</returns>
        public TypeDefinition? GetTypeByToken(MetadataToken newToken) => _typeDefTokens.GetKey(newToken);

        /// <summary>
        /// Gets the method assigned to the provided metadata token.
        /// </summary>
        /// <param name="newToken">The new token.</param>
        /// <returns>The type, or <c>null</c> if no method is assigned to the provided token.</returns>
        public MethodDefinition? GetMethodByToken(MetadataToken newToken) => _methodTokens.GetKey(newToken);
    }
}
