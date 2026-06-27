using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public abstract class AbstractCommandable : MonoBehaviour, ISelectable, IDamageable
{
    [field: SerializeField] public bool IsSelected { get; protected set; }
    [field: SerializeField] public int CurrentHealth { get; protected set; }
    [field: SerializeField] public int MaxHealth { get; protected set; }
    [field: SerializeField] public BaseCommand[] AvailableCommands { get; private set; }
    [field: SerializeField] public AbstractUnitSO UnitSO { get; private set; }

    [SerializeField] protected DecalProjector decalProjector;

    public delegate void HealthUpdatedEvent(AbstractCommandable commandable, int lastHealth, int newHealth);
    public event HealthUpdatedEvent OnHealthUpdated;

    public Transform Transform => transform;

    private BaseCommand[] initialCommands;

    protected virtual void Start()
    {
        initialCommands = AvailableCommands;
    }

    public virtual void Select()
    {
        if (decalProjector != null)
        {
            decalProjector.gameObject.SetActive(true);
        }

        IsSelected = true;
        Bus<UnitSelectedEvent>.Raise(new UnitSelectedEvent(this));
    }

    public virtual void Deselect()
    {
        if (decalProjector != null)
        {
            decalProjector.gameObject.SetActive(false);
        }

        IsSelected = false;
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

        if (IsSelected)
        {
            Bus<UnitSelectedEvent>.Raise(new UnitSelectedEvent(this));
        }
    }

    public void TakeDamage(int damage)
    {
        int lastHealth = CurrentHealth;
        CurrentHealth = Mathf.Clamp(CurrentHealth - damage, 0, CurrentHealth);

        OnHealthUpdated?.Invoke(this, lastHealth, CurrentHealth);
        if (CurrentHealth == 0)
        {
            Die();
        }
    }

    public void Die()
    {
        Destroy(gameObject);
    }

    public void Heal(int amount)
    {
        int lastHealth = CurrentHealth;
        CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0, MaxHealth);
        OnHealthUpdated?.Invoke(this, lastHealth, CurrentHealth);
    }
}
