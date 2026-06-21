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
    [field: SerializeField] public BuildingProgress Progress { get; private set; } = new (BuildingProgress.BuildingState.Destroyed, 0, 0);
    [field: SerializeField] public BuildingSO BuildingSO { get; private set; }

    [SerializeField] private Material primaryMaterial;
    [SerializeField] private NavMeshObstacle navMeshObstacle;

    public delegate void QueueUpdatedEvent(AbstractUnitSO[] unitsInQueue);
    public event QueueUpdatedEvent OnQueueUpdated;

    private IBuildingBuilder unitBuildingThis;
    private List<AbstractUnitSO> buildingQueue = new (MAX_QUEUE_SIZE);
    private const int MAX_QUEUE_SIZE = 5;


    private void Awake()
    {
        BuildingSO = UnitSO as BuildingSO;
    }

    protected override void Start()
    {
        base.Start();

        if (MainRenderer != null)
        { 
            MainRenderer.material = primaryMaterial;
        }

        Progress = new BuildingProgress(BuildingProgress.BuildingState.Completed, Progress.StartTime, 1);
        unitBuildingThis = null;
        Bus <UnitDeathEvent>.OnEvent -= HandleUnitDeath;
    }

    public void BuildUnit(AbstractUnitSO unit)
    {
        if (buildingQueue.Count == MAX_QUEUE_SIZE)
        {
            Debug.LogError("Building queue is full!");
            return;
        }


        Bus<SupplyEvent>.Raise(new SupplyEvent(-unit.Cost.Minerals, unit.Cost.MineralsSO));
        Bus<SupplyEvent>.Raise(new SupplyEvent(-unit.Cost.Gas, unit.Cost.GasSO));

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

        AbstractUnitSO unitSO = buildingQueue[index];
        Bus<SupplyEvent>.Raise(new SupplyEvent(unitSO.Cost.Minerals, unitSO.Cost.MineralsSO));
        Bus<SupplyEvent>.Raise(new SupplyEvent(unitSO.Cost.Gas, unitSO.Cost.GasSO));
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

    public void StartBuilding(IBuildingBuilder buildingBuilder)
    {
        unitBuildingThis = buildingBuilder;
        MainRenderer.material = BuildingSO.PlacementMaterial;

        Progress = new BuildingProgress(
            BuildingProgress.BuildingState.Building,
            Time.time - BuildingSO.BuildTime * Progress.Progress,
            Progress.Progress
            );

        Bus<UnitDeathEvent>.OnEvent -= HandleUnitDeath;
        Bus<UnitDeathEvent>.OnEvent += HandleUnitDeath;
    }

    private void HandleUnitDeath(UnitDeathEvent evt)
    {
        if (evt.Unit.TryGetComponent(out IBuildingBuilder buildingBuilder) && buildingBuilder == unitBuildingThis)
        {
            Progress = new BuildingProgress(
                BuildingProgress.BuildingState.Paused,
                Progress.StartTime,
                (Time.time - Progress.StartTime) / BuildingSO.BuildTime
                ); 
        }

        Bus<UnitDeathEvent>.OnEvent -= HandleUnitDeath;
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

    private void OnDestroy()
    {
        Bus<UnitDeathEvent>.OnEvent -= HandleUnitDeath;
    }
}
