using UnityEngine;

namespace StarJourney.Gameplay
{
    /// <summary>
    /// Marks a prop that gives the fur seal points when it touches it. The seal handles the contact.
    /// </summary>
    public sealed class Pickup : MonoBehaviour
    {
        /// <summary>
        /// Takes the pickup out of play. <see cref="TideSpawner"/> returns it to its pool before the next batch.
        /// </summary>
        public void Consume()
        {
            gameObject.SetActive(false);
        }
    }
}
