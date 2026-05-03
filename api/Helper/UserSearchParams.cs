namespace api.Helper
{
    public class UserSearchParams : PagingParams
    {
        public string? Search { get; set; }
        public string? Role { get; set; }
        public bool IncludeCompany { get; set; } = false;
        public string OrderBy { get; set; } = "username";
    }
}
