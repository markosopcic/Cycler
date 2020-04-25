﻿using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Text;
using Cycler.Controllers.Models;
using Cycler.Data.Models;
using Cycler.Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Cycler.Data.Repositories
{
    public class UserRepository:IUserRepository
    {

        private MongoContext context = null;

        public UserRepository(MongoContext context)
        {
            if(context == null) throw new ArgumentNullException(nameof(context));
            this.context = context;
        }
        
        public User Login(string email, string password)
        {
            var user = context.User.Find(e => e.Email == email).FirstOrDefault();
            if (user == null) return null;

            if (!VerifyPasswordHash(password, user.PasswordHash,
                user.Salt))
            {
                return null;
            }

            return user;
        }

        public User Register(RegisterModel user)
        {
            if (context.User.Find(e => e.Email == user.Email).CountDocuments() != 0) return null;
            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(user.Password, out passwordHash, out passwordSalt);
            var u = new User{FirstName = user.FirstName,LastName = user.LastName,Email = user.Email,
                PasswordHash = passwordHash,Salt = passwordSalt,
                DateJoined = DateTime.Now};
            context.User.InsertOne(u);
                return context.User.Find(e => e.Email == user.Email).First();

        }
        

        public IEnumerable<User> GetAll()
        {
            return context.User.Find(e => true).ToList();
        }

        public bool Delete(string id)
        {
            return context.User.DeleteOne(e => e.Id == new ObjectId(id)).DeletedCount > 0;
        }

        public User GetById(string id)
        {
            return context.User.Find(e => e.Id == new ObjectId(id)).FirstOrDefault();
        }
        
        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");

            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");
            if (storedHash.Length != 64) throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "passwordHash");
            if (storedSalt.Length != 128) throw new ArgumentException("Invalid length of password salt (128 bytes expected).", "passwordHash");

            using (var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }

            return true;
        }

    }
}