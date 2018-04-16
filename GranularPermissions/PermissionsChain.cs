using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using GranularPermissions.Conditions;

namespace GranularPermissions
{
    public class PermissionsChain
    {
        private readonly IConditionEvaluator _evaluator;

        public PermissionsChain(IConditionEvaluator evaluator)
        {
            _evaluator = evaluator;
        }

        private IDictionary<int, SortedList<int, IPermissionGrant>> _entries =
            new ConcurrentDictionary<int, SortedList<int, IPermissionGrant>>();

        public void Insert(IPermissionGrant grant, int identifier)
        {
            var queue = _entries.ContainsKey(identifier)
                ? _entries[identifier]
                : new SortedList<int, IPermissionGrant>(new IndexComparer<int>());

            lock (queue)
            {
                queue.Add(grant.Index, grant);
            }
            _entries[identifier] = queue;
        }

        public (PermissionResult, IEnumerable<PermissionDecision>) ResolvePermission(INode nodeToResolve,
            int identifier, IPermissionManaged resource = null)
        {
            var result = PermissionResult.Unset;
            var considered = new List<PermissionDecision>();
            if (!_entries.ContainsKey(identifier))
            {
                return (result, considered);
            }

            var items = _entries[identifier];
            lock (items)
            {
                foreach (var keyValuePair in items.Where(kvp => kvp.Value.IsFor(nodeToResolve)))
                {
                    var grant = keyValuePair.Value;
                    if (grant.PermissionType == PermissionType.Generic)
                    {
                        switch (grant.GrantType)
                        {
                            case GrantType.Allow:
                                result = PermissionResult.Allowed;
                                break;
                            case GrantType.Deny:
                                result = PermissionResult.Denied;
                                break;
                        }

                        considered.Add(new PermissionDecision(grant, result, true));
                    }
                    else
                    {
                        var resourcedGrant = grant as ResourcedPermissionGrant<IPermissionManaged>;
                        var conditionsSatisfied = true;
                        if (resourcedGrant.Condition != null)
                        {
                            conditionsSatisfied = _evaluator.Evaluate(resource, resourcedGrant.Condition);
                        }

                        switch (grant.GrantType)
                        {
                            case GrantType.Allow when conditionsSatisfied:
                                result = PermissionResult.Allowed;
                                considered.Add(new PermissionDecision(grant, result, conditionsSatisfied));
                                break;
                            case GrantType.Deny when conditionsSatisfied:
                                result = PermissionResult.Denied;
                                considered.Add(new PermissionDecision(grant, result, conditionsSatisfied));
                                break;
                            default:
                                considered.Add(new PermissionDecision(grant, PermissionResult.Unset,
                                    conditionsSatisfied));
                                break;
                        }
                    }
                }
            }

            return (result, considered);
        }
        
        class IndexComparer<TKey> : IComparer<TKey> where TKey : IComparable
        {
            public int Compare(TKey x, TKey y)
            {
                if (x == null || y == null) return 1;
                var result = x.CompareTo(y);

                return result == 0 ? 1 : result;
            }
        }
    }
}