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
using System.Linq.Expressions;

namespace CRUDtest
{
    public class PersonsGetterServiceTest
    {
        private readonly IPersonsRepository _personRepo;
        private readonly Mock<IPersonsRepository> _personRepoMock;
        private readonly IPersonsGetterService _personsGetterService;
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly IFixture _fixture;
        private readonly Mock<ILogger<PersonsGetterService>> _loggerMock;

        public PersonsGetterServiceTest(ITestOutputHelper testOutputHelper)
        {
            _fixture = new Fixture();
            _personRepoMock = new Mock<IPersonsRepository>();
            _personRepo = _personRepoMock.Object;

            _loggerMock = new Mock<ILogger<PersonsGetterService>>();
            _personsGetterService = new PersonsGetterService(_personRepo, _loggerMock.Object);
            _testOutputHelper = testOutputHelper;
        }

        #region GetPersonByID Tests

        // when supply null personID => return null
        [Fact]
        public async Task GetPersonByID_NullPersonID_ToBeNull()
        {
            // Arrange
            Guid? personID = null;

            // Act
            PersonResponse? personResponse = await Task.Run(() => _personsGetterService.GetPersonByID(personID));

            // Assert
            personResponse.Should().BeNull();
        }

        // when supply valid personID => return PersonResponse object
        [Fact]
        public async Task GetPersonByID_ValidPersonID_ToBeSucessful()
        {
            // Arrange
            Person person = _fixture.Build<Person>()
                .With(temp => temp.Email, "test@example.com")
                .With(temp => temp.Country, null as Country)
                .Create();

            PersonResponse expected_person = person.ToPersonResponse();

            _personRepoMock.Setup
                (repo => repo.GetPersonById(It.IsAny<Guid>()))
                .ReturnsAsync(person);

            // Act
            PersonResponse? person_from_get = await _personsGetterService.GetPersonByID(person.PersonID);

            // Assert
            person_from_get.Should().NotBeNull();
            person_from_get!.PersonID.Should().Be(expected_person.PersonID);
        }

        #endregion

        #region GetAllPersons Tests

        // the list should be empty initially
        [Fact]
        public async Task GetAllPersons_InitiallyEmpty_ToBeEmptyList()
        {
            // Arrange
            _personRepoMock.Setup
                (repo => repo.GetAllPersons())
                .ReturnsAsync(new List<Person>());

            // Act
            List<PersonResponse> allPersons = await Task.Run(() => _personsGetterService.GetAllPersons());
            // Assert
            allPersons.Should().BeEmpty();
        }

