namespace Restaurant_Manager.Data
{ 
    //Error codes "types" returned by the Result sealed class
    public enum ErrorCode { None, NotFound, ConflictError, InvalidRequest, DbError }

    //the result class is a helper class to help deal/identify exceptions 
    //This sealed class takes a generic, (i didnt see any mistake in giving the generic type restrictions)
    //The contructor takes a nullable generic value to allow for nullable values
    //Ok => bool value => true if the operation is a success false if not
    //Value? => the returned result if successful
    //Code => type of error that occured from the ErrorCode enum above
    //Message => message returned if the operation failed
    public sealed class Result<T>
    {
        public bool Ok { get; }
        public T? Value { get; }
        public ErrorCode Code { get; }
        public string? Message { get; }

        //the constructor is private so construction happens only if by the member methods Success() and Fail()
        private Result(bool ok, T? value, ErrorCode code, string? message)
        {
            Ok = ok; Value = value; Code = code; Message = message;
        }


        public static Result<T> Success(T value) => new(true, value, ErrorCode.None, null);
        public static Result<T> Fail(ErrorCode code, string message) => new(false, default, code, message);

        public override string ToString() =>
            Ok ? $"Success({Value})" : $"Fail({Code}: {Message})";
    }
}
