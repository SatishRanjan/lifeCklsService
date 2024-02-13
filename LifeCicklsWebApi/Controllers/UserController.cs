using LifeCicklsService.Services;
using LifeCicklsWebApi.Security;
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

    //[LifeCklsServiceAuthorize]
    // GET v1/user/{id}
    [HttpGet("user/{id}")]
    public IActionResult GetUserById(int id)
    {
        // TODO
        UserProfile user = null;//GetUserFromDatabase(id);

        if (user == null)
        {
            return NotFound(); // Return 404 if the user is not found
        }

        return Ok(user); // Return user data if found
    }

    // PUT v1/user/register
    [HttpPut("user/register")]
    public IActionResult RegisterUser([FromBody] UserRegistrationRequest registrationRequest)
    {
        if (registrationRequest == null)
        {
            return BadRequest("Invalid request data");
        }

        var registeredUser = _userService.Register(registrationRequest);

        return Ok(registeredUser);
    }   
}