using Loyc.Syntax;

namespace GranularPermissions
{
    public interface IConditionEvaluator
    {
        bool Evaluate(IPermissionManaged resource, LNode parsedNode);
    }
}