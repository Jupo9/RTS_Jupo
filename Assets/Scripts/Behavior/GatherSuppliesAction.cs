using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Gather Supplies", story: "[Unit] gather [Amount] supplies from [GatherableSupplies]", category: "Action/Units", id: "75803600d7a24ff770de59b8fb4887df")]
public partial class GatherSuppliesAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Unit;
    [SerializeReference] public BlackboardVariable<int> Amount;
    [SerializeReference] public BlackboardVariable<GatherableSupply> GatherableSupplies;

    private float enterTime;

    protected override Status OnStart()
    {
        if (GatherableSupplies.Value == null)
        {
            return Status.Failure;
        }

        enterTime = Time.time;

        GatherableSupplies.Value.BeginGather();
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (GatherableSupplies.Value.Supply.BaseGatherTime + enterTime <= Time.time)
        {
            return Status.Success;
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (GatherableSupplies.Value == null)
        {
            return; 
        }

            if (CurrentStatus == Status.Success)
        { 
            Amount.Value = Amount.Value = GatherableSupplies.Value.EndGather(); 
        }
        else
        {
            GatherableSupplies.Value.AbortGather();
        }
    }
}

