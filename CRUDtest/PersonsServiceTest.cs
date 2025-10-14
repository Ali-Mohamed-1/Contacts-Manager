using ServiceContracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Entities;
using ServiceContracts.DTOs.PersonDTOs;
using Services;
using ServiceContracts.DTOs.CountryDTOs;
using Xunit.Abstractions;
using System.Linq;
using Entities.Data;
using Microsoft.EntityFrameworkCore;
using EntityFrameworkCoreMock;

namespace CRUDtest
{
    public class PersonsServiceTest
    {
        private readonly IPersonsService _personService;
        private readonly ICountryService _countryService;
        private readonly ITestOutputHelper _testOutputHelper;

        public PersonsServiceTest(ITestOutputHelper testOutputHelper)
        {
            var countriesInitialData = new List<Country>() { };
            var personsInitialData = new List<Person>() { };

            DbContextMock<AppDbContext> dbContextMock = new DbContextMock<AppDbContext>(
                new DbContextOptionsBuilder<AppDbContext>()
                    .Options);

            var dbContext = dbContextMock.Object; // Mock AppDbContext
            dbContextMock.CreateDbSetMock(temp => temp.Countries, countriesInitialData);
            dbContextMock.CreateDbSetMock(temp => temp.Persons, personsInitialData);

            _countryService = new CountriesService(dbContext);
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
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await Task.Run(() => _personService.AddPerson(personAddRequest)));
        }

        // when supply null Name => throw ArgumentException
        [Fact]
        public async Task AddPerson_NullPersonName()
        {
            // Arrange
            PersonAddRequest? personAddRequest = new PersonAddRequest()
            {
                Name = null,
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () => await Task.Run(() => _personService.AddPerson(personAddRequest)));
        }

        // when supply vaild person details => insert into persons list and return PersonResponse object
        [Fact]
        public async Task AddPerson_ValidPersonDetails()
        {
            // Arrange
            PersonAddRequest? personAddRequest = new PersonAddRequest()
            {
                Name = "Example",
                Email = "Test@example.com",
                Address = "Test Address",
                CountryID = Guid.NewGuid(),
                Gender = "Male",
                DateOfBirth = new DateTime(2004, 4, 7),
                ReceiveNewsLetters = true
            };

            // Act
            PersonResponse? personResponse = await Task.Run(() => _personService.AddPerson(personAddRequest));
            List<PersonResponse> allPersons = await Task.Run(() => _personService.GetAllPersons());

            // Assert
            Assert.True(personResponse.PersonID != Guid.Empty);
            Assert.Contains(allPersons, person => person.PersonID == personResponse.PersonID);
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
            Assert.Empty(allPersons);
        }

        // after adding persons => return all persons
        [Fact]
        public async Task GetAllPersons_AfterAdding()
        {
            // Arrange
            CountryAddRequest countryRequest1 = new CountryAddRequest()
            {
                CountryName = "Egypt"
            };
            CountryAddRequest countryRequest2 = new CountryAddRequest()
            {
                CountryName = "China"
            };

            CountryResponse countryResponse1 = await Task.Run(() => _countryService.AddCountry(countryRequest1));
            CountryResponse countryResponse2 = await Task.Run(() => _countryService.AddCountry(countryRequest2));

            PersonAddRequest personRequest1 = new PersonAddRequest()
            {
                Name = "Ali",
                Email = "sample@gmail.com",
                CountryID = countryResponse1.CountryID,
                DateOfBirth = new DateTime(2004, 4, 4),
                Gender = "Male",
                Address = "Cairo",
                ReceiveNewsLetters = true
            };
            PersonAddRequest personRequest2 = new PersonAddRequest()
            {
                Name = "Ahmed",
                Email = "sample@gmail.com",
                CountryID = countryResponse2.CountryID,
                DateOfBirth = new DateTime(2004, 5, 4),
                Gender = "Male",
                Address = "Hong Kong",
                ReceiveNewsLetters = true
            };

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
            foreach (PersonResponse person in personsRequests)
            {
                Assert.Contains(allPersons, p => p.PersonID == person.PersonID);
            }
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
            Assert.Null(personResponse);
        }

        // when supply valid personID => return PersonResponse object
        [Fact]
        public async Task GetPersonByID_ValidPersonID()
        {

            // Act
            CountryAddRequest countryAddRequest = new CountryAddRequest()
            {
                CountryName = "Egypt"
            };
            CountryResponse countryResponse = await Task.Run(() => _countryService.AddCountry(countryAddRequest));

            PersonAddRequest personAddRequest = new PersonAddRequest()
            {
                Name = "Ali",
                Email = "sample@gmail.com",
                CountryID = countryResponse.CountryID,
                DateOfBirth = new DateTime(2004, 4, 4),
                Gender = "Male",
                Address = "Cairo",
                ReceiveNewsLetters = true
            };
            PersonResponse expectedPerson = await Task.Run(() => _personService.AddPerson(personAddRequest));

            PersonResponse? person = await Task.Run(() => _personService.GetPersonByID(expectedPerson.PersonID));

            // Assert
            Assert.Equal(person?.PersonID, expectedPerson.PersonID);
        }

