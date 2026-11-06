using System;
using UnityEngine;

namespace StarJourney.Gameplay
{
    /// <summary>
    /// Runs the phases of a game: the clear-water tide, then the sea of blood, then the end. Stops time when the
    /// game ends or the seal dies, and raises events for the UI.
    /// </summary>
    public sealed class GameFlow : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The clear-water tide. Its completion starts the sea of blood.")]
        private Tide _clearTide;

        [SerializeField]
        [Tooltip("The tide of the sea of blood, inactive until that phase. Its completion ends the game.")]
        private Tide _bloodTide;

        [SerializeField]
        [Tooltip("The player's fur seal.")]
        private FurSeal _seal;

        [SerializeField]
        [Tooltip("The music of the clear-water phase.")]
        private GameObject _mainMusic;

        [SerializeField]
        [Tooltip("The music of the sea of blood, inactive until that phase.")]
        private GameObject _bloodMusic;

        [SerializeField]
        [Tooltip("Source that plays the end-of-game sounds.")]
        private AudioSource _sfx;

        [SerializeField]
        [Tooltip("Played when the game ends with the seal alive.")]
        private AudioClip _finishSound;

        [SerializeField]
        [Tooltip("Played when the seal dies in the sea of blood.")]
        private AudioClip _bloodDeathSound;

        /// <summary>
        /// Raised when the sea of blood begins, after <see cref="IsBloodPhase"/> is set.
        /// </summary>
        public event Action BloodPhaseStarted;

        /// <summary>
        /// Raised when the game ends with the seal alive, with its final score.
        /// </summary>
        public event Action<int> GameFinished;

        /// <summary>
        /// Raised when the seal dies. <see cref="IsBloodPhase"/> tells in which phase.
        /// </summary>
        public event Action SealDied;

        /// <summary>
        /// Whether the sea of blood has begun.
        /// </summary>
        public bool IsBloodPhase { get; private set; }

        private void Awake()
        {
            if (_clearTide == null || _bloodTide == null || _seal == null || _mainMusic == null || _bloodMusic == null
                || _sfx == null || _finishSound == null || _bloodDeathSound == null)
            {
                Debug.LogError($"{nameof(GameFlow)} on '{name}' needs both tides, the seal, the music and the sounds.",
                    this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            _clearTide.Completed += StartBloodPhase;
            _bloodTide.Completed += FinishGame;
            _seal.Died += OnSealDied;
        }

        private void OnDisable()
        {
            _clearTide.Completed -= StartBloodPhase;
            _bloodTide.Completed -= FinishGame;
            _seal.Died -= OnSealDied;
        }

        private void StartBloodPhase()
        {
            IsBloodPhase = true;
            _bloodTide.gameObject.SetActive(true);
            _clearTide.gameObject.SetActive(false);
            _mainMusic.SetActive(false);
            _bloodMusic.SetActive(true);
            _seal.IsBloodPhase = true;
            BloodPhaseStarted?.Invoke();
        }

        private void FinishGame()
        {
            _sfx.PlayOneShot(_finishSound);
            GameFinished?.Invoke(_seal.Score);
            Destroy(_seal.gameObject);
            Time.timeScale = 0f;
        }

        private void OnSealDied()
        {
            if (IsBloodPhase)
            {
                _sfx.PlayOneShot(_bloodDeathSound);
            }

            SealDied?.Invoke();
            Time.timeScale = 0f;
        }
    }
}
