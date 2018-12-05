using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

using SecureNotebook.Db;
using SecureNotebook.Db.Entities;
using SecureNotebook.Encryption;
using SecureNotebook.Services.Exceptions;

namespace SecureNotebook.Services
{
    public class UserService
    {
        private NotebookContext db;

        public UserService(NotebookContext db)
        {
            this.db = db;
        }

        public User RegisterUser(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException("Username or password can't be null");
            }

            if (db.Users.FirstOrDefault(u => u.Username == username) == null)
            {
                throw new EntityValidationException($"User with username {username} already exists");
            }

            var user = new User
            {
                Username = username,
                PasswordHash = GeneratePasswordHash(password)
            };

            var userData = GenerateUserData();
            userData.User = user;
            user.UserDatas.Add(userData);

            db.Users.Add(user);
            db.UserDatas.Add(userData);
            db.SaveChanges();

            return user;
        }

        public User LoginUser(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException("Username or password can't be null");
            }

            var user = db.Users.FirstOrDefault(u => u.Username == username)
                ?? throw new EntityNotFoundException($"User with username {username} was not found");

            if (!IsPasswordCorrect(password, user.PasswordHash))
            {
                throw new EntityValidationException("Password is incorrect");
            }

            user.UserDatas.ForEach(TerminateToken);

            var userData = GenerateUserData();
            user.UserDatas.Add(userData);

            db.Users.Update(user);
            db.UserDatas.Add(userData);
            db.SaveChanges();

            return user;
        }

        private void TerminateToken(UserData userData)
        {
            if (userData.IsValid)
            {
                userData.IsValid = false;
            }
        }

        private UserData GenerateUserData()
        {
            var token = Guid.NewGuid().ToString();
            var sessionKey = GeneratorForIDEA.GetKey();
            var iv = GeneratorForIDEA.GetIV();

            return new UserData
            {
                Token = token,
                SessionKey = sessionKey,
                IV = iv,
                IsValid = true,
                ExpirationDate = DateTime.Now.AddMinutes(20)
            };
        }

        private string GeneratePasswordHash(string password)
        {
            byte[] salt = new byte[16];
            var saltGenerator = RandomNumberGenerator.Create();
            saltGenerator.GetBytes(salt);

            var pbkdf2 = new Rfc2898DeriveBytes(password, salt);

            byte[] hash = pbkdf2.GetBytes(20);
            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);

            return Convert.ToBase64String(hashBytes);
        }

        private bool IsPasswordCorrect(string password, string passwordHash)
        {
            byte[] hashBytes = Convert.FromBase64String(passwordHash);
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            var pbkdf2 = new Rfc2898DeriveBytes(password, salt);

            byte[] hash = pbkdf2.GetBytes(20);

            for (int i = 0; i < hash.Length; i++)
            {
                if (hashBytes[i + 16] != hash[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
