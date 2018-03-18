using System;

namespace GranularPermissions
{
    public interface IPermissionCondition<in T> where T : IPermissionManaged
    {
        string Key { get; }
        Func<IPermissionManaged, T, object, bool> EvaluationFunction { get; }
    }

    class PermissionCondition<T> : IPermissionCondition<T> where T : IPermissionManaged
    {
        public string Key { get; }
        public Func<IPermissionManaged, T, object, bool> EvaluationFunction { get; }

        public PermissionCondition(string key, Func<IPermissionManaged, T, object, bool> evaluationFunction)
        {
            Key = key;
            EvaluationFunction = evaluationFunction;
        }
    }
}