using System;
using System.Collections;
using ArenaShooter.Combat;
using UnityEngine;

namespace ArenaShooter.Waves
{
    public class WaveDirector : MonoBehaviour
    {
        [Header("Authored Waves")]
        [SerializeField] private WaveData[] authoredWaves;

        [Header("Enemies")]
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private GameObject[] enemyPrefabs;
        [SerializeField] private GameObject bossPrefab;
        [SerializeField, Min(1)] private int bossWaveNumber = 5;

        [Header("Spawning")]
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private Transform bossSpawnPoint;
        [SerializeField, Min(1)] private int startingEnemyCount = 3;
        [SerializeField, Min(0)] private int enemiesAddedPerWave = 2;
        [SerializeField, Min(0.1f)] private float timeBetweenSpawns = 0.6f;
        [SerializeField, Min(0f)] private float timeBetweenWaves = 3f;
        [SerializeField, Min(0f)] private float firstWaveStartDelay = 2f;
        [SerializeField, Min(0f)] private float minimumSpawnDistanceFromPlayer = 8f;
        [SerializeField] private bool startWaveOnPlay = true;

        public event Action<int> WaveStarted;
        public event Action<int> WaveCompleted;
        public event Action<int> AliveEnemyCountChanged;
        public event Action BossWaveStarted;
        public event Action RunCompleted;

        public int CurrentWave { get; private set; }
        public int AliveEnemyCount { get; private set; }
        public bool IsSpawning { get; private set; }
        public bool IsBossWave { get; private set; }
        public bool AutoStartNextWave { get; set; } = true;
        public WaveData CurrentWaveData => currentWaveData;
        public string CurrentWaveName => currentWaveData != null && !string.IsNullOrWhiteSpace(currentWaveData.WaveName) ? currentWaveData.WaveName : string.Empty;

        private Coroutine spawnRoutine;
        private bool runCompleted;
        private Transform playerTransform;
        private WaveData currentWaveData;
        private SpawnGateVisual[] spawnGateVisuals;

        private void Start()
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            playerTransform = playerObject != null ? playerObject.transform : null;
            spawnGateVisuals = FindObjectsByType<SpawnGateVisual>(FindObjectsSortMode.None);

            AliveEnemyCountChanged?.Invoke(AliveEnemyCount);

            if (startWaveOnPlay)
            {
                if (firstWaveStartDelay > 0f)
                {
                    StartCoroutine(StartFirstWaveWithDelay());
                }
                else
                {
                    StartNextWave();
                }
            }
        }

        public void StartNextWave()
        {
            if (IsSpawning || runCompleted)
            {
                return;
            }

            if (HasAuthoredWaves() && CurrentWave >= authoredWaves.Length)
            {
                return;
            }

            CurrentWave++;
            currentWaveData = GetCurrentWaveData();
            IsBossWave = currentWaveData != null ? currentWaveData.BossWave : CurrentWave >= bossWaveNumber;

            if (IsBossWave)
            {
                BossWaveStarted?.Invoke();
            }

            WaveStarted?.Invoke(CurrentWave);

            if (currentWaveData != null)
            {
                spawnRoutine = StartCoroutine(SpawnAuthoredWaveRoutine(currentWaveData));
                return;
            }

            if (IsBossWave)
            {
                spawnRoutine = StartCoroutine(SpawnBossRoutine());
                return;
            }

            int enemyCount = startingEnemyCount + (CurrentWave - 1) * enemiesAddedPerWave;
            spawnRoutine = StartCoroutine(SpawnWaveRoutine(enemyCount));
        }

        public string GetWaveHudLabel(int waveNumber)
        {
            if (waveNumber <= 0)
            {
                return "Wave: -";
            }

            if (IsBossWave)
            {
                return "Wave: BOSS";
            }

            string waveName = CurrentWaveName;
            return string.IsNullOrWhiteSpace(waveName) ? $"Wave: {waveNumber}" : $"Wave: {waveNumber} - {waveName}";
        }

        public string GetWaveBannerLabel(int waveNumber)
        {
            if (waveNumber <= 0)
            {
                return string.Empty;
            }

            if (IsBossWave)
            {
                return "BOSS WAVE";
            }

            string waveName = CurrentWaveName;
            return string.IsNullOrWhiteSpace(waveName) ? $"WAVE {waveNumber}" : $"WAVE {waveNumber}: {waveName.ToUpperInvariant()}";
        }

        private IEnumerator StartFirstWaveWithDelay()
        {
            yield return new WaitForSeconds(firstWaveStartDelay);
            StartNextWave();
        }

