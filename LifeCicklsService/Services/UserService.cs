using LifeCklsModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;
using MongoDB.Bson;
using System.Collections;
using MongoDB.Bson.Serialization;
using System.Globalization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver.Core.Connections;

namespace LifeCicklsService.Services
{
    public class UserService : IUserService
    {
        //private string _connectionString = "mongodb://localhost:55000/";

        private readonly IMongoClient _mongoClient;
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<UserProfile> _profileCollection;
        private readonly IMongoCollection<LifeCkl> _lifeCklsCollection;
        private readonly IMongoCollection<ConnectionRequest> _connectionRequestsCollection;
        private readonly IMongoCollection<Story> _storiesCollection;

        public UserService(IMongoClient mongoClient)
        {
            _mongoClient = mongoClient;
            _database = _mongoClient.GetDatabase("lifecklsstore");
            _profileCollection = _database.GetCollection<UserProfile>("profiles");
            _lifeCklsCollection = _database.GetCollection<LifeCkl>("lifeCkls");
            _connectionRequestsCollection = _database.GetCollection<ConnectionRequest>("ConnectionRequests");
            _storiesCollection = _database.GetCollection<Story>("stories");
        }

        public UserProfile Register(UserRegistrationRequest userRegistrationRequest)
        {
            UserProfile? savedProfile;
            try
            {
                savedProfile = SaveUserProfile(userRegistrationRequest);
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to save UserProfile, error: {e.Message}");
            }

            try
            {
                SaveLifeCkl(savedProfile);
            }
            catch (Exception e)
            {
                // Delete UserProfile is LifeCkl save fails
                DeleteUserProfile(savedProfile.ProfileId);
                throw new Exception($"Failed to save LifeCkls, error: {e.Message}");
            }

            return savedProfile;
        }

        public List<ConnectionRequest> GetConnectionRequests(string userName)
        {
            List<ConnectionRequest> connectionRequests = new();
            UserProfile? userProfile = FindByUserName(userName);
            if (userProfile == null
                || userProfile.IncomingConnectionRequests?.Any() == false)
            {
                return connectionRequests;
            }

            for (int i = 0; i < userProfile.IncomingConnectionRequests?.Count; i++)
            {
                var filter = Builders<ConnectionRequest>.Filter.Eq("ToUserName", userName) &
                    Builders<ConnectionRequest>.Filter.Eq("RequestStaus", "Pending");
                var connectionRequest = _connectionRequestsCollection.Find(filter).FirstOrDefault();
                if (connectionRequest == null)
                {
                    continue;
                }
                connectionRequest.FromUserFullName = userProfile.FirstName + " " + userProfile.LastName;
                connectionRequests.Add(connectionRequest);
            }

            return connectionRequests;
        }

        public List<Connection> GetConnections(string userName)
        {
            List<Connection> connections = new();
            UserProfile? userProfile = FindByUserName(userName);
            if (userProfile == null
                || userProfile.Connections == null
                || userProfile.Connections?.Any() == false)
            {
                return connections;
            }

            foreach (var connectionId in userProfile.Connections)
            {
                var filter = Builders<UserProfile>.Filter.Eq("ProfileId", connectionId);
                var connectionProfile = _profileCollection.Find(filter).FirstOrDefault();
                if (connectionProfile == null)
                {
                    continue;
                }

                Connection connection = new()
                {
                    ProfileId = connectionProfile.ProfileId,
                    UserName = connectionProfile.UserName,
                    FirstName = connectionProfile.FirstName,
                    LastName = connectionProfile.LastName,
                    Stories = new List<Story>()
                };

                var storyFilter = Builders<Story>.Filter.Eq("From", connectionProfile.UserName);
                var stories = _storiesCollection.Find(storyFilter).ToList();
                if (stories != null && stories.Any())
                {
                    foreach (var story in stories)
                    {
                        connection.Stories.Add(story);
                    }
                }

                connections.Add(connection);
            }

            return connections;
        }

        public bool IsConnected(ConnectionRequest connectionRequest)
        {
            UserProfile? fromUserProfile = FindByUserName(connectionRequest.FromUserName);
            UserProfile? toUserProfile = FindByUserName(connectionRequest.ToUserName);

            if (fromUserProfile == null
                || toUserProfile == null)
            {
                return false;
            }

            if (fromUserProfile.Connections?.Contains(toUserProfile.ProfileId) == true)
            {
                return true;
            }

            return false;
        }

