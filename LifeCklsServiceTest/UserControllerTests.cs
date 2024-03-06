using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using LifeCicklsWebApi;
using LifeCklsModels;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace LifeCklsServiceTest
{
    [TestClass]
    public class UserControllerTests
    {
        private WebApplicationFactory<Program> _factory;

        [TestInitialize]
        public void Initialize()
        {
            // Create a test server using the Startup class of your application
            _factory = new WebApplicationFactory<Program>();
        }

        [TestMethod]
        public async Task UserNotFoundTest()
        {
            // Arrange
            var client = _factory.CreateClient();
            // Act
            var response = await client.GetAsync($"/v1/user/{Guid.NewGuid().ToString()}");
            Assert.IsTrue(response.StatusCode == HttpStatusCode.NotFound);
        }

        [TestMethod]
        public async Task FindUserByNameTest()
        {
            // Arrange
            var client = _factory.CreateClient();
            // Act
            var response = await client.GetAsync($"/v1/user/{Guid.NewGuid().ToString()}");
            Assert.IsTrue(response.StatusCode == HttpStatusCode.NotFound);

            var userName = Guid.NewGuid().ToString();
            var password = Guid.NewGuid().ToString();
            var registrationRequest = new UserRegistrationRequest
            {
                UserName = userName,
                Password = password,
                FirstName = "F1",
                LastName = "L1",
                Age = 5,
                Gender = "M",
                Country = "U",
                State = "W",
                City = "S",
                Email = "test@test.com",
                PhoneNumber = "5703200471"
            };

            var jsonPayload = JsonConvert.SerializeObject(registrationRequest);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            // Act
            response = await client.PutAsync("/v1/user/register", content);
            Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);

            response = await client.GetAsync($"/v1/user/{userName}");
            Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);

            string stringContent = await response.Content.ReadAsStringAsync();

            Assert.IsTrue(stringContent.Contains(userName));

            // User name casing test
            userName = "TestUser010_" + Guid.NewGuid().ToString();
            registrationRequest = new UserRegistrationRequest
            {
                UserName = userName,
                Password = password,
                FirstName = "F1",
                LastName = "L1",
                Age = 5,
                Gender = "M",
                Country = "U",
                State = "W",
                City = "S",
                Email = "test@test.com",
                PhoneNumber = "5703200471"
            };

            jsonPayload = JsonConvert.SerializeObject(registrationRequest);
            content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            // Act
            response = await client.PutAsync("/v1/user/register", content);
            Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);

            // get user
            response = await client.GetAsync($"/v1/user/" + userName.ToUpper());
            Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);

            stringContent = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(stringContent.Contains(userName.ToLower()));

            // get user
            response = await client.GetAsync($"/v1/user/" + userName.ToLower());
            Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);

            stringContent = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(stringContent.Contains(userName.ToLower()));
        }

        [TestMethod]
        public async Task UserLoginTest()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Invalid user name or password
            var loginRequest = new UserLoginInfo
            {
                UserName = Guid.NewGuid().ToString(),
                Password = Guid.NewGuid().ToString()
            };

            // Serialize the LoginRequest object to JSON
            var jsonPayload = JsonConvert.SerializeObject(loginRequest);

            // Create a StringContent object with the JSON payload
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            // Act
            var response = await client.PostAsync("/v1/user/login", content);

            // Optionally, you can read the response content
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(string.Equals(responseContent, "Invalid user name and/or password"));

            // Successful loging test
            var userName = Guid.NewGuid().ToString();
            var password = Guid.NewGuid().ToString();
            var registrationRequest = new UserRegistrationRequest
            {
                UserName = userName,
                Password = password,
                FirstName = "F1",
                LastName = "L1",
                Age = 5,
                Gender = "M",
                Country = "U",
                State = "W",
                City = "S",
                Email = "test@test.com",
                PhoneNumber = "5703200471"
            };

            jsonPayload = JsonConvert.SerializeObject(registrationRequest);
            content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            // Act
            response = await client.PutAsync("/v1/user/register", content);
            Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);

            // try to login

            loginRequest = new UserLoginInfo
            {
                UserName = userName,
                Password = password
            };

            // Serialize the LoginRequest object to JSON
            jsonPayload = JsonConvert.SerializeObject(loginRequest);

            // Create a StringContent object with the JSON payload
            content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            // Act
            response = await client.PostAsync("/v1/user/login", content);
            Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task ConnectionRequest_Invalid()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Invalid user name or password
            var connectionRequest = new ConnectionRequest
            {
                FromUserName = "",
                ToUserName = ""
            
            };

            // Serialize the request object to JSON
            var jsonPayload = JsonConvert.SerializeObject(connectionRequest);

            // Create a StringContent object with the JSON payload
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            // Act
            var response = await client.PostAsync("/v1/user/connect", content);
            Assert.IsTrue(response.StatusCode == HttpStatusCode.BadRequest);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(string.Equals(responseContent, "Invalid connection request!"));
        }

        [TestMethod]
        public async Task ConnectionRequest_Success()
        {
            // Arrange
            var client = _factory.CreateClient();

            // From user
            var fromUserName = Guid.NewGuid().ToString();
            var password = Guid.NewGuid().ToString();
            var registrationRequest = new UserRegistrationRequest
            {
                UserName = fromUserName,
                Password = password,
                FirstName = "fromUser",
                LastName = "L1",
                Age = 5,
                Gender = "M",
                Country = "U",
                State = "W",
                City = "S",
                Email = "test@test.com",
                PhoneNumber = "5703200471"
            };

            var registrationPayload = JsonConvert.SerializeObject(registrationRequest);
            var reqisrationContent = new StringContent(registrationPayload, Encoding.UTF8, "application/json");

            // Act
            var regresponse = await client.PutAsync("/v1/user/register", reqisrationContent);
            Assert.IsTrue(regresponse.StatusCode == HttpStatusCode.OK);

            // To user
            var toUserName = Guid.NewGuid().ToString();
            password = Guid.NewGuid().ToString();
            registrationRequest = new UserRegistrationRequest
            {
                UserName = toUserName,
                Password = password,
                FirstName = "toUser",
                LastName = "L1",
                Age = 5,
                Gender = "M",
                Country = "U",
                State = "W",
                City = "S",
                Email = "test@test.com",
                PhoneNumber = "5703200471"
            };

            registrationPayload = JsonConvert.SerializeObject(registrationRequest);
            reqisrationContent = new StringContent(registrationPayload, Encoding.UTF8, "application/json");

            // Act
            regresponse = await client.PutAsync("/v1/user/register", reqisrationContent);
            Assert.IsTrue(regresponse.StatusCode == HttpStatusCode.OK);

            // Invalid user name or password
            var connectionRequest = new ConnectionRequest
            {
                FromUserName = fromUserName,
                ToUserName = toUserName
            };

            // Serialize the request object to JSON
            var jsonPayload = JsonConvert.SerializeObject(connectionRequest);

            // Create a StringContent object with the JSON payload
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            // Act
            var response = await client.PostAsync("/v1/user/connect", content);
            Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(string.Equals(responseContent, $"Connection request to user {connectionRequest.FromUserName} sent successfully!"));

            response = await client.GetAsync($"/v1/user/{toUserName}/connectionrequests");
            Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);

            responseContent = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(!string.IsNullOrEmpty(responseContent));
        }
    }
}