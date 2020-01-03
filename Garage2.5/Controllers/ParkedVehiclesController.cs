﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Garage2._5.Data;
using Garage2._5.Models;
using AutoMapper;
using Garage2._5.ViewModels;

namespace Garage2._5.Controllers
{
    public class ParkedVehiclesController : Controller
    {
        private readonly Garage2_5Context _context;
        private readonly IMapper mapper;

        public ParkedVehiclesController(Garage2_5Context context, IMapper mapper)
        {
            _context = context;
            this.mapper = mapper;
        }

        // GET: ParkedVehicles
        public async Task<IActionResult> Index()
        {
        

            // View Model to Have Owner name,CheckIn Time,Regno,Type.
            var parkedVehicles = _context.ParkedVehicle.Where(p => (p.CheckOutTime) == default(DateTime));
            var model3 = await mapper.ProjectTo<VehicleListDetails>(parkedVehicles).ToListAsync();

            return View(model3);
        }

        // GET: ParkedVehicles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parkedVehicle = await _context.ParkedVehicle
                .Include(p => p.Member)
                .Include(p => p.VehicleType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (parkedVehicle == null)
            {
                return NotFound();
            }

            return View(parkedVehicle);
        }

        // GET: ParkedVehicles/
        public IActionResult Park()

        // To Indentify Member name Unique, Included Email also to appear in the Dropdown list.
        {
            var memberList = _context.Set<Member>()
                     .Select(x => new SelectListItem
                     {
                         Value = x.Id.ToString(),
                         Text = x.FullName + "( " + x.Email + ")"
                     }).ToList();
            ViewData["MemberId"] = memberList;

         // Display the Vehicle Type in the Dropdown List.

            ViewData["VehicleTypeId"] = new SelectList(_context.Set<VehicleType>(), "Id", "Type");
            return View();
        }

        // POST: ParkedVehicles/Park
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Park([Bind("Id,RegNo,Color,Brand,Model,NoOfWheels,CheckInTime,CheckOutTime,MemberId,VehicleTypeId")] ParkedVehicle parkedVehicle)
        {
            if (ModelState.IsValid)
            {
                // Populate the current date and Time for checkIN field. 

                parkedVehicle.CheckInTime = DateTime.Now;

                parkedVehicle.CheckOutTime = default(DateTime);

                _context.Add(parkedVehicle);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MemberId"] = new SelectList(_context.Set<Member>(), "Id", "Id", parkedVehicle.MemberId);
            ViewData["VehicleTypeId"] = new SelectList(_context.Set<VehicleType>(), "Id", "Id", parkedVehicle.VehicleTypeId);
            return View(parkedVehicle);
        }

        // GET: ParkedVehicles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parkedVehicle = await _context.ParkedVehicle.FindAsync(id);
            if (parkedVehicle == null)
            {
                return NotFound();
            }
            ViewData["MemberId"] = new SelectList(_context.Set<Member>(), "Id", "Id", parkedVehicle.MemberId);
            ViewData["VehicleTypeId"] = new SelectList(_context.Set<VehicleType>(), "Id", "Id", parkedVehicle.VehicleTypeId);
            return View(parkedVehicle);
        }

        // POST: ParkedVehicles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RegNo,Color,Brand,Model,NoOfWheels,CheckInTime,CheckOutTime,MemberId,VehicleTypeId")] ParkedVehicle parkedVehicle)
        {
            if (id != parkedVehicle.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(parkedVehicle);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ParkedVehicleExists(parkedVehicle.Id))
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
            ViewData["MemberId"] = new SelectList(_context.Set<Member>(), "Id", "Id", parkedVehicle.MemberId);
            ViewData["VehicleTypeId"] = new SelectList(_context.Set<VehicleType>(), "Id", "Id", parkedVehicle.VehicleTypeId);
            return View(parkedVehicle);
        }

