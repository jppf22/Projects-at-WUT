using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Voyagers.Data;
using Voyagers.Models;
using Voyagers.Services;
using Voyagers.ViewModels;

namespace Voyagers.Controllers
{

    [Authorize] //Ensures that only logged in users can access the TripsController
    public class TripsController : Controller
    {
        private readonly VoyagersContext _context;
        private readonly ApplicationDbContext _identityContext;
        private readonly FK_USERID_Validator _validatorService;
        private readonly UserManager<IdentityUser> _userManager;

        public TripsController(VoyagersContext context, ApplicationDbContext identitycontext ,FK_USERID_Validator validator, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _validatorService = validator;
            _identityContext = identitycontext;
            _userManager = userManager;
        }

        // GET: Trips/Index
        public async Task<IActionResult> Index()
        {
            var trips = await _context.Trip.ToListAsync();
            var tripParticipants = await _context.TripParticipants.ToListAsync();
            var tripOwners = await _context.TripOwners.ToListAsync();
            var users = await _identityContext.Users.ToListAsync();

            var tp = new List<TripAndParticipants>();

            foreach(Trip trip in trips)
            {
                var participantsOnTheTrip = tripParticipants.FindAll(p => p.TripID == trip.TripID);
                var ownersOnTheTrip = tripOwners.FindAll(o => o.TripID == trip.TripID);
                tp.Add(new TripAndParticipants(trip, participantsOnTheTrip, ownersOnTheTrip));
            }

            return View(tp);
        }

        // GET: Trips/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trip = await _context.Trip
                .FirstOrDefaultAsync(m => m.TripID == id);
            if (trip == null)
            {
                return NotFound();
            }

            var trip_participants = await _context.TripParticipants
                        .Where(tp => tp.TripID == id)
                        .ToListAsync();

            var trip_owners = await _context.TripOwners
                .Where(tp => tp.TripID == id)
                .ToListAsync();


            var vm = new TripDetailsModel { Trip = trip, Participants = trip_participants, Owners = trip_owners };
            return View(vm);
        }

        // GET: Trips/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Trips/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TripID,Location,Date,Description,Duration,Capacity,Price,Rating,ConcurrencyToken")] Trip trip)
        {
            var user = await _identityContext.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user == null)
            {
                return Unauthorized();
            }


            if (ModelState.IsValid)
            {

                _context.Trip.Add(trip);

                await _context.SaveChangesAsync();

                // The owner of the trip becomes one of the owners automatically
                var trip_owner = new TripOwners
                {
                    TripID = trip.TripID,
                    UserID = user.Id
                };

                _context.TripOwners.Add(trip_owner);

                await _context.SaveChangesAsync();


                //The owner of the trip is automatically registered as a participant
                var trip_participant = new TripParticipants
                {
                    TripID = trip.TripID,
                    UserID = user.Id,
                    RegistrationDate = DateTime.Now
                };

                _context.TripParticipants.Add(trip_participant);

                await _context.SaveChangesAsync(); 

                return RedirectToAction(nameof(Index));
            }

