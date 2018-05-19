using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using AsmResolver.Net;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Signatures;
using Xunit;
using CustomAttributeNamedArgument = AsmResolver.Net.Signatures.CustomAttributeNamedArgument;

namespace AsmResolver.Tests.Net.Cts
{
    public class CustomAttributeTest
    {
        private const string DummyAssemblyName = "SomeAssembly";
        private readonly SignatureComparer _comparer = new SignatureComparer();

        [Fact]
        public void PersistentParent()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;
            var image = header.LockMetadata();
            var importer = new ReferenceImporter(image);

            var customAttribute = new CustomAttribute(
                (ICustomAttributeType) importer.ImportMethod(
                    typeof(AssemblyTitleAttribute).GetConstructor(new[] {typeof(string)})),
                new CustomAttributeSignature(new[]
                    {new CustomAttributeArgument(image.TypeSystem.String, new ElementSignature("SomeArgument"))}));
            
            image.Assembly.CustomAttributes.Add(customAttribute);

            var mapping = header.UnlockMetadata();

            image = header.LockMetadata();
            customAttribute = (CustomAttribute) image.ResolveMember(mapping[customAttribute]); 
            Assert.Equal(1, image.Assembly.CustomAttributes.Count);
            Assert.Same(customAttribute.Parent, image.Assembly);
        }
        
        [Fact]
        public void PersistentConstructor()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;
            var image = header.LockMetadata();
            var importer = new ReferenceImporter(image);

            var customAttribute = new CustomAttribute(
                (ICustomAttributeType) importer.ImportMethod(
                    typeof(AssemblyTitleAttribute).GetConstructor(new[] {typeof(string)})),
                new CustomAttributeSignature(new[]
                    {new CustomAttributeArgument(image.TypeSystem.String, new ElementSignature("SomeArgument"))}));
            
            image.Assembly.CustomAttributes.Add(customAttribute);

            header.UnlockMetadata();