        #endregion

        #region GetFilteredPersons Tests

        // if search text is empty or null => return all persons
        [Fact]
        public async Task GetFilteredPersons_EmptySearchText()
        {
            // Arrange
            CountryAddRequest countryRequest1 = new CountryAddRequest()
            {
                CountryName = "Egypt"
            };
            CountryAddRequest countryRequest2 = new CountryAddRequest()
            {
                CountryName = "China"
            };

            CountryResponse countryResponse1 = await Task.Run(() => _countryService.AddCountry(countryRequest1));
            CountryResponse countryResponse2 = await Task.Run(() => _countryService.AddCountry(countryRequest2));

            PersonAddRequest personRequest1 = new PersonAddRequest()
            {
                Name = "Ali",
                Email = "sample@gmail.com",
                CountryID = countryResponse1.CountryID,
                DateOfBirth = new DateTime(2004, 4, 4),
                Gender = "Male",
                Address = "Cairo",
                ReceiveNewsLetters = true
            };
            PersonAddRequest personRequest2 = new PersonAddRequest()
            {
                Name = "Ahmed",
                Email = "sample@gmail.com",
                CountryID = countryResponse2.CountryID,
                DateOfBirth = new DateTime(2004, 5, 4),
                Gender = "Male",
                Address = "Hong Kong",
                ReceiveNewsLetters = true
            };

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
            foreach (PersonResponse person in personsRequests)
            {
                Assert.Contains(allPersons, p => p.PersonID == person.PersonID);
            }
        }

        // when supply valid search text => return matching persons
        [Fact]
        public async Task GetFilteredPersons_ValidSearch()
        {
            // Arrange
            CountryAddRequest countryRequest1 = new CountryAddRequest()
            {
                CountryName = "Egypt"
            };
            CountryAddRequest countryRequest2 = new CountryAddRequest()
            {
                CountryName = "China"
            };

            CountryResponse countryResponse1 = await Task.Run(() => _countryService.AddCountry(countryRequest1));
            CountryResponse countryResponse2 = await Task.Run(() => _countryService.AddCountry(countryRequest2));

            PersonAddRequest personRequest1 = new PersonAddRequest()
            {
                Name = "Ali",
                Email = "sample@gmail.com",
                CountryID = countryResponse1.CountryID,
                DateOfBirth = new DateTime(2004, 4, 4),
                Gender = "Male",
                Address = "Cairo",
                ReceiveNewsLetters = true
            };
            PersonAddRequest personRequest2 = new PersonAddRequest()
            {
                Name = "Ahmed",
                Email = "sample@gmail.com",
                CountryID = countryResponse2.CountryID,
                DateOfBirth = new DateTime(2004, 5, 4),
                Gender = "Male",
                Address = "Hong Kong",
                ReceiveNewsLetters = true
            };

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
            foreach (PersonResponse person in personsRequests)
            {
                if (person.Name != null)
                {
                    if (person.Name.Contains("Ali", StringComparison.OrdinalIgnoreCase))
                    {
                        Assert.Contains(filteredPersons, p => p.PersonID == person.PersonID);
                    }
                }
            }
        }

        #endregion

        #region GetSortedPersons Tests

