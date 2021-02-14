using AsmResolver.Workspaces.Tests.Mock;
using Xunit;

namespace AsmResolver.Workspaces.Tests
{
    public class AnalyzerRepositoryTest
    {
        private readonly AnalyzerRepository _repository;

        public AnalyzerRepositoryTest()
        {
            _repository = new AnalyzerRepository();
        }

        [Fact]
        public void RegisteredAnalyzerShouldAppearInRepository()
        {
            var analyzer = GenericAnalyzer<string>.Instance;
            _repository.Register(typeof(string), analyzer);
            Assert.Contains(analyzer, _repository.GetAnalyzers(typeof(string)));
        }

        [Fact]
        public void GetAnalyzersShouldGetAllAnalyzersOfBaseTypes()
        {
            var analyzer = GenericAnalyzer<object>.Instance;
            _repository.Register(typeof(object), analyzer);
            Assert.Contains(analyzer, _repository.GetAnalyzers(typeof(string)));
        }

        [Fact]
        public void GetAnalyzersShouldNotGetAllAnalyzersOfDerivedTypes()
        {
            var analyzer = GenericAnalyzer<string>.Instance;
            _repository.Register(typeof(string), analyzer);
            Assert.DoesNotContain(analyzer, _repository.GetAnalyzers(typeof(object)));
        }

        [Fact]
        public void RegisterMultipleAnalyzersForTheSameType()
        {
            var analyzer = GenericAnalyzer<string>.Instance;
            var analyzer2 = new GenericAnalyzer<string>();
            _repository.Register(typeof(string), analyzer);
            _repository.Register(typeof(string), analyzer2);

            Assert.Contains(analyzer, _repository.GetAnalyzers(typeof(string)));
            Assert.Contains(analyzer2, _repository.GetAnalyzers(typeof(string)));
        }
    }
}
