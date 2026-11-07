using StarJourney.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace StarJourney.UI
{
    /// <summary>
    /// Speedometer-style gauge around the seal on screen that fills with the power of the jump being charged:
    /// a quarter arc of coloured segments, from green at two o'clock to red at five o'clock, over a dim track.
    /// Shown only while the seal charges.
    /// </summary>
    public sealed class ChargeGauge : MaskableGraphic
    {
        // Angles are measured clockwise from twelve o'clock, as on a clock face.
        private const float MaxDegreesPerStep = 3f;

        [SerializeField]
        [Tooltip("The seal whose charge is shown and which the gauge follows on screen.")]
        private FurSeal _seal;

        [SerializeField, Min(0f)]
        [Tooltip("Inner radius of the arc in canvas units; clears the seal's silhouette.")]
        private float _radius = 60f;

        [SerializeField, Min(1f)]
        [Tooltip("Width of the arc in canvas units.")]
        private float _thickness = 14f;

        [SerializeField, Range(0f, 360f)]
        [Tooltip("Where the arc starts, in degrees clockwise from twelve o'clock (60 is two o'clock).")]
        private float _startAngle = 60f;

        [SerializeField, Range(1f, 360f)]
        [Tooltip("How far the arc runs clockwise, in degrees (90 ends at five o'clock).")]
        private float _sweepAngle = 90f;

        [SerializeField, Range(0f, 10f)]
        [Tooltip("Gap between two segments, in degrees.")]
        private float _gapAngle = 3f;

        [SerializeField]
        [Tooltip("Colours of the segments in the order they fill, from the start of the arc.")]
        private Color[] _segmentColors =
        {
            new Color32(0x3D, 0xBE, 0x3A, 0xFF),
            new Color32(0xA5, 0xD2, 0x3A, 0xFF),
            new Color32(0xF2, 0xD3, 0x1B, 0xFF),
            new Color32(0xF3, 0x8A, 0x1E, 0xFF),
            new Color32(0xE5, 0x31, 0x2B, 0xFF),
        };

        [SerializeField, Range(0f, 1f)]
        [Tooltip("Brightness of the unfilled track relative to its segment's colour.")]
        private float _trackBrightness = 0.35f;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("Opacity of the unfilled track.")]
        private float _trackAlpha = 0.75f;

        [SerializeField]
        [Tooltip("Colour of the outline around every segment, so the gauge reads on sand and on water.")]
        private Color _outlineColor = new Color(0.08f, 0.06f, 0.05f, 0.85f);

        [SerializeField, Min(0f)]
        [Tooltip("Width of the outline in canvas units.")]
        private float _outlineWidth = 2f;

        private Camera _camera;
        private bool _isShown;
        private float _shownPower;

        /// <summary>
        /// Whether the gauge is drawn.
        /// </summary>
        public bool IsShown => _isShown;

        /// <summary>
        /// The charge power the gauge shows, 0..1.
        /// </summary>
        public float ShownPower => _shownPower;

        /// <inheritdoc/>
        protected override void Awake()
        {
            base.Awake();

            // Graphics also run in edit mode; there the gauge only stays hidden.
            if (!Application.isPlaying)
            {
                return;
            }

            Canvas parentCanvas = canvas;
            _camera = parentCanvas != null ? parentCanvas.rootCanvas.worldCamera : null;
            if (_seal == null || _camera == null || _segmentColors == null || _segmentColors.Length == 0)
            {
                Debug.LogError($"{nameof(ChargeGauge)} on '{name}' needs the seal, segment colours and a canvas "
                    + "with a camera.", this);
                enabled = false;
            }
        }

        /// <inheritdoc/>
        protected override void OnEnable()
        {
            base.OnEnable();
            Hide();
            if (_seal != null)
            {
                _seal.ChargeStarted += Show;
                _seal.ChargeEnded += Hide;
            }
        }

        /// <inheritdoc/>
        protected override void OnDisable()
        {
            if (_seal != null)
            {
                _seal.ChargeStarted -= Show;
                _seal.ChargeEnded -= Hide;
            }

            base.OnDisable();
        }

        /// <inheritdoc/>
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            int count = _segmentColors.Length;
            float span = _sweepAngle / count;
            float inner = _radius;
            float outer = _radius + _thickness;
            float outlineAngle = _outlineWidth / (inner + _thickness * 0.5f) * Mathf.Rad2Deg;
            float filled = _shownPower * count;
            for (int i = 0; i < count; i++)
            {
                float from = _startAngle + i * span + _gapAngle * 0.5f;
                float to = _startAngle + (i + 1) * span - _gapAngle * 0.5f;
                Color full = _segmentColors[i] * color;
                Color track = Color.Lerp(Color.black, full, _trackBrightness);
                track.a = _trackAlpha * full.a;

                // Later quads draw over earlier ones: outline, then track, then fill.
                AddBand(vh, from - outlineAngle, to + outlineAngle, inner - _outlineWidth, outer + _outlineWidth,
                    _outlineColor * color);
                AddBand(vh, from, to, inner, outer, track);
                float lit = Mathf.Clamp01(filled - i);
                if (lit > 0f)
                {
                    AddBand(vh, from, Mathf.Lerp(from, to, lit), inner, outer, full);
                }
            }
        }

        private static void AddBand(VertexHelper vh, float from, float to, float inner, float outer, Color32 tint)
        {
            int steps = Mathf.Max(1, Mathf.CeilToInt((to - from) / MaxDegreesPerStep));
            int first = vh.currentVertCount;
            for (int k = 0; k <= steps; k++)
            {
                float angle = Mathf.Lerp(from, to, (float)k / steps) * Mathf.Deg2Rad;
                var direction = new Vector3(Mathf.Sin(angle), Mathf.Cos(angle), 0f);
                vh.AddVert(direction * inner, tint, Vector4.zero);
                vh.AddVert(direction * outer, tint, Vector4.zero);
                if (k > 0)
                {
                    int v = first + 2 * k;
                    vh.AddTriangle(v - 2, v - 1, v + 1);
                    vh.AddTriangle(v - 2, v + 1, v);
                }
            }
        }

        private void LateUpdate()
        {
            if (!_isShown)
            {
                return;
            }

            // The seal is destroyed when it dies and when the game ends.
            if (_seal == null)
            {
                Hide();
                return;
            }

            Place();
            float power = _seal.ChargePower;
            if (power != _shownPower)
            {
                _shownPower = power;
                SetVerticesDirty();
            }
        }

        private void Show()
        {
            _isShown = true;
            _shownPower = _seal.ChargePower;
            canvasRenderer.cull = false;
            Place();

            // A culled graphic skips its rebuilds, so everything is rebuilt once it is drawn again.
            SetAllDirty();
        }

        private void Hide()
        {
            _isShown = false;
            canvasRenderer.cull = true;
        }

        private void Place()
        {
            Vector3 screenPoint = _camera.WorldToScreenPoint(_seal.BodyCentre);
            var parent = (RectTransform)rectTransform.parent;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPoint, _camera,
                out Vector2 localPoint))
            {
                rectTransform.localPosition = localPoint;
            }
        }
    }
}
