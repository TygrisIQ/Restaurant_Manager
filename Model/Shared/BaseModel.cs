using SQLite;

namespace Restaurant_Manager.Model.Shared
{
    public abstract class BaseModel
    {
        //AN ABSTRACT CLASS THAT DEFINES AN Id PARAMETER THAT ALL MODEL CLASSES INHERIT
        [SQLite.PrimaryKey, SQLite.AutoIncrement]
        public int Id { get; set; }
    }
}
