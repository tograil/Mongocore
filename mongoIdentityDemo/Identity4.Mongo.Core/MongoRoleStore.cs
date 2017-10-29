using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;

namespace Identity4.Mongo.Core
{
    public class MongoRoleStore<TRole> : IQueryableRoleStore<TRole>
        where TRole : MongoIdentityRole
    {
        private readonly IMongoCollection<TRole> _roles;
        private readonly IAutoincrement _autoincrement;

        public MongoRoleStore(IMongoCollection<TRole> roles, IAutoincrement autoincrement)
        {
            _roles = roles;
            _autoincrement = autoincrement;
        }

        public virtual void Dispose()
        {
            // no need to dispose of anything, mongodb handles connection pooling automatically
        }

        public virtual async Task<IdentityResult> CreateAsync(TRole role, CancellationToken token)
        {
            role.Id = _autoincrement.GetNext();
            await _roles.InsertOneAsync(role, cancellationToken: token);
            return IdentityResult.Success;
        }

        public virtual async Task<IdentityResult> UpdateAsync(TRole role, CancellationToken token)
        {
            var result = await _roles.ReplaceOneAsync(r => r.Id == role.Id, role, cancellationToken: token);
            // todo low priority result based on replace result
            return IdentityResult.Success;
        }

        public virtual async Task<IdentityResult> DeleteAsync(TRole role, CancellationToken token)
        {
            var result = await _roles.DeleteOneAsync(r => r.Id == role.Id, token);
            // todo low priority result based on delete result
            return IdentityResult.Success;
        }

        public virtual async Task<string> GetRoleIdAsync(TRole role, CancellationToken cancellationToken)
            => role.Id.ToString();

        public virtual async Task<string> GetRoleNameAsync(TRole role, CancellationToken cancellationToken)
            => role.Name;

        public virtual async Task SetRoleNameAsync(TRole role, string roleName, CancellationToken cancellationToken)
            => role.Name = roleName;

        // note: can't test as of yet through integration testing because the Identity framework doesn't use this method internally anywhere
        public virtual async Task<string> GetNormalizedRoleNameAsync(TRole role, CancellationToken cancellationToken)
            => role.NormalizedName;

        public virtual async Task SetNormalizedRoleNameAsync(TRole role, string normalizedName, CancellationToken cancellationToken)
            => role.NormalizedName = normalizedName;

        public virtual Task<TRole> FindByIdAsync(string roleId, CancellationToken token)
            => _roles.Find(r => r.Id.ToString() == roleId)
                .FirstOrDefaultAsync(token);

        public virtual Task<TRole> FindByNameAsync(string normalizedName, CancellationToken token)
            => _roles.Find(r => r.NormalizedName == normalizedName)
                .FirstOrDefaultAsync(token);

        public virtual IQueryable<TRole> Roles
            => _roles.AsQueryable();
    }
}
