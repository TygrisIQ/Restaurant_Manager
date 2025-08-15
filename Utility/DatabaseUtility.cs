using Restaurant_Manager.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant_Manager.Utility
{
    /// <summary>
    /// THIS CLASS IS A SANITY CHECK CLASS I USED DURING DEVELOPMENT TO TEST THE DATABSE, IT WAS INSPIRED BY CODE ON GITHUB
    /// ITS NOT USED ANYMORE AND ONLY USED DURING DEVELOPMENT
    /// </summary>
    public static class DatabaseUtility
    {
        public static async Task DbTest()
        {
            try
            {
                Debug.WriteLine("=== DB Sanity: start ===");
                await Db.Initialize();

                // 1) check if database is initialized
                Debug.WriteLine("DB init OK");

                // 2) Seeded tables?
                var tables = await Db.GetAllTables();
                Debug.Assert(tables.Count > 0, "No tables seeded");
                Debug.WriteLine($"Tables count: {tables.Count}");

                // 3) Availability for a window
                var start = DateTime.Now.AddHours(2);
                var duration = TimeSpan.FromHours(1.5);
                var available = await Db.GetAvailableTablesAsync(start, duration, minCapacity: 2);
                Debug.Assert(available.Count > 0, "No tables available (unexpected in fresh DB)");
                var table = available.First();
                Debug.WriteLine($"Using table #{table.TableNumber} (Id={table.Id})");

                // 4) Create reservation
                var r = await Db.CreateReservationAsync(table.Id, "Sanity Test", start, duration, partySize: 2, notes: "window");
                Debug.Assert(r.Id > 0, "Reservation insert failed");
                Debug.WriteLine($"Created reservation Id={r.Id}");

                // 5) Conflict check: creating overlapping res on same table should fail
                bool conflictThrown = false;
                try
                {
                    _ = await Db.CreateReservationAsync(table.Id, "Overlap", start.AddMinutes(30), TimeSpan.FromMinutes(45));
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("already booked"))
                {
                    conflictThrown = true;
                }
                Debug.Assert(conflictThrown, "Overlap did not throw");

                bool badDuration = false;
                try { _ = await Db.CreateReservationAsync(table.Id, "Bad", DateTime.Now, TimeSpan.Zero); }
                catch (ArgumentException) { badDuration = true; }
                Debug.Assert(badDuration, "Zero duration should throw");

                bool tooMany = false;
                try { _ = await Db.CreateReservationAsync(table.Id, "Too big", DateTime.Now.AddHours(4), TimeSpan.FromHours(1), partySize: 999); }
                catch (InvalidOperationException ex) when (ex.Message.Contains("exceeds capacity"))
                { tooMany = true; }
                Debug.Assert(tooMany, "Capacity check should throw");

                Debug.WriteLine("=== DB Sanity: PASS ===");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("=== DB Sanity: FAIL === " + ex);
            }
        }
    }
}
