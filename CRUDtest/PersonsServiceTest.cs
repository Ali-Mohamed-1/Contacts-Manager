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

namespace CRUDtest
{
    public class PersonsServiceTest
    {
        private readonly IPersonsService _personService;
        private readonly ICountryService _countryService;
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly IFixture _fixture;

        public PersonsServiceTest(ITestOutputHelper testOutputHelper)
        {
            _fixture = new Fixture();


            var countriesInitialData = new List<Country>() { };
            var personsInitialData = new List<Person>() { };

            DbContextMock<AppDbContext> dbContextMock = new DbContextMock<AppDbContext>(
                new DbContextOptionsBuilder<AppDbContext>()
                    .Options);

            var dbContext = dbContextMock.Object; // Mock AppDbContext
            dbContextMock.CreateDbSetMock(temp => temp.Countries, countriesInitialData);
            dbContextMock.CreateDbSetMock(temp => temp.Persons, personsInitialData);

            _countryService = new CountriesService(null);
            _personService = new PersonsService(dbContext, _countryService);
            _testOutputHelper = testOutputHelper;
        }

        #region AddPerson Tests

        // when supply null PersonAddRequest => ArgumentNullException
        [Fact]
        public async Task AddPerson_NullPersonAddRequest()
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
        public async Task AddPerson_NullPersonName()
        {
            // Arrange
            PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.Name, null as string)
                .Create();

            // Act & Assert
            Func<Task> action = async () =>
            {
                await _personService.AddPerson(personAddRequest);
            };
            await action.Should().ThrowAsync<ArgumentException>();
        }

        // when supply vaild person details => insert into persons list and return PersonResponse object
        [Fact]
        public async Task AddPerson_ValidPersonDetails()
        {
            // Arrange
            PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.Email, "test@example.com")
                .Create();
            // Act
            PersonResponse? personResponse = await Task.Run(() => _personService.AddPerson(personAddRequest));
            List<PersonResponse> allPersons = await Task.Run(() => _personService.GetAllPersons());

