using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Set NavMeshAgent Enabled", story: "[Self] sets NavMeshAgent active status to [Active]", category: "Action/Navigation", id: "8e909b648a013785c0cecd4e0b75a6b0")]
public partial class SetNavMeshAgentEnabledAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<bool> Active;

    protected override Status OnStart()
    {
        if (Self.Value == null || !Self.Value.TryGetComponent(out NavMeshAgent agent))
        {
            return Status.Failure;
        }

        agent.enabled = Active;

        return Status.Success;
    }
}

