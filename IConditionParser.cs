using Loyc.Syntax;
using Loyc.Syntax.Les;

namespace GranularPermissions
{
    public interface IConditionParser
    {
        LNode ParseConditionCode(string code);
    }

    class ConditionParser : IConditionParser
    {
        public LNode ParseConditionCode(string code)
        {
            return Les2LanguageService.Value.ParseSingle(code);
        }
    }
}