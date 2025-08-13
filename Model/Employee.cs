using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
namespace Restaurant_Manager.Model
{
    public class Employee
    {
        [PrimaryKey, AutoIncrement] public int Id { get; set; }
        [NotNull] public string Name { get; set; } = "";
        [NotNull] public int EmployeeId { get; set; } = 0;
        public int Age { get; set; }
        public string Address { get; set; } = "";
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
       
    }
}
