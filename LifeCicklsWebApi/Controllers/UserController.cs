using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("v1")]
public class UserController : ControllerBase
{
    // Existing code...

    // GET v1/user/{id}
    [HttpGet("user/{id}")]
    public IActionResult GetUserById(int id)
    {
        // Retrieve user information based on the provided id
        // Replace this with your actual logic to fetch user data
        var user = GetUserFromDatabase(id);

        if (user == null)
        {
            return NotFound(); // Return 404 if the user is not found
        }

        return Ok(user); // Return user data if found
    }

    // PUT v1/user/register
    [HttpPut("user/register")]
    public IActionResult RegisterUser([FromBody] UserRegistrationRequest userRegistrationRequest)
    {
        // Replace this with your actual logic to handle user registration
        // The userRegistrationRequest object will contain the data sent in the request body
        // You might want to validate the data and then save it to the database
        // For demonstration purposes, a simple acknowledgment is returned here.

        if (userRegistrationRequest == null)
        {
            return BadRequest("Invalid request data");
        }

        // Process user registration logic here...

        return Ok($"User registered: {userRegistrationRequest.FirstName} {userRegistrationRequest.LastName}");
    }

    // Other actions...

    private User GetUserFromDatabase(int id)
    {
        // Replace this with your actual logic to fetch user data from the database
        // For demonstration purposes, a simple User class is used here.
        return new User
        {
            Id = id,
            FirstName = "John",
            LastName = "Doe",
            // ... other user properties
        };
    }
}

public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

public class UserRegistrationRequest
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    // ... other registration properties
}
