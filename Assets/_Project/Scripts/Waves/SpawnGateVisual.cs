using System.Collections;
using System;
using UnityEngine;

namespace ArenaShooter.Waves
{
    public class SpawnGateVisual : MonoBehaviour
    {
        [SerializeField] private Transform linkedSpawnPoint;
        [SerializeField] private Transform spawnAnchor;
        [SerializeField] private GameObject laserRoot;
        [SerializeField] private bool autoFindLaserRoot = true;
        [SerializeField, Min(0f)] private float preSpawnOpenDelay = 0.35f;
        [SerializeField, Min(0f)] private float closeDelayAfterSpawn = 0.6f;

        public Transform LinkedSpawnPoint => linkedSpawnPoint;
        public Transform SpawnAnchor => spawnAnchor != null ? spawnAnchor : linkedSpawnPoint;
        public float PreSpawnOpenDelay => preSpawnOpenDelay;

        public event Action OpenedForSpawn;
        public event Action ClosedAfterSpawn;

        private Coroutine restoreRoutine;

        private void Awake()
        {
            TryAutoAssignLaserRoot();
            SetLaserState(true);
        }

        private void OnValidate()
        {
            if (spawnAnchor == null && linkedSpawnPoint != null)
            {
                spawnAnchor = linkedSpawnPoint;
            }

            if (autoFindLaserRoot && laserRoot == null)
            {
                TryAutoAssignLaserRoot();
            }
        }

        public void OpenForSpawn()
        {
            TryAutoAssignLaserRoot();

            if (laserRoot == null)
            {
                return;
            }

            SetLaserState(false);
            OpenedForSpawn?.Invoke();

            if (restoreRoutine != null)
            {
                StopCoroutine(restoreRoutine);
            }

            restoreRoutine = StartCoroutine(RestoreRoutine());
        }

        private IEnumerator RestoreRoutine()
        {
            if (closeDelayAfterSpawn > 0f)
            {
                yield return new WaitForSeconds(closeDelayAfterSpawn);
            }

            SetLaserState(true);
            ClosedAfterSpawn?.Invoke();
            restoreRoutine = null;
        }

        private void TryAutoAssignLaserRoot()
        {
            if (!autoFindLaserRoot || laserRoot != null)
            {
                return;
            }

            Transform namedChild = transform.Find("lasers");

            if (namedChild != null)
            {
                laserRoot = namedChild.gameObject;
                return;
            }

            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);

                if (child.name.ToLowerInvariant().Contains("laser"))
                {
                    laserRoot = child.gameObject;
                    return;
                }
            }
        }

        private void SetLaserState(bool isEnabled)
        {
            if (laserRoot != null && laserRoot.activeSelf != isEnabled)
            {
                laserRoot.SetActive(isEnabled);
            }
        }
    }
}
