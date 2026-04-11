using UnityEngine;

public abstract class BaseAction : ScriptableObject, ICommand
{
    [field: SerializeField] public Sprite Icon { get; private set; }
    [field: SerializeField] public bool RequiresClickToActive { get; private set; } = true;
    [field: Range(0, 8)][field: SerializeField] public int Slot { get; private set; }

    public abstract bool CanHandle(CommandContext context);
    public abstract void Handle(CommandContext context);
}
