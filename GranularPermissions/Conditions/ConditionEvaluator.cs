using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
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
        private const int StackOverflowLimit = 100;
        private IDictionary<string, Func<object>> IdentifierTable = new Dictionary<string, Func<object>>();

        private IDictionary<string, Func<ICollection<LNode>, int, LNode>> FunctionTable =
            new Dictionary<string, Func<ICollection<LNode>, int, LNode>>();

        private LNodeFactory _factory;

        public ConditionEvaluator()
        {
            FunctionTable["'&&"] = (args, sLimit) =>
            {
                return _factory.Literal(args.Aggregate(true,
                    (current, arg) => current & (bool) ResolveLiteral(arg, sLimit).Value));
            };

            FunctionTable["'||"] = (args, sLimit) =>
            {
                return _factory.Literal(args.Aggregate(false,
                    (current, arg) => current | (bool) ResolveLiteral(arg, sLimit).Value));
            };

            FunctionTable["'>"] = (args, sLimit) =>
            {
                var tuple = GetComparableArguments(args, sLimit);
                return _factory.Literal(tuple.Item1.CompareTo(tuple.Item2) > 0);
            };

            FunctionTable["'<"] = (args, sLimit) =>
            {
                var tuple = GetComparableArguments(args, sLimit);
                return _factory.Literal(tuple.Item1.CompareTo(tuple.Item2) < 0);
            };

            FunctionTable["'>="] = (args, sLimit) =>
            {
                var tuple = GetComparableArguments(args, sLimit);
                return _factory.Literal(tuple.Item1.CompareTo(tuple.Item2) >= 0);
            };

            FunctionTable["'<="] = (args, sLimit) =>
            {
                var tuple = GetComparableArguments(args, sLimit);
                return _factory.Literal(tuple.Item1.CompareTo(tuple.Item2) <= 0);
            };

            FunctionTable["'=="] = (args, sLimit) =>
            {
                EnsureBinaryFunctionArguments(args);
                var left = ResolveLiteral(args.First(), sLimit).Value;
                var right = ResolveLiteral(args.Last(), sLimit).Value;

                return _factory.Literal(left.Equals(right));
            };
            
            FunctionTable["'~="] = (args, sLimit) =>
            {
                EnsureBinaryFunctionArguments(args);
                var left = ResolveLiteral(args.First(), sLimit).Value as string;
                var right = ResolveLiteral(args.Last(), sLimit).Value as string;

                if (left == null || right == null)
                {
                    return _factory.Literal(false);
                }

                var result = false;
                try
                {
                    result = Regex.IsMatch(left, right, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(2));
                }
                catch (RegexMatchTimeoutException)
                {
                    result = false;
                }
                return _factory.Literal(result);
            };

            FunctionTable["'!="] = (args, sLimit) =>
            {
                EnsureBinaryFunctionArguments(args);
                var left = ResolveLiteral(args.First(), sLimit).Value;
                var right = ResolveLiteral(args.Last(), sLimit).Value;

                return _factory.Literal(!left.Equals(right));
            };

            FunctionTable["'!"] = (args, sLimit) =>
            {
                EnsureSingleFunctionArgument(args);
                var operand = ResolveLiteral(args.First(), sLimit).Value;
                if (!(operand is bool))
                {
                    throw new InvalidOperationException("Attempt to negate a non-boolean value. This is not JavaScript.");
                }
                return _factory.Literal(!((bool)operand));
            };

            FunctionTable["'."] = (args, sLimit) =>
            {
                EnsureBinaryFunctionArguments(args);

                var literalLeft = ResolveLiteral(args.First(), sLimit).Value;
                var identifierRight = (args.Last()).Name.Name;

                if (literalLeft.GetType().GetProperty(identifierRight) != null)
                {
                    return _factory.Literal(literalLeft.GetType().GetProperty(identifierRight).GetValue(literalLeft));
                }

                if (literalLeft.GetType().GetMethod(identifierRight) != null)
                {
                    throw new ArgumentException("Method calls prohibited due to RCE risk.");
                }

                throw new ArgumentException(
                    $"Couldn't find a {identifierRight} on the provided {literalLeft.GetType().Name}");
            };

            IdentifierTable["true"] = () => true;
            IdentifierTable["false"] = () => false;
        }

        private (IComparable, IComparable) GetComparableArguments(ICollection<LNode> args, int sLimit)
        {
            EnsureBinaryFunctionArguments(args);
            var left = ResolveLiteral(args.First(), sLimit).Value as IComparable;
            var right = ResolveLiteral(args.Last(), sLimit).Value as IComparable;
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

        private LiteralNode ResolveLiteral(LNode input, int stackOverflowLimit = StackOverflowLimit)
        {
            if ((--stackOverflowLimit) <= 0)
            {
                throw new StackOverflowException("'Stack overflow' whilst evaluating permission condition");
            }
            
            while (!input.IsLiteral)
            {
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
                        input = FunctionTable[input.Name.Name](input.Args, stackOverflowLimit);
                        continue;
                    }

                    throw new InvalidExpressionException(
                        $"Reference to function which does not exist: {input.Name.Name}");
                }

                throw new ArgumentException("Cannot resolve a " + input.Kind);
            }

            if (input.HasValue && input.Value.GetType().IsEnum)
            {
                input = _factory.Literal(input.Value.ToString());
            }
            
            return input as LiteralNode;
        }

        public bool Evaluate(IPermissionManaged resource, LNode parsedNode)
        {
            lock (IdentifierTable)
            {
                _factory = new LNodeFactory(parsedNode.Source);
                IdentifierTable[ResourceIdentifierName] = () => resource;
                var result = ResolveLiteral(parsedNode);
                return (bool) result.Value;
            }   
        }
    }
}