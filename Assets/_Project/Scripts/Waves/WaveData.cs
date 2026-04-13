using UnityEngine;

namespace ArenaShooter.Waves
{
    [CreateAssetMenu(fileName = "WaveData", menuName = "Arena Shooter/Waves/Wave Data")]
    public class WaveData : ScriptableObject
    {
        [SerializeField] private string waveName = "Wave";
        [SerializeField] private bool bossWave;
        [SerializeField, Min(0.01f)] private float timeBetweenSpawns = 0.8f;
        [SerializeField, Min(0f)] private float timeBeforeNextWave = 3f;
        [SerializeField] private WaveSpawnEntry[] spawnEntries;

        public string WaveName => waveName;
        public bool BossWave => bossWave;
        public float TimeBetweenSpawns => timeBetweenSpawns;
        public float TimeBeforeNextWave => timeBeforeNextWave;
        public WaveSpawnEntry[] SpawnEntries => spawnEntries;
    }

    [System.Serializable]
    public class WaveSpawnEntry
    {
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField, Min(1)] private int count = 1;

        public GameObject EnemyPrefab => enemyPrefab;
        public int Count => count;
    }
}
