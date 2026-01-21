namespace PSFR_Repository.Models
{
    public class UserApplicationRoleModel
    {
        public short RoleId { get; set; }

        public string Role { get; set; } = string.Empty;

        public string ClientId { get; set; } = string.Empty;
    }
}
