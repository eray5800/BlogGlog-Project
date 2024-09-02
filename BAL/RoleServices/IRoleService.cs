namespace BAL.RoleServices
{
    public interface IRoleService
    {
        Task SeedRolesAsync();

        Task AssignRoleAsync(string userId, string roleName);

        Task RemoveRoleAsync(string userId, string roleName);

        Task ChangeUserRoleAsync(string userId, string newRoleName);

        Task<bool> CheckUserRole(string userId, string role);
    }
}
