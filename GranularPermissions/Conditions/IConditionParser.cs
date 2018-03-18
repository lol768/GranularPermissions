using Loyc.Syntax;

namespace GranularPermissions.Conditions
{
    public interface IConditionParser
    {
        LNode ParseConditionCode(string code);
    }
}