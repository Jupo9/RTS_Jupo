using UnityEngine;

[CreateAssetMenu(fileName = "Cancel Building", menuName = "Units/Commands/Cancel Building")]
public class CancelBuildingCommand : BaseAction
{
    public override bool CanHandle(CommandContext context)
    {
        return context.Commandable is IBuildingBuilder;
    }

    public override void Handle(CommandContext context)
    {
        IBuildingBuilder buildingBuilder = context.Commandable as IBuildingBuilder;
        buildingBuilder.CancelBuilding();
    }
}
