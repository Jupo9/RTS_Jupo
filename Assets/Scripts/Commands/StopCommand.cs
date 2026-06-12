using UnityEngine;

[CreateAssetMenu(fileName = "Stop Action", menuName = "Units/Commands/Stop", order = 101)]
public class StopCommand : BaseAction
{
    public override bool CanHandle(CommandContext context)
    {
        return context.Commandable is AbstractUnit;
    }

    public override void Handle(CommandContext context)
    {
        AbstractUnit unit = (AbstractUnit)context.Commandable;
        unit.Stop();
    }
}
