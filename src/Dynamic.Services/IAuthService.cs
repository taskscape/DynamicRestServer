namespace Dynamic.Services
{
    public interface IAuthService
    {
        bool IsAuthorized(string tableName, string roleName, string httpMethod);
    }
}
