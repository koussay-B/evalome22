namespace api.Helper;

public class QueryParams : PagingParams
{
    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public bool SortDesc { get; set; } = false;
}