            // Assert
            personResponse.PersonID.Should().NotBe(Guid.Empty);
            allPersons.Should().Contain(person => person.PersonID == personResponse.PersonID);
        }

        #endregion

        #region GetAllPersons Tests

        // the list should be empty initially
        [Fact]
        public async Task GetAllPersons_InitiallyEmpty()
        {
            // Act
            List<PersonResponse> allPersons = await Task.Run(() => _personService.GetAllPersons());
            // Assert
            allPersons.Should().BeEmpty();
        }

        // after adding persons => return all persons
        [Fact]
        public async Task GetAllPersons_AfterAdding()
        {
            // Arrange
            CountryAddRequest countryRequest1 = _fixture.Create<CountryAddRequest>();
            CountryAddRequest countryRequest2 = _fixture.Create<CountryAddRequest>();


            CountryResponse countryResponse1 = await Task.Run(() => _countryService.AddCountry(countryRequest1));
            CountryResponse countryResponse2 = await Task.Run(() => _countryService.AddCountry(countryRequest2));

            PersonAddRequest personRequest1 = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.Email, "test1@example.com")
                .Create();
            PersonAddRequest personRequest2 = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.Email, "test2@example.com")
                .Create();

            List<PersonResponse> personsRequests = new List<PersonResponse>() {
                await Task.Run(() => _personService.AddPerson(personRequest1)),
                await Task.Run(() => _personService.AddPerson(personRequest2))
            };

            // print personsRequests list
            _testOutputHelper.WriteLine("Expected:");
            foreach (PersonResponse person in personsRequests)
            {
                _testOutputHelper.WriteLine($"PersonID: {person.PersonID}, Name: {person.Name}, CountryID: {person.CountryID}, CountryName: {person.CountryName}");
            }

            // Act
            List<PersonResponse> allPersons = await Task.Run(() => _personService.GetAllPersons());

            // print allPersons list
            _testOutputHelper.WriteLine("Actual:");
            foreach (PersonResponse person in allPersons)
            {
                _testOutputHelper.WriteLine($"PersonID: {person.PersonID}, Name: {person.Name}, CountryID: {person.CountryID}, CountryName: {person.CountryName}");
            }

            // Assert
            allPersons.Should().BeEquivalentTo(personsRequests);
        }

        #endregion

        #region GetPersonByID Tests

        // when supply null personID => return null
        [Fact]
        public async Task GetPersonByID_NullPersonID()
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
        public async Task GetPersonByID_ValidPersonID()
        {

            // Act
            CountryAddRequest countryAddRequest = _fixture.Create<CountryAddRequest>();

            CountryResponse countryResponse = await Task.Run(() => _countryService.AddCountry(countryAddRequest));

            PersonAddRequest personAddRequest = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.Email, "test@example.com")
                .With(temp => temp.CountryID, countryResponse.CountryID)
                .Create();

            PersonResponse expectedPerson = await Task.Run(() => _personService.AddPerson(personAddRequest));

            PersonResponse? person = await Task.Run(() => _personService.GetPersonByID(expectedPerson.PersonID));

            // Assert
            expectedPerson.PersonID.Should().Be(person.PersonID);
        }

        #endregion

        #region GetFilteredPersons Tests

        // if search text is empty or null => return all persons
        [Fact]
        public async Task GetFilteredPersons_EmptySearchText()
        {
            // Arrange
            CountryAddRequest countryRequest1 = _fixture.Create<CountryAddRequest>();
            CountryAddRequest countryRequest2 = _fixture.Create<CountryAddRequest>();

            CountryResponse countryResponse1 = await Task.Run(() => _countryService.AddCountry(countryRequest1));
            CountryResponse countryResponse2 = await Task.Run(() => _countryService.AddCountry(countryRequest2));

            PersonAddRequest personRequest1 = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.Email, "test1@example.com")
                .Create();
            PersonAddRequest personRequest2 = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.Email, "test2@example.com")
                .Create();

            List<PersonResponse> personsRequests = new List<PersonResponse>() {
                await Task.Run(() => _personService.AddPerson(personRequest1)),
                await Task.Run(() => _personService.AddPerson(personRequest2))
            };

            // print personsRequests list
            _testOutputHelper.WriteLine("Expected:");
            foreach (PersonResponse person in personsRequests)
            {
                _testOutputHelper.WriteLine($"PersonID: {person.PersonID}, Name: {person.Name}, CountryID: {person.CountryID}, CountryName: {person.CountryName}");
            }

            // Act
            List<PersonResponse> allPersons = await Task.Run(() => _personService.GetFilteredPersons(nameof(Person.Name), ""));

            // print allPersons list
            _testOutputHelper.WriteLine("Actual:");
            foreach (PersonResponse person in allPersons)
            {
                _testOutputHelper.WriteLine($"PersonID: {person.PersonID}, Name: {person.Name}, CountryID: {person.CountryID}, CountryName: {person.CountryName}");
            }

            // Assert
            allPersons.Should().BeEquivalentTo(personsRequests);
        }

        // when supply valid search text => return matching persons
        [Fact]
        public async Task GetFilteredPersons_ValidSearch()
        {
            // Arrange
            CountryAddRequest countryRequest1 = _fixture.Create<CountryAddRequest>();
            CountryAddRequest countryRequest2 = _fixture.Create<CountryAddRequest>();

            CountryResponse countryResponse1 = await Task.Run(() => _countryService.AddCountry(countryRequest1));
            CountryResponse countryResponse2 = await Task.Run(() => _countryService.AddCountry(countryRequest2));

            PersonAddRequest personRequest1 = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.Email, "test1@example.com")
                .Create();
            PersonAddRequest personRequest2 = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.Email, "test2@example.com")
                .Create();

            List<PersonResponse> personsRequests = new List<PersonResponse>() {
                await Task.Run(() => _personService.AddPerson(personRequest1)),
                await Task.Run(() => _personService.AddPerson(personRequest2))
            };

            // print personsRequests list
            _testOutputHelper.WriteLine("Expected:");
            foreach (PersonResponse person in personsRequests)
            {
                if (person.Name != null && person.Name.Contains("Ali", StringComparison.OrdinalIgnoreCase))
                    _testOutputHelper.WriteLine($"PersonID: {person.PersonID}, Name: {person.Name}, CountryID: {person.CountryID}, CountryName: {person.CountryName}");
            }

            // Act
            List<PersonResponse> filteredPersons = await Task.Run(() => _personService.GetFilteredPersons(nameof(Person.Name), "Ali"));

            // print allPersons list
            _testOutputHelper.WriteLine("Filtered Persons:");
            foreach (PersonResponse person in filteredPersons)
            {
                _testOutputHelper.WriteLine($"PersonID: {person.PersonID}, Name: {person.Name}, CountryID: {person.CountryID}, CountryName: {person.CountryName}");
            }

            // Assert
            filteredPersons.Should().OnlyContain(temp => temp.Name.Contains("Ali", StringComparison.OrdinalIgnoreCase));
        }

        #endregion

        #region GetSortedPersons Tests

        // when sort based on Name in DESC order => return sorted persons
        [Fact]
        public async Task GetSortedPersons_NameDESC()
        {
            // Arrange
            CountryAddRequest countryRequest1 = _fixture.Create<CountryAddRequest>();
            CountryAddRequest countryRequest2 = _fixture.Create<CountryAddRequest>();

            CountryResponse countryResponse1 = await Task.Run(() => _countryService.AddCountry(countryRequest1));
            CountryResponse countryResponse2 = await Task.Run(() => _countryService.AddCountry(countryRequest2));

            PersonAddRequest personRequest1 = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.Email, "test1@example.com")
                .Create();
            PersonAddRequest personRequest2 = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.Email, "test2@example.com")
                .Create();

            List<PersonResponse> personsRequests = new List<PersonResponse>() {
                await Task.Run(() => _personService.AddPerson(personRequest1)),
                await Task.Run(() => _personService.AddPerson(personRequest2))
            };
            personsRequests = personsRequests.OrderByDescending(p => p.Name).ToList();

            List<PersonResponse> allPersons = await Task.Run(() => _personService.GetAllPersons());

            // print personsRequests list
            _testOutputHelper.WriteLine("Expected:");
            foreach (PersonResponse person in personsRequests)
            {
                if (person.Name != null)
                    _testOutputHelper.WriteLine($"PersonID: {person.PersonID}, Name: {person.Name}, CountryID: {person.CountryID}, CountryName: {person.CountryName}");
            }

            // Act
            List<PersonResponse> filteredPersons = await Task.Run(() => _personService.GetSortedPersons(allPersons, nameof(Person.Name), false));

            // print allPersons list
            _testOutputHelper.WriteLine("Sorted:");
            foreach (PersonResponse person in filteredPersons)
            {
                _testOutputHelper.WriteLine($"PersonID: {person.PersonID}, Name: {person.Name}, CountryID: {person.CountryID}, CountryName: {person.CountryName}");
            }

            // Assert
            filteredPersons.Should().BeInDescendingOrder(p => p.Name);
        }

        #endregion

        #region UpdatePerson Tests

        // when supply null PersonUpdateRequest => ArgumentNullException
        [Fact]
        public async Task UpdatePerson_NullPersonUpdateRequest()
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
        public async Task UpdatePerson_NullPersonID()
        {
            // Arrange
            PersonUpdateRequest? personUpdateRequest = _fixture.Create<PersonUpdateRequest>();

            // Act & Assert
            Func<Task> action = async () =>
            {
                await _personService.UpdatePerson(personUpdateRequest);
            };
            await action.Should().ThrowAsync<ArgumentException>();
        }

        // when supply null Name => throw ArgumentException
        [Fact]
        public async Task UpdatePerson_NullPersonName()
        {
            // Arrange
            CountryAddRequest countryAddRequest = _fixture.Create<CountryAddRequest>();

            CountryResponse countryResponse = await Task.Run(() => _countryService.AddCountry(countryAddRequest));

            PersonAddRequest personRequest1 = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.Email, "test@example.com")
                .Create();

            PersonResponse personRequest = await Task.Run(() => _personService.AddPerson(personRequest1));

            PersonUpdateRequest? personUpdateRequest = personRequest.ToUpdateRequest();
            personUpdateRequest.Name = null;

            // Act & Assert
            Func<Task> action = async () =>
            {
                await _personService.UpdatePerson(personUpdateRequest);
            };
            await action.Should().ThrowAsync<ArgumentException>();
        }

        // when supply valid PersonUpdateRequest => update the person details and return PersonResponse object
        [Fact]
        public async Task UpdatePerson_ValidPersonUpdateRequest()
        {
            // Arrange
            CountryAddRequest countryAddRequest = _fixture.Create<CountryAddRequest>();

            CountryResponse countryResponse = await Task.Run(() => _countryService.AddCountry(countryAddRequest));

            PersonAddRequest personRequest1 = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.Email, "test@example.com")
                .Create();

            PersonResponse personRequest = await Task.Run(() => _personService.AddPerson(personRequest1));

            PersonUpdateRequest personUpdateRequest = personRequest.ToUpdateRequest();
            personUpdateRequest.Name = "Khaled";
            personUpdateRequest.Email = "khaled@gmail.com";

            // Act
            PersonResponse updatedPerson = await Task.Run(() => _personService.UpdatePerson(personUpdateRequest));

            // Assert
            personUpdateRequest.PersonID.Should().Be(updatedPerson.PersonID);
        }

        #endregion

        #region DeletePerson Tests

        // when supply null personID => return false
        [Fact]
        public async Task DeletePerson_NullPersonID()
        {
            // Arrange
            Guid personID = Guid.NewGuid();
            // Act
            bool isDeleted = await Task.Run(() => _personService.DeletePerson(personID));
            // Assert
            isDeleted.Should().BeFalse();
        }

        // when supply valid personID => return true
        [Fact]
        public async Task DeletePerson_ValidPersonID()
        {
            // Arrange
            CountryAddRequest countryAddRequest = _fixture.Create<CountryAddRequest>();

            CountryResponse countryResponse = await Task.Run(() => _countryService.AddCountry(countryAddRequest));

            PersonAddRequest personRequest = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.Email, "test@example.com")
                .Create();

            PersonResponse personResponse = await Task.Run(() => _personService.AddPerson(personRequest));

            // Act
            bool isDeleted = await Task.Run(() => _personService.DeletePerson(personResponse.PersonID));

            // Assert
            isDeleted.Should().BeTrue();
        }

        #endregion
    }
}