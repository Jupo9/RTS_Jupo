using UnityEngine;

[CreateAssetMenu(fileName = "Gather Action", menuName = "Units/Commands/Gather", order = 105)]
public class GatherCommands : BaseCommand
{
    [SerializeField] private AbstractUnitSO commandPostSO;

    public override bool CanHandle(CommandContext context)
    {
        return context.Commandable is Unit_Worker
            && context.Hit.collider != null
            && isGatherableSupplyOrCommandPost(context.Hit.collider);
    }

    public override void Handle(CommandContext context)
    {
        Unit_Worker worker = context.Commandable as Unit_Worker;
        if (context.Hit.collider.TryGetComponent(out GatherableSupply supply))
        {
            worker.Gather(supply);
        }
        else if (IsCommandPost(context.Hit.collider) && worker.HasSupplies)
        {
            worker.ReturnSupplies(context.Hit.collider.gameObject);
        }
        else
        {
            worker.MoveTo(context.Hit.collider.gameObject.transform.position);
        }
    }

    public override bool IsLocked(CommandContext context) => false;

    private bool isGatherableSupplyOrCommandPost(Collider collider) => collider.TryGetComponent(out GatherableSupply _) || IsCommandPost(collider);
    private bool IsCommandPost(Collider collider) => collider.TryGetComponent(out BaseBuilding building) && building.UnitSO.Equals(commandPostSO);
}
