using UnityEngine;

namespace StarJourney.Gameplay
{
    /// <summary>
    /// A coloured ring lying on the sand under a prop, so that pickups and hazards can be told apart from the high
    /// camera. Lives on a child quad of the prop and follows the prop's position over the sand. Stays hidden until a
    /// <see cref="Beach"/> is attached.
    /// </summary>
    [RequireComponent(typeof(MeshRenderer))]
    public sealed class PropRing : MonoBehaviour
    {
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        [SerializeField]
        [Tooltip("Colour of the ring. Pickups and hazards use different colours.")]
        private Color _color = new Color(0.784f, 0.941f, 0.290f, 0.85f);

        [SerializeField, Min(0.1f)]
        [Tooltip("Outer radius of the ring on the sand, in world units.")]
        private float _radius = 1.3f;

        [SerializeField, Min(0f)]
        [Tooltip("Height of the ring above the sand along the sand's normal. Keeps it from sinking into the terrain.")]
        private float _lift = 0.04f;

        [SerializeField]
        [Tooltip("Centre of the ring in the prop's local space, as (x, z): the middle of the model's footprint, "
            + "which is not at the origin of every prop.")]
        private Vector2 _footprintCenter;

        private MeshRenderer _renderer;
        private MaterialPropertyBlock _properties;
        private Beach _beach;

        /// <summary>
        /// Puts the ring on the sand of <paramref name="beach"/> under the prop and shows it.
        /// </summary>
        /// <param name="beach">The beach the prop lies on.</param>
        public void Attach(Beach beach)
        {
            if (!enabled)
            {
                return;
            }

            _beach = beach;
            Place();
            _renderer.enabled = true;
        }

        private void Awake()
        {
            _renderer = GetComponent<MeshRenderer>();
            _renderer.enabled = false;
            if (transform.parent == null || _renderer.sharedMaterial == null)
            {
                Debug.LogError($"{nameof(PropRing)} on '{name}' needs a material and must be a child of its prop.",
                    this);
                enabled = false;
                return;
            }

            // The radius is in world units; some props are scaled at their root.
            Vector3 parentScale = transform.parent.lossyScale;
            float diameter = 2f * _radius;
            transform.localScale = new Vector3(diameter / parentScale.x, diameter / parentScale.y,
                diameter / parentScale.z);

            _properties = new MaterialPropertyBlock();
            _properties.SetColor(ColorId, _color);
            _renderer.SetPropertyBlock(_properties);
        }

        // The prop's transform holds its rendered pose by now.
        private void LateUpdate()
        {
            if (_beach != null)
            {
                Place();
            }
        }

        private void Place()
        {
            Vector3 center = transform.parent.TransformPoint(new Vector3(_footprintCenter.x, 0f, _footprintCenter.y));
            float sand = _beach.SandHeight(center.x, center.z);
            Vector3 normal = _beach.SandNormal(center.x, center.z);

            // The quad faces -Z; its shader draws both sides.
            transform.SetPositionAndRotation(new Vector3(center.x, sand, center.z) + normal * _lift,
                Quaternion.FromToRotation(Vector3.back, normal));
        }
    }
}
