namespace OfficeDeskReservation.API.Dtos
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public List<ReservationResponseDto> Reservations { get; set; } = new();
    }
}
