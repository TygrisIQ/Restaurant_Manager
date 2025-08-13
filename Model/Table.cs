using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
namespace Restaurant_Manager.Model
{
    public class Table
    {

        [PrimaryKey, AutoIncrement] public int Id { get; set; }

        public short TableNumber { get; set; }

        public int Capacity { get; set; } = 4;
    }
}
