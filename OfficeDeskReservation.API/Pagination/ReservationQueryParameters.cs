namespace OfficeDeskReservation.API.Pagination
{
    public class ReservationQueryParameters : QueryParameters
    {
        public int? UserId { get; set; }
        public int? DeskId { get; set; }
        public int? RoomId { get; set; }
    }
}
