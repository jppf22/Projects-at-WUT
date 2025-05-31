using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voyagers.Models;

namespace Voyagers.Data
{
    public class VoyagersContext : DbContext
    {
        public VoyagersContext (DbContextOptions<VoyagersContext> options)
            : base(options)
        {
        }

        public DbSet<Voyagers.Models.Trip> Trip { get; set; } = default!;
        public DbSet<Voyagers.Models.TripParticipants> TripParticipants { get; set; } = default!;

        public DbSet<Voyagers.Models.TripOwners> TripOwners { get; set; } = default!;
    }
}
