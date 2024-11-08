namespace CustomerOrderApi.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
    }

    public class RegisterUserDto
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        
    }
    public class LoginUserDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
    public class CheckUserDto
    {
        public string Email { get; set; }
    }
}

