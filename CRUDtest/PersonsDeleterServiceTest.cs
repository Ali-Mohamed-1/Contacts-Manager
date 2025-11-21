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
    public class PersonsDeleterServiceTest
    {
        private readonly IPersonsRepository _personRepo;
        private readonly Mock<IPersonsRepository> _personRepoMock;
        private readonly IPersonsDeleterService _personsDeleterService;
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly IFixture _fixture;
        private readonly Mock<ILogger<PersonsGetterService>> _loggerMock;

        public PersonsDeleterServiceTest(ITestOutputHelper testOutputHelper)
        {
            _fixture = new Fixture();
            _personRepoMock = new Mock<IPersonsRepository>();
            _personRepo = _personRepoMock.Object;

            _loggerMock = new Mock<ILogger<PersonsGetterService>>();
            _personsDeleterService = new PersonsDeleterService(_personRepo, _loggerMock.Object);
            _testOutputHelper = testOutputHelper;
        }

        #region DeletePerson Tests

        // when supply null personID => return false
        [Fact]
        public async Task DeletePerson_InvalidPersonID_ToBeFalse()
        {
            // Arrange
            Guid personID = Guid.NewGuid();

            _personRepoMock.Setup
                (repo => repo.GetPersonById(It.IsAny<Guid>()))
                .ReturnsAsync(null as Person);

            // Act
            bool isDeleted = await _personsDeleterService.DeletePerson(personID);
            // Assert
            isDeleted.Should().BeFalse();
        }

        // when supply valid personID => return true
        [Fact]
        public async Task DeletePerson_ValidPersonID_ToBeSucessful()
        {
            // Arrange
            Person person = _fixture.Build<Person>()
                .With(temp => temp.Email, "test@example.com")
                .With(Deletedperson => Deletedperson.Country, null as Country)
                .Create();

            List<Person> persons = new List<Person>()
            {
                person
            };

            PersonResponse expected = person.ToPersonResponse();

            _personRepoMock.Setup
                (repo => repo.GetPersonById(It.IsAny<Guid>()))
                .ReturnsAsync(person);

            _personRepoMock.Setup
                (repo => repo.DeletePersonByID(It.IsAny<Guid>()))
                .ReturnsAsync(true);

            // Act
            bool isDeleted = await _personsDeleterService.DeletePerson(expected.PersonID);

            // Assert
            isDeleted.Should().BeTrue();
        }

        #endregion
    }
}

