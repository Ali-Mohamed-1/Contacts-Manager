using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.AspNetCore.Mvc;
using Fizzler;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;

namespace CRUDtest
{
    public class PersonControllerIntegrationTest : IClassFixture<CustomeWebApplicationFactory>
    {
        private readonly HttpClient _client;
        public PersonControllerIntegrationTest(CustomeWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        #region Index

        [Fact]
        public async Task Index_ToRturnViewAsync()
        {
            // Arrange
            // Act
            HttpResponseMessage response = await _client.GetAsync("/Persons/Index");

            // Assert
            response.IsSuccessStatusCode.Should().BeTrue();
            
            string responseBody = await response.Content.ReadAsStringAsync();
            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(responseBody);

            var document = html.DocumentNode;
            var header = document.QuerySelectorAll("table.persons").Should().NotBeNull(); // load the persons table in Index.cshtml
        }

        #endregion
    }
}
