using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class CommandContext
{
    public AbstractCommandable Commandable { get; private set; }
    public RaycastHit Hit { get; private set; }
    public int UnitIndex { get; private set; }
    public MouseButton Button { get; private set; }

    public CommandContext(
        AbstractCommandable commandable,
        RaycastHit hit,
        int unitIndex = 0,
        MouseButton mouseButton = MouseButton.Left)
    {
        Commandable = commandable;
        Hit = hit;
        UnitIndex = unitIndex;
        Button = mouseButton;
    }
}
