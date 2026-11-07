using UnityEngine;

namespace StarJourney.Gameplay
{
    /// <summary>
    /// A coloured ring lying on the sand under a prop, so that pickups and hazards can be told apart from the high
    /// camera. Lives on a child quad of the prop and follows the prop's position over the sand. Stays hidden until a
    /// <see cref="Beach"/> is attached, fades out where the water covers the sand and dims while the prop is carried.
    /// </summary>
    [RequireComponent(typeof(MeshRenderer))]
    public sealed class PropRing : MonoBehaviour
    {
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int FadeId = Shader.PropertyToID("_Fade");

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

        [SerializeField, Min(0.01f)]
        [Tooltip("Depth of the sand under the water surface at which the ring has faded out completely.")]
        private float _fadeDepth = 0.3f;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("Opacity of the ring while the water carries or moves the prop.")]
        private float _carriedFade = 0.4f;

        private MeshRenderer _renderer;
        private MaterialPropertyBlock _properties;
        private Beach _beach;
        private float _surfaceHeight = float.NegativeInfinity;
        private bool _isCarried;

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

        /// <summary>
        /// Hides the ring and forgets the beach and the water.
        /// </summary>
        public void Detach()
        {
            _beach = null;
            _surfaceHeight = float.NegativeInfinity;
            _isCarried = false;
            if (_renderer != null)
            {
                _renderer.enabled = false;
            }
        }

        /// <summary>
        /// Tells the ring where the water is and whether the prop is being moved by it. Applied in the next frame.
        /// </summary>
        /// <param name="surfaceHeight">World height of the water surface.</param>
        /// <param name="carried">Whether the water carries, throws or drags the prop.</param>
        public void SetWater(float surfaceHeight, bool carried)
        {
            _surfaceHeight = surfaceHeight;
            _isCarried = carried;
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
            _properties.SetFloat(FadeId, 1f);
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

            // Fully visible on dry sand, gone where the sand is _fadeDepth under the water: no pop at the edge.
            float fade = Mathf.Clamp01((sand - _surfaceHeight) / _fadeDepth + 1f) * (_isCarried ? _carriedFade : 1f);
            _properties.SetFloat(FadeId, fade);
            _renderer.SetPropertyBlock(_properties);
        }
    }
}
