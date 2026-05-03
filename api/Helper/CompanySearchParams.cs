namespace api.Helper;

public class CompanySearchParams : PagingParams
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public string OrderBy { get; set; } = "created";
    public string? Status { get; set; }
}
