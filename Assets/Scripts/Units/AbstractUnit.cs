using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

[RequireComponent(typeof(NavMeshAgent), typeof(BehaviorGraphAgent))]
public abstract class AbstractUnit : AbstractCommandable, IMoveable, IAttacker
{
    public float AgentRadius => agent.radius;
    [SerializeField] private DamageableSensor DamageableSensor;
    private NavMeshAgent agent;
    protected BehaviorGraphAgent graphAgent;
    protected UnitSO unitSO;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        graphAgent = GetComponent<BehaviorGraphAgent>();

        unitSO = UnitSO as UnitSO;

        graphAgent.SetVariableValue("Command", UnitCommands.Stop);
        graphAgent.SetVariableValue("AttackConfig", unitSO.AttackConfig);
    }

    protected override void Start()
    {
        base.Start();

        CurrentHealth = UnitSO.Health;
        MaxHealth = UnitSO.Health;

        Bus<UnitSpawnEvent>.Raise(new UnitSpawnEvent(this));


        if (DamageableSensor != null)
        {

            DamageableSensor.OnUnitEnter += HandleUnitEnterOrExit;
            DamageableSensor.OnUnitExit += HandleUnitEnterOrExit;
            DamageableSensor.SetupFrom(unitSO.AttackConfig);
        }
    }

    public void MoveTo(Vector3 position)
    {
        SetCommandOverrides(null);
        // "TargetLocation" need to have the exact same name as the variable in the behavior graph, otherwise it won't work! 
        graphAgent.SetVariableValue("TargetLocation", position);
        graphAgent.SetVariableValue("Command", UnitCommands.Move);
    }

    public void Stop()
    {
        SetCommandOverrides(null);
        graphAgent.SetVariableValue("Command", UnitCommands.Stop);
    }
    public void Attack(IDamageable damageable)
    {
        Debug.Log($"{name} should attack {damageable.Transform.name}");
    }


    private void HandleUnitEnterOrExit(IDamageable damageable)
    {

        List<GameObject> nearbyEnemies = DamageableSensor.Damageables.ConvertAll(damageable => damageable.Transform.gameObject);
        nearbyEnemies.Sort(new ClosestGameObjectComparer(transform.position));

        graphAgent.SetVariableValue("NearbyEnemies", nearbyEnemies);
    }

    private void OnDestroy()
    {
        Bus<UnitDeathEvent>.Raise(new UnitDeathEvent(this));
    }
}
