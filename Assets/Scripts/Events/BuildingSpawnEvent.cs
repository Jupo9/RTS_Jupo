public struct BuildingSpawnEvent : IEvent
{
    public BaseBuilding Building { get; private set; }

    public BuildingSpawnEvent(BaseBuilding building)
    {
        Building = building;
    }
}
