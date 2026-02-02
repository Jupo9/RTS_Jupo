using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(NavMeshAgent))]
public class Unit_Worker : MonoBehaviour, ISelectable, IMoveable
{
    [SerializeField] private DecalProjector decalProjector;

    private NavMeshAgent agent;

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

    public void MoveTo(Vector3 position)
    {
        agent.SetDestination(position);
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }
}
