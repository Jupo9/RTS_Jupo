using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RuntimeUI : MonoBehaviour
{
    [SerializeField] private ActionsUI actionsUI;
    [SerializeField] private BuildingBuilidingUI buildingBuildingUI;

    private HashSet<AbstractCommandable> selectedUnits = new(12);

    private void Awake()
    {
        Bus<UnitSelectedEvent>.OnEvent += HandleUnitSelected;
        Bus<UnitDeselectEvent>.OnEvent += HandleUnitDeselected;
        Bus<UnitDeathEvent>.OnEvent += HandleUnitDeath;
        Bus<SupplyEvent>.OnEvent += HandleSupplyChange;
    }

    private void Start()
    {
        actionsUI.Disable();
        buildingBuildingUI.Disable();
    }

    private void OnDestroy()
    {
        Bus<UnitSelectedEvent>.OnEvent -= HandleUnitSelected;
        Bus<UnitDeselectEvent>.OnEvent -= HandleUnitDeselected;
        Bus<UnitDeathEvent>.OnEvent -= HandleUnitDeath;
        Bus<SupplyEvent>.OnEvent -= HandleSupplyChange;
    }

    private void HandleUnitSelected(UnitSelectedEvent evt)
    {
        if (evt.Unit is AbstractCommandable commandable)
        {
            selectedUnits.Add(commandable);
            actionsUI.EnableFor(selectedUnits);
        }

        if (selectedUnits.Count == 1 && evt.Unit is BaseBuilding building)
        {
            buildingBuildingUI.EnableFor(building);
        }
    }

    private void HandleUnitDeselected(UnitDeselectEvent evt)
    {
        if (evt.Unit is AbstractCommandable commandable)
        {
            selectedUnits.Remove(commandable);

            RefreshUI();
        }
    }

    private void RefreshUI()
    {
        if (selectedUnits.Count > 0)
        {
            actionsUI.EnableFor(selectedUnits);

            if (selectedUnits.Count == 1 && selectedUnits.First() is BaseBuilding building)
            {
                buildingBuildingUI.EnableFor(building);
            }
            else
            {
                buildingBuildingUI.Disable();
            }
        }
        else
        {
            actionsUI.Disable();
            buildingBuildingUI.Disable();
        }
    }

    private void HandleUnitDeath(UnitDeathEvent evt)
    {
        selectedUnits.Remove(evt.Unit);
        RefreshUI();
    }

    private void HandleSupplyChange(SupplyEvent evt)
    {
        actionsUI.EnableFor(selectedUnits);
    }
}
