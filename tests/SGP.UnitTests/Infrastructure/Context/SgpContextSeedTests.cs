using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SGP.Infrastructure.Context;
using SGP.SharedTests.Constants;
using SGP.SharedTests.Mocks;
using SGP.UnitTests.Fixtures;
using System.Threading.Tasks;
using Xunit;
using Xunit.Categories;

namespace SGP.UnitTests.Infrastructure.Context
{
    [UnitTest(TestCategories.Infrastructure)]
    public class SgpContextSeedTests : IClassFixture<EfSqliteFixture>
    {
        private readonly EfSqliteFixture _fixture;

        public SgpContextSeedTests(EfSqliteFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Should_ReturnsRowsAffected_WhenEnsureSeedData()
        {
            // Arrange
            var context = _fixture.Context;

            // Act
            var actual = await context.EnsureSeedDataAsync(LoggerFactoryMock.Create());
            var totalRegioes = await context.Regioes.AsNoTracking().LongCountAsync();
            var totalEstados = await context.Estados.AsNoTracking().LongCountAsync();
            var totalCidades = await context.Cidades.AsNoTracking().LongCountAsync();

            // Assert
            actual.Should().Be(totalRegioes + totalEstados + totalCidades);
            totalRegioes.Should().Be(Totais.Regioes);
            totalEstados.Should().Be(Totais.Estados);
            totalCidades.Should().Be(Totais.Cidades);
        }
    }
}