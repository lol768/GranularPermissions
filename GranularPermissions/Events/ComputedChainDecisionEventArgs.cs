using System;
using System.Collections.Generic;

namespace GranularPermissions.Events
{
    public class ComputedChainDecisionEventArgs : EventArgs
    {
        public IEnumerable<PermissionDecision> Decisions { get; }
        public string ChainName { get; }
        public int Identifier { get; }
        public PermissionResult FinalResult { get; }
        public INode NodeInQuestion { get; }

        public ComputedChainDecisionEventArgs(IEnumerable<PermissionDecision> decisions, string chainName, int identifier, PermissionResult finalResult, INode nodeInQuestion)
        {
            Decisions = decisions;
            ChainName = chainName;
            Identifier = identifier;
            FinalResult = finalResult;
            NodeInQuestion = nodeInQuestion;
        }
    }
}