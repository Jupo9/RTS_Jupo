using System.Collections;
using Unity.Behavior;
using UnityEngine;

public class Grenadier : Unit_Military
{
    [SerializeField] private GameObject grenade;
    [SerializeField] private ParticleSystem explosionParticles;

    private Transform grenadeParent;
    private Vector3 defaultGrenadePosition;

    protected override void Awake()
    {
        base.Awake();

        if (grenade == null || explosionParticles == null)
        {
            Debug.LogError($"Grenadier {name} is missing a grenade or explosion particles! They will not work!");
            return;
        }

        defaultGrenadePosition = grenade.transform.localPosition;
        grenadeParent = grenade.transform.parent;
    }

    // Animation Event
    public void OnThrowGrenade()
    {
        grenade.transform.SetParent(null);
        Vector3 startPosition = grenade.transform.position;
        Vector3 endPosition = grenade.transform.position + grenade.transform.forward * 3;
        IDamageable damageable = null;

        if (graphAgent.GetVariable("TargetGameObject", out BlackboardVariable<GameObject> targetVariable)
            && targetVariable != null)
        {
            endPosition = targetVariable.Value.transform.position + Vector3.up;
            damageable = targetVariable.Value.GetComponent<IDamageable>();
        }
        else if (graphAgent.GetVariable("TargetLocation", out BlackboardVariable<Vector3> targetLocationVariable))
        {
            endPosition = targetLocationVariable;
        }

        StartCoroutine(AnimateGrenadeMovement(startPosition, endPosition, damageable));
    }

    private IEnumerator AnimateGrenadeMovement(Vector3 startPosition, Vector3 endPosition, IDamageable damageable)
    {
        float time = 0;
        const float speed = 2;

        while (time < 1)
        {
            grenade.transform.position = Vector3.Lerp(startPosition, endPosition, time);
            time += Time.deltaTime * speed;
            yield return null;
        }

        damageable?.TakeDamage(unitSO.AttackConfig.Damage);

        explosionParticles.transform.SetParent(null);
        explosionParticles.transform.position = endPosition;
        explosionParticles.Play();

        grenade.transform.SetParent(grenadeParent);
        grenade.transform.localPosition = defaultGrenadePosition;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Destroy(grenade);
        Destroy(explosionParticles.gameObject);
    }
}