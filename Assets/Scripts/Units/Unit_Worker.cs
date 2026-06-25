using System;
using Unity.AppUI.UI;
using Unity.Behavior;
using UnityEngine;

public class Unit_Worker : AbstractUnit, IBuildingBuilder
{
    public bool IsBuilding => graphAgent.GetVariable("Command", out BlackboardVariable<UnitCommands> command) 
                               && command.Value == UnitCommands.BuildBuilding;

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

    [SerializeField] private BaseCommand CancelBuildingCommand; 

    protected override void Start()
    {
        base.Start();
        if (graphAgent.GetVariable("GatherSuppliesEvent", out BlackboardVariable<GatherSuppliesEventChannel> eventChannelVariable))
        {
            eventChannelVariable.Value.Event += HandleGatherSupplies;
        }
        if (graphAgent.GetVariable("BuildingEventChannel", out BlackboardVariable<BuildingEventChannel> buildingEventChannelVariable))
        {
            buildingEventChannelVariable.Value.Event += HandleBuildingSupplies;
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
        if (!instance.TryGetComponent(out BaseBuilding baseBuilding))
        {
            Debug.LogError($"Missing BaseBuilding on Prefab for BuildingSO \"{building.name}\"! Connat build!");
            return null;
        }

        graphAgent.SetVariableValue("BuildingUnderConstruction", baseBuilding);     //for refund supplies if the building is canceled
        graphAgent.SetVariableValue("BuildingSO", building);
        graphAgent.SetVariableValue("TargetLocation", targetLocation);
        graphAgent.SetVariableValue("Ghost", instance);
        graphAgent.SetVariableValue("Command", UnitCommands.BuildBuilding);

        SetCommandOverrides(new BaseCommand[] { CancelBuildingCommand });
        Bus<SupplyEvent>.Raise(new SupplyEvent(-building.Cost.Minerals, building.Cost.MineralsSO));
        Bus<SupplyEvent>.Raise(new SupplyEvent(-building.Cost.Gas, building.Cost.GasSO));

        return instance;
    }

    public void ResumeBuilding(BaseBuilding building)
    {
        graphAgent.SetVariableValue("TargetLocation", building.transform.position);
        graphAgent.SetVariableValue("BuildingUnderConstruction", building);
        graphAgent.SetVariableValue("BuildingSO", building.BuildingSO);
        graphAgent.SetVariableValue<GameObject>("Ghost", null);
        graphAgent.SetVariableValue("Command", UnitCommands.BuildBuilding);
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

            BuildingSO buildingSO = buildingVariable.Value.BuildingSO;
            Bus<SupplyEvent>.Raise(new SupplyEvent(
                Mathf.FloorToInt(0.75f * buildingSO.Cost.Minerals),
                buildingSO.Cost.MineralsSO
            ));
            Bus<SupplyEvent>.Raise(new SupplyEvent(
                Mathf.FloorToInt(0.75f * buildingSO.Cost.Gas),
                buildingSO.Cost.GasSO
            ));
        }

        SetCommandOverrides(Array.Empty<BaseCommand>());
        Stop();
    }

    public override void Deselect()
    {
        if (decalProjector != null)
        {
            decalProjector.gameObject.SetActive(false);
        }

        IsSelected = false;
        if (!IsBuilding)
        { 
            SetCommandOverrides(null);
        }

        Bus<UnitDeselectEvent>.Raise(new UnitDeselectEvent(this));
    }

    private void HandleGatherSupplies(GameObject self, int amount, SupplySO supply)
    {
        Bus<SupplyEvent>.Raise(new SupplyEvent(amount, supply));
    }

    private void HandleBuildingSupplies(GameObject self, BuildingEventType eventType, BaseBuilding building)
    {
        switch(eventType)
        {
            case BuildingEventType.ArrivedAt:
                if (building != null && building.Progress.State == BuildingProgress.BuildingState.Building)
                {
                    Stop();
                    break;
                }
                SetCommandOverrides(new BaseCommand[] { CancelBuildingCommand });
                break;
            case BuildingEventType.Begin:
                SetCommandOverrides(new BaseCommand[] { CancelBuildingCommand });
                break;
            case BuildingEventType.Abort:
            case BuildingEventType.Cancel:
            case BuildingEventType.Completed:
                SetCommandOverrides(null);
                break;
            default:
                break;
        }
    }
}
