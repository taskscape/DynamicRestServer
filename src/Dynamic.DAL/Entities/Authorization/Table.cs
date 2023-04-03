namespace Dynamic.DAL.Entities.Authorization
{
    public class Table
    {
        public string Name { get; }
        public string EntityName { get; set; }

        public Table(string name)
        {
            Name = name;
            EntityName = name.Replace("_", "").ToLower();
        }
    }
}
