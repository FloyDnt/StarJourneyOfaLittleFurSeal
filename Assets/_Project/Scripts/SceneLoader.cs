using UnityEngine.SceneManagement;

namespace StarJourney
{
    /// <summary>
    /// The scenes of the game. Each name equals the scene's file name.
    /// </summary>
    public enum GameScene
    {
        /// <summary>The main menu.</summary>
        MainMenu,

        /// <summary>The beach where the game is played.</summary>
        Game,
    }

    /// <summary>
    /// Loads game scenes by <see cref="GameScene"/> instead of by string.
    /// </summary>
    public static class SceneLoader
    {
        /// <summary>
        /// Replaces the open scenes with <paramref name="scene"/>.
        /// </summary>
        /// <param name="scene">The scene to load.</param>
        public static void Load(GameScene scene)
        {
            SceneManager.LoadScene(scene.ToString());
        }
    }
}
