using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

[CreateAssetMenu(fileName = "Build Building", menuName = "Units/Commands/Build Building")]
public class BuildBuildingCommand : BaseCommand
{
    [field: SerializeField] public BuildingSO Building { get; private set; }

    public override bool CanHandle(CommandContext context)
    {

        if (context.Commandable is not IBuildingBuilder builder || builder.IsBuilding)
        {
            return false;
        }

        if (context.Hit.collider != null && context.Button == MouseButton.Right)
        {
            return context.Hit.collider.TryGetComponent(out BaseBuilding building)
                && Building == building.BuildingSO
                && (building.Progress.State == BuildingProgress.BuildingState.Paused
                    || building.Progress.State == BuildingProgress.BuildingState.Destroyed
                );
        }

        return HasEnoughSupplies() && AllRestrictionsPass(context.Hit.point);
    }

    public override void Handle(CommandContext context)
    {
        IBuildingBuilder builder = (IBuildingBuilder)context.Commandable;

        if (context.Hit.collider != null && context.Hit.collider.TryGetComponent(out BaseBuilding building))
        {
            builder.ResumeBuilding(building);
        }
        else if (HasEnoughSupplies() && AllRestrictionsPass(context.Hit.point))
        {
            builder.Build(Building, context.Hit.point);
        }
    }

    public override bool IsLocked(CommandContext context) => !HasEnoughSupplies();

    private bool HasEnoughSupplies() => Building.Cost.Minerals <= Supplies.Minerals && Building.Cost.Gas <= Supplies.Gas;
}
