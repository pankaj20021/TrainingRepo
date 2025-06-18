namespace SchoolProject.DTOs.UserDtos
{
    public class UserRegisterDTos
    {    
        public String? FullName { get; set; }
        public String? Email { get; set; }
        public String Password { get; set; }
        public Role Role { get; set; }
    }

    public enum Role
    {
        None,
        Admin,
        User,
        Hr
    }
}
