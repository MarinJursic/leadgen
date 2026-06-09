namespace leadgen.Models.Identity;

public static class LeadgenRoles
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Editors = Admin + "," + Manager;
}
