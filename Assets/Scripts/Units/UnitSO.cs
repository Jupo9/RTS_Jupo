using UnityEngine;

[CreateAssetMenu(fileName = "Unit", menuName = "Units/Unit")]
public class UnitSO : AbstractUnitSO
{
    [field: SerializeField] public TransportConfigSO TransportConfig { get; private set; }
}

