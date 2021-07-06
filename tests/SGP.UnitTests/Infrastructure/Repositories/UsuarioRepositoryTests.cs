using Bogus;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SGP.Domain.Entities;
using SGP.Domain.Repositories;
using SGP.Domain.ValueObjects;
using SGP.Infrastructure.Repositories;
using SGP.Infrastructure.UoW;
using SGP.Shared.Interfaces;
using SGP.SharedTests.Constants;
using SGP.SharedTests.Extensions;
using SGP.UnitTests.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Categories;

namespace SGP.UnitTests.Infrastructure.Repositories
{
    [Category(TestCategories.Infrastructure)]
    public class UsuarioRepositoryTests : IClassFixture<EfSqliteFixture>
    {
        private readonly EfSqliteFixture _fixture;

        public UsuarioRepositoryTests(EfSqliteFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Devera_RetonarVerdadeiro_QuandoVerificarSeEmailJaExiste()
        {
            // Arrange
            var (repositorio, usuario) = await InserirUsuario();

            // Act
            var actual = await repositorio.VerificaSeEmailExisteAsync(usuario.Email);

            // Assert
            actual.Should().Be(true);
        }

        [Fact]
        public async Task Devera_RetornarUsuario_QuandoObterPorEmail()
        {
            // Arrange
            var (repositorio, usuarioInserido) = await InserirUsuario();

            // Act
            var actual = await repositorio.ObterPorEmailAsync(usuarioInserido.Email);

            // Assert
            actual.Should().NotBeNull();
            actual.Id.Should().NotBeEmpty().And.Be(usuarioInserido.Id);
            actual.Nome.Should().NotBeNullOrWhiteSpace().And.Be(usuarioInserido.Nome);
            actual.Email.Should().NotBeNull().And.Be(usuarioInserido.Email);
            actual.HashSenha.Should().NotBeNullOrWhiteSpace().And.Be(usuarioInserido.HashSenha);
            actual.Tokens.Should().NotBeEmpty().And.HaveCount(usuarioInserido.Tokens.Count)
                .And.Subject.ForEach(t =>
                {
                    t.Id.Should().NotBeEmpty();
                    t.UsuarioId.Should().Be(usuarioInserido.Id);
                    t.Token.Should().NotBeNullOrWhiteSpace();
                    t.CriadoEm.Should().BeBefore(t.ExpiraEm);
                    t.ExpiraEm.Should().BeAfter(t.CriadoEm);
                });
        }

        [Fact]
        public async Task Devera_RetornarUsuario_QuandoObterPorId()
        {
            // Arrange
            var (repositorio, usuarioInserido) = await InserirUsuario();

            // Act
            var actual = await repositorio.GetByIdAsync(usuarioInserido.Id);

            // Assert
            actual.Should().NotBeNull();
            actual.Id.Should().NotBeEmpty().And.Be(usuarioInserido.Id);
            actual.Nome.Should().NotBeNullOrWhiteSpace().And.Be(usuarioInserido.Nome);
            actual.Email.Should().NotBeNull().And.Be(usuarioInserido.Email);
            actual.HashSenha.Should().NotBeNullOrWhiteSpace().And.Be(usuarioInserido.HashSenha);
            actual.Tokens.Should().NotBeEmpty().And.HaveCount(usuarioInserido.Tokens.Count)
                .And.Subject.ForEach(t =>
                {
                    t.Id.Should().NotBeEmpty();
                    t.UsuarioId.Should().Be(usuarioInserido.Id);
                    t.Token.Should().NotBeNullOrWhiteSpace();
                    t.CriadoEm.Should().BeBefore(t.ExpiraEm);
                    t.ExpiraEm.Should().BeAfter(t.CriadoEm);
                });
        }

        [Fact]
        public async Task Devera_RetornarUsuario_QuandoObterPorToken()
        {
            // Arrange
            const int quantidadeTokens = 3;
            var (repositorio, usuarioInserido) = await InserirUsuario(quantidadeTokens);

            // Act
            var actual = await repositorio.ObterPorTokenAsync(usuarioInserido.Tokens[0].Token);

            // Assert
            actual.Should().NotBeNull();
            actual.Id.Should().NotBeEmpty().And.Be(usuarioInserido.Id);
            actual.Nome.Should().NotBeNullOrWhiteSpace().And.Be(usuarioInserido.Nome);
            actual.Email.Should().NotBeNull().And.Be(usuarioInserido.Email);
            actual.HashSenha.Should().NotBeNullOrWhiteSpace().And.Be(usuarioInserido.HashSenha);
            actual.Tokens.Should().NotBeEmpty().And.HaveCount(usuarioInserido.Tokens.Count)
                .And.Subject.ForEach(t =>
                {
                    t.Id.Should().NotBeEmpty();
                    t.UsuarioId.Should().Be(usuarioInserido.Id);
                    t.Token.Should().NotBeNullOrWhiteSpace();
                    t.CriadoEm.Should().BeBefore(t.ExpiraEm);
                    t.ExpiraEm.Should().BeAfter(t.CriadoEm);
                });
        }

        private static Usuario CriarUsuario(int quantidadeTokens)
        {
            return new Faker<Usuario>()
                .UsePrivateConstructor()
                .RuleFor(u => u.Id, Guid.NewGuid())
                .RuleFor(u => u.Nome, f => f.Person.UserName)
                .RuleFor(u => u.Email, f => new Email(f.Person.Email))
                .RuleFor(u => u.HashSenha, f => f.Internet.Password(60))
                .FinishWith((f, u) =>
                {
                    for (int i = 0; i < quantidadeTokens; i++)
                    {
                        var criadoEm = i == 0 ? DateTime.Now : DateTime.Now.AddDays(i + 1);
                        var expiraEm = criadoEm.AddHours(8);
                        u.AdicionarToken(new TokenAcesso(f.Internet.Password(2048), criadoEm, expiraEm));
                    }
                })
                .Generate();
        }

        private IUsuarioRepository CriarRepositorio()
            => new UsuarioRepository(_fixture.Context);

        private IUnitOfWork CriarUoW()
            => new UnitOfWork(_fixture.Context, Mock.Of<ILogger<UnitOfWork>>());

        private async Task<(IUsuarioRepository, Usuario)> InserirUsuario(int quantidadeTokens = 1)
        {
            var uow = CriarUoW();
            var repositorio = CriarRepositorio();

            var usuario = CriarUsuario(quantidadeTokens);

            repositorio.Add(usuario);
            await uow.SaveChangesAsync();

            return (repositorio, usuario);
        }
    }
}