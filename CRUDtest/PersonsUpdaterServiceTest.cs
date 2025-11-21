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
    public class PersonsUpdaterServiceTest
    {
        private readonly IPersonsRepository _personRepo;
        private readonly Mock<IPersonsRepository> _personRepoMock;
        private readonly IPersonsUpdaterService _personsUpdaterService;
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly IFixture _fixture;
        private readonly Mock<ILogger<PersonsGetterService>> _loggerMock;

        public PersonsUpdaterServiceTest(ITestOutputHelper testOutputHelper)
        {
            _fixture = new Fixture();
            _personRepoMock = new Mock<IPersonsRepository>();
            _personRepo = _personRepoMock.Object;

            _loggerMock = new Mock<ILogger<PersonsGetterService>>();
            _personsUpdaterService = new PersonsUpdaterService(_personRepo, _loggerMock.Object);
            _testOutputHelper = testOutputHelper;
        }

        #region UpdatePerson Tests

        // when supply null PersonUpdateRequest => ArgumentNullException
        [Fact]
        public async Task UpdatePerson_NullPersonUpdateRequest_ToBeArgumentNullException()
        {
            // Arrange
            PersonUpdateRequest? personUpdateRequest = null;

            // Act & Assert
            Func<Task> action = async () =>
            {
                await _personsUpdaterService.UpdatePerson(personUpdateRequest);
            };
            await action.Should().ThrowAsync<ArgumentNullException>();
        }

        // when supply invalid PersonID => return ArgumentException
        [Fact]
        public async Task UpdatePerson_InvalidPersonID_ToBeArgumentException()
        {
            // Arrange
            PersonUpdateRequest? personUpdateRequest = _fixture.Create<PersonUpdateRequest>();

            _personRepoMock.Setup
                (repo => repo.GetPersonById(It.IsAny<Guid>()))
                .ReturnsAsync(null as Person);

            // Act & Assert
            Func<Task> action = async () =>
            {
                await _personsUpdaterService.UpdatePerson(personUpdateRequest);
            };
            await action.Should().ThrowAsync<ArgumentException>();
        }

        // when supply null Name => throw ArgumentException
        [Fact]
        public async Task UpdatePerson_NullPersonName_ToBeArgumentException()
        {
            // Arrange
            Person person = _fixture.Build<Person>()
                .With(temp => temp.Email, "test@example.com")
                .With(temp => temp.Country, null as Country)
                .Create();

            PersonUpdateRequest? personUpdateRequest = person.ToPersonResponse().ToUpdateRequest();
            personUpdateRequest.Name = null;

            _personRepoMock.Setup
                (repo => repo.GetPersonById(It.IsAny<Guid>()))
                .ReturnsAsync(person);

            // Act & Assert
            Func<Task> action = async () =>
            {
                await _personsUpdaterService.UpdatePerson(personUpdateRequest);
            };
            await action.Should().ThrowAsync<ArgumentException>();
        }

        // when supply valid PersonUpdateRequest => update the person details and return PersonResponse object
        [Fact]
        public async Task UpdatePerson_ValidPersonUpdateRequest_ToBeSuccessful()
        {
            // Arrange
            Person person = _fixture.Build<Person>()
                .With(temp => temp.Email, "test@example.com")
                .With(temp => temp.Country, null as Country)
                .Create();

            PersonUpdateRequest personUpdateRequest = person.ToPersonResponse().ToUpdateRequest();
            personUpdateRequest.Name = "Khaled";
            personUpdateRequest.Email = "khaled@gmail.com";

            _personRepoMock.Setup
                (repo => repo.GetPersonById(It.IsAny<Guid>()))
                .ReturnsAsync(person);

            _personRepoMock.Setup
                (repo => repo.UpdatePerson(It.IsAny<Person>()))
                .ReturnsAsync(person);

            // Act
            PersonResponse updatedPerson = await _personsUpdaterService.UpdatePerson(personUpdateRequest);

            // Assert
            updatedPerson.PersonID.Should().Be(personUpdateRequest.PersonID);
            updatedPerson.Name.Should().Be("Khaled");
            updatedPerson.Email.Should().Be("khaled@gmail.com");
        }

        #endregion
    }
}

