using AsmResolver.DotNet.Code.Cil;
using Xunit;

namespace AsmResolver.DotNet.Tests.Code.Cil
{
    public class CilLocalVariableCollectionTest
    {
        private readonly CilLocalVariableCollection _collection = new CilLocalVariableCollection();
        private readonly ModuleDefinition _module = new ModuleDefinition("Test.dll");
        
        private void AssertCollectionIndicesAreConsistent(CilLocalVariableCollection collection)
        {
            for (int i = 0; i < collection.Count; i++)
                Assert.Equal(i, collection[i].Index);
        }

        [Fact]
        public void NoIndex()
        {
            var variable = new CilLocalVariable(_module.CorLibTypeFactory.Object);
            Assert.Equal(-1, variable.Index);
        }
        
        
        [Fact]
        public void AddToEmptyListShouldSetIndexToZero()
        {
            _collection.Add(new CilLocalVariable(_module.CorLibTypeFactory.Object));
            AssertCollectionIndicesAreConsistent(_collection);
        }

        [Fact]
        public void AddToEndOfListShouldSetIndexToCount()
        {
            _collection.Add(new CilLocalVariable(_module.CorLibTypeFactory.Object));
            _collection.Add(new CilLocalVariable(_module.CorLibTypeFactory.Object));
            _collection.Add(new CilLocalVariable(_module.CorLibTypeFactory.Object));
            
            _collection.Add(new CilLocalVariable(_module.CorLibTypeFactory.Object));
            AssertCollectionIndicesAreConsistent(_collection);
        }

        [Fact]
        public void InsertShouldUpdateIndices()
        {
            _collection.Add(new CilLocalVariable(_module.CorLibTypeFactory.Object));
            _collection.Add(new CilLocalVariable(_module.CorLibTypeFactory.Object));
            _collection.Add(new CilLocalVariable(_module.CorLibTypeFactory.Object));

            _collection.Insert(1, new CilLocalVariable(_module.CorLibTypeFactory.String));
            AssertCollectionIndicesAreConsistent(_collection);
        }

        [Fact]
        public void RemoveShouldUpdateIndices()
        {
            _collection.Add(new CilLocalVariable(_module.CorLibTypeFactory.Object));
            _collection.Add(new CilLocalVariable(_module.CorLibTypeFactory.Object));
            _collection.Add(new CilLocalVariable(_module.CorLibTypeFactory.Object));
            _collection.Add(new CilLocalVariable(_module.CorLibTypeFactory.Object));

            var variable = _collection[1];
            _collection.RemoveAt(1);
            AssertCollectionIndicesAreConsistent(_collection);
            Assert.Equal(-1, variable.Index);
        }

        [Fact]
        public void ClearingVariablesShouldUpdateAllIndices()
        {
            var variables = new[]
            {
                new CilLocalVariable(_module.CorLibTypeFactory.Object),
                new CilLocalVariable(_module.CorLibTypeFactory.Object),
                new CilLocalVariable(_module.CorLibTypeFactory.Object),
                new CilLocalVariable(_module.CorLibTypeFactory.Object)
            };

            foreach (var variable in variables)
                _collection.Add(variable);
            
            _collection.Clear();

            foreach (var variable in variables)
                Assert.Equal(-1, variable.Index);
        }
    }
}