        private IEnumerator SpawnAuthoredWaveRoutine(WaveData waveData)
        {
            IsSpawning = true;

            if (waveData != null && waveData.SpawnEntries != null)
            {
                float spawnDelay = waveData.TimeBetweenSpawns > 0f ? waveData.TimeBetweenSpawns : timeBetweenSpawns;

                for (int i = 0; i < waveData.SpawnEntries.Length; i++)
                {
                    WaveSpawnEntry entry = waveData.SpawnEntries[i];

                    if (entry == null || entry.EnemyPrefab == null || entry.Count <= 0)
                    {
                        continue;
                    }

                    for (int j = 0; j < entry.Count; j++)
                    {
                        Transform spawnPoint = entry.EnemyPrefab == bossPrefab && bossSpawnPoint != null ? bossSpawnPoint : GetSpawnPoint();
                        yield return SpawnEnemyAtPoint(entry.EnemyPrefab, spawnPoint);

                        bool isLastSpawn = i == waveData.SpawnEntries.Length - 1 && j == entry.Count - 1;

                        if (!isLastSpawn)
                        {
                            yield return new WaitForSeconds(spawnDelay);
                        }
                    }
                }
            }

            IsSpawning = false;
            spawnRoutine = null;

            if (AliveEnemyCount == 0)
            {
                StartCoroutine(CompleteWaveRoutine());
            }
        }

        private IEnumerator SpawnWaveRoutine(int enemyCount)
        {
            IsSpawning = true;

            for (int i = 0; i < enemyCount; i++)
            {
                GameObject prefabToSpawn = GetEnemyPrefab();

                if (prefabToSpawn == null)
                {
                    Debug.LogError("WaveDirector has no enemy prefab assigned.", this);
                    continue;
                }

                Transform spawnPoint = GetSpawnPoint();
                yield return SpawnEnemyAtPoint(prefabToSpawn, spawnPoint);

                if (i < enemyCount - 1)
                {
                    yield return new WaitForSeconds(timeBetweenSpawns);
                }
            }

            IsSpawning = false;
            spawnRoutine = null;

            if (AliveEnemyCount == 0)
            {
                StartCoroutine(CompleteWaveRoutine());
            }
        }

        private IEnumerator SpawnBossRoutine()
        {
            IsSpawning = true;

            if (bossPrefab == null)
            {
                Debug.LogError("WaveDirector has no boss prefab assigned.", this);
                IsSpawning = false;
                spawnRoutine = null;
                yield break;
            }

            Transform spawnPoint = bossSpawnPoint != null ? bossSpawnPoint : GetSpawnPoint();
            yield return SpawnEnemyAtPoint(bossPrefab, spawnPoint);

            IsSpawning = false;
            spawnRoutine = null;
        }

        private IEnumerator SpawnEnemyAtPoint(GameObject prefab, Transform spawnPoint)
        {
            if (prefab == null)
            {
                yield break;
            }

            SpawnGateVisual gateVisual = GetSpawnGateVisual(spawnPoint);

            if (gateVisual != null)
            {
                gateVisual.OpenForSpawn();

                if (gateVisual.PreSpawnOpenDelay > 0f)
                {
                    yield return new WaitForSeconds(gateVisual.PreSpawnOpenDelay);
                }
            }

            Transform spawnTransform = gateVisual != null && gateVisual.SpawnAnchor != null
                ? gateVisual.SpawnAnchor
                : spawnPoint;

            Vector3 position = spawnTransform != null ? spawnTransform.position : transform.position;
            Quaternion rotation = spawnTransform != null ? spawnTransform.rotation : Quaternion.identity;
            SpawnEnemyInstance(prefab, position, rotation);
        }

        private void SpawnEnemyInstance(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            GameObject enemyObject = Instantiate(prefab, position, rotation);

            if (!enemyObject.TryGetComponent(out Health enemyHealth))
            {
                Debug.LogError("Spawned enemy prefab must have a Health component.", enemyObject);
                Destroy(enemyObject);
                return;
            }

            if (!enemyObject.TryGetComponent(out WaveEnemy waveEnemy))
            {
                waveEnemy = enemyObject.AddComponent<WaveEnemy>();
            }

            AliveEnemyCount++;
            AliveEnemyCountChanged?.Invoke(AliveEnemyCount);
            waveEnemy.Died += HandleWaveEnemyDied;
        }

