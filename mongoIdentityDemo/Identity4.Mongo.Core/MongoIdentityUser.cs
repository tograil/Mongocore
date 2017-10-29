using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Identity4.Mongo.Core
{
    public class MongoIdentityUser
    {
        public MongoIdentityUser()
        {
            //Id = ObjectId.GenerateNewId().ToString();
            Roles = new List<string>();
            Logins = new List<MongoUserLogin>();
            Claims = new List<MongoUserClaims>();
            Tokens = new List<MongoUserToken>();
        }

        [BsonId]
        public int Id { get; set; }

        public string UserName { get; set; }

        public string NormalizedUserName { get; set; }

        public string SecurityStamp { get; set; }

        public string Email { get; set; }

        public string NormalizedEmail { get; set; }

        public bool EmailConfirmed { get; set; }

        public string PhoneNumber { get; set; }

        public bool PhoneNumberConfirmed { get; set; }

        public bool TwoFactorEnabled { get; set; }

        public DateTime? LockoutEndDateUtc { get; set; }

        public bool LockoutEnabled { get; set; }

        public int AccessFailedCount { get; set; }

        [BsonIgnoreIfNull]
        public List<string> Roles { get; set; }

        public virtual void AddRole(string role)
        {
            Roles.Add(role);
        }

        public virtual void RemoveRole(string role)
        {
            Roles.Remove(role);
        }

        [BsonIgnoreIfNull]
        public virtual string PasswordHash { get; set; }

        [BsonIgnoreIfNull]
        public List<MongoUserLogin> Logins { get; set; }

        public virtual void AddLogin(UserLoginInfo login)
        {
            Logins.Add(new MongoUserLogin(login));
        }

        public virtual void RemoveLogin(string loginProvider, string providerKey)
        {
            Logins.RemoveAll(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey);
        }

        public virtual bool HasPassword()
        {
            return false;
        }

        [BsonIgnoreIfNull]
        public List<MongoUserClaims> Claims { get; set; }

        public virtual void AddClaim(Claim claim)
        {
            Claims.Add(new MongoUserClaims());
        }

        public virtual void RemoveClaim(Claim claim)
        {
            Claims.RemoveAll(c => c.Type == claim.Type && c.Value == claim.Value);
        }

        public virtual void ReplaceClaim(Claim existingClaim, Claim newClaim)
        {
            var claimExists = Claims
                .Any(c => c.Type == existingClaim.Type && c.Value == existingClaim.Value);
            if (!claimExists)
            {
                // note: nothing to update, ignore, no need to throw
                return;
            }
            RemoveClaim(existingClaim);
            AddClaim(newClaim);
        }

        [BsonIgnoreIfNull]
        public List<MongoUserToken> Tokens { get; set; }

        private MongoUserToken GetToken(string loginProider, string name)
            => Tokens
                .FirstOrDefault(t => t.LoginProvider == loginProider && t.Name == name);

        public virtual void SetToken(string loginProider, string name, string value)
        {
            var existingToken = GetToken(loginProider, name);
            if (existingToken != null)
            {
                existingToken.Value = value;
                return;
            }

            Tokens.Add(new MongoUserToken
            {
                LoginProvider = loginProider,
                Name = name,
                Value = value
            });
        }

        public virtual string GetTokenValue(string loginProider, string name)
        {
            return GetToken(loginProider, name)?.Value;
        }

        public virtual void RemoveToken(string loginProvider, string name)
        {
            Tokens.RemoveAll(t => t.LoginProvider == loginProvider && t.Name == name);
        }

        public override string ToString() => UserName;

    }
}
