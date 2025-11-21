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
    public class PersonsSorterServiceTest
    {
        private readonly IPersonsRepository _personRepo;
        private readonly Mock<IPersonsRepository> _personRepoMock;
        private readonly IPersonsSorterService _personsSorterService;
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly IFixture _fixture;
        private readonly Mock<ILogger<PersonsGetterService>> _loggerMock;

        public PersonsSorterServiceTest(ITestOutputHelper testOutputHelper)
        {
            _fixture = new Fixture();
            _personRepoMock = new Mock<IPersonsRepository>();
            _personRepo = _personRepoMock.Object;

            _loggerMock = new Mock<ILogger<PersonsGetterService>>();
            _personsSorterService = new PersonsSorterService(_personRepo, _loggerMock.Object);
            _testOutputHelper = testOutputHelper;
        }

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
            List<PersonResponse> sortedPersons = await _personsSorterService.GetSortedPersons(personsRequests, nameof(PersonResponse.Name), false);

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
    }
}

