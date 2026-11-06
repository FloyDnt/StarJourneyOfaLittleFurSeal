using UnityEngine;

namespace StarJourney.Gameplay
{
    /// <summary>
    /// Marks a prop that hits the fur seal when it touches it. The seal handles the contact.
    /// </summary>
    public sealed class Hazard : MonoBehaviour
    {
        /// <summary>
        /// Takes the hazard out of play. <see cref="TideSpawner"/> returns it to its pool before the next batch.
        /// </summary>
        public void Consume()
        {
            gameObject.SetActive(false);
        }
    }
}
