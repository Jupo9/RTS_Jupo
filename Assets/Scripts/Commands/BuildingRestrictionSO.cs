using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "Building Restriction", menuName = "Buildings/Restriction", order = 7)]
public class BuildingRestrictionSO : ScriptableObject
{
    [field: SerializeField] public float Radius { get; private set; } = 1f;
    [field: SerializeField] public LayerMask LayerMask { get; private set; }
    [field: SerializeField] public OverlapStyles HitDetectionStyle { get; private set; } = OverlapStyles.Sphere;

    [field: SerializeField] public bool MustBeFullyOnNavMesh { get; private set; } = true;
    [field: SerializeField] public int NavMeshAgentTypeID { get; private set; }
    [field: SerializeField] public float NavMeshTolerance { get; private set; } = 0.1f;
    [field: SerializeField] public Vector3 Extents { get; private set; } = Vector3.one;

    private Collider[] hitColliders = new Collider[1];

    public bool CanPlace(Vector3 position)
    {

        int hits = HitDetectionStyle switch
        {
            OverlapStyles.Sphere => Physics.OverlapSphereNonAlloc(position, Radius, hitColliders, LayerMask),
            OverlapStyles.Box => Physics.OverlapBoxNonAlloc(position, Extents, hitColliders, Quaternion.identity, LayerMask)
        };

        if (MustBeFullyOnNavMesh)
        {
            NavMeshQueryFilter queryFilter = new() { areaMask = NavMesh.AllAreas, agentTypeID = NavMeshAgentTypeID };

            bool isOnNavMesh = IsFullyOnNavMesh(position, queryFilter);

            return hits == 0 && isOnNavMesh;
        }

        return hits == 0;
    }

    private bool IsFullyOnNavMesh(Vector3 position, NavMeshQueryFilter queryFilter)
    {
        bool isOnNavMesh = NavMesh.SamplePosition(
                            position + new Vector3(Extents.x, 0, Extents.z),
                            out NavMeshHit _, NavMeshTolerance, queryFilter
                            );
        isOnNavMesh = isOnNavMesh && NavMesh.SamplePosition(
                            position + new Vector3(Extents.x, 0, -Extents.z),
                            out NavMeshHit _, NavMeshTolerance, queryFilter
                            );
        isOnNavMesh = isOnNavMesh && NavMesh.SamplePosition(
                            position + new Vector3(-Extents.x, 0, -Extents.z),
                            out NavMeshHit _, NavMeshTolerance, queryFilter
                            );
        isOnNavMesh = isOnNavMesh && NavMesh.SamplePosition(
                            position + new Vector3(-Extents.x, 0, Extents.z),
                            out NavMeshHit _, NavMeshTolerance, queryFilter
                            );

        return isOnNavMesh;
    }

    public enum OverlapStyles
    {
        Sphere,
        Box
    }
}
