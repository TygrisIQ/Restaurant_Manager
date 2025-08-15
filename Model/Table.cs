using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Restaurant_Manager.Model.Shared;
using SQLite;
namespace Restaurant_Manager.Model 
{
    public class Table : BaseModel
    {


        public short TableNumber { get; set; }

        public int Capacity { get; set; } = 4;
    }
}
