using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public abstract class AbstractCommandable : MonoBehaviour, ISelectable
{
    [field: SerializeField] public int CurrentHealth { get; private set; }
    [field: SerializeField] public int MaxHealth { get; private set; }
    [field: SerializeField] public BaseCommand[] AvailableCommands { get; private set; }
    [field: SerializeField] public AbstractUnitSO UnitSO { get; private set; }

    [SerializeField] private DecalProjector decalProjector;

    private BaseCommand[] initialCommands;

    protected virtual void Start()
    {
        CurrentHealth = UnitSO.Health;
        MaxHealth = UnitSO.Health;

        initialCommands = AvailableCommands;
    }

    public void Select()
    {
        if (decalProjector != null)
        {
            decalProjector.gameObject.SetActive(true);
        }

        Bus<UnitSelectedEvent>.Raise(new UnitSelectedEvent(this));
    }

    public void Deselect()
    {
        if (decalProjector != null)
        {
            decalProjector.gameObject.SetActive(false);
        }

        SetCommandOverrides(null);

        Bus<UnitDeselectEvent>.Raise(new UnitDeselectEvent(this));
    }

    public void SetCommandOverrides(BaseCommand[] commands)
    {
        if (commands == null || commands.Length == 0)
        {
            AvailableCommands = initialCommands;
        }
        else
        {
            AvailableCommands = commands;
        }

        Bus<UnitSelectedEvent>.Raise(new UnitSelectedEvent(this));
    }
}
