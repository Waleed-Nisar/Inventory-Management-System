namespace IMS.Domain.Enums
{
    /// <summary>
    /// System roles for authorization
    /// </summary>
    public static class UserRole
    {
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string Staff = "Staff";
        public const string Viewer = "Viewer";

        /// <summary>
        /// Gets all available roles
        /// </summary>
        public static string[] GetAllRoles() => new[] { Admin, Manager, Staff, Viewer };
    }
}
