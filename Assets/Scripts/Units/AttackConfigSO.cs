using UnityEngine;

[CreateAssetMenu(fileName = "Attack Config", menuName = "Units/Attack Config", order = 7)]
public class AttackConfigSO : ScriptableObject
{
    [field: SerializeField] public float AttackRange { get; private set; } = 1.5f;
    [field: SerializeField] public float AttackDelay { get; private set; } = 1;
    [field: SerializeField] public int Damage { get; private set; } = 5;
}
