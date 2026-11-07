using StarJourney.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;

namespace StarJourney.UI
{
    /// <summary>
    /// Full-screen layer under every other UI element that turns left-button presses and releases into the seal's
    /// jump. A press on a button is taken by the button and never reaches this layer.
    /// </summary>
    public sealed class JumpInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField]
        [Tooltip("The seal that charges on press and jumps on release.")]
        private FurSeal _seal;

        /// <inheritdoc/>
        public void OnPointerDown(PointerEventData eventData)
        {
            // The seal is destroyed when it dies and when the game ends.
            if (eventData.button == PointerEventData.InputButton.Left && _seal != null)
            {
                _seal.Press();
            }
        }

        /// <inheritdoc/>
        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left && _seal != null)
            {
                _seal.Release();
            }
        }

        private void Awake()
        {
            if (_seal == null)
            {
                Debug.LogError($"{nameof(JumpInput)} on '{name}' needs the seal.", this);
                enabled = false;
            }
        }
    }
}
