using System;
using System.Collections;
using UnityEngine;

public class BuildingBuilidingUI : MonoBehaviour, IUIElement<BaseBuilding>
{
    [SerializeField] private UIBuildQueueButton[] unitButtons;
    [SerializeField] private ProgressBar progressBar;

    private Coroutine buildCoroutine;
    private BaseBuilding building;

    public void EnableFor(BaseBuilding item)
    {
        progressBar.SetProgress(0);
        gameObject.SetActive(true);
        building = item;
        building.OnQueueUpdated += HandleQueueUpdated;

        SetupUnitButtons();

        buildCoroutine = StartCoroutine(UpdateUnitProgress());
    }

    private void SetupUnitButtons()
    {
        // int i = 0 is outside the for loop so that it can be used in both loops. The first loop enables the buttons for the units in the queue, and the second loop disables any remaining buttons.
        int i = 0;
        for (; i < building.QueueSize; i++)
        {
            int index = i;
            unitButtons[i].EnableFor(building.Queue[i], () => building.CancelBuildingUnit(index));
        }
        for (; i < unitButtons.Length; i++)
        {
            unitButtons[i].Disable();
        }
    }

    public void Disable()
    {
        if (building != null)
        {
            building.OnQueueUpdated -= HandleQueueUpdated;
        }

        gameObject.SetActive(false);
        building = null;
        buildCoroutine = null;
    }

    private void HandleQueueUpdated(AbstractUnitSO[] unitsInQueue)
    {
        if (unitsInQueue.Length == 1 && buildCoroutine == null)
        {
           buildCoroutine = StartCoroutine(UpdateUnitProgress());
        }

        SetupUnitButtons();
    }

    private IEnumerator UpdateUnitProgress()
    {
        while (building != null && building.QueueSize > 0)
        {
            float startTime = building.CurrentQueueStartTime;
            float endTime = startTime + building.BuildingUnit.BuildTime;

            float progress = Mathf.Clamp01((Time.time -startTime) / (endTime - startTime));

            progressBar.SetProgress(progress);
            yield return null;
        }

        buildCoroutine = null;
    }
}
