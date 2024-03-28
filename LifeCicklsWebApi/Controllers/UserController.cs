using LifeCicklsService.Services;
using LifeCklsModels;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;


[ApiController]
[Route("v1")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    public UserController(IMongoClient mongoClient)
    {
        _userService = new UserService(mongoClient);
    }

    //[LifeCklsServiceAuthorize]
    // GET v1/user/{username}
    [HttpGet("user/{username}")]
    public IActionResult GetUserByUserName(string username)
    {
        if (string.IsNullOrEmpty(username))
        {
            return BadRequest("User name cannot be empty");
        }

        UserProfile? user = _userService.FindByUserName(username);
        if (user == null)
        {
            return NotFound(); // Return 404 if the user is not found
        }

        return Ok(user);
    }

    // PUT v1/user/register
    [HttpPut("user/register")]
    public IActionResult RegisterUser([FromBody] UserRegistrationRequest registrationRequest)
    {
        if (registrationRequest == null)
        {
            return BadRequest("Invalid request data");
        }

        // If user by name already exist, handle accordingly
        var userProfile = _userService.FindByUserName(registrationRequest.UserName);
        if (userProfile != null)
        {
            return BadRequest($"User Name {registrationRequest.UserName} is taken");
        }

        var registeredUser = _userService.Register(registrationRequest);

        return Ok(registeredUser);
    }

    // POST v1/user/login
    [HttpPost("user/login")]
    public IActionResult Login([FromBody] UserLoginInfo userLoginInfo)
    {
        if (userLoginInfo == null
            || string.IsNullOrEmpty(userLoginInfo.UserName)
            || string.IsNullOrEmpty(userLoginInfo.Password))
        {
            return BadRequest("User and password is required to login!");
        }

        // Handle invalid user name or password
        var userProfile = _userService.Login(userLoginInfo.UserName, userLoginInfo.Password);
        if (userProfile == null)
        {
            return BadRequest("Invalid user name and/or password");
        }

        return Ok(userProfile);
    }

    // POST v1/user/connect
    [HttpPost("user/connect")]
    public IActionResult Connect([FromBody] ConnectionRequest connectionRequest)
    {
        if (connectionRequest == null
            || string.IsNullOrEmpty(connectionRequest.FromUserName)
            || string.IsNullOrEmpty(connectionRequest.ToUserName)
            || string.Equals(connectionRequest.FromUserName, connectionRequest.ToUserName, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Invalid connection request!");
        }

        // If connection request to the user is already pending
        if (_userService.IsConnectionPending(connectionRequest))
        {
            return StatusCode(409, "Connection request is pending for the user");
        }

        // If users are already connected to each other
        if (_userService.IsConnected(connectionRequest))
        {
            return StatusCode(409, "User is already connected");
        }

        try
        {
            var requestResult = _userService.Connect(connectionRequest);
            if (requestResult == null)
            {
                return BadRequest("Invalid connection request!");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.ToString());
        }

        return Ok($"Connection request to user {connectionRequest.ToUserName} sent successfully!");
    }

    // POST v1/user/connectionrequestresult
    [HttpPost("user/connectionrequestresult")]
    public IActionResult ConnectionRequestResult([FromBody] ConnectionRequestResult connectionOutcome)
    {
        if (connectionOutcome == null
            || string.IsNullOrEmpty(connectionOutcome.RequestId))
        {
            return BadRequest(new { message = "Invalid connection request!" });
        }

        try
        {
            string result = _userService.UpdateConnectionOutcome(connectionOutcome);
           return Ok(new { message = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // GET v1/user/{username}/connectionrequests
    [HttpGet("user/{username}/connectionrequests")]
    public IActionResult GetConnectionRequests(string username)
    {
        if (string.IsNullOrEmpty(username))
        {
            return BadRequest("Invalid request.");
        }

        try
        {
            var requestResult = _userService.GetConnectionRequests(username);
            if (requestResult == null || !requestResult.Any())
            {
                return NotFound("No connection requests found for the specified user.");
            }

            return Ok(requestResult);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    // GET v1/user/{username}/connections
    [HttpGet("user/{username}/connections")]
    public IActionResult GetConnections(string username)
    {
        if (string.IsNullOrEmpty(username))
        {
            return BadRequest(new { message = "Invalid request." });
        }

        try
        {
            var connections = _userService.GetConnections(username);
            if (connections == null || !connections.Any())
            {
                return NotFound(new { message = "No connection requests found for the specified user." });
            }

            return Ok(connections);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new {message = $"An error occurred: {ex.Message}" });
        }
    }
}