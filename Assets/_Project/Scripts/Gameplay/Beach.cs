using UnityEngine;

namespace StarJourney.Gameplay
{
    /// <summary>
    /// The sand of the beach: its height and slope anywhere on the terrain, and the direction toward the sea.
    /// Pure math over the terrain's heightfield, which is also its collider.
    /// </summary>
    public sealed class Beach
    {
        private readonly Terrain _terrain;
        private readonly TerrainData _data;
        private readonly Vector3 _origin;
        private readonly Vector3 _size;

        /// <summary>
        /// Creates the beach for a terrain that does not move.
        /// </summary>
        /// <param name="terrain">The terrain the props lie on.</param>
        /// <param name="seaDirection">Direction from the beach to the sea; only its horizontal part is used.</param>
        public Beach(Terrain terrain, Vector3 seaDirection)
        {
            _terrain = terrain;
            _data = terrain.terrainData;
            _origin = terrain.GetPosition();
            _size = _data.size;
            seaDirection.y = 0f;
            SeaDirection = seaDirection.normalized;
        }

        /// <summary>
        /// Horizontal unit direction from the beach to the sea.
        /// </summary>
        public Vector3 SeaDirection { get; }

        /// <summary>
        /// World height of the sand at a horizontal position.
        /// </summary>
        /// <param name="x">World x.</param>
        /// <param name="z">World z.</param>
        /// <returns>The world y of the terrain surface.</returns>
        public float SandHeight(float x, float z)
        {
            // SampleHeight returns the height relative to the terrain's own position.
            return _terrain.SampleHeight(new Vector3(x, 0f, z)) + _origin.y;
        }

        /// <summary>
        /// Unit normal of the sand at a horizontal position.
        /// </summary>
        /// <param name="x">World x.</param>
        /// <param name="z">World z.</param>
        /// <returns>The interpolated normal of the terrain surface.</returns>
        public Vector3 SandNormal(float x, float z)
        {
            return _data.GetInterpolatedNormal((x - _origin.x) / _size.x, (z - _origin.z) / _size.z);
        }
    }
}
