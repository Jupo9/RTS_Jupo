using UnityEngine;

[CreateAssetMenu(fileName = "Gather Action", menuName = "AI/Commands/Gather", order = 105)]
public class GatherCommands : BaseAction
{

    public override bool CanHandle(CommandContext context)
    {
        return context.Commandable is Unit_Worker 
            && context.Hit.collider != null 
            && context.Hit.collider.TryGetComponent(out GatherableSupply _);
    }

    public override void Handle(CommandContext context)
    {
        Unit_Worker worker = context.Commandable as Unit_Worker;
        worker.Gather(context.Hit.collider.GetComponent<GatherableSupply>());
    }
}
