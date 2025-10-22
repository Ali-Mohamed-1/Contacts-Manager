using ServiceContracts;
using Entities;
using ServiceContracts.DTOs.PersonDTOs;
using Services;
using ServiceContracts.DTOs.CountryDTOs;
using Xunit.Abstractions;
using Entities.Data;
using Microsoft.EntityFrameworkCore;
using EntityFrameworkCoreMock;
using AutoFixture;
using FluentAssertions;
using RepositoryContracts;
using Moq;
using System.Linq.Expressions;

namespace CRUDtest
{
    public class PersonsServiceTest
    {
        private readonly IPersonsRepository _personRepo;
        private readonly Mock<IPersonsRepository> _personRepoMock;
        private readonly Mock<ICountriesRepository> _countriesRepoMock;

        private readonly IPersonsService _personService;
        private readonly ICountryService _countryService;
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly IFixture _fixture;

        public PersonsServiceTest(ITestOutputHelper testOutputHelper)
        {
            _fixture = new Fixture();
            _personRepoMock = new Mock<IPersonsRepository>();
            _personRepo = _personRepoMock.Object;

            _countriesRepoMock = new Mock<ICountriesRepository>();
            _countryService = new CountriesService(_countriesRepoMock.Object);
            
            _personService = new PersonsService(_personRepo);
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
                await _personService.AddPerson(personAddRequest);
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
                await _personService.AddPerson(personAddRequest);
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
            PersonResponse personResponse = await _personService.AddPerson(personAddRequest);
            personResponse1.PersonID = personResponse.PersonID;

            // Assert
            personResponse.PersonID.Should().NotBe(Guid.Empty);
            personResponse.PersonID.Should().Be(personResponse1.PersonID);
        }

        #endregion

        #region GetPersonByID Tests

        // when supply null personID => return null
        [Fact]
        public async Task GetPersonByID_NullPersonID_ToBeNull()
        {
            // Arrange
            Guid? personID = null;

            // Act
            PersonResponse? personResponse = await Task.Run(() => _personService.GetPersonByID(personID));

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
            PersonResponse? person_from_get = await _personService.GetPersonByID(person.PersonID);

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
            List<PersonResponse> allPersons = await Task.Run(() => _personService.GetAllPersons());
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
            List<PersonResponse> allPersons = await _personService.GetAllPersons();

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
                .Create()
            };

            List<PersonResponse> person_response_list_expected = persons.Select(person => person.ToPersonResponse()).ToList();

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
            List<PersonResponse> allPersons = await _personService.GetFilteredPersons(nameof(Person.Name), "");

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
            List<PersonResponse> filteredPersons = await Task.Run(() => _personService.GetFilteredPersons(nameof(Person.Name), "Ali"));

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

        #region GetSortedPersons Tests

        // when sort based on Name in DESC order => return sorted persons
        [Fact]
        public async Task GetSortedPersons_NameDESC_ToBeSuccessful()
        {
            // Arrange
            List<PersonResponse> personsRequests = new List<PersonResponse>() {
                _fixture.Build<PersonResponse>()
                .With(temp => temp.Email, "test1@example.com")
                .With(temp => temp.Name, "Alice")
                .Create(),
                _fixture.Build<PersonResponse>()
                .With(temp => temp.Email, "test2@example.com")
                .With(temp => temp.Name, "Bob")
                .Create()
            };

            // print personsRequests list
            _testOutputHelper.WriteLine("Input:");
            foreach (PersonResponse person in personsRequests)
            {
                if (person.Name != null)
                    _testOutputHelper.WriteLine($"PersonID: {person.PersonID}, Name: {person.Name}, CountryID: {person.CountryID}, CountryName: {person.CountryName}");
            }

            // Act
            List<PersonResponse> sortedPersons = await _personService.GetSortedPersons(personsRequests, nameof(PersonResponse.Name), false);

            // print sorted list
            _testOutputHelper.WriteLine("Sorted:");
            foreach (PersonResponse person in sortedPersons)
            {
                _testOutputHelper.WriteLine($"PersonID: {person.PersonID}, Name: {person.Name}, CountryID: {person.CountryID}, CountryName: {person.CountryName}");
            }

            // Assert
            sortedPersons.Should().BeInDescendingOrder(p => p.Name);
        }

        #endregion

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
                await _personService.UpdatePerson(personUpdateRequest);
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
                await _personService.UpdatePerson(personUpdateRequest);
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
                await _personService.UpdatePerson(personUpdateRequest);
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
            PersonResponse updatedPerson = await _personService.UpdatePerson(personUpdateRequest);

            // Assert
            updatedPerson.PersonID.Should().Be(personUpdateRequest.PersonID);
            updatedPerson.Name.Should().Be("Khaled");
            updatedPerson.Email.Should().Be("khaled@gmail.com");
        }

        #endregion

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
            bool isDeleted = await _personService.DeletePerson(personID);
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
            bool isDeleted = await _personService.DeletePerson(expected.PersonID);

            // Assert
            isDeleted.Should().BeTrue();
        }

        #endregion
    }
}