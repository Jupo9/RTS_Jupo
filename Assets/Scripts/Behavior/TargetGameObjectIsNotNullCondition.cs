using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "TargetGameObject is not null", story: "[TargetGameObject] is not null", category: "Conditions", id: "ee59abe2763475892a66a976dff18f7f")]
public partial class TargetGameObjectIsNotNullCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> TargetGameObject;

    public override bool IsTrue()
    {
        return TargetGameObject != null && TargetGameObject.Value != null;
    }
}
