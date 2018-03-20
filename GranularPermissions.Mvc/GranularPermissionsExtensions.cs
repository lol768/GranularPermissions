using System;
using GranularPermissions.Conditions;
using GranularPermissions.Events;
using Microsoft.Extensions.DependencyInjection;

namespace GranularPermissions.Mvc
{
    public static class GranularPermissionsExtensions
    {
        public static IServiceCollection AddGranularPermissions(this IServiceCollection collection,
            Type basePermissionsClass)
        {
            collection.AddTransient<IPermissionsScanner, PermissionsScanner>();
            collection.AddScoped<IConditionEvaluator, ConditionEvaluator>();
            collection.AddScoped<IConditionParser, ConditionParser>();
            collection.AddScoped<IPermissionAuditLogCollector, PermissionAuditLogCollector>();
            Func<IServiceProvider, PermissionsService> serviceBuilder = sp =>
            {
                using (var scope = sp.CreateScope())
                {
                    var dictionary = scope.ServiceProvider.GetService<IPermissionsScanner>().All(basePermissionsClass);
                    var parser = scope.ServiceProvider.GetService<IConditionParser>();
                    var evaluator = scope.ServiceProvider.GetService<IConditionEvaluator>();
                    var grantProvider = scope.ServiceProvider.GetService<IPermissionGrantProvider>();

                    var instance = new PermissionsService(parser, dictionary, evaluator);
                    var list = grantProvider.AllGrants();
                    foreach (var permissionGrant in list)
                    {
                        instance.InsertSerialized(permissionGrant);
                    }

                    return instance;
                }
            };
            collection.AddSingleton<IPermissionsService>(serviceBuilder);
            collection.AddSingleton<IPermissionsEventBroadcaster>(serviceBuilder);

            return collection;
        }
    }
}