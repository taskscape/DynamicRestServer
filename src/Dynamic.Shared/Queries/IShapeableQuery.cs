namespace Dynamic.Shared.Queries
{
    public interface IShapeableQuery
    {
        string Include { get; set; }
        string Select { get; set; }
    }
}
