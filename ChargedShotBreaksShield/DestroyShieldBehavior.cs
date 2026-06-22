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
        Debug.Log("Destroy revolver shield radius: " + _destroyRevolverShieldRadius);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_fullyCharged)
            return;

        var protection = other.GetComponentInParent<NmiProtection>();
        if (protection != null)
        {
            protection.ManageDestruction(true);
        }
        var field = other.GetComponentInParent<NmiField>();
        if (field != null)
        {
            if (field.gameObject.name == "NmiField_FieldForceSpheric")
                Destroy(field.gameObject);
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
                protection.ManageDestruction(true);
            }
        }
    }
}