            image = header.LockMetadata();
            Assert.Equal(1, image.Assembly.CustomAttributes.Count);
            Assert.Equal(customAttribute.Constructor, image.Assembly.CustomAttributes[0].Constructor, _comparer);
        }
        
        [Fact]
        public void PersistentNullArgument()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;
            var image = header.LockMetadata();
            var importer = new ReferenceImporter(image);

            var customAttribute = new CustomAttribute(
                (ICustomAttributeType) importer.ImportMethod(
                    typeof(DisplayNameAttribute).GetConstructor(new[] {typeof(string)})),
                new CustomAttributeSignature(new[]
                    {new CustomAttributeArgument(image.TypeSystem.String, new ElementSignature(null))}));
            
            image.Assembly.CustomAttributes.Add(customAttribute);

            header.UnlockMetadata();

            image = header.LockMetadata();
            Assert.Equal(1, image.Assembly.CustomAttributes.Count);
            Assert.Null(image.Assembly.CustomAttributes[0].Signature.FixedArguments[0].Elements[0].Value);
        }
        
        [Fact]
        public void PersistentStringArgument()
        {
            string argument = "SomeArgument";
            
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;
            var image = header.LockMetadata();
            var importer = new ReferenceImporter(image);


            var customAttribute = new CustomAttribute(
                (ICustomAttributeType) importer.ImportMethod(
                    typeof(DisplayNameAttribute).GetConstructor(new[] {typeof(string)})),
                new CustomAttributeSignature(new[]
                    {new CustomAttributeArgument(image.TypeSystem.String, new ElementSignature(argument))}));
            
            image.Assembly.CustomAttributes.Add(customAttribute);

            header.UnlockMetadata();

            image = header.LockMetadata();
            Assert.Equal(1, image.Assembly.CustomAttributes.Count);
            Assert.Equal(argument, image.Assembly.CustomAttributes[0].Signature.FixedArguments[0].Elements[0].Value);
        }
        
        [Fact]
        public void PersistentTypeArgument()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;
            var image = header.LockMetadata();
            var importer = new ReferenceImporter(image);

            var argument = importer.ImportTypeSignature(typeof(object));
            
            var customAttribute = new CustomAttribute(
                (ICustomAttributeType) importer.ImportMethod(
                    typeof(DesignerAttribute).GetConstructor(new[] {typeof(Type)})),
                new CustomAttributeSignature(new[]
                {
                    new CustomAttributeArgument(image.TypeSystem.Type,
                        new ElementSignature(argument))
                }));
            
            image.Assembly.CustomAttributes.Add(customAttribute);

            header.UnlockMetadata();
            
            image = header.LockMetadata();
            Assert.Equal(1, image.Assembly.CustomAttributes.Count);
            Assert.Equal(argument, image.Assembly.CustomAttributes[0].Signature.FixedArguments[0].Elements[0].Value as ITypeDescriptor, _comparer);
        }
        
        [Fact]
        public void PersistentEnumArgument()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;
            var image = header.LockMetadata();
            var importer = new ReferenceImporter(image);

            const int argument = (int) DebuggableAttribute.DebuggingModes.EnableEditAndContinue;
            
            var customAttribute = new CustomAttribute(
                (ICustomAttributeType) importer.ImportMethod(
                    typeof(DebuggableAttribute).GetConstructor(new[] {typeof(DebuggableAttribute.DebuggingModes)})),
                new CustomAttributeSignature(new[]
                {
                    new CustomAttributeArgument(importer.ImportTypeSignature(typeof(DebuggableAttribute.DebuggingModes)),
                        new ElementSignature(argument))
                }));
            
            image.Assembly.CustomAttributes.Add(customAttribute);

            header.UnlockMetadata();
            
            image = header.LockMetadata();
            Assert.Equal(1, image.Assembly.CustomAttributes.Count);
            Assert.Equal(argument, image.Assembly.CustomAttributes[0].Signature.FixedArguments[0].Elements[0].Value);
        }

        [Fact]
        public void PersistentNamedArgument()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;
            var image = header.LockMetadata();
            var importer = new ReferenceImporter(image);

            var namedArg = new CustomAttributeNamedArgument(CustomAttributeArgumentMemberType.Property,
                image.TypeSystem.Boolean, "Exclude",
                new CustomAttributeArgument(image.TypeSystem.Boolean, new ElementSignature(true)));

            var customAttribute = new CustomAttribute(
                (ICustomAttributeType) importer.ImportMethod(
                    typeof(ObfuscationAttribute).GetConstructor(Type.EmptyTypes)),
                new CustomAttributeSignature {NamedArguments = {namedArg}});
            
            image.Assembly.CustomAttributes.Add(customAttribute);

            header.UnlockMetadata();
            
            image = header.LockMetadata();
            Assert.Equal(1, image.Assembly.CustomAttributes.Count);
            var newArg = image.Assembly.CustomAttributes[0].Signature.NamedArguments[0];
            Assert.Equal(namedArg.MemberName, newArg.MemberName);
            Assert.Equal(namedArg.ArgumentMemberType, newArg.ArgumentMemberType);
            Assert.Equal(namedArg.Argument.ArgumentType, newArg.Argument.ArgumentType, _comparer);
            Assert.Equal(namedArg.Argument.Elements[0].Value, newArg.Argument.Elements[0].Value);
        }

        [Fact]
        public void PersistentExtraData()
        {
            var assembly = NetAssemblyFactory.CreateAssembly(DummyAssemblyName, true);
            var header = assembly.NetDirectory.MetadataHeader;
            var image = header.LockMetadata();
            var importer = new ReferenceImporter(image);

            var extraData = new byte[] {1, 2, 3, 4};
            
            var customAttribute = new CustomAttribute(
                (ICustomAttributeType) importer.ImportMethod(
                    typeof(ObfuscationAttribute).GetConstructor(Type.EmptyTypes)),
                new CustomAttributeSignature {ExtraData = extraData});
            
            image.Assembly.CustomAttributes.Add(customAttribute);

            header.UnlockMetadata();
            
            image = header.LockMetadata();
            Assert.Equal(extraData, image.Assembly.CustomAttributes[0].Signature.ExtraData);
        }
    }
}