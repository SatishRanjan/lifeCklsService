﻿using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LifeCklsModels
{
    public class UserProfile
    {
        public ObjectId Id { get; set; }
        public string ProfileId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
        public string? Email { get; set; }
        public string Country { get; set; }
        public string? State { get; set; }
        public string? City { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
