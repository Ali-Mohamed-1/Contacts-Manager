using ServiceContracts;
using Entities;
using ServiceContracts.DTOs.PersonDTOs;
using Services;
using Xunit.Abstractions;
using AutoFixture;
using FluentAssertions;
using RepositoryContracts;
using Moq;
using Microsoft.Extensions.Logging;

namespace CRUDtest
{
    public class PersonsAdderServiceTest
    {
        private readonly IPersonsRepository _personRepo;
        private readonly Mock<IPersonsRepository> _personRepoMock;
        private readonly IPersonsAdderService _personsAdderService;
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly IFixture _fixture;
        private readonly Mock<ILogger<PersonsGetterService>> _loggerMock;

        public PersonsAdderServiceTest(ITestOutputHelper testOutputHelper)
        {
            _fixture = new Fixture();
            _personRepoMock = new Mock<IPersonsRepository>();
            _personRepo = _personRepoMock.Object;

            _loggerMock = new Mock<ILogger<PersonsGetterService>>();
            _personsAdderService = new PersonsAdderService(_personRepo, _loggerMock.Object);
            _testOutputHelper = testOutputHelper;
        }

        #region AddPerson Tests

        // when supply null PersonAddRequest => ArgumentNullException
        [Fact]
        public async Task AddPerson_NullPersonAddRequest_ToBeArgumentNullException()
        {
            // Arrange
            PersonAddRequest? personAddRequest = null;

            // Act & Assert
            Func<Task> action = async () =>
            {
                await _personsAdderService.AddPerson(personAddRequest);
            };
            await action.Should().ThrowAsync<ArgumentNullException>();    
        }

        // when supply null Name => throw ArgumentException
        [Fact]
        public async Task AddPerson_NullPersonName_ToBeArgumentException()
        {
            // Arrange
            PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.Name, null as string)
                .Create();

            Person person = personAddRequest.ToPerson();

            _personRepoMock.Setup
                (temp => temp.AddPerson(It.IsAny<Person>()))
                .ReturnsAsync(person);

            // Act & Assert
            Func<Task> action = async () =>
            {
                await _personsAdderService.AddPerson(personAddRequest);
            };
            await action.Should().ThrowAsync<ArgumentException>();
        }

        // when supply vaild person details => insert into persons list and return PersonResponse object
        [Fact]
        public async Task AddPerson_ValidPersonDetails_ToBeSuccessful()
        {
            // Arrange
            PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.Email, "test@example.com")
                .Create();

            Person person = personAddRequest.ToPerson();
            PersonResponse personResponse1 = person.ToPersonResponse();

            _personRepoMock.Setup
                (repo => repo.AddPerson(It.IsAny<Person>()))
                .ReturnsAsync(person);

            // Act
            PersonResponse personResponse = await _personsAdderService.AddPerson(personAddRequest);
            personResponse1.PersonID = personResponse.PersonID;

            // Assert
            personResponse.PersonID.Should().NotBe(Guid.Empty);
            personResponse.PersonID.Should().Be(personResponse1.PersonID);
        }

        #endregion
    }
}

