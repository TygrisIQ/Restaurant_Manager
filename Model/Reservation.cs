using Restaurant_Manager.Model.Enums;
using SQLite;
namespace Restaurant_Manager.Model
{
    public class Reservation
    {
        [PrimaryKey, AutoIncrement] public int Id { get; set; }

        [NotNull] public int TableId { get; set; }
        [NotNull] public string ReservationName { get; set; } = "";

        public int ReservationSize { get; set; } = 3;

        [NotNull] public DateTime StartUtc { get; set; }
        [NotNull] public DateTime EndUtc { get; set; }

        public ReservationStatus reservationStatus { get; set; } = ReservationStatus.Free;
    }
}
