using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class GetPersons_SortedProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            string sp_GetAllPersons = @"
                CREATE PROCEDURE [dbo].[GetAllPersons]
                AS
                BEGIN
                    SELECT PersonID, Name, Email, DateOfBirth, Gender, CountryID, Address, ReceiveNewsLetters
                        FROM Persons
                    ORDER BY Name ASC;
                END             
            ";

            migrationBuilder.Sql(sp_GetAllPersons);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            string sp_GetAllPersons = @"
                drop PROCEDURE [dbo].[GetAllPersons]            
            ";

            migrationBuilder.Sql(sp_GetAllPersons);
        }
    }
}
