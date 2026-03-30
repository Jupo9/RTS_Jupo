public struct ActionSelectedEvent : IEvent
{
    public BaseAction Action { get; }

    public ActionSelectedEvent(BaseAction action)
    {
        Action = action;
    }
}
