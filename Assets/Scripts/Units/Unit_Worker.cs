using System;
using Unity.AppUI.UI;
using Unity.Behavior;
using UnityEngine;

public class Unit_Worker : AbstractUnit, IBuildingBuilder
{
    public bool HasSupplies
    {
        get
        {
            if (graphAgent != null && graphAgent.GetVariable("SupplyAmountHeld", out BlackboardVariable<int> heldVariable))
            {
                return heldVariable.Value > 0;
            }

            return false;
        }
    }

    [SerializeField] private BaseAction CancelBuildingCommand; 

    protected override void Start()
    {
        base.Start();
        if (graphAgent.GetVariable("GatherSuppliesEvent", out BlackboardVariable<GatherSuppliesEventChannel> eventChannelVariable))
        {
            eventChannelVariable.Value.Event += HandleGatherSupplies;
        }
    }

    public void Gather(GatherableSupply supply)
    {
        graphAgent.SetVariableValue("Supply", supply);
        graphAgent.SetVariableValue("TargetGameObject", supply.gameObject);
        graphAgent.SetVariableValue("Command", UnitCommands.Gather);
    }

    public void ReturnSupplies(GameObject commandPost)
    {
        graphAgent.SetVariableValue("CommandPost", commandPost);
        graphAgent.SetVariableValue("Command", UnitCommands.ReturnSupplies);
    }

    public GameObject Build(BuildingSO building, Vector3 targetLocation)
    {
        GameObject instance = Instantiate(building.Prefab, targetLocation, Quaternion.identity);
        if (!instance.TryGetComponent(out BaseBuilding _))
        {
            Debug.LogError($"Missing BaseBuilding on Prefab for BuildingSO \"{building.name}\"! Connat build!");
            return null;
        }

        graphAgent.SetVariableValue("BuildingSO", building);
        graphAgent.SetVariableValue("TargetLocation", targetLocation);
        graphAgent.SetVariableValue("Ghost", instance);
        graphAgent.SetVariableValue("Command", UnitCommands.BuildBuilding);

        SetCommandOverrides(new BaseAction[] { CancelBuildingCommand });
        Bus<UnitSelectedEvent>.Raise(new UnitSelectedEvent(this));

        return instance;
    }

    public void ResumeBuilding(BaseBuilding building)
    {
        graphAgent.SetVariableValue("TargetLocation", building.transform.position);
        graphAgent.SetVariableValue("BuildingUnderConstruction", building);
        graphAgent.SetVariableValue("BuildingSO", building.BuildingSO);
        graphAgent.SetVariableValue<GameObject>("Ghost", null);
        graphAgent.SetVariableValue("Command", UnitCommands.BuildBuilding);

        SetCommandOverrides(new BaseAction[] { CancelBuildingCommand });
        Bus<UnitSelectedEvent>.Raise(new UnitSelectedEvent(this));
    }

    public void CancelBuilding()
    {

        if (graphAgent.GetVariable("Ghost", out BlackboardVariable<GameObject> ghostVariable)
            && ghostVariable.Value != null)
        {
            Destroy(ghostVariable.Value);
        }
        if (graphAgent.GetVariable("BuildingUnderConstruction", out BlackboardVariable<BaseBuilding> buildingVariable)
            && buildingVariable.Value != null)
        {
            Destroy(buildingVariable.Value.gameObject);
        }

        SetCommandOverrides(Array.Empty<BaseAction>());
        Stop();
    }

    private void HandleGatherSupplies(GameObject self, int amount, SupplySO supply)
    {
        Bus<SupplyEvent>.Raise(new SupplyEvent(amount, supply));
    }

}
