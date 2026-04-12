using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public abstract class AbstractCommandable : MonoBehaviour, ISelectable
{
    [field: SerializeField] public int CurrentHealth { get; private set; }
    [field: SerializeField] public int MaxHealth { get; private set; }
    [field: SerializeField] public BaseAction[] AvailableCommands { get; private set; }
    [field: SerializeField] public UnitSO UnitSO { get; private set; }

    [SerializeField] private DecalProjector decalProjector;

    protected virtual void Start()
    {
        CurrentHealth = UnitSO.Health;
        MaxHealth = UnitSO.Health;
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

        Bus<UnitDeselectEvent>.Raise(new UnitDeselectEvent(this));
    }
}
