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
        private string _collectionName = "users";
        private IMongoCollection<BsonDocument> _collection;

        public UserService()
        {
            // Create a MongoClient to connect to the server
            var client = new MongoClient(_connectionString);

            // Get a reference to the database
            var database = client.GetDatabase(_dbName);

            // Get a reference to the collection
            _collection = database.GetCollection<BsonDocument>(_collectionName);
        }

        public User Register(User user)
        {
            user.LifeCklId = $"@{user.FirstName}{user.LastName.Substring(0,1)}";

            // Convert the User object to a BsonDocument
            BsonDocument userDoc = user.ToBsonDocument();
            _collection.InsertOne(userDoc);

            return user;
        }
    }
}
