using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlaneTicket.Data;
using PlaneTicket.Models;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PlaneTicket.Controllers
{
    [Authorize]
    public class FlightsController : Controller
    {
        private readonly AppDbContext _context;

        public FlightsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Flights
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var currentTime = DateTime.UtcNow; // Use UTC time if your times are in UTC
            var upcomingFlights = await _context.Flights
                .Include(f => f.Airplane) // Include the Airplane data
                .Where(f => f.DepartureTime > currentTime) // Filter out overdue flights
                .ToListAsync();
            return View(upcomingFlights);
        }


        // GET: Flights/Details/{id}
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var flight = await _context.Flights.FirstOrDefaultAsync(m => m.Id == id);
            if (flight == null)
            {
                return NotFound();
            }

            return View(flight);
        }

        // GET: Flights/Create
        [HttpGet]
        [Authorize(Policy = "AdminOrRedirect")]
        public IActionResult Create()
        {
            var airplaneList = _context.Airplanes.Select(a => new { a.Id, a.Name }).ToList();
            ViewBag.AirplaneId = new SelectList(airplaneList, "Id", "Name");
            return View();
        }

        // POST: Flights/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOrRedirect")]
        public async Task<IActionResult> Create(
            [Bind("FlightNumber,DepartureTime,ArrivalTime,Origin,Destination,Price,AirplaneId")] Flight flight)
        {
            flight.DepartureTime = flight.DepartureTime.ToUniversalTime();
            flight.ArrivalTime = flight.ArrivalTime.ToUniversalTime();
            flight.FlightDuration = CalculateFlightDuration(flight.DepartureTime, flight.ArrivalTime);
            flight.FlightStatus = "On-Time"; // Default status, adjust as necessary

            _context.Add(flight);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        // GET: Flights/Edit/{id}
        [HttpGet]
        [Authorize(Policy = "AdminOrRedirect")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var flight = await _context.Flights.FindAsync(id);
            if (flight == null)
            {
                return NotFound();
            }

            return View(flight);
        }

        // POST: Flights/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOrRedirect")]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,FlightNumber,DepartureTime,ArrivalTime,Origin,Destination,Price")]
            Flight flight)
        {
            if (id != flight.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    flight.FlightDuration = CalculateFlightDuration(flight.DepartureTime, flight.ArrivalTime);
                    _context.Update(flight);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FlightExists(flight.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            return View(flight);
        }

        // POST: Flights/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOrRedirect")]
        public async Task<IActionResult> Delete(int id)
        {
            var flight = await _context.Flights.FindAsync(id);
            if (flight == null)
            {
                return NotFound();
            }

            _context.Flights.Remove(flight);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private TimeSpan CalculateFlightDuration(DateTime departureTime, DateTime arrivalTime)
        {
            var duration = arrivalTime - departureTime;
            return TimeSpan.FromHours(Math.Round(duration.TotalHours));
        }

        private bool FlightExists(int id)
        {
            return _context.Flights.Any(e => e.Id == id);
        }
    }
}