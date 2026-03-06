namespace OfficeDeskReservation.API.Pagination
{
    public class UserQueryParameters : QueryParameters
    {
        public string? FirstNameTerm { get; set; }
        public string? LastNameTerm { get; set; }
        public string? EmailTerm { get; set; }
    }
}
