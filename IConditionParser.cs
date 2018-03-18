using Loyc.Syntax;

namespace GranularPermissions
{
    public interface IConditionParser
    {
        LNode ParseConditionCode(string code);
    }
}