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

namespace LifeCicklsService.Services
{
    public class UserService : IUserService
    {
        private string _connectionString = "mongodb://localhost:55000/";
        private string _dbName = "lifecklsstore";
        private string _lifeCklsCollectionName = "lifeCkls";
        private string _profileCollectionName = "profiles";
        private IMongoCollection<BsonDocument> _lifeCklsCollection;
        private IMongoCollection<BsonDocument> _profileCollection;

        public UserService()
        {
            // Create a MongoClient to connect to the server
            var client = new MongoClient(_connectionString);

            // Get a reference to the database
            var database = client.GetDatabase(_dbName);

            // Get a reference to the collection
            _lifeCklsCollection = database.GetCollection<BsonDocument>(_lifeCklsCollectionName);
            _profileCollection = database.GetCollection<BsonDocument>(_profileCollectionName);
        }

        public UserProfile Register(UserRegistrationRequest userRegistrationRequest)
        {
            var savedProfile = SaveUserProfile(userRegistrationRequest);
            SaveLifeCkl(savedProfile);

            return savedProfile;
        }

        public UserProfile SaveUserProfile(UserRegistrationRequest userRegistrationRequest)
        {
            UserProfile userProfile = new UserProfile
            {
                UserName = userRegistrationRequest.UserName,
                FirstName = userRegistrationRequest.FirstName,
                LastName = userRegistrationRequest.LastName,
                Age = userRegistrationRequest.Age,
                Gender = userRegistrationRequest.Gender,
                Country = userRegistrationRequest.Country,
                State = userRegistrationRequest.State,
                City = userRegistrationRequest.City,
                Email = userRegistrationRequest.Email,
                PhoneNumber = userRegistrationRequest.PhoneNumber
            };

            userProfile.ProfileId = Guid.NewGuid().ToString();
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
    }
}
