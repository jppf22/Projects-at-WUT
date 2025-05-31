using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Voyagers.Models
{
    [PrimaryKey(nameof(TripID),nameof(UserID))]
    public class TripParticipants
    {
        [ForeignKey("TripID")]
        public required int TripID { get; set; }
   
        public required string UserID { get; set; }

        public DateTime RegistrationDate { get; set; }
    }
}
