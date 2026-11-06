using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace StarJourney.Gameplay
{
    /// <summary>
    /// Washes a batch of props onto the beach at every crest of the two tides. Each batch replaces the previous one,
    /// and the props are reused through one pool per prefab.
    /// </summary>
    public sealed class TideSpawner : MonoBehaviour
    {
        // The odds of a pickup are set in thirds.
        private const int SpawnOutcomes = 3;

        // Props are spawned at a whole number of degrees of yaw, as they always were.
        private const int FullTurnDegrees = 360;

        [SerializeField]
        [Tooltip("The clear-water tide.")]
        private Tide _clearTide;

        [SerializeField]
        [Tooltip("What the clear-water tide washes up.")]
        private SpawnTable _clearSpawns = new SpawnTable();

        [SerializeField]
        [Tooltip("The tide of the sea of blood.")]
        private Tide _bloodTide;

        [SerializeField]
        [Tooltip("What the sea of blood washes up.")]
        private SpawnTable _bloodSpawns = new SpawnTable();

        [SerializeField]
        [Tooltip("Points on the beach where props appear. Each prop picks one at random.")]
        private Transform[] _spawnPoints;

        [SerializeField, Min(1)]
        [Tooltip("Props spawned at every crest.")]
        private int _batchSize = 10;

        private readonly Dictionary<GameObject, ObjectPool<GameObject>> _pools =
            new Dictionary<GameObject, ObjectPool<GameObject>>();

        private readonly List<SpawnedProp> _batch = new List<SpawnedProp>();

        private void Awake()
        {
            if (_clearTide == null || _bloodTide == null || !_clearSpawns.IsValid || !_bloodSpawns.IsValid
                || !IsFilled(_spawnPoints))
            {
                Debug.LogError($"{nameof(TideSpawner)} on '{name}' needs both tides, their spawn tables and the "
                    + "spawn points.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            _clearTide.Crested += SpawnClear;
            _bloodTide.Crested += SpawnBlood;
        }

        private void OnDisable()
        {
            _clearTide.Crested -= SpawnClear;
            _bloodTide.Crested -= SpawnBlood;
        }

        private void OnDestroy()
        {
            foreach (ObjectPool<GameObject> pool in _pools.Values)
            {
                pool.Clear();
            }
        }

        private void SpawnClear()
        {
            Spawn(_clearSpawns);
        }

        private void SpawnBlood()
        {
            Spawn(_bloodSpawns);
        }

        // Draws the random numbers in the order the original spawner did: kind, prefab, point, yaw.
        private void Spawn(SpawnTable table)
        {
            ReleaseBatch();

            for (int i = 0; i < _batchSize; i++)
            {
                GameObject prefab = table.PickPrefab();
                Transform point = _spawnPoints[Random.Range(0, _spawnPoints.Length)];
                Quaternion rotation = Quaternion.Euler(0f, Random.Range(0, FullTurnDegrees), 0f);

                ObjectPool<GameObject> pool = GetPool(prefab);
                GameObject prop = pool.Get();
                prop.transform.SetPositionAndRotation(point.position, rotation);
                prop.SetActive(true);
                _batch.Add(new SpawnedProp(prop, pool));
            }
        }

        // Props the seal consumed are already inactive; they go back to their pools here.
        private void ReleaseBatch()
        {
            for (int i = 0; i < _batch.Count; i++)
            {
                _batch[i].Pool.Release(_batch[i].Prop);
            }

            _batch.Clear();
        }

        private ObjectPool<GameObject> GetPool(GameObject prefab)
        {
            if (!_pools.TryGetValue(prefab, out ObjectPool<GameObject> pool))
            {
                pool = new ObjectPool<GameObject>(
                    () => Instantiate(prefab), null, ReturnToPool, DestroyProp, true, _batchSize, _batchSize);
                _pools.Add(prefab, pool);
            }

            return pool;
        }

        private static bool IsFilled<T>(T[] items)
            where T : UnityEngine.Object
        {
            if (items == null || items.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == null)
                {
                    return false;
                }
            }

            return true;
        }

        private static void ReturnToPool(GameObject prop)
        {
            prop.SetActive(false);
        }

        private static void DestroyProp(GameObject prop)
        {
            // Idle props are already gone when the whole scene is being unloaded.
            if (prop != null)
            {
                Destroy(prop);
            }
        }

        [Serializable]
        private sealed class SpawnTable
        {
            [SerializeField]
            [Tooltip("Props that give points. One is picked at random when a pickup is due.")]
            private GameObject[] _pickups;

            [SerializeField]
            [Tooltip("Props that hit the seal. One is picked at random when a hazard is due.")]
            private GameObject[] _hazards;

            [SerializeField, Range(0, SpawnOutcomes)]
            [Tooltip("Thirds of the spawns that are pickups; the rest are hazards.")]
            private int _pickupThirds = 2;

            public bool IsValid => IsFilled(_pickups) && IsFilled(_hazards);

            public GameObject PickPrefab()
            {
                GameObject[] prefabs = Random.Range(0, SpawnOutcomes) < _pickupThirds ? _pickups : _hazards;
                return prefabs[Random.Range(0, prefabs.Length)];
            }
        }

        private readonly struct SpawnedProp
        {
            public SpawnedProp(GameObject prop, ObjectPool<GameObject> pool)
            {
                Prop = prop;
                Pool = pool;
            }

            public GameObject Prop { get; }

            public ObjectPool<GameObject> Pool { get; }
        }
    }
}
