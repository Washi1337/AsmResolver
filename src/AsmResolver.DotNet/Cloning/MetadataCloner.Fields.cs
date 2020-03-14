using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Cil;

namespace AsmResolver.DotNet.Cloning
{
    public partial class MetadataCloner
    {
        private void CreateFieldStubs(MetadataCloneContext context)
        {
            foreach (var field in _fieldsToClone)
            {
                var stub = CreateFieldStub(context, field);
                
                // If field's declaring type is cloned as well, add the cloned method to the cloned type.
                if (context.ClonedMembers.TryGetValue(field.DeclaringType, out var member)
                    && member is TypeDefinition declaringType)
                {
                    declaringType.Fields.Add(stub);
                }
            }
        }
        
        private FieldDefinition CreateFieldStub(MetadataCloneContext context, FieldDefinition field)
        {
            var clonedField = new FieldDefinition(
                field.Name, 
                field.Attributes,
                context.Importer.ImportFieldSignature(field.Signature));

            return clonedField;
        }

        private void DeepCopyFields(MetadataCloneContext context)
        {
            foreach (var field in _fieldsToClone)
                DeepCopyField(context, field);
        }

        private void DeepCopyField(MetadataCloneContext context, FieldDefinition field)
        {
        }
    }
}