        // when sort based on Name in DESC order => return sorted persons
        [Fact]
        public async Task GetSortedPersons_NameDESC()
        {
            // Arrange
            CountryAddRequest countryRequest1 = new CountryAddRequest()
            {
                CountryName = "Egypt"
            };
            CountryAddRequest countryRequest2 = new CountryAddRequest()
            {
                CountryName = "China"
            };

            CountryResponse countryResponse1 = await Task.Run(() => _countryService.AddCountry(countryRequest1));
            CountryResponse countryResponse2 = await Task.Run(() => _countryService.AddCountry(countryRequest2));

            PersonAddRequest personRequest1 = new PersonAddRequest()
            {
                Name = "Ali",
                Email = "sample@gmail.com",
                CountryID = countryResponse1.CountryID,
                DateOfBirth = new DateTime(2004, 4, 4),
                Gender = "Male",
                Address = "Cairo",
                ReceiveNewsLetters = true
            };
            PersonAddRequest personRequest2 = new PersonAddRequest()
            {
                Name = "Ahmed",
                Email = "sample@gmail.com",
                CountryID = countryResponse2.CountryID,
                DateOfBirth = new DateTime(2004, 5, 4),
                Gender = "Male",
                Address = "Hong Kong",
                ReceiveNewsLetters = true
            };

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
            List<PersonResponse> filteredPersons = await Task.Run(() => _personService.GetSortedPersons(allPersons, nameof(Person.Email), false));

            // print allPersons list
            _testOutputHelper.WriteLine("Sorted:");
            foreach (PersonResponse person in filteredPersons)
            {
                _testOutputHelper.WriteLine($"PersonID: {person.PersonID}, Name: {person.Name}, CountryID: {person.CountryID}, CountryName: {person.CountryName}");
            }

            // Assert
            foreach (PersonResponse person in personsRequests)
            {
                Assert.Contains(filteredPersons, p => p.PersonID == person.PersonID);
            }
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
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await Task.Run(() => _personService.UpdatePerson(personUpdateRequest)));
        }

        // when supply invalid PersonID => return ArgumentException
        [Fact]
        public async Task UpdatePerson_NullPersonID()
        {
            // Arrange
            PersonUpdateRequest? personUpdateRequest = new PersonUpdateRequest()
            {
                // new ID that doesn't exist in persons list
                PersonID = Guid.NewGuid()
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () => await Task.Run(() => _personService.UpdatePerson(personUpdateRequest)));
        }

        // when supply null Name => throw ArgumentException
        [Fact]
        public async Task UpdatePerson_NullPersonName()
        {
            // Arrange
            CountryAddRequest countryAddRequest = new CountryAddRequest()
            {
                CountryName = "Egypt"
            };
            CountryResponse countryResponse = await Task.Run(() => _countryService.AddCountry(countryAddRequest));

            PersonAddRequest personRequest1 = new PersonAddRequest()
            {
                Name = "Ali",
                Email = "sample@gmail.com",
                CountryID = countryResponse.CountryID,
                DateOfBirth = new DateTime(2004, 4, 4),
                Gender = "Male",
                Address = "Cairo",
                ReceiveNewsLetters = true
            };
            PersonResponse personRequest = await Task.Run(() => _personService.AddPerson(personRequest1));

            PersonUpdateRequest? personUpdateRequest = personRequest.ToUpdateRequest();
            personUpdateRequest.Name = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () => await Task.Run(() => _personService.UpdatePerson(personUpdateRequest)));
        }

        // when supply valid PersonUpdateRequest => update the person details and return PersonResponse object
        [Fact]
        public async Task UpdatePerson_ValidPersonUpdateRequest()
        {
            // Arrange
            CountryAddRequest countryAddRequest = new CountryAddRequest()
            {
                CountryName = "Egypt"
            };
            CountryResponse countryResponse = await Task.Run(() => _countryService.AddCountry(countryAddRequest));

            PersonAddRequest personRequest1 = new PersonAddRequest()
            {
                Name = "Ali",
                Email = "sample@gmail.com",
                CountryID = countryResponse.CountryID,
                DateOfBirth = new DateTime(2004, 4, 4),
                Gender = "Male",
                Address = "Cairo",
                ReceiveNewsLetters = true
            };
            PersonResponse personRequest = await Task.Run(() => _personService.AddPerson(personRequest1));

            PersonUpdateRequest personUpdateRequest = personRequest.ToUpdateRequest();
            personUpdateRequest.Name = "Khaled";
            personUpdateRequest.Email = "khaled@gmail.com";

            // Act
            PersonResponse updatedPerson = await Task.Run(() => _personService.UpdatePerson(personUpdateRequest));

            // Assert
            Assert.Equal(personUpdateRequest.PersonID, updatedPerson.PersonID);
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
            Assert.False(isDeleted);
        }

        // when supply valid personID => return true
        [Fact]
        public async Task DeletePerson_ValidPersonID()
        {
            // Arrange
            CountryAddRequest countryAddRequest = new CountryAddRequest()
            {
                CountryName = "Egypt"
            };
            CountryResponse countryResponse = await Task.Run(() => _countryService.AddCountry(countryAddRequest));

            PersonAddRequest personRequest = new PersonAddRequest()
            {
                Name = "Ali",
                Email = "sample@gmail.com",
                CountryID = countryResponse.CountryID,
                DateOfBirth = new DateTime(2004, 4, 4),
                Gender = "Male",
                Address = "Cairo",
                ReceiveNewsLetters = true
            };
            PersonResponse personResponse = await Task.Run(() => _personService.AddPerson(personRequest));

            // Act
            bool isDeleted = await Task.Run(() => _personService.DeletePerson(personResponse.PersonID));

            // Assert
            Assert.True(isDeleted);
        }

        #endregion
    }
}