using UnityEngine;

[CreateAssetMenu(fileName = "Build Unit", menuName = "Buildings/Commands/Build Unit", order = 120)]
class BuildUnitCommand : BaseAction
{
    [field: SerializeField] public UnitSO Unit { get; private set; }

    public override bool CanHandle(CommandContext context)
    {
        return context.Commandable is BaseBuilding;
    }

    public override void Handle(CommandContext context)
    {
        BaseBuilding building = (BaseBuilding)context.Commandable;
        building.BuildUnit(Unit);
    }
}
