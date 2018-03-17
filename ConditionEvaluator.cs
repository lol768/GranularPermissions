using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using GranularPermissions.Models;
using Loyc.Syntax;
using Loyc.Syntax.Les;
using NUnit.Framework.Constraints;

namespace GranularPermissions
{
    public class ConditionEvaluator : IConditionEvaluator
    {
        private IDictionary<string, Func<object>> IdentifierTable = new Dictionary<string, Func<object>>();

        private IDictionary<string, Func<ICollection<LNode>, LNode>> FunctionTable =
            new Dictionary<string, Func<ICollection<LNode>, LNode>>();

        // TODO: investigate thread safety
        private LNodeFactory _factory;

        public ConditionEvaluator()
        {
            FunctionTable["'&&"] = args =>
            {
                return _factory.Literal(args.Aggregate(true,
                    (current, arg) => current & (bool) ResolveLiteral(arg).Value));
            };
            
            FunctionTable["'||"] = args =>
            {
                return _factory.Literal(args.Aggregate(true,
                    (current, arg) => current | (bool) ResolveLiteral(arg).Value));
            };

            FunctionTable["'>"] = args =>
            {
                EnsureBinaryFunctionArguments(args);
                var left = ResolveLiteral(args.First()).Value as IComparable;
                var right = ResolveLiteral(args.Last()).Value as IComparable;
                if (left == null || right == null)
                {
                    throw new ArgumentException("Arguments are not comparable!");
                }

                return _factory.Literal(left.CompareTo(right) > 0);
            };
            
            FunctionTable["'<"] = args =>
            {
                EnsureBinaryFunctionArguments(args);
                var left = ResolveLiteral(args.First()).Value as IComparable;
                var right = ResolveLiteral(args.Last()).Value as IComparable;
                if (left == null || right == null)
                {
                    throw new ArgumentException("Arguments are not comparable!");
                }

                return _factory.Literal(left.CompareTo(right) < 0);
            };
            
            FunctionTable["'=="] = args =>
            {
                EnsureBinaryFunctionArguments(args);
                var left = ResolveLiteral(args.First()).Value;
                var right = ResolveLiteral(args.Last()).Value;

                return _factory.Literal(left.Equals(right));
            };
            
            FunctionTable["'!="] = args =>
            {
                EnsureBinaryFunctionArguments(args);
                var left = ResolveLiteral(args.First()).Value;
                var right = ResolveLiteral(args.Last()).Value;

                return _factory.Literal(!left.Equals(right));
            };
            
            FunctionTable["'."] = args =>
            {
                EnsureBinaryFunctionArguments(args);

                var literalLeft = ResolveLiteral(args.First()).Value;
                var identifierRight = (args.Last()).Name.Name;
                
                if (literalLeft.GetType().GetProperty(identifierRight) != null)
                {
                    return _factory.Literal(literalLeft.GetType().GetProperty(identifierRight).GetValue(literalLeft));
                }

                if (literalLeft.GetType().GetMethod(identifierRight) != null)
                {
                    throw new ArgumentException("Method calls prohibted due to RCE risk.");
                }
                throw new ArgumentException($"Couldn't find a {identifierRight} on the provided {literalLeft.GetType().Name}");
            };

            IdentifierTable["product"] = () => new Product
            {
                Name = "Americano",
                Price = 3.0d,
                Category = new Category()
            };

            IdentifierTable["isOwned"] = () => true;
        }

        private static void EnsureBinaryFunctionArguments(ICollection<LNode> args)
        {
            if (args.Count != 2)
            {
                throw new ArgumentException($"Function call had {args.Count} arguments instead of 2");
            }
        }

        private LiteralNode ResolveLiteral(LNode input)
        {
            var stackOverflowLimit = 10;
            while (stackOverflowLimit > 0)
            {
                stackOverflowLimit--;
                if (input.IsLiteral)
                {
                    return input as LiteralNode;
                }

                if (input.IsId)
                {
                    if (IdentifierTable.ContainsKey(input.Name.Name))
                    {
                        input = _factory.Literal(IdentifierTable[input.Name.Name]());
                        continue;
                    }

                    throw new InvalidExpressionException(
                        $"Reference to identifier which does not exist: {input.Name.Name}");
                }

                if (input.IsCall)
                {
                    if (FunctionTable.ContainsKey(input.Name.Name))
                    {
                        return ResolveLiteral(FunctionTable[input.Name.Name](input.Args));
                    }

                    throw new InvalidExpressionException(
                        $"Reference to function which does not exist: {input.Name.Name}");
                }

                throw new ArgumentException("Cannot resolve a " + input.Kind);
            }

            throw new StackOverflowException("'Stack overflow' whilst evaluating permission condition");
        }

        public bool Evaluate(IPermissionManaged resource, LNode parsedNode)
        {
            _factory = new LNodeFactory(parsedNode.Source);
            var result = ResolveLiteral(parsedNode);
            return (bool) result.Value;
        }
    }
}