using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Loyc.Syntax;

namespace GranularPermissions.Conditions
{
    /// <summary>
    /// Evaluates parsed conditions.
    /// 
    /// Do not use across multiple threads at the same time.
    /// </summary>
    public class ConditionEvaluator : IConditionEvaluator
    {
        private const string ResourceIdentifierName = "resource";
        private IDictionary<string, Func<object>> IdentifierTable = new Dictionary<string, Func<object>>();

        private IDictionary<string, Func<ICollection<LNode>, LNode>> FunctionTable =
            new Dictionary<string, Func<ICollection<LNode>, LNode>>();

        private LNodeFactory _factory;
        private int _stackOverflowLimit;

        public ConditionEvaluator()
        {
            FunctionTable["'&&"] = args =>
            {
                return _factory.Literal(args.Aggregate(true,
                    (current, arg) => current & (bool) ResolveLiteral(arg).Value));
            };

            FunctionTable["'||"] = args =>
            {
                return _factory.Literal(args.Aggregate(false,
                    (current, arg) => current | (bool) ResolveLiteral(arg).Value));
            };

            FunctionTable["'>"] = args =>
            {
                var tuple = GetComparableArguments(args);
                return _factory.Literal(tuple.Item1.CompareTo(tuple.Item2) > 0);
            };

            FunctionTable["'<"] = args =>
            {
                var tuple = GetComparableArguments(args);
                return _factory.Literal(tuple.Item1.CompareTo(tuple.Item2) < 0);
            };

            FunctionTable["'>="] = args =>
            {
                var tuple = GetComparableArguments(args);
                return _factory.Literal(tuple.Item1.CompareTo(tuple.Item2) >= 0);
            };

            FunctionTable["'<="] = args =>
            {
                var tuple = GetComparableArguments(args);
                return _factory.Literal(tuple.Item1.CompareTo(tuple.Item2) <= 0);
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

            FunctionTable["'!"] = args =>
            {
                EnsureSingleFunctionArgument(args);
                var operand = ResolveLiteral(args.First()).Value;
                if (!(operand is bool))
                {
                    throw new InvalidOperationException("Attempt to negate a non-boolean value. This is not JavaScript.");
                }
                return _factory.Literal(!((bool)operand));
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

                throw new ArgumentException(
                    $"Couldn't find a {identifierRight} on the provided {literalLeft.GetType().Name}");
            };

            IdentifierTable["isOwned"] = () => true;

            IdentifierTable["true"] = () => true;
            IdentifierTable["false"] = () => false;
        }

        private (IComparable, IComparable) GetComparableArguments(ICollection<LNode> args)
        {
            EnsureBinaryFunctionArguments(args);
            var left = ResolveLiteral(args.First()).Value as IComparable;
            var right = ResolveLiteral(args.Last()).Value as IComparable;
            if (left == null || right == null)
            {
                throw new ArgumentException("Arguments are not comparable!");
            }

            return (left, right);
        }
        
        private static void EnsureSingleFunctionArgument(ICollection<LNode> args)
        {
            AssertNumberOfArguments(args, 1);
        }

        private static void EnsureBinaryFunctionArguments(ICollection<LNode> args)
        {
            AssertNumberOfArguments(args, 2);
        }

        private static void AssertNumberOfArguments(ICollection<LNode> args, int desired)
        {
            if (args.Count != desired)
            {
                throw new ArgumentException($"Function call had {args.Count} arguments instead of 2");
            }
        }

        private LiteralNode ResolveLiteral(LNode input)
        {
            while (_stackOverflowLimit > 0)
            {
                _stackOverflowLimit--;
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
                        input = FunctionTable[input.Name.Name](input.Args);
                        continue;
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
            lock (IdentifierTable)
            {
                _factory = new LNodeFactory(parsedNode.Source);
                _stackOverflowLimit = 10;

                IdentifierTable[ResourceIdentifierName] = () => resource;
                var result = ResolveLiteral(parsedNode);
                return (bool) result.Value;
            }   
        }
    }
}