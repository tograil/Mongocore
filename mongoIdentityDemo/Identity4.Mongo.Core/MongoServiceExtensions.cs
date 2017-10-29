using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Identity4.Mongo.Core
{
    public static class MongoServiceExtensions
    {
        public static IdentityBuilder RegisterMongoStores<TUser, TRole>(this IdentityBuilder builder, string connectionString)
            where TRole : MongoIdentityRole
            where TUser : MongoIdentityUser
        {
            var url = new MongoUrl(connectionString);
            var client = new MongoClient(url);
            if (url.DatabaseName == null)
            {
                throw new ArgumentException("Your connection string must contain a database name", connectionString);
            }
            var database = client.GetDatabase(url.DatabaseName);

            return builder.RegisterMongoStores(
                p => database.GetCollection<TUser>("users"),
                p => database.GetCollection<TRole>("roles"),
                p => database.GetCollection<MongoDocumentCounter>("counters"));
        }

        /// <summary>
        ///     If you want control over creating the users and roles collections, use this overload.
        ///     This method only registers mongo stores, you also need to call AddIdentity.
        /// </summary>
        /// <typeparam name="TUser"></typeparam>
        /// <typeparam name="TRole"></typeparam>
        /// <param name="builder"></param>
        /// <param name="usersCollectionFactory"></param>
        /// <param name="rolesCollectionFactory"></param>
        /// <param name="autoincrementCollectionFactory"></param>
        public static IdentityBuilder RegisterMongoStores<TUser, TRole>(this IdentityBuilder builder,
            Func<IServiceProvider, IMongoCollection<TUser>> usersCollectionFactory,
            Func<IServiceProvider, IMongoCollection<TRole>> rolesCollectionFactory,
            Func<IServiceProvider, IMongoCollection<MongoDocumentCounter>> autoincrementCollectionFactory)
            where TRole : MongoIdentityRole
            where TUser : MongoIdentityUser
        {
            if (typeof(TUser) != builder.UserType)
            {
                var message = "User type passed to RegisterMongoStores must match user type passed to AddIdentity. "
                              + $"You passed {builder.UserType} to AddIdentity and {typeof(TUser)} to RegisterMongoStores, "
                              + "these do not match.";
                throw new ArgumentException(message);
            }
            if (typeof(TRole) != builder.RoleType)
            {
                var message = "Role type passed to RegisterMongoStores must match role type passed to AddIdentity. "
                              + $"You passed {builder.RoleType} to AddIdentity and {typeof(TRole)} to RegisterMongoStores, "
                              + "these do not match.";
                throw new ArgumentException(message);
            }

            builder.Services.AddSingleton<IUserStore<TUser>>(p => new MongoUserStore<TUser>(usersCollectionFactory(p), new Autoincrement(autoincrementCollectionFactory(p), nameof(MongoIdentityUser))));
            builder.Services.AddSingleton<IRoleStore<TRole>>(p => new MongoRoleStore<TRole>(rolesCollectionFactory(p), new Autoincrement(autoincrementCollectionFactory(p), nameof(MongoIdentityRole))));
            return builder;
        }

        /// <summary>
        ///     This method registers identity services and MongoDB stores using the IdentityUser and IdentityRole types.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="connectionString">Connection string must contain the database name</param>
        public static IdentityBuilder AddIdentityWithMongoStores(this IServiceCollection services, string connectionString)
        {
            return services.AddIdentityWithMongoStoresUsingCustomTypes<MongoIdentityUser, MongoIdentityRole>(connectionString);
        }

        /// <summary>
        ///     This method allows you to customize the user and role type when registering identity services
        ///     and MongoDB stores.
        /// </summary>
        /// <typeparam name="TUser"></typeparam>
        /// <typeparam name="TRole"></typeparam>
        /// <param name="services"></param>
        /// <param name="connectionString">Connection string must contain the database name</param>
        public static IdentityBuilder AddIdentityWithMongoStoresUsingCustomTypes<TUser, TRole>(this IServiceCollection services, string connectionString)
            where TUser : MongoIdentityUser
            where TRole : MongoIdentityRole
        {
            return services.AddIdentity<TUser, TRole>()
                .RegisterMongoStores<TUser, TRole>(connectionString);
        }
    }
}
