using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SGP.Domain.Entities;
using SGP.Domain.Repositories;
using SGP.Infrastructure.Context;
using SGP.Infrastructure.Repositories;
using SGP.Tests.Fixtures;
using System.Threading.Tasks;
using Xunit;
using Xunit.Categories;

namespace SGP.Tests.UnitTests.Infrastructure.Repositories
{
    [Category(TestCategories.Infrastructure)]
    public class CidadeRepositoryTests : IClassFixture<EfFixture>
    {
        private readonly EfFixture _fixture;

        public CidadeRepositoryTests(EfFixture fixture)
        {
            _fixture = fixture;
            Task.Run(() => SeedAsync()).Wait();
        }

        [Theory]
        [UnitTest]
        [InlineData("SP", 645)]
        [InlineData("RJ", 92)]
        [InlineData("DF", 1)]
        public async Task Should_ReturnsCities_WhenGetByExistingState(string state, int expectedCount)
        {
            // Arrange
            var repository = CreateRepository();

            // Act
            var act = await repository.GetAllAsync(state);

            // Assert
            act.Should().NotBeNullOrEmpty()
                .And.OnlyHaveUniqueItems()
                .And.HaveCount(expectedCount);
        }

        [Fact]
        [UnitTest]
        public async Task Should_ReturnsAllStates_WhenGetAllStates()
        {
            // Arrange
            var repository = CreateRepository();

            // Act
            var act = await repository.GetAllEstadosAsync();

            // Assert
            act.Should().NotBeNullOrEmpty()
                .And.OnlyHaveUniqueItems()
                .And.BeInAscendingOrder()
                .And.HaveCount(27);
        }

        [Fact]
        [UnitTest]
        public async Task Should_ReturnsCity_WhenGetByExistingIbge()
        {
            // Arrange
            var repository = CreateRepository();
            const string ibge = "3557105";

            // Act
            var act = await repository.GetByIbgeAsync(ibge);

            // Assert
            act.Should().NotBeNull().And.BeEquivalentTo(new Cidade(ibge, "SP", "Votuporanga"));
        }

        [Theory]
        [UnitTest]
        [InlineData("")]
        [InlineData("0")]
        [InlineData("00000")]
        [InlineData("ab2c3")]
        public async Task Should_ReturnsNull_WhenGetByInexistingIbge(string ibge)
        {
            // Arrange
            var repository = CreateRepository();

            // Act
            var act = await repository.GetByIbgeAsync(ibge);

            // Assert
            act.Should().BeNull();
        }

        private ICidadeRepository CreateRepository()
        {
            return new CidadeRepository(_fixture.Context);
        }

        private async Task SeedAsync()
        {
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            loggerFactoryMock
                .Setup(s => s.CreateLogger(It.IsAny<string>()))
                .Returns(Mock.Of<ILogger>());

            await _fixture.Context.EnsureSeedDataAsync(loggerFactoryMock.Object);
        }
    }
}