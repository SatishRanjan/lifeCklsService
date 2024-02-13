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

namespace LifeCicklsService.Services
{
    public class UserService : IUserService
    {
        //private string _connectionString = "mongodb://localhost:55000/";
        private readonly string _connectionString = "COSMOSDB_MONGO_CONNECTION";
        private readonly string _dbName = "lifecklsstore";
        private readonly string _lifeCklsCollectionName = "lifeCkls";
        private readonly string _profileCollectionName = "profiles";
        private readonly IMongoCollection<BsonDocument> _lifeCklsCollection;
        private readonly IMongoCollection<BsonDocument> _profileCollection;

        public UserService()
        {
            // Create a MongoClient to connect to the server
            var client = new MongoClient(Environment.GetEnvironmentVariable(_connectionString));

            // Get a reference to the database
            var database = client.GetDatabase(_dbName);

            // Get a reference to the collection
            _lifeCklsCollection = database.GetCollection<BsonDocument>(_lifeCklsCollectionName);
            _profileCollection = database.GetCollection<BsonDocument>(_profileCollectionName);
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

        public UserProfile SaveUserProfile(UserRegistrationRequest userRegistrationRequest)
        {
            UserProfile userProfile = new()
            {
                UserName = userRegistrationRequest.UserName,
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
                ProfileId = Guid.NewGuid().ToString()
            };
            BsonDocument bsonDoc = userProfile.ToBsonDocument();
            _profileCollection.InsertOne(bsonDoc);

            return userProfile;
        }

        public LifeCkl SaveLifeCkl(UserProfile userProfile)
        {
            LifeCkl lifeCkl = new LifeCkl { ProfileId = userProfile.ProfileId };

            userProfile.ProfileId = Guid.NewGuid().ToString();
            BsonDocument bsonDoc = lifeCkl.ToBsonDocument();
            _lifeCklsCollection.InsertOne(bsonDoc);
            return lifeCkl;
        }

        public bool DeleteUserProfile(string profileId)
        {
            // Specify the filter to identify the document to be deleted
            var filter = Builders<BsonDocument>.Filter.Eq("ProfileId", profileId);

            // Delete the document matching the filter
            var result = _profileCollection.DeleteOne(filter);

            if (result.DeletedCount > 0)
            {
                return true;
            }

            return false;
        }

        public UserProfile? FindByUserName(string userName)
        {
            // Define the filter to find the user by username
            var filter = Builders<BsonDocument>.Filter.Eq("UserName", userName);
            // Execute the find operation
            var user = _profileCollection.Find(filter).FirstOrDefault();

            if (user == null)
            {
                return null;
            }

            return ConvertToUserProfile(user);
        }

        public UserProfile ConvertToUserProfile(BsonDocument bsonDocument)
        {
            var userProfile = BsonSerializer.Deserialize<UserProfile>(bsonDocument);
            return userProfile;
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