            return View(trip);
        }

        // GET: Trips/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trip = await _context.Trip
                .AsNoTracking() //No tracking, makes sure changes are only bidding when post is succesfull
                .FirstOrDefaultAsync(m => m.TripID == id);

            if (trip == null)
            {
                return NotFound();
            }
            return View(trip);
        }

        // POST: Trips/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TripID,Location,Date,Description,Duration,Capacity,Price,Rating,ConcurrencyToken")] Trip trip)
        {
            if (id != trip.TripID)
            {
                return NotFound();
            }

            var previous_trip = await _context.Trip.FirstOrDefaultAsync(t => t.TripID == id); //TODO: USE ALREADY EXISTING METHOD PREVIOUS_TRIP(ID)

            if (previous_trip == null)
            {
                TempData["ErrorMessage"] = "Unable to save changes. The trip was deleted by another user.";
                return RedirectToAction(nameof(Index));
            }


            var user = await _identityContext.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            var previous_trip_owners = await _context.TripOwners
                .Where(t => t.TripID == id)
                .Select(t => t.UserID)
                .ToListAsync();


            if (user == null || !(previous_trip_owners.Contains(user.Id)) )
            {
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                _context.Entry(previous_trip).Property("ConcurrencyToken").OriginalValue = trip.ConcurrencyToken;
                if (await TryUpdateModelAsync(previous_trip, "",
                    t => t.Location,
                    t => t.Date,
                    t => t.Description,
                    t => t.Duration,
                    t => t.Capacity,
                    t => t.Price,
                    t => t.Rating))
                {
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    catch(DbUpdateConcurrencyException ex)
                    {
                        var exceptionEntry = ex.Entries.Single();
                        var tripValues = (Trip)exceptionEntry.Entity;
                        var databaseEntry = exceptionEntry.GetDatabaseValues();
                        if (databaseEntry == null)
                        {
                            ModelState.AddModelError(string.Empty, "Unable to save changes. The trip was deleted by another user.");
                        }
                        else
                        {
                            var databaseValues = (Trip)databaseEntry.ToObject();

                            if(databaseValues.Capacity != tripValues.Capacity)
                            {
                                ModelState.AddModelError(nameof(Trip.Capacity), "The capacity value has changed. Please reload the page.");
                            }
                            if (databaseValues.Date != tripValues.Date)
                            {
                                ModelState.AddModelError(nameof(Trip.Date), "The date value has changed. Please reload the page.");
                            }
                            if (databaseValues.Description != tripValues.Description)
                            {
                                ModelState.AddModelError(nameof(Trip.Description), "The description value has changed. Please reload the page.");
                            }
                            if (databaseValues.Duration != tripValues.Duration)
                            {
                                ModelState.AddModelError(nameof(Trip.Duration), "The duration value has changed. Please reload the page.");
                            }
                            if (databaseValues.Location != tripValues.Location)
                            {
                                ModelState.AddModelError(nameof(Trip.Location), "The location value has changed. Please reload the page.");
                            }
                            if (databaseValues.Price != tripValues.Price)
                            {
                                ModelState.AddModelError(nameof(Trip.Price), "The price value has changed. Please reload the page.");
                            }
                            if (databaseValues.Rating != tripValues.Rating)
                            {
                                ModelState.AddModelError(nameof(Trip.Rating), "The rating value has changed. Please reload the page.");
                            }



                            ModelState.AddModelError(string.Empty, "The trip has been modified by another user. The edit was not saved.");
                            previous_trip.ConcurrencyToken = databaseValues.ConcurrencyToken;
                            ModelState.Remove((nameof(previous_trip.ConcurrencyToken)));
                        }
                    }
                }
            }
            return View(previous_trip);
        }

        // GET: Trips/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trip = await _context.Trip
                .FirstOrDefaultAsync(m => m.TripID == id);
            if (trip == null)
            {
                return NotFound();
            }

            return View(trip);
        }

        // POST: Trips/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trip = await _context.Trip.FindAsync(id);
            var user = await _identityContext.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
             
            var trip_owners = await _context.TripOwners
                .Where(t => t.TripID == id)
                .ToListAsync();

            var trip_owners_ids = trip_owners.Select(t => t.UserID).ToList();

            if (trip != null)
            {
                if (user == null || !(trip_owners_ids.Contains(user.Id)))
                {
                    return Unauthorized();
                }

                var trip_participants = await _context.TripParticipants
                    .Where(tp => tp.TripID == id)
                    .ToListAsync();

                foreach (var trip_participant in trip_participants)
                {
                    _context.TripParticipants.Remove(trip_participant);
                }

                foreach(var trip_owner in trip_owners)
                {
                    _context.TripOwners.Remove(trip_owner);
                }

                _context.Trip.Remove(trip);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Trips/Register/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(int id)
        {
            var user = await _identityContext.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user == null)
            {
                return Unauthorized();
            }

            var trip = await _context.Trip
                .AsNoTracking() //makes it so parallel capacity fields can be done, by not locking the capacity field
                .FirstOrDefaultAsync(t => t.TripID == id);

            if (trip == null)
            {
                return NotFound();
            }

            using (var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable))
            {
                int currentParticipantCount = await _context.TripParticipants
                    .Where(tp => tp.TripID == id)
                    .CountAsync();

                if (currentParticipantCount < trip.Capacity)
                {
                    var trip_participant = new TripParticipants
                    {
                        TripID = id,
                        UserID = user.Id,
                        RegistrationDate = DateTime.Now
                    };
                    _context.TripParticipants.Add(trip_participant);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                else
                {
                    TempData["ErrorMessage"] = "Trip is full. Unable to register.";
                    await transaction.RollbackAsync();

                }

                return RedirectToAction(nameof(Index));
            }
        }



        // POST: Trips/Unregister/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unregister(int id) //MISSING: ERROR MESSAGE FOR OWNERS TRYING TO UNREGISTER
        {
            var trip = await _context.Trip.FindAsync(id);
            var user = await _identityContext.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            var trip_owners_ids = await _context.TripOwners
                .Where(t => t.TripID == id)
                .Select(t => t.UserID)
                .ToListAsync();


            if (trip != null)
            {
                if (user == null || trip_owners_ids.Contains(user.Id)) //A trip owner can't unregister from its own trip, only if ownership status is removed
                {
                    return Unauthorized();
                }
                var trip_participant = await _context.TripParticipants.FirstOrDefaultAsync(tp => tp.TripID == id && tp.UserID == user.Id);
                if (trip_participant != null)
                {
                    _context.TripParticipants.Remove(trip_participant);
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Trips/RemoveParticipant/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveParticipant(int tripId, string userId)
        {
            var trip = await _context.Trip.FindAsync(tripId);
            var user = await _identityContext.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            var trip_owners_ids = await _context.TripOwners
                .Where(t => t.TripID == tripId)
                .Select(t => t.UserID)
                .ToListAsync();
            if (trip != null)
            {
                if (user == null || !(trip_owners_ids.Contains(user.Id)))
                {
                    return Unauthorized();
                }
                var trip_participant = await _context.TripParticipants.FirstOrDefaultAsync(tp => tp.TripID == tripId && tp.UserID == userId);
                if (trip_participant != null)
                {
                    _context.TripParticipants.Remove(trip_participant);
                }
            }
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = tripId });
        }

        // POST: Trips/RemoveOwner/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveOwner(int tripId, string userId)
        {
            var trip = await _context.Trip.FindAsync(tripId);
            var user = await _identityContext.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            var trip_owners_ids = await _context.TripOwners
                .Where(t => t.TripID == tripId)
                .Select(t => t.UserID)
                .ToListAsync();
            if (trip != null)
            {
                if (user == null || !(trip_owners_ids.Contains(user.Id)) || user.Id == userId) // Can only remove ownership of other owners, not is own
                {
                    return Unauthorized();
                }
                var trip_owner = await _context.TripOwners.FirstOrDefaultAsync(tp => tp.TripID == tripId && tp.UserID == userId);
                if (trip_owner != null)
                {
                    _context.TripOwners.Remove(trip_owner);
                }
            }
            await _context.SaveChangesAsync();


            return RedirectToAction("Details", new { id = tripId });
        }

        // POST: Trips/MakeOwner/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MakeOwner(int tripId, string userId)
        {
            var trip = await _context.Trip.FindAsync(tripId);
            var user = await _identityContext.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            var trip_owners_ids = await _context.TripOwners
                .Where(t => t.TripID == tripId)
                .Select(t => t.UserID)
                .ToListAsync();
            if (trip != null)
            {
                if (user == null || !(trip_owners_ids.Contains(user.Id)))
                {
                    return Unauthorized();
                }

                var trip_owner = new TripOwners { TripID = tripId, UserID = userId };

                _context.TripOwners.Add(trip_owner);
            }
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = tripId });
        }


        private bool TripExists(int id)
        {
            return _context.Trip.Any(e => e.TripID == id);
        }
        // Method to fetch the previous trip
        public async Task<IActionResult> PreviousTrip(int id)
        {
            var trip = await _context.Trip
                .Where(t => t.TripID < id)
                .OrderByDescending(t => t.TripID)
                .FirstOrDefaultAsync();

            if (trip == null)
            {
                return NotFound();
            }

            return View(trip);
        }
    }
}
