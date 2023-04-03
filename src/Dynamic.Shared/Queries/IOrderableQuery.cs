namespace Dynamic.Shared.Queries
{
    public interface IOrderableQuery
    {
        string OrderBy { get; set; }
        string SortOrder { get; set; }
    }
}
