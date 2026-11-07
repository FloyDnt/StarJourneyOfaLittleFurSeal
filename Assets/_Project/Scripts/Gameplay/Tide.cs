using System;
using UnityEngine;

namespace StarJourney.Gameplay
{
    /// <summary>
    /// Moves a body of water up and down in waves: it rises from the low to the high height, crests, falls back,
    /// rests and rises again until it has made all of its waves. Runs on fixed steps; the game's timeline is a count
    /// of them.
    /// </summary>
    public sealed class Tide : MonoBehaviour
    {
        // The water counts as arrived when it is this close to a height, which happens one step before
        // MoveTowards would snap it there at the default speed. It turns at that point, as it always did.
        private const float ArrivalTolerance = 0.01f;

        [SerializeField]
        [Tooltip("World height of the water at rest.")]
        private float _lowHeight = -8f;

        [SerializeField]
        [Tooltip("World height of the water at the crest of a wave.")]
        private float _highHeight = -6f;

        [SerializeField]
        [Tooltip("Height of the water surface above the object's position: half the water cube's height.")]
        private float _surfaceOffset = 0.5f;

        [SerializeField, Min(0f)]
        [Tooltip("Rising and falling speed in units per second.")]
        private float _speed = 1f;

        [SerializeField, Min(0f)]
        [Tooltip("Seconds the water rests at the low height between two waves.")]
        private float _restDuration = 10f;

        [SerializeField, Min(0f)]
        [Tooltip("Seconds the water waits at the low height after it is enabled, before its first wave.")]
        private float _startDelay;

        [SerializeField, Min(1)]
        [Tooltip("Number of waves. The tide completes after the last one.")]
        private int _waveCount = 7;

        [SerializeField]
        [Tooltip("Off: the tide completes when the last wave is back down. On: it completes at the end of the rest "
            + "after the last wave.")]
        private bool _completeAfterLastRest;

        [SerializeField]
        [Tooltip("Source that plays the wave sounds.")]
        private AudioSource _sfx;

        [SerializeField]
        [Tooltip("Sounds played at the start of every rise.")]
        private AudioClip[] _riseSounds;

        [SerializeField]
        [Tooltip("Sounds played at the start of the first rise only.")]
        private AudioClip[] _firstRiseSounds;

        private TidePhase _phase;
        private float _timer;
        private int _crests;
        private bool _isRiseStarted;
        private bool _hasRisen;

        /// <summary>
        /// Raised in the step in which the water starts to rise, before it moves in that step.
        /// </summary>
        public event Action RiseStarted;

        /// <summary>
        /// Raised in the step in which a wave reaches the high height.
        /// </summary>
        public event Action Crested;

        /// <summary>
        /// Raised right after <see cref="Crested"/>, in the same step: the water falls from the next step on.
        /// </summary>
        public event Action FallStarted;

        /// <summary>
        /// Raised once, after the last wave. The tide stops moving.
        /// </summary>
        public event Action Completed;

        /// <summary>
        /// What the water is doing.
        /// </summary>
        public TidePhase Phase => _phase;

        /// <summary>
        /// World height of the water surface now.
        /// </summary>
        public float SurfaceHeight => transform.position.y + _surfaceOffset;

        /// <summary>
        /// World height of the water surface at rest.
        /// </summary>
        public float LowSurfaceHeight => _lowHeight + _surfaceOffset;

        /// <summary>
        /// World height of the water surface at the crest of a wave.
        /// </summary>
        public float HighSurfaceHeight => _highHeight + _surfaceOffset;

        /// <summary>
        /// How far the current rise or fall has got, from 0 to 1. Zero while the water waits or rests.
        /// </summary>
        public float Progress
        {
            get
            {
                float range = _highHeight - _lowHeight;
                if (range <= 0f)
                {
                    return 0f;
                }

                float y = transform.position.y;
                switch (_phase)
                {
                    case TidePhase.Rising:
                        return Mathf.Clamp01((y - _lowHeight) / range);
                    case TidePhase.Falling:
                        return Mathf.Clamp01((_highHeight - y) / range);
                    default:
                        return 0f;
                }
            }
        }

        private void Awake()
        {
            if (_sfx == null || _riseSounds == null || _firstRiseSounds == null)
            {
                Debug.LogError($"{nameof(Tide)} on '{name}' needs a sound source and its sound lists.", this);
                enabled = false;
                return;
            }

            _phase = TidePhase.Waiting;
            _timer = _startDelay;
        }

        private void FixedUpdate()
        {
            switch (_phase)
            {
                case TidePhase.Waiting:
                    Wait();
                    break;
                case TidePhase.Rising:
                    Rise();
                    break;
                case TidePhase.Falling:
                    if (MoveTo(_lowHeight))
                    {
                        _phase = TidePhase.Resting;
                        Rest();
                    }

                    break;
                case TidePhase.Resting:
                    // Keeps pulling the water onto the low height, which it reached only within the tolerance.
                    MoveTo(_lowHeight);
                    Rest();
                    break;
            }
        }

        // The start delay counts down while it is positive; the first rise begins in the step that finds it run out.
        private void Wait()
        {
            if (_timer > 0f)
            {
                _timer -= Time.fixedDeltaTime;
                return;
            }

            _timer = _restDuration;
            _phase = TidePhase.Rising;
            Rise();
        }

        private void Rise()
        {
            if (!_isRiseStarted)
            {
                _isRiseStarted = true;
                PlayAll(_riseSounds);
                if (!_hasRisen)
                {
                    _hasRisen = true;
                    PlayAll(_firstRiseSounds);
                }

                RiseStarted?.Invoke();
            }

            if (MoveTo(_highHeight))
            {
                _crests++;
                _phase = TidePhase.Falling;
                Crested?.Invoke();
                FallStarted?.Invoke();
            }
        }

        // A rest starts in the step the water arrives and counts down in that step already. When the time is up,
        // that step only turns the tide and the rise begins in the next one.
        private void Rest()
        {
            if (_timer <= 0f)
            {
                _timer = _restDuration;
                _isRiseStarted = false;
                _phase = TidePhase.Rising;
                return;
            }

            _timer -= Time.fixedDeltaTime;

            if (_crests >= _waveCount && (!_completeAfterLastRest || _timer <= 0f))
            {
                _phase = TidePhase.Done;
                Completed?.Invoke();
            }
        }

        private bool MoveTo(float height)
        {
            Vector3 position = transform.position;
            var target = new Vector3(position.x, height, position.z);
            position = Vector3.MoveTowards(position, target, _speed * Time.fixedDeltaTime);
            transform.position = position;
            return Mathf.Abs(position.y - height) < ArrivalTolerance;
        }

        private void PlayAll(AudioClip[] clips)
        {
            for (int i = 0; i < clips.Length; i++)
            {
                _sfx.PlayOneShot(clips[i]);
            }
        }
    }
}
