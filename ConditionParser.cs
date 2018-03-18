﻿using Loyc.Syntax;
using Loyc.Syntax.Les;

namespace GranularPermissions
{
    class ConditionParser : IConditionParser
    {
        public LNode ParseConditionCode(string code)
        {
            return Les2LanguageService.Value.ParseSingle(code);
        }
    }
}