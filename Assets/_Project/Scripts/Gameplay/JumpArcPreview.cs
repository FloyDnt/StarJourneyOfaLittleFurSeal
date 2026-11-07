using UnityEngine;

namespace StarJourney.Gameplay
{
    /// <summary>
    /// Draws the path a jump will take, simulated with the physics step and stopped where the seal would first touch
    /// something, as a line along the ground under the seal.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public sealed class JumpArcPreview : MonoBehaviour
    {
        [SerializeField, Min(0.01f)]
        [Tooltip("Radius of the sphere swept along the path: a little less than the seal's collider, so the ground "
            + "the seal stands on is not hit at the start.")]
        private float _probeRadius = 1.15f;

        [SerializeField]
        [Tooltip("Height of the seal's collider centre above the ground it rests on: the line is drawn this much "
            + "below the simulated centre, along the ground.")]
        private float _groundOffset = 1.2f;

        [SerializeField, Min(1)]
        [Tooltip("Physics steps between two points of the line.")]
        private int _stepsPerPoint = 2;

        [SerializeField, Min(1)]
        [Tooltip("Physics steps simulated at most.")]
        private int _maxSteps = 120;

        [SerializeField, Min(0f)]
        [Tooltip("Points closer than this to the start are not drawn, so the line starts beside the seal.")]
        private float _startSkip = 1f;

        [SerializeField]
        [Tooltip("Colour of the line at zero power.")]
        private Color _lowPowerColor = new Color(1f, 0.953f, 0.627f, 0.9f);

        [SerializeField]
        [Tooltip("Colour of the line at full power.")]
        private Color _fullPowerColor = new Color(1f, 0.478f, 0.239f, 0.9f);

        [SerializeField, Min(0f)]
        [Tooltip("Width of the line's start at zero power.")]
        private float _minWidth = 0.1f;

        [SerializeField, Min(0f)]
        [Tooltip("Width of the line's start at full power.")]
        private float _maxWidth = 0.16f;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("Width of the line's end as a share of its start width.")]
        private float _endWidthFactor = 0.55f;

        [SerializeField]
        [Tooltip("Layers the previewed jump stops at.")]
        private LayerMask _mask = 1;

        private LineRenderer _line;
        private Vector3[] _centres;
        private Vector3[] _positions;
        private Vector3 _endPoint;

        /// <summary>
        /// Last point of the line: the ground under the seal where the previewed jump ends.
        /// </summary>
        public Vector3 EndPoint => _endPoint;

        /// <summary>
        /// Whether the line is shown.
        /// </summary>
        public bool IsVisible => _line.enabled;

        /// <summary>
        /// Simulates a jump and shows its path.
        /// </summary>
        /// <param name="start">World position of the seal's collider centre at launch.</param>
        /// <param name="velocity">Launch velocity.</param>
        /// <param name="power">Jump power, 0..1, for the line's colour and width.</param>
        public void Show(Vector3 start, Vector3 velocity, float power)
        {
            int count = Simulate(start, velocity);

            // Keep at least the last two points, so even the shortest jump draws a line.
            int first = 0;
            while (first < count - 2 && (_centres[first] - start).sqrMagnitude < _startSkip * _startSkip)
            {
                first++;
            }

            Vector3 down = new Vector3(0f, _groundOffset, 0f);
            int drawn = count - first;
            for (int i = 0; i < drawn; i++)
            {
                _positions[i] = _centres[first + i] - down;
            }

            _endPoint = _positions[drawn - 1];
            _line.positionCount = drawn;
            _line.SetPositions(_positions);

            Color color = Color.Lerp(_lowPowerColor, _fullPowerColor, power);
            _line.startColor = color;
            _line.endColor = color;
            float width = Mathf.Lerp(_minWidth, _maxWidth, power);
            _line.startWidth = width;
            _line.endWidth = width * _endWidthFactor;
            _line.enabled = true;
        }

        /// <summary>
        /// Hides the line.
        /// </summary>
        public void Hide()
        {
            _line.enabled = false;
        }

        private void Awake()
        {
            _line = GetComponent<LineRenderer>();
            int points = (_maxSteps + _stepsPerPoint - 1) / _stepsPerPoint + 1;
            _centres = new Vector3[points];
            _positions = new Vector3[points];
            _line.useWorldSpace = true;
            _line.positionCount = 0;
            _line.enabled = false;
        }

        // Steps the launch the way the physics engine does (semi-implicit Euler: velocity first, then position) and
        // stores every few steps; sweeps each stored segment after the first and ends the path at the first hit.
        private int Simulate(Vector3 start, Vector3 velocity)
        {
            float dt = Time.fixedDeltaTime;
            Vector3 gravity = Physics.gravity;
            Vector3 position = start;
            _centres[0] = start;
            int count = 1;
            for (int step = 1; step <= _maxSteps; step++)
            {
                velocity += gravity * dt;
                position += velocity * dt;
                if (step % _stepsPerPoint != 0 && step != _maxSteps)
                {
                    continue;
                }

                if (count >= 2)
                {
                    Vector3 from = _centres[count - 1];
                    Vector3 segment = position - from;
                    float length = segment.magnitude;
                    if (length > 0f && Physics.SphereCast(from, _probeRadius, segment / length, out RaycastHit hit,
                        length, _mask, QueryTriggerInteraction.Ignore))
                    {
                        _centres[count++] = from + segment / length * hit.distance;
                        return count;
                    }
                }

                _centres[count++] = position;
            }

            return count;
        }
    }
}
