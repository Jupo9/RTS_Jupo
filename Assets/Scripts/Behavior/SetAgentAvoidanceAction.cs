using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Set Agent Avoidance", story: "Set [Agent] avoidance quality to [AvoidanceQuality]", category: "Action", id: "4aac8e89581961ae08ca75176f5fee91")]
public partial class SetAgentAvoidanceAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<int> AvoidanceQuality;
    protected override Status OnStart()
    {
        if (!Agent.Value.TryGetComponent(out NavMeshAgent agent) || AvoidanceQuality > 4 || AvoidanceQuality < 0)
        {
            return Status.Failure;
        }

        agent.obstacleAvoidanceType = (ObstacleAvoidanceType)AvoidanceQuality.Value;

        return Status.Running;
    }
}

