using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace StarJourney.Gameplay
{
    /// <summary>
    /// Washes a batch of props onto the beach with every wave of the two tides. The props surface as the water starts
    /// to rise and ride its edge to their resting points; the previous batch is swept out by the same wave. Every
    /// prop is moved from this component's fixed step, after the tides have moved, and reused through one pool per
    /// prefab.
    /// </summary>
    [DefaultExecutionOrder(10)]
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
        [Tooltip("Points on the beach where props come to rest. Each prop picks one at random.")]
        private Transform[] _spawnPoints;

        [SerializeField, Min(1)]
        [Tooltip("Props washed up by every wave.")]
        private int _batchSize = 10;

        [SerializeField]
        [Tooltip("The beach the props are washed onto.")]
        private Terrain _terrain;

        [SerializeField]
        [Tooltip("Horizontal direction from the beach to the sea; props come from and go back that way.")]
        private Vector3 _seaDirection = new Vector3(-0.984f, 0f, -0.177f);

        [SerializeField, Min(1)]
        [Tooltip("Instances kept per prefab: one incoming and one outgoing wave.")]
        private int _poolCapacity = 20;

        private readonly Dictionary<GameObject, ObjectPool<WashedProp>> _pools =
            new Dictionary<GameObject, ObjectPool<WashedProp>>();

        private List<SpawnedProp> _batch;
        private List<SpawnedProp> _outgoing;
        private Beach _beach;
        private Tide _activeTide;

        // The drag-back rolls have their own sequence, so the spawns draw exactly the numbers they always did.
        private System.Random _dragRandom;

        private void Awake()
        {
            if (_clearTide == null || _bloodTide == null || !_clearSpawns.IsValid || !_bloodSpawns.IsValid
                || !Validation.IsFilled(_spawnPoints) || _terrain == null
                || new Vector3(_seaDirection.x, 0f, _seaDirection.z) == Vector3.zero)
            {
                Debug.LogError($"{nameof(TideSpawner)} on '{name}' needs both tides, their spawn tables of props "
                    + "with a WashedProp, the spawn points, the beach terrain and a horizontal sea direction.", this);
                enabled = false;
                return;
            }

            _beach = new Beach(_terrain, _seaDirection);
            _batch = new List<SpawnedProp>(_batchSize);
            _outgoing = new List<SpawnedProp>(_poolCapacity);

            // Seeded from the global sequence without consuming from it.
            Random.State state = Random.state;
            _dragRandom = new System.Random(Random.Range(int.MinValue, int.MaxValue));
            Random.state = state;
        }

        private void OnEnable()
        {
            _clearTide.RiseStarted += SpawnClear;
            _clearTide.Crested += OnCrested;
            _bloodTide.RiseStarted += SpawnBlood;
            _bloodTide.Crested += OnCrested;
        }

        private void OnDisable()
        {
            _clearTide.RiseStarted -= SpawnClear;
            _clearTide.Crested -= OnCrested;
            _bloodTide.RiseStarted -= SpawnBlood;
            _bloodTide.Crested -= OnCrested;
        }

        private void OnDestroy()
        {
            foreach (ObjectPool<WashedProp> pool in _pools.Values)
            {
                pool.Clear();
            }
        }

        // Runs after both tides have moved in this step.
        private void FixedUpdate()
        {
            if (_activeTide == null)
            {
                return;
            }

            float surface = _activeTide.SurfaceHeight;
            TidePhase phase = _activeTide.Phase;
            float deltaTime = Time.fixedDeltaTime;
            Advance(_batch, surface, phase, deltaTime);
            Advance(_outgoing, surface, phase, deltaTime);
        }

        private static void Advance(List<SpawnedProp> props, float surface, TidePhase phase, float deltaTime)
        {
            for (int i = props.Count - 1; i >= 0; i--)
            {
                WashedProp prop = props[i].Prop;
                if (!prop.gameObject.activeSelf)
                {
                    // Consumed by the seal; it goes back to its pool with the rest of its wave.
                    continue;
                }

                prop.Advance(surface, phase, deltaTime);
                if (prop.IsDone)
                {
                    props[i].Pool.Release(prop);
                    props.RemoveAt(i);
                }
            }
        }

        private void SpawnClear()
        {
            Spawn(_clearTide, _clearSpawns);
        }

        private void SpawnBlood()
        {
            Spawn(_bloodTide, _bloodSpawns);
        }

        // Draws the random numbers in the order the original spawner did: kind, prefab, point, yaw.
        private void Spawn(Tide tide, SpawnTable table)
        {
            _activeTide = tide;
            SweepBatch();

            for (int i = 0; i < _batchSize; i++)
            {
                GameObject prefab = table.PickPrefab();
                Transform point = _spawnPoints[Random.Range(0, _spawnPoints.Length)];
                Quaternion rotation = Quaternion.Euler(0f, Random.Range(0, FullTurnDegrees), 0f);

                ObjectPool<WashedProp> pool = GetPool(prefab);
                WashedProp prop = pool.Get();
                prop.BeginWash(_beach, point.position, rotation, tide.LowSurfaceHeight, _dragRandom);
                prop.gameObject.SetActive(true);
                _batch.Add(new SpawnedProp(prop, pool));
            }
        }

        // The current batch becomes the outgoing one. Props the seal consumed go back to their pools at once; the
        // others wait for the rising water to cover them.
        private void SweepBatch()
        {
            for (int i = 0; i < _batch.Count; i++)
            {
                SpawnedProp spawned = _batch[i];
                if (spawned.Prop.gameObject.activeSelf)
                {
                    spawned.Prop.BeginSweepOut();
                    _outgoing.Add(spawned);
                }
                else
                {
                    spawned.Pool.Release(spawned.Prop);
                }
            }

            _batch.Clear();

            for (int i = _outgoing.Count - 1; i >= 0; i--)
            {
                SpawnedProp spawned = _outgoing[i];
                if (!spawned.Prop.gameObject.activeSelf)
                {
                    spawned.Pool.Release(spawned.Prop);
                    _outgoing.RemoveAt(i);
                }
            }
        }

        private void OnCrested()
        {
            float highSurface = _activeTide.HighSurfaceHeight;
            for (int i = 0; i < _batch.Count; i++)
            {
                _batch[i].Prop.OnCrest(highSurface);
            }

            for (int i = 0; i < _outgoing.Count; i++)
            {
                _outgoing[i].Prop.OnCrest(highSurface);
            }
        }

        private ObjectPool<WashedProp> GetPool(GameObject prefab)
        {
            if (!_pools.TryGetValue(prefab, out ObjectPool<WashedProp> pool))
            {
                pool = new ObjectPool<WashedProp>(() => CreateProp(prefab), null, ReturnToPool, DestroyProp, true,
                    _batchSize, _poolCapacity);
                _pools.Add(prefab, pool);
            }

            return pool;
        }

        // A new prop starts inactive, like a pooled one, so that it is placed before its body wakes up.
        private static WashedProp CreateProp(GameObject prefab)
        {
            GameObject instance = Instantiate(prefab);
            instance.SetActive(false);
            return instance.GetComponent<WashedProp>();
        }

        private static void ReturnToPool(WashedProp prop)
        {
            prop.gameObject.SetActive(false);
        }

        private static void DestroyProp(WashedProp prop)
        {
            // Idle props are already gone when the whole scene is being unloaded.
            if (prop != null)
            {
                Destroy(prop.gameObject);
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

            public bool IsValid => AreWashed(_pickups) && AreWashed(_hazards);

            public GameObject PickPrefab()
            {
                GameObject[] prefabs = Random.Range(0, SpawnOutcomes) < _pickupThirds ? _pickups : _hazards;
                return prefabs[Random.Range(0, prefabs.Length)];
            }

            private static bool AreWashed(GameObject[] prefabs)
            {
                if (!Validation.IsFilled(prefabs))
                {
                    return false;
                }

                for (int i = 0; i < prefabs.Length; i++)
                {
                    if (prefabs[i].GetComponent<WashedProp>() == null)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        private readonly struct SpawnedProp
        {
            public SpawnedProp(WashedProp prop, ObjectPool<WashedProp> pool)
            {
                Prop = prop;
                Pool = pool;
            }

            public WashedProp Prop { get; }

            public ObjectPool<WashedProp> Pool { get; }
        }
    }
}
