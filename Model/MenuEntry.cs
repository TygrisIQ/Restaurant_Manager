using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Restaurant_Manager.Model.Enums;
using SQLite;
namespace Restaurant_Manager.Model
{
    public class MenuEntry
    {
        [PrimaryKey, AutoIncrement] public int Id { get; set; }

        [NotNull] public string Name { get; set; } = "";
        public string? Description { get; set; }

        public bool IsAvailable {  get; set; } = true;

        public MenuEntryType Type { get; set; }

        public decimal Price { get; set; }

    }
}
