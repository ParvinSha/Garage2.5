﻿using Garage2._5.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Bogus;
using System.Threading.Tasks;

namespace Garage2._5.Data
{
    public class SeedData
    {
        public static void Initialize(IServiceProvider services)
        {
           
                var option = services.GetRequiredService<DbContextOptions<Garage2_5Context>>();
            using (var context = new Garage2_5Context(option))
            {
                // Load Vehicle Type for the first time. If Data already loaded in the table do not load it again.


                var fake = new Faker("sv");
                var vehicles = new List<VehicleType>();
                if (context.VehicleType.Any())
                {
                    //  context.VehicleType.RemoveRange(context.VehicleType);

                    //context.SaveChanges();
                }

                else
                {
                    for (int i = 0; i < 5; i++)
                    {

                        var ftype = fake.Vehicle.Type();

                        var vehicletype = new VehicleType

                        {
                            Type = ftype,
                        };

                        vehicles.Add(vehicletype);



                    }

                    context.AddRange(vehicles);
                    context.SaveChanges();
                }

            }
            }

        }
    }

