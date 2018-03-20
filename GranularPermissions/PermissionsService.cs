using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GranularPermissions.Conditions;
using GranularPermissions.Events;

namespace GranularPermissions
{
    public class PermissionsService : IPermissionsService, IPermissionsEventBroadcaster
    {
        private readonly IConditionParser _parser;
        private readonly IDictionary<string, INode> _nodeDefinitions;
        private readonly IConditionEvaluator _evaluator;
        public event ComputedChainDecisionHandler ComputedChainDecision;


        private IDictionary<string, PermissionsChain> Chains = new ConcurrentDictionary<string, PermissionsChain>();

        public PermissionsService(IConditionParser parser, IDictionary<string, INode> nodeDefinitions,
            IConditionEvaluator evaluator)
        {
            _parser = parser;
            _nodeDefinitions = nodeDefinitions;
            _evaluator = evaluator;
        }

        public PermissionResult GetResultUsingChain(string chainName, INode permissionToCheck, int identifier,
            IPermissionManaged resource = null)
        {
            if (!Chains.ContainsKey(chainName))
            {
                Chains[chainName] = new PermissionsChain(_evaluator);
            }

            var tuple = Chains[chainName].ResolvePermission(permissionToCheck, identifier, resource);

            ComputedChainDecision?.Invoke(this, new ComputedChainDecisionEventArgs
            (
                tuple.Item2, chainName, identifier, tuple.Item1, permissionToCheck
            ));

            return tuple.Item1;
        }

        public IDictionary<string, INode> GetDefinedNodes()
        {
            return new ReadOnlyDictionary<string, INode>(_nodeDefinitions);
        }

        public void InsertSerialized(IPermissionGrantSerialized serialized)
        {
            var chainName = serialized.PermissionChain;
            var identifier = serialized.Identifier;

            var tableInstance = Chains.ContainsKey(chainName) ? Chains[chainName] : new PermissionsChain(_evaluator);
            if (!_nodeDefinitions.ContainsKey(serialized.NodeKey))
            {
                throw new ArgumentException("No such permission node in definition list");
            }

            var potentialNode = _nodeDefinitions[serialized.NodeKey];

            Chains[chainName] = tableInstance;

            if (serialized.PermissionType == PermissionType.Generic)
            {
                if (potentialNode.PermissionType != PermissionType.Generic)
                {
                    throw new ArgumentException(
                        $"Mismatch in resource/generic node type, was expecting generic for {serialized.NodeKey}");
                }

                var genericToAdd = new GenericPermissionGrant(serialized.GrantType, potentialNode as GenericNode,
                    serialized.Index);
                tableInstance.Insert(genericToAdd, identifier);

                return;
            }

            if (potentialNode.PermissionType != PermissionType.ResourceBound)
            {
                throw new ArgumentException(
                    $"Mismatch in resource/generic node type, was expecting resource bound for {serialized.NodeKey}");
            }

            var compiled = _parser.ParseConditionCode(serialized.ConditionCode);
            var resourcedToAdd = new ResourcedPermissionGrant<IPermissionManaged>(
                serialized.GrantType, (IResourceNode) potentialNode, compiled, serialized.Index
            );

            tableInstance.Insert(resourcedToAdd, identifier);
        }
    }
}