        // after adding persons => return all persons
        [Fact]
        public async Task GetAllPersons_AfterAdding_ToBeSucessful()
        {
            // Arrange
            List<Person> persons = new List<Person>()
            {
                _fixture.Build<Person>()
                .With(temp => temp.Email, "test1@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),
                
                _fixture.Build<Person>()
                .With(temp => temp.Email, "test2@example.com")
                .With(temp => temp.Country, null as Country)
                .Create()
            };

            List<PersonResponse> personResponse_list_expected = persons
                .Select(person => person.ToPersonResponse())
                .ToList();

            // print personsRequests list
            _testOutputHelper.WriteLine("Expected:");
            foreach (PersonResponse person in personResponse_list_expected)
            {
                _testOutputHelper.WriteLine($"PersonID: {person.PersonID}, Name: {person.Name}, CountryID: {person.CountryID}, CountryName: {person.CountryName}");
            }

            _personRepoMock.Setup
                (repo => repo.GetAllPersons())
                .ReturnsAsync(persons);

            // Act
            List<PersonResponse> allPersons = await _personsGetterService.GetAllPersons();

            // print allPersons list
            _testOutputHelper.WriteLine("Actual:");
            foreach (PersonResponse person in allPersons)
            {
                _testOutputHelper.WriteLine($"PersonID: {person.PersonID}, Name: {person.Name}, CountryID: {person.CountryID}, CountryName: {person.CountryName}");
            }

            // Assert
            allPersons.Should().BeEquivalentTo(personResponse_list_expected);
        }

        #endregion

        #region GetFilteredPersons Tests

        // if search text is empty or null => return all persons
        [Fact]
        public async Task GetFilteredPersons_EmptySearchText_ToBeAllPersons()
        {
            // Arrange
            List<Person> persons = new List<Person>()
            {
                _fixture.Build<Person>()
                .With(temp => temp.Email, "test1@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),

                _fixture.Build<Person>()
                .With(temp => temp.Email, "test2@example.com")
                .With(temp => temp.Country, null as Country)
                .Create()
            };

            List<PersonResponse> person_response_list_expected = persons.Select(person =>
            {
                var response = person.ToPersonResponse();
                response.CountryName = person.Country?.CountryName;
                return response;
            }).ToList();

            // print personsRequests list
            _testOutputHelper.WriteLine("Expected:");
            foreach (PersonResponse person in person_response_list_expected)
            {
                _testOutputHelper.WriteLine($"PersonID: {person.PersonID}, Name: {person.Name}, CountryID: {person.CountryID}, CountryName: {person.CountryName}");
            }

            _personRepoMock
                .Setup(repo => repo.GetFilteredPersons(It.IsAny<Expression<Func<Person, bool>>>()))
                .ReturnsAsync(persons);


            // Act
            List<PersonResponse> allPersons = await _personsGetterService.GetFilteredPersons(nameof(Person.Name), "");

            // print allPersons list
            _testOutputHelper.WriteLine("Actual:");
            foreach (PersonResponse person in allPersons)
            {
                _testOutputHelper.WriteLine($"PersonID: {person.PersonID}, Name: {person.Name}, CountryID: {person.CountryID}, CountryName: {person.CountryName}");
            }

            // Assert
            allPersons.Should().BeEquivalentTo(person_response_list_expected);
        }

        // when supply valid search text => return matching persons
        [Fact]
        public async Task GetFilteredPersons_ValidSearch_ToBeSucessful()
        {
            // Arrange
            List<Person> persons = new List<Person>()
            {
                 _fixture.Build<Person>()
                .With(temp => temp.Email, "test1@example.com")
                .With(temp => temp.Name, "Ali Ahmed")
                .With(temp => temp.Country, null as Country)
                .Create(),

                    _fixture.Build<Person>()
                .With(temp => temp.Email, "test2@example.com")
                .With(temp => temp.Name, "Ali Mohamed")
                .With(temp => temp.Country, null as Country)
                .Create()
            };

            List<PersonResponse> person_list_expected = persons
                .Select(person => person.ToPersonResponse())
                .ToList();

            // print personsRequests list
            _testOutputHelper.WriteLine("Expected:");
            foreach (PersonResponse person in person_list_expected)
            {
                if (person.Name != null && person.Name.Contains("Ali", StringComparison.OrdinalIgnoreCase))
                    _testOutputHelper.WriteLine($"PersonID: {person.PersonID}, Name: {person.Name}, CountryID: {person.CountryID}, CountryName: {person.CountryName}");
            }

            _personRepoMock
                .Setup(repo => repo.GetFilteredPersons(It.IsAny<Expression<Func<Person, bool>>>()))
                .ReturnsAsync(persons.Where(p => p.Name != null && p.Name.Contains("Ali", StringComparison.OrdinalIgnoreCase)).ToList());

            // Act
            List<PersonResponse> filteredPersons = await Task.Run(() => _personsGetterService.GetFilteredPersons(nameof(Person.Name), "Ali"));

            // print allPersons list
            _testOutputHelper.WriteLine("Filtered Persons:");
            foreach (PersonResponse person in filteredPersons)
            {
                _testOutputHelper.WriteLine($"PersonID: {person.PersonID}, Name: {person.Name}, CountryID: {person.CountryID}, CountryName: {person.CountryName}");
            }

            // Assert
            filteredPersons.Should().OnlyContain(temp => temp.Name != null && temp.Name.Contains("Ali", StringComparison.OrdinalIgnoreCase));
        }

        #endregion
    }
}

