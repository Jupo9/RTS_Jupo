using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Building Is In Progress", story: "[BaseBuilding] is being built", category: "Conditions", id: "61b3cd8b5e214c2aacfc42f0701ce43c")]
public partial class BuildingIsInProgressCondition : Condition
{
    [SerializeReference] public BlackboardVariable<BaseBuilding> BaseBuilding;

    public override bool IsTrue()
    {
        return BaseBuilding.Value != null && BaseBuilding.Value.Progress.State == BuildingProgress.BuildingState.Building;
    }
}
