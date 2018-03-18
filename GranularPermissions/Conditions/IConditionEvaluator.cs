using Loyc.Syntax;

namespace GranularPermissions.Conditions
{
    public interface IConditionEvaluator
    {
        bool Evaluate(IPermissionManaged resource, LNode parsedNode);
    }
}