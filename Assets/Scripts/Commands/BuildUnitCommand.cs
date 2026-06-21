using UnityEngine;

[CreateAssetMenu(fileName = "Build Unit", menuName = "Buildings/Commands/Build Unit", order = 120)]
class BuildUnitCommand : BaseCommand
{
    [field: SerializeField] public AbstractUnitSO Unit { get; private set; }

    public override bool CanHandle(CommandContext context)
    {
        return context.Commandable is BaseBuilding && HasEnoughSupplies();
    }

    public override void Handle(CommandContext context)
    {
        if (!HasEnoughSupplies())
        {
            return;
        }

        BaseBuilding building = (BaseBuilding)context.Commandable;
        building.BuildUnit(Unit);
    }

    public override bool IsLocked(CommandContext context) => !HasEnoughSupplies();

    private bool HasEnoughSupplies() => Unit.Cost.Minerals <= Supplies.Minerals && Unit.Cost.Gas <= Supplies.Gas;

}
