using UnityEngine;

[CreateAssetMenu(fileName = "Attack", menuName = "Units/Commands/Attack", order = 99)]
public class AttackCommand : BaseCommand
{
    public override bool CanHandle(CommandContext context)
    {
        return context.Commandable is IAttacker && context.Hit.collider != null;
    }

    public override void Handle(CommandContext context)
    {
        IAttacker attacker = context.Commandable as IAttacker;

        if (context.Hit.collider.TryGetComponent(out IDamageable damageable))
        {
            attacker.Attack(damageable);
        }
        else
        {
            attacker.Attack(context.Hit.point);
        }
    }

    public override bool IsLocked(CommandContext context) => false;
}