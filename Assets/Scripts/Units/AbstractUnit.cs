using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

[RequireComponent(typeof(NavMeshAgent), typeof(BehaviorGraphAgent))]
public abstract class AbstractUnit : AbstractCommandable, IMoveable
{
    public float AgentRadius => agent.radius;
    private NavMeshAgent agent;
    protected BehaviorGraphAgent graphAgent; 

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        graphAgent = GetComponent<BehaviorGraphAgent>();
        graphAgent.SetVariableValue("Command", UnitCommands.Stop);
    }

    protected override void Start()
    {
        base.Start();

        CurrentHealth = UnitSO.Health;
        MaxHealth = UnitSO.Health;

        Bus<UnitSpawnEvent>.Raise(new UnitSpawnEvent(this));
    }

    public void MoveTo(Vector3 position)
    {
        // "TargetLocation" need to have the exact same name as the variable in the behavior graph, otherwise it won't work! 
        graphAgent.SetVariableValue("TargetLocation", position);
        graphAgent.SetVariableValue("Command", UnitCommands.Move);
    }

    public void Stop()
    {
        graphAgent.SetVariableValue("Command", UnitCommands.Stop);
    }

    private void OnDestroy()
    {
        Bus<UnitDeathEvent>.Raise(new UnitDeathEvent(this));
    }
}
