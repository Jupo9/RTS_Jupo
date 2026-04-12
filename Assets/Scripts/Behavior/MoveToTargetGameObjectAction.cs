using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Move To Target GameObject", story: "[Agent] moves to [TargetGameObject]", category: "Action/Navigation", id: "f148b8c70cb5e07fb4541a94a04dcfe5")]
public partial class MoveToTargetGameObjectAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> TargetGameObject;

    private NavMeshAgent agent;

    protected override Status OnStart()
    {
        if (!Agent.Value.TryGetComponent(out agent))
        {
            return Status.Failure;
        }

        Vector3 targetPosition = GetTargetPosition();

        if (Vector3.Distance(agent.transform.position, targetPosition) <= agent.stoppingDistance)
        {
            return Status.Success;
        }

        agent.SetDestination(targetPosition);

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            return Status.Success;
        }

        return Status.Running;
    }

    private Vector3 GetTargetPosition()
    {
        Vector3 targetPosition;

        if (TargetGameObject.Value.TryGetComponent(out Collider collider))
        {
            targetPosition = collider.ClosestPoint(agent.transform.position);
        }
        else
        {
            targetPosition = TargetGameObject.Value.transform.position;
        }

        return targetPosition;
    }
}