        // GET: ParkedVehicles/Delete/5
        public async Task<IActionResult> Unpark(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parkedVehicle = await _context.ParkedVehicle
                .Include(p => p.Member)
                .Include(p => p.VehicleType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (parkedVehicle == null)
            {
                return NotFound();
            }

            return View(parkedVehicle);
        }

        // POST: ParkedVehicles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Check out will just populate Checkout Time but will not delete any record from DB.

            var parkedVehicle = await _context.ParkedVehicle.FindAsync(id);
            parkedVehicle.CheckOutTime = DateTime.Now;
           // _context.ParkedVehicle.Remove(parkedVehicle);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ParkedVehicleExists(int id)
        {
            return _context.ParkedVehicle.Any(e => e.Id == id);
        }


        // Check whether the Regno has already been parked.
        public IActionResult CheckRegno(string regno)
        {
            if (_context.ParkedVehicle.Any(s => s.RegNo == regno && s.CheckOutTime == default(DateTime)))
            {
                return Json($"{regno} is already Parked");
            }

            return Json(true);
        }

        public async Task<IActionResult> Receipt(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parkedVehicle = await _context.ParkedVehicle.FirstOrDefaultAsync(m => m.Id == id);
            

            if (parkedVehicle == null)
            {
                return NotFound();
            }

            var model = new Receipt();
            model.RegNo = parkedVehicle.RegNo;

            var parkedMemberId = parkedVehicle.MemberId;
            var parkedvehicletypeId = parkedVehicle.VehicleTypeId;
            var memberdetails = await _context.Member.FirstOrDefaultAsync(m => m.Id == parkedMemberId);
            var vehicletypedetails = await _context.VehicleType.FirstOrDefaultAsync(m => m.Id == parkedvehicletypeId);

            model.FullName = memberdetails.FullName;
            model.Type = vehicletypedetails.Type;
            model.CheckInTime = parkedVehicle.CheckInTime;
            model.CheckOutTime = DateTime.Now;
            var totaltime = model.CheckOutTime - model.CheckInTime;
            var morethanhr = (totaltime.Seconds > 0) ? 1 : 0;


            if (totaltime.Days == 0)
            {
                model.Totalparkingtime = totaltime.Hours + " Hrs " + totaltime.Minutes + " Mins " + totaltime.Seconds + " Secs";
                model.Totalprice = ((totaltime.Hours + morethanhr) * 5) + "Kr";
            }
            else
            {
                model.Totalparkingtime = totaltime.Days + "Days" + " " + totaltime.Hours + " hrs " + " " + totaltime.Minutes + " Mins " + +totaltime.Seconds + " Secs";
                model.Totalprice = (totaltime.Days * 100) + ((totaltime.Hours + morethanhr) * 5) + "Kr";
            }

            parkedVehicle.CheckOutTime = DateTime.Now;
            await _context.SaveChangesAsync();

            return View(model);
        }


        public async Task<IActionResult> GetStatistic()
        {
            //ViewBag.NoOfFreePlaces = GetFreeSlotsNo();
            //ViewBag.NoOfFreePlacesForMotorcycle = GetFreeSlotsNoForMotorcycle();

            int totalWheels = 0;
            double totalMin = 0;
            DateTime nowTime = DateTime.Now;
            int nowTimeResult = (nowTime.Day * 100) + nowTime.Hour + nowTime.Minute;
            double timePrice = 0;
            double totalParkTimePrice = 0;

            var model = new Statistics();

            // Get car count wheels
            var parkedVehicles = _context.ParkedVehicle.Where(p => (p.CheckOutTime) == default(DateTime));
            var Wheels = parkedVehicles.Select(m => (m.NoOfWheels));
            foreach (var wheel in Wheels)
            {
                totalWheels += wheel;
            }

            // Get car count            
            var carCount = _context.ParkedVehicle
                .Include(p => p.VehicleType)
                .Where(p => p.VehicleType.Type.Equals("Car") && p.CheckOutTime == default(DateTime)).ToList();

            model.TotalCar = carCount.Count();

            // Get Boat count
            var BoatCount = _context.ParkedVehicle
              .Include(p => p.VehicleType)
              .Where(p => p.VehicleType.Type.Equals("Boat") && p.CheckOutTime == default(DateTime)).ToList();

            model.TotalBoat = BoatCount.Count();

            // Get Bus count
            var BusCount = _context.ParkedVehicle
            .Include(p => p.VehicleType)
            .Where(p => p.VehicleType.Type.Equals("Bus") && p.CheckOutTime == default(DateTime)).ToList();

            model.TotalBus = BusCount.Count();

            // Get Airplane count
            var AirplaneCount = _context.ParkedVehicle
          .Include(p => p.VehicleType)
          .Where(p => p.VehicleType.Type.Equals("Airplane") && p.CheckOutTime == default(DateTime)).ToList();

            model.TotalAirplane = AirplaneCount.Count();
                       
       
        // Get Motorcycle count
        var MotorcycleCount = _context.ParkedVehicle
           .Include(p => p.VehicleType)
           .Where(p => p.VehicleType.Type.Equals("Motorbike") && p.CheckOutTime == default(DateTime)).ToList();

            model.TotalMotorbike = MotorcycleCount.Count();

            //Total Parking time



            var totTimeChIn = _context.ParkedVehicle.Where(p => (p.CheckOutTime) == default(DateTime)).Select(m => (m.CheckInTime));
            int TotalPrice = 0;

            foreach (var chTime in totTimeChIn)
            {
                var totaltimenow = DateTime.Now - chTime;
                var morehr = (totaltimenow.Seconds > 0) ? 1 : 0;

                if (totaltimenow.Days == 0)
                {
                     TotalPrice += ((totaltimenow.Hours + morehr) * 5);
                }
                else
                {
                    TotalPrice += (totaltimenow.Days * 100) + ((totaltimenow.Hours + morehr) * 5);
                }

            }

            model.TotalParkedVehiclePrice =  TotalPrice + "Kr";

            model.TotalVehicles = parkedVehicles.Count();
            model.TotalWheels = totalWheels;

            await _context.SaveChangesAsync();

            return View(model);
        }

        public async Task<IActionResult> Sort(string columnName)
        {
            //ViewBag.NoOfFreePlaces = GetFreeSlotsNo();
            //ViewBag.NoOfFreePlacesForMotorcycle = GetFreeSlotsNoForMotorcycle();

            var model = await _context.ParkedVehicle.Where(m => m.CheckOutTime.Equals(default(DateTime))).ToListAsync();
            switch (columnName)
            {
                case "Type":
                    model = model.OrderByDescending(m => m.VehicleType).ToList();
                    break;
                case "RegNo":
                    model = model.OrderByDescending(m => m.RegNo).ToList();
                    break;
                case "OwnerName":
                    model = model.OrderByDescending(m => m.Member).ToList();
                    break;
                case "CheckInTime":
                    model = model.OrderByDescending(m => m.CheckInTime).ToList();
                    break;
                default:
                    break;
            }
            return View(nameof(Index), model);
        }
    }
}
