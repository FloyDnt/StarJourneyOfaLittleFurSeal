using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace StarJourney.Gameplay
{
    /// <summary>
    /// The player's fur seal: turns on the spot while it stands on the ground, charges a jump while the button is
    /// held and jumps the way it faces on release, collects pickups and dies on its third hazard.
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
    public sealed class FurSeal : MonoBehaviour
    {
        private const int PointsPerPickup = 100;
        private const int HitsToDie = 3;
        private const float FullTurnDegrees = 360f;

        // The ground probe is a sphere this much smaller than the collider, swept down from the collider's centre.
        // It starts inside the seal's own collider, which a sphere cast never reports.
        private const float ProbeInset = 0.1f;
        private const float ProbeDistance = 0.2f;

        // Ground steeper than 60 degrees, such as the face of a wall, does not count.
        private const float MinGroundNormalY = 0.5f;

        // A landing needs the collider to be within this gap of the ground, or to have stopped falling.
        private const float LandingGap = 0.03f;
        private const float MaxLandingVerticalSpeed = 0.05f;

        [SerializeField, Min(0f)]
        [Tooltip("Degrees per second the seal turns around its vertical axis while idle on the ground.")]
        private float _turnSpeed = 120f;

        [SerializeField, Min(0.01f)]
        [Tooltip("Seconds of holding the button that charge a jump to full power.")]
        private float _chargeDuration = 0.75f;

        [SerializeField]
        [Tooltip("Maps the held fraction of the charge time (0..1) to jump power (0..1).")]
        private AnimationCurve _chargeCurve =
            new AnimationCurve(new Keyframe(0f, 0f, 0f, 2f), new Keyframe(1f, 1f, 0f, 0f));

        [SerializeField, Min(0.01f)]
        [Tooltip("Flat-ground distance of a jump released at zero power.")]
        private float _minJumpDistance = 1f;

        [SerializeField, Min(0.01f)]
        [Tooltip("Flat-ground distance of a jump at full power; holding longer adds nothing.")]
        private float _maxJumpDistance = 6f;

        [SerializeField, Range(20f, 70f)]
        [Tooltip("Launch angle above the horizontal, in degrees.")]
        private float _launchAngle = 45f;

        [SerializeField]
        [Tooltip("Layers the seal can stand on.")]
        private LayerMask _groundMask = 1;

        [SerializeField, Min(0f)]
        [Tooltip("Seconds after launch before the ground is checked for a landing.")]
        private float _minAirTime = 0.1f;

        [SerializeField, Min(0f)]
        [Tooltip("Seconds without ground under the seal before it counts as falling.")]
        private float _coyoteTime = 0.1f;

        [SerializeField, Min(0f)]
        [Tooltip("Seconds the seal settles after landing before it is idle.")]
        private float _landingDuration = 0.15f;

        [SerializeField]
        [Tooltip("Transform scaled for the squash and stretch; the collider stays on the root.")]
        private Transform _squash;

        [SerializeField, Range(0f, 0.9f)]
        [Tooltip("How much the seal squashes down at full charge, as a share of its height.")]
        private float _chargeSquash = 0.2f;

        [SerializeField, Min(1f)]
        [Tooltip("Vertical scale the seal stretches to at launch before it eases back.")]
        private float _launchStretch = 1.25f;

        [SerializeField, Range(0.1f, 1f)]
        [Tooltip("Vertical scale the seal squashes to on landing before it eases back.")]
        private float _landingSquash = 0.72f;

        [SerializeField, Min(0.01f)]
        [Tooltip("Seconds the launch stretch takes to ease back.")]
        private float _launchStretchDuration = 0.18f;

        [SerializeField, Min(0.01f)]
        [Tooltip("Seconds the landing squash takes to ease back.")]
        private float _landingSquashDuration = 0.22f;

        [SerializeField, Min(0.01f)]
        [Tooltip("Seconds the charge squash takes to ease back when the charge is cancelled.")]
        private float _cancelSquashDuration = 0.15f;

        [SerializeField]
        [Tooltip("Source that plays the hop, pickup and hit sounds.")]
        private AudioSource _sfx;

        [SerializeField]
        [Tooltip("One of these plays at random on every jump.")]
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

        private Rigidbody _body;
        private SphereCollider _collider;
        private JumpState _state;
        private float _yaw;
        private float _holdTime;
        private bool _launchPending;
        private float _launchPower;
        private float _airTime;
        private float _groundLostTime;
        private float _landingTimer;
        private bool _isGrounded;
        private float _groundGap;
        private float _squashScale = 1f;
        private float _squashFrom = 1f;
        private float _squashTimer;
        private float _squashDuration;
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
        /// Raised when a press starts charging a jump.
        /// </summary>
        public event Action ChargeStarted;

        /// <summary>
        /// Raised once for every <see cref="ChargeStarted"/>: when the button is released, when the seal loses the
        /// ground and the charge is cancelled, or when the seal is disabled or destroyed while charging.
        /// </summary>
        public event Action ChargeEnded;

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

        /// <summary>
        /// What the seal is doing: turning, charging a jump, flying or settling after a landing.
        /// </summary>
        public JumpState State => _state;

        /// <summary>
        /// Share of the charge time held so far, 0..1; 0 unless the seal is charging.
        /// </summary>
        public float Charge => _state == JumpState.Charging ? HeldFraction : 0f;

        /// <summary>
        /// Power of the jump being charged, 0..1, eased the way the jump distance grows; 0 when no charge is held.
        /// </summary>
        public float ChargePower => IsChargeHeld ? Power(HeldFraction) : 0f;

        /// <summary>
        /// World position of the centre of the seal's body as rendered this frame.
        /// </summary>
        public Vector3 BodyCentre => transform.TransformPoint(_collider.center);

        /// <summary>
        /// Flat-ground distance of a jump released now.
        /// </summary>
        public float JumpDistance => Mathf.Lerp(_minJumpDistance, _maxJumpDistance, Power(Charge));

        /// <summary>
        /// Whether the ground probe found ground under the seal in the last physics step.
        /// </summary>
        public bool IsGrounded => _isGrounded;

        /// <summary>
        /// Horizontal unit direction the seal faces and jumps in.
        /// </summary>
        public Vector3 Forward
        {
            get
            {
                Vector3 forward = _body.rotation * Vector3.right;
                forward.y = 0f;
                return forward.normalized;
            }
        }

        private bool IsDead => _hits >= HitsToDie;

        private bool IsChargeHeld => _state == JumpState.Charging && !_launchPending;

        private float HeldFraction => Mathf.Clamp01(_holdTime / _chargeDuration);

        private Vector3 Centre => _body.position + _body.rotation * _collider.center;

        /// <summary>
        /// What a press of the jump button does: starts charging a jump if the seal stands on the ground and is
        /// not already busy. Presses in the air are ignored.
        /// </summary>
        public void Press()
        {
            if (!enabled || IsDead || !_isGrounded || (_state != JumpState.Idle && _state != JumpState.Landing))
            {
                return;
            }

            _state = JumpState.Charging;
            _holdTime = 0f;
            _launchPending = false;
            _squashTimer = 0f;
            ChargeStarted?.Invoke();
        }

        /// <summary>
        /// What a release of the jump button does: jumps with the power charged so far. The launch is applied in
        /// the next physics step.
        /// </summary>
        public void Release()
        {
            if (!enabled || !IsChargeHeld)
            {
                return;
            }

            _launchPower = Power(HeldFraction);
            _launchPending = true;
            ChargeEnded?.Invoke();
        }

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
            _collider = GetComponent<SphereCollider>();

            // From here on the yaw is accumulated and never read back from the body, so the turn cannot drift.
            _yaw = _body.rotation.eulerAngles.y;

            if (_chargeCurve == null || _chargeCurve.length == 0 || _squash == null || _sfx == null
                || !Validation.IsFilled(_hopSounds) || !Validation.IsFilled(_pickupSounds)
                || !Validation.IsFilled(_hitSounds) || _pickupSplash == null || _pickupSplashBlood == null
                || _hitSplash == null || _hitSplashBlood == null)
            {
                Debug.LogError($"{nameof(FurSeal)} on '{name}' needs a charge curve, its squash transform, a sound "
                    + "source, its sounds and its four splashes.", this);
                enabled = false;
            }
        }

        private void Update()
        {
            if (IsChargeHeld)
            {
                _holdTime += Time.deltaTime;
            }

            UpdateSquash();
        }

        private void FixedUpdate()
        {
            // Velocities cannot be set on a kinematic body. The game never makes the seal kinematic; tools may.
            if (_body.isKinematic)
            {
                return;
            }

            ProbeGround();

            if (_launchPending)
            {
                Launch();
                return;
            }

            float dt = Time.fixedDeltaTime;
            if (_state == JumpState.Airborne)
            {
                _airTime += dt;
                float verticalSpeed = _body.linearVelocity.y;
                bool isTouching = _groundGap <= LandingGap || Mathf.Abs(verticalSpeed) <= MaxLandingVerticalSpeed;
                if (_airTime >= _minAirTime && _isGrounded && isTouching && verticalSpeed <= MaxLandingVerticalSpeed)
                {
                    Land();
                }
            }
            else
            {
                _groundLostTime = _isGrounded ? 0f : _groundLostTime + dt;
                if (_groundLostTime >= _coyoteTime)
                {
                    Fall();
                }
                else if (_state == JumpState.Landing)
                {
                    _landingTimer -= dt;
                    if (_landingTimer <= 0f)
                    {
                        _state = JumpState.Idle;
                    }
                }
            }

            if (_state == JumpState.Idle || _state == JumpState.Landing)
            {
                _yaw = Mathf.Repeat(_yaw + _turnSpeed * dt, FullTurnDegrees);
                _body.MoveRotation(Quaternion.Euler(0f, _yaw, 0f));
            }

            // Standing on the ground the seal neither slides down the slope nor keeps a push from a prop.
            if (_state != JumpState.Airborne && _isGrounded)
            {
                _body.linearVelocity = new Vector3(0f, _body.linearVelocity.y, 0f);
            }
        }

        private void OnDisable()
        {
            // Death and the end of the game destroy the seal mid-charge; the listeners still hear the charge end.
            if (IsChargeHeld)
            {
                _state = JumpState.Idle;
                ChargeEnded?.Invoke();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Unity sends collision messages to disabled components too.
            if (!enabled || IsDead)
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

        private float Power(float heldFraction)
        {
            return Mathf.Clamp01(_chargeCurve.Evaluate(heldFraction));
        }

        private Vector3 LaunchVelocity(float power)
        {
            float distance = Mathf.Lerp(_minJumpDistance, _maxJumpDistance, power);
            float angle = _launchAngle * Mathf.Deg2Rad;
            float speed = Mathf.Sqrt(-Physics.gravity.y * distance / Mathf.Sin(2f * angle));
            return speed * (Mathf.Cos(angle) * Forward + Mathf.Sin(angle) * Vector3.up);
        }

        private void ProbeGround()
        {
            float radius = _collider.radius - ProbeInset;
            _isGrounded = Physics.SphereCast(Centre, radius, Vector3.down, out RaycastHit hit, ProbeDistance,
                    _groundMask, QueryTriggerInteraction.Ignore)
                && hit.normal.y >= MinGroundNormalY;

            // Gap between the collider and the ground along the ground's normal.
            _groundGap = _isGrounded ? hit.distance * hit.normal.y - ProbeInset : float.PositiveInfinity;
        }

        private void Launch()
        {
            _launchPending = false;
            _body.linearVelocity = LaunchVelocity(_launchPower);
            PlayRandom(_hopSounds);

            _state = JumpState.Airborne;
            _airTime = 0f;
            StartSquash(_launchStretch, _launchStretchDuration);
        }

        private void Land()
        {
            _body.linearVelocity = Vector3.zero;
            _state = JumpState.Landing;
            _landingTimer = _landingDuration;
            _groundLostTime = 0f;
            StartSquash(_landingSquash, _landingSquashDuration);
        }

        private void Fall()
        {
            if (_state == JumpState.Charging)
            {
                bool wasHeld = !_launchPending;
                _launchPending = false;
                StartSquash(_squashScale, _cancelSquashDuration);
                if (wasHeld)
                {
                    ChargeEnded?.Invoke();
                }
            }

            _state = JumpState.Airborne;
            _airTime = 0f;
        }

        private void StartSquash(float from, float duration)
        {
            _squashFrom = from;
            _squashDuration = duration;
            _squashTimer = duration;
        }

        private void UpdateSquash()
        {
            float scale = 1f;
            if (_state == JumpState.Charging)
            {
                scale = 1f - _chargeSquash * Power(HeldFraction);
            }
            else if (_squashTimer > 0f)
            {
                _squashTimer = Mathf.Max(0f, _squashTimer - Time.deltaTime);
                float t = Mathf.SmoothStep(0f, 1f, 1f - _squashTimer / _squashDuration);
                scale = Mathf.Lerp(_squashFrom, 1f, t);
            }

            if (scale == _squashScale)
            {
                return;
            }

            // Scaling the model about the root keeps the feet on the ground; the widths keep the volume.
            _squashScale = scale;
            float width = 1f / Mathf.Sqrt(scale);
            _squash.localScale = new Vector3(width, scale, width);
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
    }
}
