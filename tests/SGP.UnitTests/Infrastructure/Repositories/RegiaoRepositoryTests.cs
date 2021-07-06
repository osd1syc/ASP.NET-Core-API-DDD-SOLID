using FluentAssertions;
using SGP.Domain.Repositories;
using SGP.Infrastructure.Repositories;
using SGP.SharedTests.Constants;
using SGP.SharedTests.Extensions;
using SGP.UnitTests.Fixtures;
using System.Threading.Tasks;
using Xunit;
using Xunit.Categories;

namespace SGP.UnitTests.Infrastructure.Repositories
{
    [UnitTest(TestCategories.Infrastructure)]
    public class RegiaoRepositoryTests : IClassFixture<EfSqliteFixture>
    {
        private readonly EfSqliteFixture _fixture;

        public RegiaoRepositoryTests(EfSqliteFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Devera_ObterTodasRegioes_RetornaRegioes()
        {
            // Arrange
            await _fixture.SeedDataAsync();
            var repository = CriarRepositorio();

            // Act
            var actual = await repository.ObterTodosAsync();

            // Assert
            actual.Should().NotBeEmpty()
                .And.OnlyHaveUniqueItems()
                .And.HaveCount(Totais.Regioes)
                .And.Subject.ForEach(regiao =>
                {
                    regiao.Id.Should().NotBeEmpty();
                    regiao.Nome.Should().NotBeNullOrWhiteSpace();
                });
        }

        private IRegiaoRepository CriarRepositorio()
            => new RegiaoRepository(_fixture.Context);
    }
}