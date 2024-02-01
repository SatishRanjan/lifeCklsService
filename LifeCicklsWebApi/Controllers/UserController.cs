using LifeCicklsService.Services;
using LifeCklsModels;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("v1")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    public UserController() 
    {
        _userService = new UserService();
    }

    // GET v1/user/{id}
    [HttpGet("user/{id}")]
    public IActionResult GetUserById(int id)
    {
        // TODO
        var user = GetUserFromDatabase(id);

        if (user == null)
        {
            return NotFound(); // Return 404 if the user is not found
        }

        return Ok(user); // Return user data if found
    }

    // PUT v1/user/register
    [HttpPut("user/register")]
    public IActionResult RegisterUser([FromBody] User registrationRequest)
    {
        if (registrationRequest == null)
        {
            return BadRequest("Invalid request data");
        }

        var registeredUser = _userService.Register(registrationRequest);

        return Ok(registeredUser);
    }

    private User GetUserFromDatabase(int id)
    {
        return new User
        {
            LifeCklId = id.ToString(),
            FirstName = "John",
            LastName = "Doe"
        };
    }
}