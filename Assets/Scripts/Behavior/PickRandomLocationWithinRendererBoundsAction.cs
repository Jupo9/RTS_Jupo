using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "PickRandomLocationWithinRendererBounds", story: "Set [TargetLocation] to a random point within [BuildingUnderConstruction]", category: "Action", id: "3b27f7e1d241366b3eb4feedffea51e5")]
public partial class PickRandomLocationWithinRendererBoundsAction : Action
{
    [SerializeReference] public BlackboardVariable<Vector3> TargetLocation;
    [SerializeReference] public BlackboardVariable<BaseBuilding> BuildingUnderConstruction;

    protected override Status OnStart()
    {
       if (BuildingUnderConstruction.Value == null || BuildingUnderConstruction.Value.MainRenderer == null)
       { 
            return Status.Failure;
       }

        Renderer renderer = BuildingUnderConstruction.Value.MainRenderer;

        Bounds bounds = renderer.bounds;

        TargetLocation.Value = new Vector3(UnityEngine.Random.Range(bounds.min.x, bounds.max.x), TargetLocation.Value.y, UnityEngine.Random.Range(bounds.min.z, bounds.max.z));

        return Status.Success;
    }
}

