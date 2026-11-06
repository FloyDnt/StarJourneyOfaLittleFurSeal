using StarJourney.Gameplay;
using TMPro;
using UnityEngine;

namespace StarJourney.UI
{
    /// <summary>
    /// Shows the death screen of the current phase when the seal dies, and the final screen with the score when the
    /// game ends with the seal alive.
    /// </summary>
    public sealed class EndScreens : MonoBehaviour
    {
        private const string FinalScoreFormat = "CURRENT SCORE: {0}";

        [SerializeField]
        [Tooltip("The game whose end is shown.")]
        private GameFlow _gameFlow;

        [SerializeField]
        [Tooltip("The screen shown when the seal dies in clear water.")]
        private GameObject _deathScreen;

        [SerializeField]
        [Tooltip("The screen shown when the seal dies in the sea of blood.")]
        private GameObject _bloodDeathScreen;

        [SerializeField]
        [Tooltip("The screen shown when the game ends with the seal alive.")]
        private GameObject _finalScreen;

        [SerializeField]
        [Tooltip("Text on the final screen that shows the score.")]
        private TMP_Text _finalScoreText;

        private void Awake()
        {
            if (_gameFlow == null || _deathScreen == null || _bloodDeathScreen == null || _finalScreen == null
                || _finalScoreText == null)
            {
                Debug.LogError($"{nameof(EndScreens)} on '{name}' needs the game flow, the three screens and the "
                    + "final score text.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            _gameFlow.SealDied += ShowDeathScreen;
            _gameFlow.GameFinished += ShowFinalScreen;
        }

        private void OnDisable()
        {
            _gameFlow.SealDied -= ShowDeathScreen;
            _gameFlow.GameFinished -= ShowFinalScreen;
        }

        private void ShowDeathScreen()
        {
            (_gameFlow.IsBloodPhase ? _bloodDeathScreen : _deathScreen).SetActive(true);
        }

        private void ShowFinalScreen(int score)
        {
            _finalScreen.SetActive(true);
            _finalScoreText.SetText(FinalScoreFormat, score);
        }
    }
}