        public bool IsConnectionPending(ConnectionRequest connectionRequest)
        {
            UserProfile? fromUserProfile = FindByUserName(connectionRequest.FromUserName);
            UserProfile? toUserProfile = FindByUserName(connectionRequest.ToUserName);

            if (fromUserProfile == null
                || toUserProfile == null)
            {
                return false;
            }

            if (fromUserProfile.SentConnectionRequests != null)
            {
                if (fromUserProfile.SentConnectionRequests.Contains(toUserProfile.ProfileId))
                {
                    return true;
                }
            }

            return false;
        }

        public ConnectionRequest? Connect(ConnectionRequest connectionRequest)
        {
            UserProfile? fromUserProfile = FindByUserName(connectionRequest.FromUserName);
            UserProfile? toUserProfile = FindByUserName(connectionRequest.ToUserName);

            if (fromUserProfile == null
                || toUserProfile == null)
            {
                return null;
            }

            connectionRequest.RequestId = Guid.NewGuid().ToString();
            connectionRequest.RequestDateTimeUtc = DateTime.UtcNow;
            connectionRequest.RequestStaus = "Pending";
            _connectionRequestsCollection.InsertOne(connectionRequest);

            // Update the incommig connection requests for the touser
            toUserProfile.IncomingConnectionRequests ??= new List<string>();
            toUserProfile.IncomingConnectionRequests.Add(fromUserProfile.ProfileId);
            var filter = Builders<UserProfile>.Filter.Eq("UserName", connectionRequest.ToUserName);
            try
            {
                var update = Builders<UserProfile>.Update.Set("IncomingConnectionRequests", toUserProfile.IncomingConnectionRequests);
                _profileCollection.UpdateOne(filter, update);
            }
            catch
            {
                // Cleanup the connection request collection
                DeleteConnectionRequest(connectionRequest.RequestId);
                throw;
            }

            // Update the sent connection requests
            fromUserProfile.SentConnectionRequests ??= new List<string>();
            fromUserProfile.SentConnectionRequests.Add(toUserProfile.ProfileId);
            filter = Builders<UserProfile>.Filter.Eq("UserName", connectionRequest.FromUserName);
            try
            {
                var update = Builders<UserProfile>.Update.Set("SentConnectionRequests", fromUserProfile.SentConnectionRequests);
                _profileCollection.UpdateOne(filter, update);
            }
            catch
            {
                // Cleanup the connection request collection
                DeleteConnectionRequest(connectionRequest.RequestId);
                throw;
            }

            return connectionRequest;
        }

        public string UpdateConnectionOutcome(ConnectionRequestResult connectionResult)
        {
            var filter = Builders<ConnectionRequest>.Filter.Eq("RequestId", connectionResult.RequestId);
            var connectionRequest = _connectionRequestsCollection.Find(filter).FirstOrDefault();

            if (connectionRequest == null)
            {
                return "Connection request not found";
            }

            UserProfile? fromUserProfile = FindByUserName(connectionRequest.FromUserName);
            UserProfile? toUserProfile = FindByUserName(connectionRequest.ToUserName);

            if (fromUserProfile == null
                || toUserProfile == null)
            {
                return "From User or To User profile not found";
            }

            // Update the connection request status
            var updateConnectionRequest = Builders<ConnectionRequest>.Update.Set("RequestStaus", connectionResult.ConnectionRequestOutcome);
            _connectionRequestsCollection.UpdateOne(filter, updateConnectionRequest);

            // Remove the incommig connection requests for the touser
            toUserProfile.IncomingConnectionRequests ??= new List<string>();
            toUserProfile.IncomingConnectionRequests.Remove(toUserProfile.ProfileId);
            var profileFilter = Builders<UserProfile>.Filter.Eq("UserName", connectionRequest.ToUserName);
            var update = Builders<UserProfile>.Update.Set("IncomingConnectionRequests", toUserProfile.IncomingConnectionRequests);
            _profileCollection.UpdateOne(profileFilter, update);

            // Remove the sent connection requests for the from user
            fromUserProfile.SentConnectionRequests ??= new List<string>();
            fromUserProfile.SentConnectionRequests.Remove(fromUserProfile.ProfileId);
            profileFilter = Builders<UserProfile>.Filter.Eq("UserName", connectionRequest.FromUserName);
            update = Builders<UserProfile>.Update.Set("SentConnectionRequests", fromUserProfile.SentConnectionRequests);
            _profileCollection.UpdateOne(profileFilter, update);

            // Add the connection to the user profile
            toUserProfile.Connections ??= new List<string>();
            toUserProfile.Connections.Add(fromUserProfile.ProfileId);
            var toUserFilter = Builders<UserProfile>.Filter.Eq("UserName", connectionRequest.ToUserName);
            update = Builders<UserProfile>.Update.Set("Connections", toUserProfile.Connections);
            _profileCollection.UpdateOne(toUserFilter, update);

            // Add the connection to the from profile as well since both users are not connected
            fromUserProfile.Connections ??= new List<string>();
            fromUserProfile.Connections.Add(toUserProfile.ProfileId);
            var fromUserFilter = Builders<UserProfile>.Filter.Eq("UserName", connectionRequest.FromUserName);
            update = Builders<UserProfile>.Update.Set("Connections", fromUserProfile.Connections);
            _profileCollection.UpdateOne(fromUserFilter, update);

            return "Connection request outcome has been successfully recorded";
        }

