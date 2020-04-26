﻿using Cycler.Data.Models;
using Cycler.Data.Repositories;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Cycler.Data
{
    public class MongoContext
    {
        private readonly IMongoDatabase database = null;

        public IMongoCollection<User> User => database.GetCollection<User>("User");

        public IMongoCollection<Event> Event => database.GetCollection<Event>("Event");
        
        public IMongoCollection<Invitation> Invitation => database.GetCollection<Invitation>("Invitation");

        public MongoContext(string connectionString,string databaseName)
        {
            var client = new MongoClient(connectionString);
            database = client.GetDatabase(databaseName);
            
        }
    }
}