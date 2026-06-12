using UnityEngine;

[CreateAssetMenu(fileName = "Building", menuName = "Buildings/Building")]
public class BuildingSO : AbstractUnitSO
{
    [field:  SerializeField] public Material PlacementMaterial { get; private set; }
}
