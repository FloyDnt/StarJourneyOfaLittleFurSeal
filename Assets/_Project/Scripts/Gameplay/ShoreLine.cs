using UnityEngine;

namespace StarJourney.Gameplay
{
    /// <summary>
    /// The sand along a straight line from a point on the beach toward the sea, sampled once. The profile is made to
    /// fall all the way down the line, so that every water height meets it at exactly one point: the water's edge
    /// moves along the line smoothly, even across flat or bumpy sand.
    /// </summary>
    public sealed class ShoreLine
    {
        private readonly float[] _heights;
        private readonly float _step;
        private Vector3 _origin;
        private Vector3 _direction;

        /// <summary>
        /// Creates an empty line with room for its samples. No allocation happens after this.
        /// </summary>
        /// <param name="length">Distance from the origin to the far end of the line.</param>
        /// <param name="step">Distance between two samples.</param>
        public ShoreLine(float length, float step)
        {
            _step = step;
            _heights = new float[Mathf.CeilToInt(length / step) + 1];
        }

        /// <summary>
        /// Distance from the origin to the far end of the line.
        /// </summary>
        public float Length => (_heights.Length - 1) * _step;

        /// <summary>
        /// Samples the sand from <paramref name="origin"/> toward the sea of <paramref name="beach"/>.
        /// </summary>
        /// <param name="beach">The beach to sample.</param>
        /// <param name="origin">Start of the line; only its horizontal position is used.</param>
        /// <param name="minSlope">Least drop per unit of distance the profile keeps toward the sea.</param>
        public void Build(Beach beach, Vector3 origin, float minSlope)
        {
            _origin = new Vector3(origin.x, 0f, origin.z);
            _direction = beach.SeaDirection;
            float minDrop = minSlope * _step;
            for (int i = 0; i < _heights.Length; i++)
            {
                Vector3 point = _origin + _direction * (i * _step);
                float sand = beach.SandHeight(point.x, point.z);
                _heights[i] = i == 0 ? sand : Mathf.Min(sand, _heights[i - 1] - minDrop);
            }
        }

        /// <summary>
        /// Height of the profile at a distance along the line. At the origin it is the sand height there.
        /// </summary>
        /// <param name="distance">Distance from the origin, clamped to the line.</param>
        /// <returns>The world height of the profile.</returns>
        public float HeightAt(float distance)
        {
            float index = Mathf.Clamp(distance / _step, 0f, _heights.Length - 1);
            int below = Mathf.Min((int)index, _heights.Length - 2);
            return Mathf.Lerp(_heights[below], _heights[below + 1], index - below);
        }

        /// <summary>
        /// Distance along the line at which the profile has fallen to <paramref name="height"/>.
        /// </summary>
        /// <param name="height">World height to find.</param>
        /// <returns>The distance; 0 if the origin is already that low, <see cref="Length"/> if the line never
        /// gets that low.</returns>
        public float DistanceTo(float height)
        {
            if (height >= _heights[0])
            {
                return 0f;
            }

            for (int i = 1; i < _heights.Length; i++)
            {
                if (_heights[i] <= height)
                {
                    float fraction = (_heights[i - 1] - height) / (_heights[i - 1] - _heights[i]);
                    return (i - 1 + fraction) * _step;
                }
            }

            return Length;
        }

        /// <summary>
        /// Horizontal position of a point on the line, at the given height.
        /// </summary>
        /// <param name="distance">Distance from the origin.</param>
        /// <param name="height">World height of the returned point.</param>
        /// <returns>The point.</returns>
        public Vector3 PointAt(float distance, float height)
        {
            Vector3 point = _origin + _direction * distance;
            point.y = height;
            return point;
        }
    }
}
