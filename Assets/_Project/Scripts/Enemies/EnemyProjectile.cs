using ArenaShooter.Combat;
using UnityEngine;

namespace ArenaShooter.Enemies
{
    public class EnemyProjectile : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float speed = 12f;
        [SerializeField, Min(1f)] private float damage = 10f;
        [SerializeField, Min(0.1f)] private float lifeTime = 4f;
        [SerializeField] private LayerMask hitMask = ~0;

        private Vector3 direction;
        private Transform ownerRoot;
        private float deathTime;
        private bool isInitialized;

        private void OnEnable()
        {
            deathTime = Time.time + lifeTime;
        }

        private void Update()
        {
            if (!isInitialized)
            {
                direction = transform.forward;
                isInitialized = true;
            }

            transform.position += direction * speed * Time.deltaTime;

            if (Time.time >= deathTime)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsInHitMask(other.gameObject.layer) || IsOwnerCollider(other))
            {
                return;
            }

            IDamageable damageable =
                other.GetComponent<IDamageable>() ??
                other.GetComponentInParent<IDamageable>();

            if (damageable is Health targetHealth)
            {
                UnityEngine.Object sourceObject = ownerRoot != null ? ownerRoot.gameObject : gameObject;
                string sourceLabel = ownerRoot != null ? $"{ownerRoot.name} projectile" : $"{name} projectile";
                targetHealth.TakeDamage(damage, sourceObject, sourceLabel);
            }

            Destroy(gameObject);
        }

        public void Launch(Vector3 launchDirection, float projectileDamage, Transform projectileOwner = null)
        {
            direction = launchDirection.sqrMagnitude > 0.001f ? launchDirection.normalized : transform.forward;
            damage = projectileDamage;
            ownerRoot = projectileOwner;
            isInitialized = true;
        }

        private bool IsOwnerCollider(Collider other)
        {
            return ownerRoot != null && (other.transform == ownerRoot || other.transform.IsChildOf(ownerRoot));
        }

        private bool IsInHitMask(int layer)
        {
            int layerBit = 1 << layer;
            return (hitMask.value & layerBit) != 0;
        }
    }
}
