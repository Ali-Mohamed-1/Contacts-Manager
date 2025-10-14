using System.ComponentModel.DataAnnotations;

namespace Entities
{
    /// <summary>
    /// Domain model for storing country information
    /// </summary>
    public class Country
    {
        [Key]
        public Guid CountryID { get; set; }

        [StringLength(60)]
        public string? CountryName { get; set; }

    }
}
