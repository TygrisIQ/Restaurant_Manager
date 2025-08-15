using Restaurant_Manager.Model;
namespace Restaurant_Manager.Data
{
    //This class was made to make the job for the ui teammates easier
    //the idea is for them to call oneliners and get the result
    //which saves them from reading the
    //long Db class and avoiding tinkering with it
    public static class DatabaseApi
    {
        //Makes sure the database is initialized (started)
        //call this before making any other calls to the database
        public static async Task<Result<bool>> EnsureInitialized()
        {
            try { await Db.Initialize(); return Result<bool>.Success(true); }
            catch (Exception e) { return Result<bool>.Fail(ErrorCode.DbError, e.Message); }
        }

        // --- MenuEntry (Menu food item)

        //Call this to get all food items from the menu
        //it will return a list of all food items available in the restaurant
        public static async Task<Result<List<MenuEntry>>> GetMenu()
        {
            try { return Result<List<MenuEntry>>.Success(await Db.GetMenu()); }
            catch (Exception e) { return Result<List<MenuEntry>>.Fail(ErrorCode.DbError, e.Message); }
        }
        //return a single menu item by its id if found, exception is thrown if not found
        public static async Task<Result<MenuEntry>> GetMenuItemById(int id)
        {
            try
            {
                var i = await Db.GetMenuItemById(id);
                return i is null ?
                    Result<MenuEntry>.Fail(ErrorCode.NotFound, "Not Found!") : Result<MenuEntry>.Success(i);
            }
            catch (Exception e)
            {
                return Result<MenuEntry>.Fail(ErrorCode.DbError, e.Message);
            }
        }
        // call this with a MenuEntry Object to save it as a new menu item
        public static async Task<Result<MenuEntry>> SaveMenuEntry(MenuEntry m)
        {
            try
            {
                await Db.SaveMenuEntry(m);
                return Result<MenuEntry>.Success(m);
            }
            catch (ArgumentException ex) { return Result<MenuEntry>.Fail(ErrorCode.InvalidRequest, ex.Message); }
            catch (InvalidOperationException ex) when (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            { return Result<MenuEntry>.Fail(ErrorCode.NotFound, ex.Message); }
            catch (Exception ex) { return Result<MenuEntry>.Fail(ErrorCode.DbError, ex.Message); }
        }
        //call this to delete a food from the menu
        //if you dont use it its fine, but its here just in case
        public static async Task<Result<int>> DeleteMenuEntry(int id)
        {
            try { var n = await Db.DeleteMenuEntry(id); return Result<int>.Success(n); }
            catch (InvalidOperationException ex) { return Result<int>.Fail(ErrorCode.NotFound, ex.Message); }
            catch (Exception ex) { return Result<int>.Fail(ErrorCode.DbError, ex.Message); }
        }
        // ---------- Tables ---------- Add,Delete, Update,Read

        //Get all available tables 
        //returns a list of all available tables......
        //YOUSSIF
        public static async Task<Result<List<Table>>> GetAllTables()
        {
            try { return Result<List<Table>>.Success(await Db.GetAllTables()); }
            catch (Exception ex) { return Result<List<Table>>.Fail(ErrorCode.DbError, ex.Message); }
        }
        //get all available tables for booking
        //give it a start time and a duration and a table capacity and it will look if any tables are avialable
        //if an error occurs it will return an exception with a message,
        //you can handle the returned exception however you want, i recommend displaying a message in MAUI to the user
        //YOUSSIF
        public static async Task<Result<List<Table>>> GetAvailableTables(DateTime startLocal, TimeSpan duration, int minCapacity = 1)
        {
            try
            {
                var list = await Db.GetAvailableTablesAsync(startLocal, duration, minCapacity);
                return Result<List<Table>>.Success(list);
            }
            catch (ArgumentException ex) { return Result<List<Table>>.Fail(ErrorCode.InvalidRequest, ex.Message); }
            catch (Exception ex) { return Result<List<Table>>.Fail(ErrorCode.DbError, ex.Message); }
        }

        // ---------- Reservations ----------

        //CALL THIS TO CREATE A RESERVATION
        //it will return an InvalidOPerationException if a conflict occurs or no suitable table is found
        //it will return an ArgumentException if the arguments are invalid for a reservation
        //and will return a generic exception with a message incase of any other error
        public static async Task<Result<Reservation>> CreateReservation(
            int tableId, string name, DateTime startLocal, TimeSpan duration,
            int partySize = 2, string? phone = null, string? notes = null)
        {
            //try to create a reservation with given arguments
            try
            {
                var r = await Db.CreateReservationAsync(tableId, name, startLocal, duration, partySize, phone, notes);
                return Result<Reservation>.Success(r);
            }
            //incase of invalidarguments
            catch (ArgumentException ex) { return Result<Reservation>.Fail(ErrorCode.InvalidRequest, ex.Message); }
            //incase a table was not found or a reservation being already booked
            //these three catches will be returned, the reservation is checked for these errors in the CreateReservationAsync method in Data/Db.cs
            catch (InvalidOperationException ex) when (ex.Message.Contains("Table not found", StringComparison.OrdinalIgnoreCase))
            { return Result<Reservation>.Fail(ErrorCode.NotFound, ex.Message); }
            catch (InvalidOperationException ex) when (ex.Message.Contains("exceeds capacity", StringComparison.OrdinalIgnoreCase))
            { return Result<Reservation>.Fail(ErrorCode.InvalidRequest, ex.Message); }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already booked", StringComparison.OrdinalIgnoreCase))
            { return Result<Reservation>.Fail(ErrorCode.ConflictError, ex.Message); }
            //in case of an exception that does not match any of the previous cases a generic exception is returned with a message
            catch (Exception ex) { return Result<Reservation>.Fail(ErrorCode.DbError, ex.Message); }
        }

        //CALL THIS TO UPDATE "CHANGE" A RESERVATION
        //SAME procedure as the CreateReservation
        //same procedure with the exceptions
        public static async Task<Result<Reservation>> UpdateReservation(Reservation r, bool checkConflicts = true)
        {
            try
            {
                await Db.UpdateReservationAsync(r, checkConflicts);
                return Result<Reservation>.Success(r);
            }
            catch (ArgumentException ex) { return Result<Reservation>.Fail(ErrorCode.InvalidRequest, ex.Message); }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already booked", StringComparison.OrdinalIgnoreCase))
            { return Result<Reservation>.Fail(ErrorCode.ConflictError, ex.Message); }
            catch (InvalidOperationException ex) when (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            { return Result<Reservation>.Fail(ErrorCode.NotFound, ex.Message); }
            catch (Exception ex) { return Result<Reservation>.Fail(ErrorCode.DbError, ex.Message); }
        }
        //CALL THIS TO CANCEL "DELETE" A RESERVATION
        public static async Task<Result<int>> CancelReservation(int id)
        {
            try { var n = await Db.CancelReservationAsync(id); return Result<int>.Success(n); }
            catch (InvalidOperationException ex) { return Result<int>.Fail(ErrorCode.NotFound, ex.Message); }
            catch (Exception ex) { return Result<int>.Fail(ErrorCode.DbError, ex.Message); }
        }
        //get all reservations for a given day
        public static async Task<Result<List<Reservation>>> GetReservationsForDay(DateTime dayLocal)
        {
            try { return Result<List<Reservation>>.Success(await Db.GetReservationsForDayAsync(dayLocal)); }
            catch (Exception ex) { return Result<List<Reservation>>.Fail(ErrorCode.DbError, ex.Message); }
        }

        // ---------- Employees ----------

        //get all employees
        //returns a list of all employees of the restaurant
        public static async Task<Result<List<Employee>>> GetEmployees()
        {
            try { return Result<List<Employee>>.Success(await Db.GetEmployeesAsync()); }
            catch (Exception ex) { return Result<List<Employee>>.Fail(ErrorCode.DbError, ex.Message); }
        }
        
        //Create a new employee
        //this method excepts to receive an employee object to save it to database
        //in case of an error,
        //returns an ArgumentException if caught by Db.SaveEmployeeAsync otherwise throws a generic exception
        public static async Task<Result<Employee>> SaveEmployee(Employee e)
        {
            try { await Db.SaveEmployeeAsync(e); return Result<Employee>.Success(e); }
            catch (ArgumentException ex) { return Result<Employee>.Fail(ErrorCode.InvalidRequest, ex.Message); }
            catch (Exception ex) { return Result<Employee>.Fail(ErrorCode.DbError, ex.Message); }
        }
        //takes an employee object and attempts to delete it, returns an integer value of 1 if successful
        //returns a Result class instance with exception message if failed
        public static async Task<Result<int>> DeleteEmployee(Employee e)
        {
            try { var n = await Db.DeleteEmployee(e); return Result<int>.Success(n); }
            catch (InvalidOperationException ex) { return Result<int>.Fail(ErrorCode.InvalidRequest, ex.Message); }
            catch (Exception ex) { return Result<int>.Fail(ErrorCode.DbError, ex.Message); }
        }
    }
}

