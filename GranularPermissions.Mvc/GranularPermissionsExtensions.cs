using System;
using GranularPermissions.Conditions;
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
            collection.AddSingleton<IPermissionsService>(sp =>
            {
                var dictionary = sp.GetService<IPermissionsScanner>().All(basePermissionsClass);
                var parser = sp.GetService<IConditionParser>();
                var evaluator = sp.GetService<IConditionEvaluator>();
                var grantProvider = sp.GetService<IPermissionGrantProvider>();

                var instance = new PermissionsService(parser, dictionary, evaluator);
                var list = grantProvider.AllGrants();
                foreach (var permissionGrant in list)
                {
                    instance.InsertSerialized(permissionGrant);
                }

                return instance;
            });

            return collection;
        }
    }
}