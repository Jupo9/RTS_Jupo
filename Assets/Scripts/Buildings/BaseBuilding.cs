using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BaseBuilding : AbstractCommandable
{
    public int QueueSize => buildingQueue.Count;
    public AbstractUnitSO[] Queue => buildingQueue.ToArray();
    [field: SerializeField] public float CurrentQueueStartTime { get; private set; }
    [field: SerializeField] public AbstractUnitSO BuildingUnit { get; private set; }
    [field: SerializeField] public MeshRenderer MainRenderer { get; private set; }

    [SerializeField] private Material primaryMaterial;
    [SerializeField] private NavMeshObstacle navMeshObstacle; 

    public delegate void QueueUpdatedEvent(AbstractUnitSO[] unitsInQueue);
    public event QueueUpdatedEvent OnQueueUpdated;

    private BuildingSO buildingSO;
    private List<AbstractUnitSO> buildingQueue = new (MAX_QUEUE_SIZE);
    private const int MAX_QUEUE_SIZE = 5;

    private void Awake()
    {
        buildingSO = UnitSO as BuildingSO;
    }

    protected override void Start()
    {
        base.Start();

        if (MainRenderer != null)
        { 
            MainRenderer.material = primaryMaterial;
        }
    }

    public void BuildUnit(AbstractUnitSO unit)
    {
        if (buildingQueue.Count == MAX_QUEUE_SIZE)
        {
            Debug.LogError("Building queue is full!");
            return;
        }

        buildingQueue.Add(unit);

        if (buildingQueue.Count == 1)
        { 
            StartCoroutine(DoBuildUnits());
        }
        else
        {
            OnQueueUpdated?.Invoke(buildingQueue.ToArray());
        }
    }

    public void CancelBuildingUnit(int index)
    {
        if (index < 0 || index >= buildingQueue.Count)
        {
            Debug.LogError("Attempting to cancel building a unit  outside the bounds if the queue!");
            return;
        }

        buildingQueue.RemoveAt(index);

        if (index == 0)
        {
            StopAllCoroutines();

            if (buildingQueue.Count > 0)
            {
                StartCoroutine(DoBuildUnits());
            }
            else
            {       
                OnQueueUpdated?.Invoke(buildingQueue.ToArray());
            }
        }
        else
        {
            OnQueueUpdated?.Invoke(buildingQueue.ToArray());
        }
    }

    public void ShowGhostVisuals()
    {
        MainRenderer.material = buildingSO.PlacementMaterial;
    }

    private IEnumerator DoBuildUnits()
    {
        while (buildingQueue.Count > 0)
        {
            BuildingUnit = buildingQueue[0];
            CurrentQueueStartTime = Time.time;
            OnQueueUpdated?.Invoke(buildingQueue.ToArray());

            yield return new WaitForSeconds(BuildingUnit.BuildTime);

            Instantiate(BuildingUnit.Prefab, transform.position, Quaternion.identity);
            buildingQueue.RemoveAt(0);
        }

        OnQueueUpdated?.Invoke(buildingQueue.ToArray());
    }
}
