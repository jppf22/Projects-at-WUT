using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Voyagers.Models
{
    public class Trip
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public required int TripID { get; set; }

        //public required string UserID { get; set; }

        [MinLength(2)]
        [MaxLength(50)]
        public required string Location { get; set; }

        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [MaxLength(512)]
        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }

        [Range(1, 365)]
        public int? Duration { get; set; }

        [Range(1, 50)]
        public int Capacity { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        [DataType(DataType.Currency)]
        public decimal? Price { get; set; }

        [Range(1, 5)]
        public int? Rating { get; set; }

        [Timestamp]
        public byte[]? ConcurrencyToken { get; set; } = null!;
    }
}
