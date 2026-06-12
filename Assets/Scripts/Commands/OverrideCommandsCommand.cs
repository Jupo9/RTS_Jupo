using UnityEngine;

[CreateAssetMenu(fileName = "Override Commands", menuName = "Units/Commands/Override Commands", order = 110)]
public class OverrideCommandsCommand : BaseAction
{
    [field: SerializeField] public BaseAction[] Commands { get; private set; }
    public override bool CanHandle(CommandContext context)
    {
        return context.Commandable != null;
    }

    public override void Handle(CommandContext context)
    {
        context.Commandable.SetCommandOverrides(Commands);
    }
}
