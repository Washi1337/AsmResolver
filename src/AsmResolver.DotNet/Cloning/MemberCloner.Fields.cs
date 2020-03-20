using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Cil;

namespace AsmResolver.DotNet.Cloning
{
    public partial class MemberCloner
    {
        private void CreateFieldStubs(MemberCloneContext context)
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
        
        private FieldDefinition CreateFieldStub(MemberCloneContext context, FieldDefinition field)
        {
            var clonedField = new FieldDefinition(
                field.Name, 
                field.Attributes,
                context.Importer.ImportFieldSignature(field.Signature));

            context.ClonedMembers.Add(field, clonedField);
            return clonedField;
        }

        private void DeepCopyFields(MemberCloneContext context)
        {
            foreach (var field in _fieldsToClone)
                DeepCopyField(context, field);
        }

        private void DeepCopyField(MemberCloneContext context, FieldDefinition field)
        {
            var clonedField = (FieldDefinition) context.ClonedMembers[field];
            CloneCustomAttributes(context, field, clonedField);
            clonedField.ImplementationMap = CloneImplementationMap(context, field.ImplementationMap);
            clonedField.Constant = CloneConstant(context, field.Constant);
            clonedField.FieldRva = FieldRvaCloner.CloneFieldRvaData(field);
        }

    }
}