        public UserProfile SaveUserProfile(UserRegistrationRequest userRegistrationRequest)
        {
            UserProfile userProfile = new()
            {
                UserName = userRegistrationRequest.UserName.ToLower(),
                Password = PasswordHelper.GetPasswordHash(userRegistrationRequest.Password),
                FirstName = userRegistrationRequest.FirstName,
                LastName = userRegistrationRequest.LastName,
                Age = userRegistrationRequest.Age,
                Gender = userRegistrationRequest.Gender,
                Country = userRegistrationRequest.Country,
                State = userRegistrationRequest.State,
                City = userRegistrationRequest.City,
                Email = userRegistrationRequest.Email,
                PhoneNumber = userRegistrationRequest.PhoneNumber,
                ProfileId = Guid.NewGuid().ToString(),
                CreatedAtUtc = DateTime.UtcNow,
            };

            _profileCollection.InsertOne(userProfile);
            return userProfile;
        }

        public LifeCkl SaveLifeCkl(UserProfile userProfile)
        {
            LifeCkl lifeCkl = new LifeCkl { ProfileId = userProfile.ProfileId };
            _lifeCklsCollection.InsertOne(lifeCkl);
            return lifeCkl;
        }

        public bool DeleteUserProfile(string profileId)
        {
            // Specify the filter to identify the document to be deleted
            var filter = Builders<UserProfile>.Filter.Eq("ProfileId", profileId);
            var result = _profileCollection.DeleteOne(filter);
            return result.DeletedCount > 0;
        }

        public bool DeleteConnectionRequest(string requestId)
        {
            // Specify the filter to identify the document to be deleted
            var filter = Builders<ConnectionRequest>.Filter.Eq("RequestId", requestId);
            var result = _connectionRequestsCollection.DeleteOne(filter);
            return result.DeletedCount > 0;
        }

        public UserProfile? FindByUserName(string userName)
        {
            // Define the filter to find the user by username
            var filter = Builders<UserProfile>.Filter.Eq("UserName", userName);

            // Execute the find operation
            var user = _profileCollection.Find(filter).FirstOrDefault();

            if (user == null)
            {
                filter = Builders<UserProfile>.Filter.Eq("UserName", userName.ToUpper());
                user = _profileCollection.Find(filter).FirstOrDefault();
            }

            if (user == null)
            {
                filter = Builders<UserProfile>.Filter.Eq("UserName", userName.ToLower());
                user = _profileCollection.Find(filter).FirstOrDefault();
            }

            return user == null ? null : user;
        }


        public UserProfile? Login(string username, string password)
        {
            var user = FindByUserName(username);
            if (user == null)
            {
                return null;
            }

            var passwordHash = PasswordHelper.GetPasswordHash(password);

            if (!string.Equals(user.Password, passwordHash))
            {
                return null;
            }

            return user;
        }

        public string CreateStory(Story story)
        {
            try
            {
                story.StoryId = Guid.NewGuid().ToString();
                story.Id = ObjectId.GenerateNewId();
                _storiesCollection.InsertOne(story);
                return "Story created successfully";
            }
            catch (Exception e)
            {
                return $"Failed to create story, error: {e.Message}";
            }
        }

        public List<Story> GetStories(string userName)
        {
            var filter = Builders<Story>.Filter.Eq("From", userName);
            var connectionProfile = _storiesCollection.Find(filter).ToEnumerable();
            return connectionProfile.ToList();
        }

        public string HandleStoryParticipation(StoryParticipantInfo participantInfo)
        {
            var storyFilter = Builders<Story>.Filter.Eq("StoryId", participantInfo.StoryId);
            var story = _storiesCollection.Find(storyFilter).FirstOrDefault();

            if (story == null)
            {
                return "Story not found";
            }

            if (story != null)
            {
                story.Participants ??= new List<string>();
                string participant = participantInfo.FirstName + " " + participantInfo.LastName;
                if (story.Participants.Contains(participant))
                {
                    return string.Empty;
                }

                story.Participants.Add(participantInfo.FirstName + " " + participantInfo.LastName);
                var update = Builders<Story>.Update.Set("Participants", story.Participants);
                _storiesCollection.UpdateOne(storyFilter, update);
            }

            return string.Empty;
        }
    }
}
