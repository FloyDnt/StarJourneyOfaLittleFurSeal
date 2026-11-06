using StarJourney.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarJourney.UI
{
    /// <summary>
    /// Shows the seal's score and remaining health during the game, in the sprites of the current phase.
    /// </summary>
    public sealed class Hud : MonoBehaviour
    {
        private const string ScoreFormat = "Score: {0}";

        private static readonly Color DimmedIcon = new Color(1f, 1f, 1f, 0.5f);

        [SerializeField]
        [Tooltip("The seal whose score and hits are shown.")]
        private FurSeal _seal;

        [SerializeField]
        [Tooltip("The game, whose phase changes switch the HUD's sprites and hide the score.")]
        private GameFlow _gameFlow;

        [SerializeField]
        [Tooltip("The health icons in the order the hits dim them.")]
        private Image[] _healthIcons;

        [SerializeField]
        [Tooltip("Text that shows the score.")]
        private TMP_Text _scoreText;

        [SerializeField]
        [Tooltip("The panel with the score, hidden when the game ends with the seal alive.")]
        private GameObject _scorePanel;

        [SerializeField]
        [Tooltip("Image of the button that returns to the main menu.")]
        private Image _closeButtonImage;

        [SerializeField]
        [Tooltip("Health icon sprite of the sea of blood.")]
        private Sprite _bloodHealthSprite;

        [SerializeField]
        [Tooltip("Close button sprite of the sea of blood.")]
        private Sprite _bloodCloseSprite;

        private void Awake()
        {
            if (_seal == null || _gameFlow == null || !IsFilled(_healthIcons) || _scoreText == null
                || _scorePanel == null || _closeButtonImage == null || _bloodHealthSprite == null
                || _bloodCloseSprite == null)
            {
                Debug.LogError($"{nameof(Hud)} on '{name}' needs the seal, the game flow, its widgets and the blood "
                    + "sprites.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            _seal.ScoreChanged += ShowScore;
            _seal.HitsChanged += ShowHits;
            _gameFlow.BloodPhaseStarted += SwitchToBlood;
            _gameFlow.GameFinished += HideScore;
        }

        private void OnDisable()
        {
            _seal.ScoreChanged -= ShowScore;
            _seal.HitsChanged -= ShowHits;
            _gameFlow.BloodPhaseStarted -= SwitchToBlood;
            _gameFlow.GameFinished -= HideScore;
        }

        private void Start()
        {
            ShowScore(_seal.Score);
            ShowHits(_seal.Hits);
        }

        private void ShowScore(int score)
        {
            _scoreText.SetText(ScoreFormat, score);
        }

        private void ShowHits(int hits)
        {
            for (int i = 0; i < _healthIcons.Length; i++)
            {
                _healthIcons[i].color = i < hits ? DimmedIcon : Color.white;
            }
        }

        private void SwitchToBlood()
        {
            for (int i = 0; i < _healthIcons.Length; i++)
            {
                _healthIcons[i].sprite = _bloodHealthSprite;
            }

            _closeButtonImage.sprite = _bloodCloseSprite;
        }

        private void HideScore(int finalScore)
        {
            _scorePanel.SetActive(false);
        }

        private static bool IsFilled(Image[] images)
        {
            if (images == null || images.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < images.Length; i++)
            {
                if (images[i] == null)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
