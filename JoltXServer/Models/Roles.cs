namespace JoltXServer.Models

{
    public static class Roles
    {
        public static List<string> GetValidRoles()
        {
            return new List<string>() { "Admin", "User" };
        }  
    }
}