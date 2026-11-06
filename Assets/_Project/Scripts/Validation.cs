namespace StarJourney
{
    /// <summary>
    /// Checks that components use in <c>Awake</c> to make sure their serialized references are set.
    /// </summary>
    internal static class Validation
    {
        /// <summary>
        /// Whether <paramref name="items"/> has at least one element and none of its elements is missing.
        /// </summary>
        /// <typeparam name="T">Type of the referenced objects.</typeparam>
        /// <param name="items">The serialized array to check.</param>
        /// <returns><c>true</c> if the array is not empty and every element is set and not destroyed.</returns>
        internal static bool IsFilled<T>(T[] items)
            where T : UnityEngine.Object
        {
            if (items == null || items.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == null)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
