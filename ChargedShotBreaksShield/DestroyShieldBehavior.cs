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

        var protection = other.GetComponentInParent<NmiProtection>();
        var field = other.GetComponentInParent<NmiField>();
        if (protection != null && protection.type == NmiProtection.Type.FieldForce)
        {
            protection.type = NmiProtection.Type.ShieldBasic;
        }
        else if (field != null && field.gameObject.name == "NmiField_FieldForceSpheric")
        {
            Destroy(field.gameObject);
        }

        var advance = other.GetComponentInParent<NmiAdvance>();
        if (advance != null)
        {
            advance.bouclier.displaycoef = 0;
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