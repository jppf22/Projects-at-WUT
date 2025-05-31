
using Voyagers.Models;

namespace Voyagers.ViewModels
{
    public class TripDetailsModel
    {
        public required Trip Trip { get; set; }

        public List<TripOwners>? Owners { get; set; }
        public List<TripParticipants>? Participants { get; set; }
    }
}
