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

        private Phase _phase;
        private float _timer;
        private int _crests;
        private bool _isRiseStarted;
        private bool _hasRisen;

        /// <summary>
        /// Raised in the step in which a wave reaches the high height.
        /// </summary>
        public event Action Crested;

        /// <summary>
        /// Raised once, after the last wave. The tide stops moving.
        /// </summary>
        public event Action Completed;

        private enum Phase
        {
            Waiting,
            Rising,
            Falling,
            Resting,
            Done,
        }

        private void Awake()
        {
            if (_sfx == null || _riseSounds == null || _firstRiseSounds == null)
            {
                Debug.LogError($"{nameof(Tide)} on '{name}' needs a sound source and its sound lists.", this);
                enabled = false;
                return;
            }

            _phase = Phase.Waiting;
            _timer = _startDelay;
        }

        private void FixedUpdate()
        {
            switch (_phase)
            {
                case Phase.Waiting:
                    Wait();
                    break;
                case Phase.Rising:
                    Rise();
                    break;
                case Phase.Falling:
                    if (MoveTo(_lowHeight))
                    {
                        _phase = Phase.Resting;
                        Rest();
                    }

                    break;
                case Phase.Resting:
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
            _phase = Phase.Rising;
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
            }

            if (MoveTo(_highHeight))
            {
                _crests++;
                _phase = Phase.Falling;
                Crested?.Invoke();
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
                _phase = Phase.Rising;
                return;
            }

            _timer -= Time.fixedDeltaTime;

            if (_crests >= _waveCount && (!_completeAfterLastRest || _timer <= 0f))
            {
                _phase = Phase.Done;
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
