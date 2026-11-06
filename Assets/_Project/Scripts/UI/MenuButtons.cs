using UnityEngine;

namespace StarJourney.UI
{
    /// <summary>
    /// Targets for the OnClick events of the menu, HUD and end screen buttons.
    /// </summary>
    public sealed class MenuButtons : MonoBehaviour
    {
        /// <summary>
        /// Starts a new game. Resumes time, which the death and final screens stop.
        /// </summary>
        public void Play()
        {
            Time.timeScale = 1f;
            SceneLoader.Load(GameScene.Game);
        }

        /// <summary>
        /// Returns to the main menu. Resumes time, which the death and final screens stop, so that the menu's
        /// water keeps moving.
        /// </summary>
        public void OpenMainMenu()
        {
            Time.timeScale = 1f;
            SceneLoader.Load(GameScene.MainMenu);
        }

        /// <summary>
        /// Quits the game, or leaves play mode in the editor.
        /// </summary>
        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
        }
    }
}
