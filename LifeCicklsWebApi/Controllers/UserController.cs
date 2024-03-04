﻿using LifeCicklsService.Services;
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
            || string.IsNullOrEmpty(connectionRequest.ToUserName))
        {
            return BadRequest("Invalid connection request!");
        }

        // Handle invalid user names
        try
        {
            var userProfile = _userService.Connect(connectionRequest);
            if (userProfile == null)
            {
                return BadRequest("Invalid connection request!");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.ToString());
        }

        return Ok($"Connection request to user {connectionRequest.FromUserName} sent successfully!");
    }
}