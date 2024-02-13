using LifeCicklsService.Services;
using LifeCklsModels;

namespace ConsoleTestApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            UserService userService = new UserService();
            userService.Register(new UserRegistrationRequest
            {
                UserName = "u1",
                Password = "P1",
                FirstName = "F1",
                LastName = "L1",
                Age = 5,
                Gender = "M",
                Country = "U",
                State = "W",
                City = "S",
                Email = "test@test.com",
                PhoneNumber = "5703200471"
            });
        }
    }
}
