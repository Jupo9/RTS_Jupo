using System.Linq;
using UnityEngine;

public abstract class BaseCommand : ScriptableObject, ICommand
{
    [field: SerializeField] public string Name { get; private set; } = "Command";
    [field: SerializeField] public Sprite Icon { get; private set; }
    [field: SerializeField] public bool RequiresClickToActive { get; private set; } = true;
    [field: Range(0, 8)][field: SerializeField] public int Slot { get; private set; }
    [field: SerializeField] public GameObject GhostPrefab { get; private set; }
    [field: SerializeField] public BuildingRestrictionSO[] Restrictions { get; private set; }

    public abstract bool CanHandle(CommandContext context);
    public abstract void Handle(CommandContext context);
    public abstract bool IsLocked(CommandContext context);

    public bool AllRestrictionsPass(Vector3 point) =>
    Restrictions.Length == 0 || Restrictions.All(restriction => restriction.CanPlace(point));
}
