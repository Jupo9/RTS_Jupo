public struct UnitDeselectEvent : IEvent
{
    public ISelectable Unit { get; private set; }

    public UnitDeselectEvent(ISelectable unit)
    {
        Unit = unit;
    }
}

