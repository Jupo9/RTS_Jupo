using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Stop Agent", story: "[Agent] stops moving", category: "Action/Navigation", id: "d20c2d79bfe094d694e81491a2908c52")]
public partial class StopAgentAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;

    protected override Status OnStart()
    {
        if (Agent.Value.TryGetComponent(out NavMeshAgent agent))
        {
            if (agent.TryGetComponent(out Animator animator))
            {
                animator.SetFloat(AnimationConstants.SPEED, 0);
            }

            agent.ResetPath();
            return Status.Success;
        }
        return Status.Failure;
    }
}

