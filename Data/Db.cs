using Restaurant_Manager.Model;
using Restaurant_Manager.Model.Enums;
using SQLite;

namespace Restaurant_Manager.Data
{
    /// <summary>
    /// THIS IS THE MAIN DATABASE FILE 
    /// IMPORTANT(((IF YOU ARE WORKING ON UI JUST USE THE DatabaseApi.cs class i made that so you can call methods EASIER AND FASTER WITHOUT READING THIS!)))
    /// THIS CLASS INITIALIZES SQLite and creates the .db file
    /// HAS CRUD METHODS FOR ALL Model objects in the program
    /// CHECKS FOR CONFLICTS AND CREATES INDEXES in case of RESERVATIONS/TABLES
    /// AT THE MOMENT IT ALSO DELETES AND RE-CREATES DATABASE ON EACH RUN SO ITS EASIER FOR DEBUGGING
    /// AND ALSO DOES SEEDING AFTER CREATING THE DATABASE
    /// YOUSSIF AL-HALAWCHE
    /// </summary>
    public static class Db
    {
        //RESET DATABASE OF EVERY RUN FLAG; USE THIS WHEN YOU WANT TO START WITH A FRESH DATABASE 
        //
        public static bool ResetDbOnStart { get; set; } = false;
        //The Connection object and the initialized boolean are global so methods are able to check of connection/initialization status
        static SQLiteAsyncConnection? _conn;
        static bool _initialized;

        //MENU ITEMS AND EMPLOYEES SEEDING LISTS

        static readonly MenuEntry[] menuEntries = new[]
        {
            new MenuEntry { Name="Margherita Pizza", Price=12.5m, IsAvailable=true, Type=MenuEntryType.MainDish },
            new MenuEntry { Name="Shawrma Chicken Wrap", Price=6.0m, IsAvailable=true, Type=MenuEntryType.MainDish },
            new MenuEntry { Name="Diet Coke", Price=2.0m, IsAvailable=true, Type=MenuEntryType.Drink },
            new MenuEntry { Name="Chicken Biryani", Price=12.0m, IsAvailable=true, Type=MenuEntryType.MainDish },
            new MenuEntry { Name="Chocolate Cake", Price=6.0m, IsAvailable=true, Type=MenuEntryType.Dessert },
            new MenuEntry { Name="Lamb on rice", Price=17.0m, IsAvailable=true, Type=MenuEntryType.MainDish },
        };
        static readonly Employee[] EmployeesList = new[]
        {
        new Employee { Name="Someone", Age=26, City="Calgary", Country="CA" },
        new Employee { Name="Youssif", Age=24, City="Calgary", Country="CA" },
        new Employee { Name="Paulie", Age=67, City="New Jersey", Country="US" },
        new Employee { Name="Tony", Age=44, City="New Jersey", Country="US" },
        new Employee { Name="Aidriana", Age=24, City="Calgary", Country="CA" },
        };
        ///
        public static async Task Initialize()
        {
            //check if the databse is already initialzied, if so returns
            if (_initialized)
            {
                return;
            }
            //establish connection
            var path = Path.Combine(FileSystem.AppDataDirectory, "res.db");
            System.Diagnostics.Debug.WriteLine($"======================== PATH IS AT {path} ========================");
            //Added this check to delete previous databases if found
            //so each run of the app starts with a fresh version of the database
            //to avoid confusion and cut on 
            //debugging time for this assignemnt
            if(ResetDbOnStart && File.Exists(path))
            {
                File.Delete(path);
            }
            _conn = new SQLiteAsyncConnection(path);
            //and create tables for our models
            //
            await _conn.CreateTableAsync<Employee>();
            await _conn.CreateTableAsync<MenuEntry>();
            await _conn.CreateTableAsync<Table>();
            await _conn.CreateTableAsync<Reservation>();
            //Create indexes for tables for faster lookups
            await _conn.CreateIndexAsync(nameof(Table), new[] { nameof(Table.TableNumber) }, unique: true);
            await _conn.CreateIndexAsync(nameof(Reservation),
                new[] { nameof(Reservation.TableId), nameof(Reservation.StartUtc), nameof(Reservation.EndUtc) }, unique: false);
             
            if (await _conn.Table<Table>().CountAsync() == 0)
            {
                await _conn.InsertAllAsync(new[]
                {
                    new Table { TableNumber =1, Capacity = 2},
                    new Table { TableNumber =2, Capacity = 12},
                    new Table { TableNumber =3, Capacity = 6},
                    new Table { TableNumber =4, Capacity = 4}
                });
            }
            //
            // seeding MenuItems and employees so the ui is not empty on start
            //
            //
            if (await _conn.Table<MenuEntry>().CountAsync() == 0)
            {
                await _conn.InsertAllAsync(menuEntries);
            }
            //If we do not have employees then seed the table with employees same procedure as the previous seeding of MenuItems
            if (await _conn.Table<Employee>().CountAsync() == 0)
            {
                await _conn.InsertAllAsync(EmployeesList);
            }
            //signal teh database initialized
            _initialized = true;
        }

        // MENU ITEM CRUD , EACH ITEM CHECKS IF DB IS INITIALIZED FIRST

