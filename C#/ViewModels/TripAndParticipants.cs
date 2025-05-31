using Voyagers.Models;

namespace Voyagers.ViewModels
{
    public class TripAndParticipants
    {
        public TripAndParticipants(Trip trip, List<TripParticipants> participants, List<TripOwners> owners)
        {
            Trip = trip;
            Participants = participants;
            Owners = owners;
        }

        public Trip Trip { get; set; } = default!;

        public List<TripOwners> Owners { get; set; } = default!;
        public List<TripParticipants> Participants { get; set; } = default!;
    }
}
