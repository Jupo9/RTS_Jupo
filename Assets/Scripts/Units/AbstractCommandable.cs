using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public abstract class AbstractCommandable : MonoBehaviour, ISelectable
{
    [field: SerializeField] public int CurrentHealth { get; private set; }
    [field: SerializeField] public int MaxHealth { get; private set; }

    [SerializeField] private DecalProjector decalProjector;
    [SerializeField] private UnitSO unitSO;

    protected virtual void Start()
    {
        CurrentHealth = unitSO.Health;
        MaxHealth = unitSO.Health;
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
