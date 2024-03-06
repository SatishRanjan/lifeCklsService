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

        public UserService(IMongoClient mongoClient)
        {
            _mongoClient = mongoClient;
            _database = _mongoClient.GetDatabase("lifecklsstore");
            _profileCollection = _database.GetCollection<UserProfile>("profiles");
            _lifeCklsCollection = _database.GetCollection<LifeCkl>("lifeCkls");
            _connectionRequestsCollection = _database.GetCollection<ConnectionRequest>("ConnectionRequests");
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

            for (int i = 0; i < userProfile.IncomingConnectionRequests.Count; i++)
            {
                var filter = Builders<ConnectionRequest>.Filter.Eq("ToUserName", userName);
                var connectionRequest = _connectionRequestsCollection.Find(filter).FirstOrDefault();
                connectionRequest.FromUserFullName = userProfile.FirstName + " " + userProfile.LastName;
                connectionRequests.Add(connectionRequest);
            }

            return connectionRequests;
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
    }
}
