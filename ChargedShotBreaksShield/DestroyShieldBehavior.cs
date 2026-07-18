using UnityEngine;

namespace ChargedShotsBreakShields;

public class DestroyShieldBehavior : MonoBehaviour
{
    private float _destroyRevolverShieldRadius = 1.5f;

    private bool _fullyCharged;

    private static readonly Collider[] SharedBuffer = new Collider[32];

    private void Start()
    {
        _fullyCharged = FullyChargedShotTracker.GetDidPlayerShootFullyChargedShot();
        _destroyRevolverShieldRadius = GetComponent<Projectile>().overlapSphereRadiusAtInit;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_fullyCharged)
            return;

        var damageable = other.GetComponentInParent<IDamageable>();
        if (damageable != null)
        {
            if (other.GetComponentInParent<NmiProtection>())
            {
                damageable.TakeDamage(null, 1f, 0, Damage.DamageType.Melee, Vector3.up, other.transform.position, other);
            }
        }
    }

    private void FixedUpdate()
    {
        if (!_fullyCharged)
            return;

        var hits = Physics.OverlapSphereNonAlloc(transform.position, _destroyRevolverShieldRadius, SharedBuffer, -1);
        for (int i = 0; i < hits; i++)
        {
            if (SharedBuffer[i] == null)
                continue;
            var protection = SharedBuffer[i].GetComponentInParent<NmiProtection>();
            if (protection != null)
            {
                var destructible = protection.GetComponentInChildren<Destructible>();
                if (destructible != null && destructible.destructibleBy.HasFlag(Damage.DamageTypeFlag.Revolver))
                    protection.ManageDestruction(true);
            }
        }
    }
}