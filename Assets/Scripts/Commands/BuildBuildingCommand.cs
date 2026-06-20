using UnityEngine;

[CreateAssetMenu(fileName = "Build Building", menuName = "Units/Commands/Build Building")]
public class BuildBuildingCommand : BaseAction
{
    [field: SerializeField] public BuildingSO Building { get; private set; }

    public override bool CanHandle(CommandContext context)
    {

        if (context.Commandable is not IBuildingBuilder) return false;

        if (context.Hit.collider != null)
        {
            return context.Hit.collider.TryGetComponent(out BaseBuilding building)
                && Building == building.BuildingSO
                && (building.Progress.State == BuildingProgress.BuildingState.Paused
                    || building.Progress.State == BuildingProgress.BuildingState.Destroyed
                );
        }

        return true;
    }

    public override void Handle(CommandContext context)
    {
        IBuildingBuilder builder = (IBuildingBuilder)context.Commandable;

        if (context.Hit.collider != null && context.Hit.collider.TryGetComponent(out BaseBuilding building))
        {
            builder.ResumeBuilding(building);
        }
        else
        {
            builder.Build(Building, context.Hit.point);
        }
    }
}
