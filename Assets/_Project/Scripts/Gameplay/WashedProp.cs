using UnityEngine;

namespace StarJourney.Gameplay
{
    /// <summary>
    /// A prop the waves move. It surfaces at the water's edge and rides it up the beach to its resting point, is
    /// thrown there at the crest if the water does not reach it, may be dragged back by the receding water if it is
    /// light, and is swept out by the next wave. <see cref="TideSpawner"/> drives it from its own fixed step; the
    /// prop moves as a kinematic body, so the seal still collides with it.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class WashedProp : MonoBehaviour
    {
        // The sand along the prop's path is sampled this often and this far toward the sea.
        private const float ShoreStep = 0.5f;
        private const float ShoreLength = 40f;

        // Keeps the path's profile falling over flat sand, such as the shelf at the low-tide line, so that the
        // water's edge crosses it in finite time instead of jumping across it.
        private const float MinShoreSlope = 0.05f;

        // The prop's top is this far under the surface when it appears, so it is out of sight under the water.
        private const float HiddenDepth = 0.2f;

        // The water covers the prop when the surface is this far above its top.
        private const float CoveredDepth = 0.1f;

        private const float FullTurnDegrees = 360f;

        [SerializeField]
        [Tooltip("Light props can be dragged back by the receding wave.")]
        private bool _isLight;

        [SerializeField]
        [Tooltip("Model that shrinks when the prop leaves play.")]
        private Transform _model;

        [SerializeField]
        [Tooltip("Ring on the sand under the prop.")]
        private PropRing _ring;

        [SerializeField, Min(0f)]
        [Tooltip("How far below the water's edge the prop's sand point stays while it is carried.")]
        private float _carryDepth = 0.15f;

        [SerializeField, Min(0.01f)]
        [Tooltip("Seconds the prop takes to rise from under the water to the edge after it appears.")]
        private float _surfaceDuration = 0.4f;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("Chance that the receding wave drags a light prop back.")]
        private float _dragChance = 0.5f;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("Share of dragged props that go all the way back into the sea.")]
        private float _fullDragShare = 0.4f;

        [SerializeField]
        [Tooltip("Range of the distance a partly dragged prop travels toward the sea.")]
        private Vector2 _partialDragDistance = new Vector2(1.5f, 4f);

        [SerializeField, Min(0.1f)]
        [Tooltip("Fastest a dragged prop moves to keep up with the receding edge, in units per second.")]
        private float _dragSpeed = 8f;

        [SerializeField, Min(0.1f)]
        [Tooltip("Speed at which a prop dragged into the sea drifts out until it is covered.")]
        private float _driftOutSpeed = 3f;

        [SerializeField, Min(0.1f)]
        [Tooltip("Longest a prop drifts out before it goes, so it is gone before the next wave.")]
        private float _maxDriftDuration = 4f;

        [SerializeField, Min(0f)]
        [Tooltip("Seconds of a throw onto the beach, plus the next value per unit of distance.")]
        private float _tossBaseDuration = 0.35f;

        [SerializeField, Min(0f)]
        [Tooltip("Extra seconds of a throw per unit of horizontal distance.")]
        private float _tossDurationPerUnit = 0.05f;

        [SerializeField, Min(0.05f)]
        [Tooltip("Longest a throw lasts, however far it goes, so every throw of a wave ends soon after its crest.")]
        private float _maxTossDuration = 0.65f;

        [SerializeField, Min(0f)]
        [Tooltip("Height of a throw above its chord, plus the next value per unit of distance.")]
        private float _tossBaseHeight = 0.5f;

        [SerializeField, Min(0f)]
        [Tooltip("Extra height of a throw per unit of horizontal distance.")]
        private float _tossHeightPerUnit = 0.15f;

        [SerializeField, Min(0.01f)]
        [Tooltip("Seconds the model shrinks before the prop returns to its pool.")]
        private float _vanishDuration = 0.25f;

        private Rigidbody _body;
        private Beach _beach;
        private ShoreLine _line;
        private System.Random _random;
        private Vector3 _modelScale;
        private float _topHeight;

        private Quaternion _restRotation;
        private float _restDistance;
        private float _sandOffset;
        private float _seaDepth;
        private float _seaDistance;
        private float _distance;
        private Vector3 _position;
        private float _timer;

        private bool _isDragRolled;
        private bool _isFullDrag;
        private float _dragDistance;
        private float _driftTimer;

        private Vector3 _tossFrom;
        private Vector3 _tossTo;
        private Quaternion _tossRotation;
        private Vector3 _tossAxis;
        private float _tossDuration;
        private float _tossHeight;
        private bool _isTossOut;

        private bool _isVanishing;
        private float _vanishTimer;

        /// <summary>
        /// What the water is doing with the prop.
        /// </summary>
        public WashState State { get; private set; }

        /// <summary>
        /// Where the prop lies when it rests. Moves down the beach when the prop is partly dragged back.
        /// </summary>
        public Vector3 RestPoint { get; private set; }

        /// <summary>
        /// Whether the prop is light enough to be dragged back.
        /// </summary>
        public bool IsLight => _isLight;

        /// <summary>
        /// Whether the prop has left play and can return to its pool.
        /// </summary>
        public bool IsDone { get; private set; }

        /// <summary>
        /// Starts a wave's ride: puts the prop under the water on its path from the sea to <paramref name="rest"/>.
        /// Call it while the prop is inactive, then activate it, so its body starts at that pose.
        /// </summary>
        /// <param name="beach">The beach the prop is washed onto.</param>
        /// <param name="rest">Where the prop comes to rest.</param>
        /// <param name="rotation">Rotation of the prop at rest.</param>
        /// <param name="lowSurface">Height of the water surface at rest.</param>
        /// <param name="random">Source of the drag-back rolls.</param>
        public void BeginWash(Beach beach, Vector3 rest, Quaternion rotation, float lowSurface,
            System.Random random)
        {
            ResetWash();
            _beach = beach;
            _random = random;
            _restRotation = rotation;
            RestPoint = rest;
            _line.Build(beach, rest, MinShoreSlope);
            _restDistance = 0f;
            _sandOffset = rest.y - _line.HeightAt(0f);
            _seaDepth = Mathf.Max(_carryDepth, _topHeight + _sandOffset + HiddenDepth);
            _seaDistance = _line.DistanceTo(lowSurface - _seaDepth);
            _distance = _seaDistance;

            // Out at sea the line may not get deep enough; the prop then starts sunk into the sand under the water.
            _position = _line.PointAt(_distance, lowSurface - _seaDepth + _sandOffset);
            transform.SetPositionAndRotation(_position, rotation);
            State = WashState.Riding;
            _ring.Attach(beach);
            _ring.SetWater(lowSurface, true);
        }

        /// <summary>
        /// Moves the prop by one fixed step.
        /// </summary>
        /// <param name="surfaceHeight">World height of the water surface after this step's tide move.</param>
        /// <param name="phase">What the water is doing.</param>
        /// <param name="deltaTime">Length of the step.</param>
        public void Advance(float surfaceHeight, TidePhase phase, float deltaTime)
        {
            switch (State)
            {
                case WashState.Riding:
                    Ride(surfaceHeight, deltaTime);
                    break;
                case WashState.Tossing:
                    Toss(deltaTime);
                    break;
                case WashState.Resting:
                    if (!_isDragRolled && phase == TidePhase.Falling
                        && surfaceHeight - _carryDepth < _line.HeightAt(_restDistance))
                    {
                        RollDrag();
                    }

                    break;
                case WashState.DraggingBack:
                    DragBack(surfaceHeight, phase, deltaTime);
                    break;
                case WashState.SweepingOut:
                    if (IsCovered(surfaceHeight))
                    {
                        IsDone = true;
                    }

                    break;
            }

            if (_isVanishing)
            {
                _vanishTimer += deltaTime;
                float left = 1f - Mathf.Clamp01(_vanishTimer / _vanishDuration);
                _model.localScale = _modelScale * left;
                IsDone |= left <= 0f;
            }

            _ring.SetWater(surfaceHeight, State != WashState.Resting);
        }

        /// <summary>
        /// The wave has crested: a prop still riding is thrown onto its resting point, and a prop of the previous
        /// wave that the water never reached is thrown into the water.
        /// </summary>
        /// <param name="highSurface">Height of the water surface at the crest.</param>
        public void OnCrest(float highSurface)
        {
            if (State == WashState.Riding)
            {
                StartToss(RestPoint, false);
            }
            else if (State == WashState.SweepingOut)
            {
                float sand = highSurface - (_topHeight + _sandOffset + HiddenDepth);
                float distance = Mathf.Min(_line.DistanceTo(sand), _seaDistance);
                StartToss(OnSand(distance), true);
            }
        }

        /// <summary>
        /// The next wave has begun: the prop waits for the rising water to cover it and then leaves play.
        /// </summary>
        public void BeginSweepOut()
        {
            if (State != WashState.Idle && !_isVanishing)
            {
                State = WashState.SweepingOut;
            }
        }

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
            if (_model == null || _ring == null || !_body.isKinematic)
            {
                Debug.LogError($"{nameof(WashedProp)} on '{name}' needs its model, its ring and a kinematic body.",
                    this);
                enabled = false;
                return;
            }

            _line = new ShoreLine(ShoreLength, ShoreStep);
            _modelScale = _model.localScale;

            // Measured once from the model as instantiated, upright: how far its top is above the prop's origin.
            float top = float.NegativeInfinity;
            foreach (Renderer modelRenderer in _model.GetComponentsInChildren<Renderer>())
            {
                top = Mathf.Max(top, modelRenderer.bounds.max.y);
            }

            _topHeight = float.IsNegativeInfinity(top) ? 0f : top - transform.position.y;
        }

        private void OnDisable()
        {
            ResetWash();
        }

        private void ResetWash()
        {
            State = WashState.Idle;
            IsDone = false;
            _isDragRolled = false;
            _isVanishing = false;
            _timer = 0f;
            _driftTimer = 0f;
            if (_model != null)
            {
                _model.localScale = _modelScale;
            }

            if (_ring != null)
            {
                _ring.Detach();
            }
        }

        // The prop's path is a function of the surface height, so it keeps step with the tide exactly.
        private void Ride(float surfaceHeight, float deltaTime)
        {
            _timer += deltaTime;
            float depth = Mathf.Lerp(_seaDepth, _carryDepth, Mathf.SmoothStep(0f, 1f, _timer / _surfaceDuration));
            float sand = surfaceHeight - depth;
            if (sand >= _line.HeightAt(_restDistance))
            {
                Settle(RestPoint);
                return;
            }

            _distance = Mathf.Min(_line.DistanceTo(sand), _seaDistance);
            MoveTo(_line.PointAt(_distance, sand + _sandOffset));
        }

        private void StartToss(Vector3 target, bool isOut)
        {
            _tossFrom = _position;
            _tossTo = target;
            _tossRotation = _body.rotation;
            Vector3 chord = target - _position;
            chord.y = 0f;
            float distance = chord.magnitude;
            _tossAxis = distance > 1e-3f ? Vector3.Cross(Vector3.up, chord / distance) : transform.right;
            _tossDuration = Mathf.Min(_tossBaseDuration + _tossDurationPerUnit * distance, _maxTossDuration);
            _tossHeight = _tossBaseHeight + _tossHeightPerUnit * distance;
            _isTossOut = isOut;
            _timer = 0f;
            State = WashState.Tossing;
        }

        // A parabola over the chord with one full tumble about the axis across the throw.
        private void Toss(float deltaTime)
        {
            _timer += deltaTime;
            float t = Mathf.Clamp01(_timer / _tossDuration);
            Vector3 position = Vector3.Lerp(_tossFrom, _tossTo, t);
            position.y += 4f * _tossHeight * t * (1f - t);
            MoveTo(position);
            _body.MoveRotation(Quaternion.AngleAxis(FullTurnDegrees * Mathf.SmoothStep(0f, 1f, t), _tossAxis)
                * _tossRotation);
            if (t < 1f)
            {
                return;
            }

            if (_isTossOut)
            {
                State = WashState.SweepingOut;
                StartVanish();
            }
            else
            {
                Settle(_tossTo);
            }
        }

        private void RollDrag()
        {
            _isDragRolled = true;
            if (!_isLight || _random.NextDouble() >= _dragChance)
            {
                return;
            }

            _isFullDrag = _random.NextDouble() < _fullDragShare;
            _dragDistance = Mathf.Lerp(_partialDragDistance.x, _partialDragDistance.y, (float)_random.NextDouble());
            _distance = _restDistance;
            _driftTimer = 0f;
            State = WashState.DraggingBack;
        }

        // Follows the receding edge down the prop's path; a partial drag stops after its distance, a full one
        // follows the edge to the low-tide line and then drifts out until the water covers it.
        private void DragBack(float surfaceHeight, TidePhase phase, float deltaTime)
        {
            if (phase == TidePhase.Falling)
            {
                float edge = Mathf.Min(_line.DistanceTo(surfaceHeight - _carryDepth), _seaDistance);
                _distance = Mathf.MoveTowards(_distance, Mathf.Max(edge, _distance), _dragSpeed * deltaTime);
                if (!_isFullDrag && _distance >= _restDistance + _dragDistance)
                {
                    _distance = _restDistance + _dragDistance;
                    SettleAlong(_distance);
                    return;
                }

                MoveTo(OnSand(_distance));
                return;
            }

            if (!_isFullDrag)
            {
                // The water stopped falling before the prop had travelled its whole distance.
                SettleAlong(_distance);
                return;
            }

            _driftTimer += deltaTime;
            _distance = Mathf.Min(_distance + _driftOutSpeed * deltaTime, _seaDistance);
            MoveTo(OnSand(_distance));
            if (!_isVanishing && (IsCovered(surfaceHeight) || _distance >= _seaDistance
                || _driftTimer >= _maxDriftDuration))
            {
                StartVanish();
            }
        }

        private void SettleAlong(float distance)
        {
            _restDistance = distance;
            Settle(OnSand(distance));
        }

        // A point of the path on the sand itself, not on its always-falling profile.
        private Vector3 OnSand(float distance)
        {
            Vector3 point = _line.PointAt(distance, 0f);
            point.y = _beach.SandHeight(point.x, point.z) + _sandOffset;
            return point;
        }

        private void Settle(Vector3 point)
        {
            RestPoint = point;
            MoveTo(point);
            _body.MoveRotation(_restRotation);
            State = WashState.Resting;
        }

        private void StartVanish()
        {
            _isVanishing = true;
            _vanishTimer = 0f;
        }

        private bool IsCovered(float surfaceHeight)
        {
            return surfaceHeight >= _position.y + _topHeight + CoveredDepth;
        }

        private void MoveTo(Vector3 position)
        {
            _position = position;
            _body.MovePosition(position);
        }
    }
}