        //get all menu items
        public static async Task<List<MenuEntry>> GetMenu()
        { await Initialize(); return await _conn!.Table<MenuEntry>().OrderBy
                (x => x.Name).ToListAsync();
        }
        //Insert or Update by id
        public static async Task<int> SaveMenuEntry(MenuEntry m)
        { await Initialize(); return m.Id == 0? 
                await _conn!.InsertAsync(m) : await _conn!.UpdateAsync(m);}
        
        //Delete a menu item by its ID
        public static async Task<int> DeleteMenuEntry(int id)
        {
            await Initialize();
            var m = await _conn!.FindAsync<MenuEntry>(id) ?? throw new InvalidOperationException("Menu Entry NOt found!");
            return await _conn!.DeleteAsync(m);
        }

        // TABLE CRUD. SAME PROCEDURE AS MENUENTRY CRUD

        //Get ALl tables
        public static async Task<List<Table>> GetAllTables()
        {
            await Initialize(); 
            return await _conn!.Table<Table>().OrderBy(t => t.TableNumber).ToListAsync();
        }

        static async Task<bool> HasReservationConflictAsync(int tableId, DateTime startUtc, DateTime endUtc, int? excludeId = null)
        {
            var q = _conn!.Table<Reservation>()
        .Where(r => r.TableId == tableId
                 && r.reservationStatus == ReservationStatus.Booked
                 && r.StartUtc < endUtc
                 && r.EndUtc > startUtc);

            if (excludeId.HasValue)
                q = q.Where(r => r.Id != excludeId.Value); 

            var hit = await q.FirstOrDefaultAsync();
            return hit is not null;
        }

        public static async Task<Reservation> CreateReservationAsync(
            int tableId, string name, DateTime startLocal, TimeSpan duration,
            int partySize = 2, string? phone = null, string? notes = null)
        {
            await Initialize();
            if (duration <= TimeSpan.Zero) throw new ArgumentException("Duration must be > 0");

            var startUtc = startLocal.ToUniversalTime();
            var endUtc = startUtc.Add(duration);

            var table = await _conn!.FindAsync<Table>(tableId) ?? throw new InvalidOperationException("Table not found.");
            if (partySize > table.Capacity) throw new InvalidOperationException($"Party exceeds capacity ({table.Capacity}).");
            if (await HasReservationConflictAsync(tableId, startUtc, endUtc)) throw new InvalidOperationException("Time slot already booked.");

            var res = new Reservation
            {
                TableId = tableId,
                ReservationName = name,
                ReservationSize = partySize,
                StartUtc = startUtc,
                EndUtc = endUtc,
                reservationStatus = ReservationStatus.Booked,
            };
            await _conn.InsertAsync(res);
            return res;
        }

        public static async Task<int> UpdateReservationAsync(Reservation r, bool checkConflicts = true)
        {
            await Initialize();
            if (r.EndUtc <= r.StartUtc) throw new ArgumentException("End must be after start.");
            if (checkConflicts && await HasReservationConflictAsync(r.TableId, r.StartUtc, r.EndUtc, r.Id))
                throw new InvalidOperationException("Time slot already booked.");
            return await _conn!.UpdateAsync(r);
        }

        public static async Task<int> CancelReservationAsync(int id)
        {
            await Initialize();
            var r = await _conn!.FindAsync<Reservation>(id) ?? throw new InvalidOperationException("Reservation not found.");
            r.reservationStatus = ReservationStatus.Canceled;
            return await _conn.UpdateAsync(r);
        }

        public static async Task<List<Reservation>> GetReservationsForDayAsync(DateTime dayLocal)
        {
            await Initialize();
            var start = new DateTime(dayLocal.Year, dayLocal.Month, dayLocal.Day, 0, 0, 0, DateTimeKind.Local).ToUniversalTime();
            var end = start.AddDays(1);
            return await _conn!.Table<Reservation>()
                .Where(r => r.StartUtc < end && r.EndUtc > start)
                .OrderBy(r => r.StartUtc)
                .ToListAsync();
        }

        public static async Task<List<Table>> GetAvailableTablesAsync(DateTime startLocal, TimeSpan duration, int minCapacity = 1)
        {
            await Initialize();
            var startUtc = startLocal.ToUniversalTime();
            var endUtc = startUtc.Add(duration);

            var tables = await _conn!.Table<Table>().Where(t => t.Capacity >= minCapacity).ToListAsync();
            var conflicts = await _conn.Table<Reservation>()
                .Where(r => r.reservationStatus == ReservationStatus.Booked
                         && r.StartUtc < endUtc && r.EndUtc > startUtc)
                .ToListAsync();
            var busy = conflicts.Select(r => r.TableId).ToHashSet();
            return tables.Where(t => !busy.Contains(t.Id)).OrderBy(t => t.TableNumber).ToList();
        }

        // ---------- Employees  ----------
        public static async Task<List<Employee>> GetEmployeesAsync()
        { await Initialize(); return await _conn!.Table<Employee>().OrderBy(e => e.Name).ToListAsync(); }

        public static async Task<int> SaveEmployeeAsync(Employee e)
        { await Initialize(); return e.Id == 0 ? await _conn!.InsertAsync(e) : await _conn!.UpdateAsync(e); }

        public static async Task<int> DeleteEmployee(Employee e)
        {
            await Initialize(); return e.Id == 0 ? throw new InvalidOperationException("No employee found!") : await _conn!.DeleteAsync(e);
        }
    }
}
