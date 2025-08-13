using Restaurant_Manager.Model.Enums;
using SQLite;
namespace Restaurant_Manager.Model
{
    // This models the menu items offered by the restaurant
    
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
