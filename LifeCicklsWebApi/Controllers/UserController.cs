﻿using LifeCicklsService.Services;
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
    // GET v1/user/{username}
    [HttpGet("user/{username}")]
    public IActionResult GetUserByUserName(string username)
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
            return BadRequest("Invalid user name or password");
        }

        return Ok(userProfile);
    }
}