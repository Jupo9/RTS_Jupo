using UnityEngine;

public interface IAttacker
{
    public Transform Transform { get; }
    public void Attack(IDamageable damageable);
}
