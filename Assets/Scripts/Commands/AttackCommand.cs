using UnityEngine;

[CreateAssetMenu(fileName = "Attack", menuName = "Units/Commands/Attack", order = 99)]
public class AttackCommand : BaseCommand
{
    public override bool CanHandle(CommandContext context)
    {
        return context.Commandable is IAttacker
            && context.Hit.collider != null 
            && context.Hit.collider.TryGetComponent(out IDamageable _);
    }

    public override void Handle(CommandContext context)
    {
        IAttacker attacker = context.Commandable as IAttacker;
        attacker.Attack(context.Hit.collider.GetComponent<IDamageable>());
    }

    public override bool IsLocked(CommandContext context) => false;
}