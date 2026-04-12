using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using System.Collections.Generic;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Find Closest Command Post ", story: "[Unit] finds nearest [CommandPost]", category: "Action/Units", id: "8fcf3621f1cfe88be79bc6947f90616b")]
public partial class FindClosestCommandPostAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Unit;
    [SerializeReference] public BlackboardVariable<GameObject> CommandPost;
    [SerializeReference] public BlackboardVariable<float> SearchRadius = new(10);
    [SerializeReference] public BlackboardVariable<UnitSO> CommandPostBuilding;

    protected override Status OnStart()
    {
        Collider[] colliders = Physics.OverlapSphere(Unit.Value.transform.position, SearchRadius.Value, LayerMask.GetMask("Buildings"));

        List<BaseBuilding> nearbyCommandPost = new();

        foreach (Collider collider in colliders)
        {
            if (collider.TryGetComponent(out BaseBuilding building) && building.UnitSO.Equals(CommandPostBuilding.Value))
            {
                nearbyCommandPost.Add(building);

            }
        }

        if (nearbyCommandPost.Count == 0)
        {
            return Status.Failure;
        }

        CommandPost.Value = nearbyCommandPost[0].gameObject;

        return Status.Success;

    }
}