        private GameObject GetEnemyPrefab()
        {
            if (enemyPrefabs != null && enemyPrefabs.Length > 0)
            {
                int unlockedCount = Mathf.Min(CurrentWave, enemyPrefabs.Length);
                int validCount = 0;
                int firstValidIndex = -1;

                for (int i = 0; i < unlockedCount; i++)
                {
                    if (enemyPrefabs[i] == null)
                    {
                        continue;
                    }

                    if (firstValidIndex < 0)
                    {
                        firstValidIndex = i;
                    }

                    validCount++;
                }

                if (validCount > 0)
                {
                    int pick = UnityEngine.Random.Range(0, validCount);

                    for (int i = 0; i < unlockedCount; i++)
                    {
                        if (enemyPrefabs[i] == null)
                        {
                            continue;
                        }

                        if (pick == 0)
                        {
                            return enemyPrefabs[i];
                        }

                        pick--;
                    }

                    return enemyPrefabs[firstValidIndex];
                }
            }

            return enemyPrefab;
        }

        private Transform GetSpawnPoint()
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                return null;
            }

            if (playerTransform == null || minimumSpawnDistanceFromPlayer <= 0f)
            {
                return spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
            }

            Transform farthestPoint = spawnPoints[0];
            float farthestDistance = -1f;
            int validCount = 0;

            for (int i = 0; i < spawnPoints.Length; i++)
            {
                Transform point = spawnPoints[i];

                if (point == null)
                {
                    continue;
                }

                float distance = Vector3.Distance(point.position, playerTransform.position);

                if (distance > farthestDistance)
                {
                    farthestDistance = distance;
                    farthestPoint = point;
                }

                if (distance >= minimumSpawnDistanceFromPlayer)
                {
                    validCount++;
                }
            }

            if (validCount == 0)
            {
                return farthestPoint;
            }

            int pick = UnityEngine.Random.Range(0, validCount);

            for (int i = 0; i < spawnPoints.Length; i++)
            {
                Transform point = spawnPoints[i];

                if (point == null)
                {
                    continue;
                }

                float distance = Vector3.Distance(point.position, playerTransform.position);

                if (distance < minimumSpawnDistanceFromPlayer)
                {
                    continue;
                }

                if (pick == 0)
                {
                    return point;
                }

                pick--;
            }

            return farthestPoint;
        }

        private SpawnGateVisual GetSpawnGateVisual(Transform spawnPoint)
        {
            if (spawnPoint == null)
            {
                return null;
            }

            if (spawnGateVisuals == null || spawnGateVisuals.Length == 0)
            {
                spawnGateVisuals = FindObjectsByType<SpawnGateVisual>(FindObjectsSortMode.None);
            }

            for (int i = 0; i < spawnGateVisuals.Length; i++)
            {
                SpawnGateVisual gateVisual = spawnGateVisuals[i];

                if (gateVisual != null && gateVisual.LinkedSpawnPoint == spawnPoint)
                {
                    return gateVisual;
                }
            }

            return null;
        }

        private void HandleWaveEnemyDied(WaveEnemy waveEnemy)
        {
            waveEnemy.Died -= HandleWaveEnemyDied;

            AliveEnemyCount = Mathf.Max(0, AliveEnemyCount - 1);
            AliveEnemyCountChanged?.Invoke(AliveEnemyCount);

            if (AliveEnemyCount == 0 && !IsSpawning)
            {
                if (ShouldCompleteRun())
                {
                    runCompleted = true;
                    RunCompleted?.Invoke();
                    return;
                }

                StartCoroutine(CompleteWaveRoutine());
            }
        }

        private IEnumerator CompleteWaveRoutine()
        {
            int completedWave = CurrentWave;
            float delay = currentWaveData != null ? currentWaveData.TimeBeforeNextWave : timeBetweenWaves;
            WaveCompleted?.Invoke(completedWave);

            yield return new WaitForSeconds(delay);

            if (AutoStartNextWave)
            {
                StartNextWave();
            }
        }

        private bool ShouldCompleteRun()
        {
            if (IsBossWave)
            {
                return true;
            }

            return HasAuthoredWaves() && CurrentWave >= authoredWaves.Length;
        }

        private WaveData GetCurrentWaveData()
        {
            if (!HasAuthoredWaves())
            {
                return null;
            }

            int index = CurrentWave - 1;
            return index >= 0 && index < authoredWaves.Length ? authoredWaves[index] : null;
        }

        private bool HasAuthoredWaves()
        {
            return authoredWaves != null && authoredWaves.Length > 0;
        }

        private void OnDisable()
        {
            if (spawnRoutine != null)
            {
                StopCoroutine(spawnRoutine);
                spawnRoutine = null;
            }

            IsSpawning = false;
        }
    }
}
