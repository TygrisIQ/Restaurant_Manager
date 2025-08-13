using Restaurant_Manager.Model;

namespace Restaurant_Manager.Data
{
    public enum ErrorCode { None, NotFound, Conflict, Invalid, Db}
    

    public sealed class Result<T>
    {
        public bool Ok { get; }
        public T? Value { get; }
        public ErrorCode Code { get; }
        public string? Message { get; }

        private Result(bool ok, T? value, ErrorCode code, string? message)
        {
            Ok = ok; Value = value; Code = code; Message = message;
        }

        public static Result<T> Success(T value) => new(true, value, ErrorCode.None, null);
        public static Result<T> Fail(ErrorCode code, string message) => new(false, default, code, message);
        
        public override string ToString() =>
            Ok ? $"Success({Value})" : $"Fail({Code}: {Message})";
    }
    //This class was made to make the job for the ui teammates easier
    //the idea is for them to call oneliners and get the result
    //which saves them from reading the
    //long Db class and avoiding tinkering with it
    public static class DatabaseApi
    {

        //Ensure Database is initialized, return true if yes false if not initialized
        //Makes sure the database is initialized (started)
        //call this before making any other calls to the database
        public static async Task<Result<bool>> EnsureInitialized()
        {
            try { await Db.Initialize(); return Result<bool>.Success(true); }
            catch (Exception e) { return Result<bool>.Fail(ErrorCode.Db, e.Message); }
        }

        // --- MenuEntry (Menu food item)

        //Call this to get all food items from the menu
        //it will return a list of all food items available in the restaurant
        public static async Task<Result<List<MenuEntry>>> GetMenu()
        {
            try { return Result<List<MenuEntry>>.Success(await Db.GetMenu()); }
            catch (Exception e) { return Result<List<MenuEntry>>.Fail(ErrorCode.Db, e.Message); }
        }
        // call this with a MenuEntry Object to save it as a new menu item
        public static async Task<Result<MenuEntry>> SaveMenuEntry(MenuEntry m)
        {
            try
            {
                await Db.SaveMenuEntry(m);
                return Result<MenuEntry>.Success(m);
            }
            catch (ArgumentException ex) { return Result<MenuEntry>.Fail(ErrorCode.Invalid, ex.Message); }
            catch (InvalidOperationException ex) when (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            { return Result<MenuEntry>.Fail(ErrorCode.NotFound, ex.Message); }
            catch (Exception ex) { return Result<MenuEntry>.Fail(ErrorCode.Db, ex.Message); }
        }
        //call this to delete a food from the menu
        //if you dont use it its fine, but its here just in case
        public static async Task<Result<int>> DeleteMenuEntry(int id)
        {
            try { var n = await Db.DeleteMenuEntry(id); return Result<int>.Success(n); }
            catch (InvalidOperationException ex) { return Result<int>.Fail(ErrorCode.NotFound, ex.Message); }
            catch (Exception ex) { return Result<int>.Fail(ErrorCode.Db, ex.Message); }
        }
        // ---------- Tables ---------- Add,Delete, Update,Read

        //Get all available tables 
        //returns a list of all available tables......
        //YOUSSIF
        public static async Task<Result<List<Table>>> GetAllTables()
        {
            try { return Result<List<Table>>.Success(await Db.GetAllTables()); }
            catch (Exception ex) { return Result<List<Table>>.Fail(ErrorCode.Db, ex.Message); }
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
            catch (ArgumentException ex) { return Result<List<Table>>.Fail(ErrorCode.Invalid, ex.Message); }
            catch (Exception ex) { return Result<List<Table>>.Fail(ErrorCode.Db, ex.Message); }
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
            catch (ArgumentException ex) { return Result<Reservation>.Fail(ErrorCode.Invalid, ex.Message); }
            //incase a table was not found or a reservation being already booked
            //these three catches will be returned, the reservation is checked for these errors in the CreateReservationAsync method in Data/Db.cs
            catch (InvalidOperationException ex) when (ex.Message.Contains("Table not found", StringComparison.OrdinalIgnoreCase))
            { return Result<Reservation>.Fail(ErrorCode.NotFound, ex.Message); }
            catch (InvalidOperationException ex) when (ex.Message.Contains("exceeds capacity", StringComparison.OrdinalIgnoreCase))
            { return Result<Reservation>.Fail(ErrorCode.Invalid, ex.Message); }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already booked", StringComparison.OrdinalIgnoreCase))
            { return Result<Reservation>.Fail(ErrorCode.Conflict, ex.Message); }
            //in case of an exception that does not match any of the previous cases a generic exception is returned with a message
            catch (Exception ex) { return Result<Reservation>.Fail(ErrorCode.Db, ex.Message); }
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
            catch (ArgumentException ex) { return Result<Reservation>.Fail(ErrorCode.Invalid, ex.Message); }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already booked", StringComparison.OrdinalIgnoreCase))
            { return Result<Reservation>.Fail(ErrorCode.Conflict, ex.Message); }
            catch (InvalidOperationException ex) when (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            { return Result<Reservation>.Fail(ErrorCode.NotFound, ex.Message); }
            catch (Exception ex) { return Result<Reservation>.Fail(ErrorCode.Db, ex.Message); }
        }
        //CALL THIS TO CANCEL "DELETE" A RESERVATION
        public static async Task<Result<int>> CancelReservation(int id)
        {
            try { var n = await Db.CancelReservationAsync(id); return Result<int>.Success(n); }
            catch (InvalidOperationException ex) { return Result<int>.Fail(ErrorCode.NotFound, ex.Message); }
            catch (Exception ex) { return Result<int>.Fail(ErrorCode.Db, ex.Message); }
        }
        //get all reservations for a given day
        public static async Task<Result<List<Reservation>>> GetReservationsForDay(DateTime dayLocal)
        {
            try { return Result<List<Reservation>>.Success(await Db.GetReservationsForDayAsync(dayLocal)); }
            catch (Exception ex) { return Result<List<Reservation>>.Fail(ErrorCode.Db, ex.Message); }
        }

        // ---------- Employees ----------

        //get all employees
        //returns a list of all employees of the restaurant
        public static async Task<Result<List<Employee>>> GetEmployees()
        {
            try { return Result<List<Employee>>.Success(await Db.GetEmployeesAsync()); }
            catch (Exception ex) { return Result<List<Employee>>.Fail(ErrorCode.Db, ex.Message); }
        }
        //Create a new employee
        //this method excepts to receive an employee object to save it to database
        //in case of an error,
        //returns an ArgumentException if caught by Db.SaveEmployeeAsync otherwise throws a generic exception
        public static async Task<Result<Employee>> SaveEmployee(Employee e)
        {
            try { await Db.SaveEmployeeAsync(e); return Result<Employee>.Success(e); }
            catch (ArgumentException ex) { return Result<Employee>.Fail(ErrorCode.Invalid, ex.Message); }
            catch (Exception ex) { return Result<Employee>.Fail(ErrorCode.Db, ex.Message); }
        }
    }
}

