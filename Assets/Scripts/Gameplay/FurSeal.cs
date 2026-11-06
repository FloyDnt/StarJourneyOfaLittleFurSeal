using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace StarJourney.Gameplay
{
    /// <summary>
    /// The player's fur seal: turns on the spot all the time, hops the way it faces on a click, collects pickups
    /// and dies on its third hazard.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class FurSeal : MonoBehaviour
    {
        private const int HopMouseButton = 0;
        private const int PointsPerPickup = 100;
        private const int HitsToDie = 3;
        private const float FullTurnDegrees = 360f;

        [SerializeField, Min(0f)]
        [Tooltip("Degrees the seal turns around its vertical axis every rendered frame.")]
        private float _turnStepDegrees = 2f;

        [SerializeField]
        [Tooltip("Child point that a hop moves the seal to.")]
        private Transform _hopTarget;

        [SerializeField, Min(0f)]
        [Tooltip("Longest distance in units that one hop covers. Longer than the way to the hop target, so a hop "
            + "always lands on it.")]
        private float _hopDistance = 2f;

        [SerializeField]
        [Tooltip("Source that plays the hop, pickup and hit sounds.")]
        private AudioSource _sfx;

        [SerializeField]
        [Tooltip("One of these plays at random on every hop.")]
        private AudioClip[] _hopSounds;

        [SerializeField]
        [Tooltip("One of these plays at random on every pickup.")]
        private AudioClip[] _pickupSounds;

        [SerializeField]
        [Tooltip("One of these plays at random on every hit.")]
        private AudioClip[] _hitSounds;

        [SerializeField]
        [Tooltip("Splash played on a pickup in the clear-water phase.")]
        private ParticleSystem _pickupSplash;

        [SerializeField]
        [Tooltip("Splash played on a pickup in the blood phase.")]
        private ParticleSystem _pickupSplashBlood;

        [SerializeField]
        [Tooltip("Splash played on a hit in the clear-water phase.")]
        private ParticleSystem _hitSplash;

        [SerializeField]
        [Tooltip("Splash played on a hit in the blood phase.")]
        private ParticleSystem _hitSplashBlood;

        private float _yaw;
        private Vector3 _pendingHop;
        private int _score;
        private int _hits;

        /// <summary>
        /// Raised after a pickup, with the new score.
        /// </summary>
        public event Action<int> ScoreChanged;

        /// <summary>
        /// Raised after a hit, with the number of hits taken so far.
        /// </summary>
        public event Action<int> HitsChanged;

        /// <summary>
        /// Raised on the hit that kills the seal, right before its GameObject is destroyed.
        /// </summary>
        public event Action Died;

        /// <summary>
        /// Points collected so far.
        /// </summary>
        public int Score => _score;

        /// <summary>
        /// Hazards that have hit the seal so far.
        /// </summary>
        public int Hits => _hits;

        /// <summary>
        /// Whether the sea of blood has begun. Selects the blood variants of the splashes.
        /// </summary>
        public bool IsBloodPhase { get; set; }

        private bool IsDead => _hits >= HitsToDie;

        /// <summary>
        /// Requests one hop the way the seal faces now and plays a hop sound. The hop is applied in the next
        /// physics step, together with any other hop requested before it.
        /// </summary>
        public void Hop()
        {
            // The offset is taken now because the seal keeps turning every frame until the physics step. The hop
            // target moves with the seal, so the offset does not depend on hops that are still pending.
            Vector3 position = transform.position;
            _pendingHop += Vector3.MoveTowards(position, _hopTarget.position, _hopDistance) - position;
            PlayRandom(_hopSounds);
        }

        private void Awake()
        {
            if (_hopTarget == null || _sfx == null || IsEmpty(_hopSounds) || IsEmpty(_pickupSounds)
                || IsEmpty(_hitSounds) || _pickupSplash == null || _pickupSplashBlood == null || _hitSplash == null
                || _hitSplashBlood == null)
            {
                Debug.LogError($"{nameof(FurSeal)} on '{name}' needs a hop target, a sound source, its sounds and "
                    + "its four splashes.", this);
                enabled = false;
            }
        }

        private void Update()
        {
            // Mouse clicks on UI buttons hop too, as they always did.
            if (Input.GetMouseButtonDown(HopMouseButton))
            {
                Hop();
            }

            // The turn is a fixed step per rendered frame, so its speed follows the frame rate as it always did.
            _yaw += _turnStepDegrees;
            if (_yaw > FullTurnDegrees)
            {
                _yaw -= FullTurnDegrees;
            }

            transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
        }

        private void FixedUpdate()
        {
            // A hop is a teleport of the Transform, as it always was, and the physics step picks it up together
            // with the turn made in Update. Writing Rigidbody.position instead drops the hop's Z part, because the
            // body's Z position is frozen.
            if (_pendingHop != Vector3.zero)
            {
                transform.position += _pendingHop;
                _pendingHop = Vector3.zero;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (IsDead)
            {
                return;
            }

            // The props have no Rigidbody, so this is the GameObject of the collider that was touched.
            GameObject other = collision.gameObject;
            if (other.TryGetComponent(out Pickup pickup))
            {
                Collect(pickup);
            }
            else if (other.TryGetComponent(out Hazard hazard))
            {
                TakeHit(hazard);
            }
        }

        private void Collect(Pickup pickup)
        {
            pickup.Consume();
            (IsBloodPhase ? _pickupSplashBlood : _pickupSplash).Play();
            PlayRandom(_pickupSounds);

            _score += PointsPerPickup;
            ScoreChanged?.Invoke(_score);
        }

        private void TakeHit(Hazard hazard)
        {
            hazard.Consume();
            (IsBloodPhase ? _hitSplashBlood : _hitSplash).Play();
            PlayRandom(_hitSounds);

            _hits++;
            HitsChanged?.Invoke(_hits);

            if (IsDead)
            {
                Died?.Invoke();
                Destroy(gameObject);
            }
        }

        private void PlayRandom(AudioClip[] clips)
        {
            _sfx.PlayOneShot(clips[Random.Range(0, clips.Length)]);
        }

        private static bool IsEmpty(AudioClip[] clips)
        {
            return clips == null || clips.Length == 0;
        }
    }
}
