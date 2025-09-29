using System.Collections;
using UnityEngine;
using YJ.Interaction;

namespace YJ.Environment
{
    [RequireComponent(typeof(Rigidbody))]
    public class FloatingPlatform : MonoBehaviour
    {
        private static readonly int ColorProperty = Shader.PropertyToID("_Color");
        private static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");

        [Header("Movement")]
        [SerializeField] private Transform startPoint;
        [SerializeField] private Transform endPoint;
        [SerializeField] private float travelTime = 3f;
        [SerializeField] private AnimationCurve travelCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [SerializeField] private bool returnAfterReachEnd = true;
        [SerializeField] private float waitTimeAtEachEnd = 1f;

        [Header("Activation")]
        [SerializeField] private float activationRange = 3f;
        [SerializeField] private KeyCode activationKey = KeyCode.F;
        [SerializeField] private string requiredKeyId;

        [Header("Visuals")]
        [SerializeField] private Renderer platformRenderer;
        [SerializeField] private Color inactiveColor = Color.red;
        [SerializeField] private Color readyColor = Color.green;
        [SerializeField] private Color movingColor = new Color(0.5f, 1f, 0.5f);

        private Vector3 _startPosition;
        private Vector3 _endPosition;
        private Coroutine _movementRoutine;
        private Rigidbody _rigidbody;
        private readonly MaterialPropertyBlock _propertyBlock = new MaterialPropertyBlock();
        private bool _isAtStart = true;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.isKinematic = true;

            if (!platformRenderer)
            {
                platformRenderer = GetComponentInChildren<Renderer>();
            }
        }

        private void Start()
        {
            CacheEndpoints();
            transform.position = _startPosition;
            _isAtStart = true;
            UpdatePlatformColor();
        }

        private void Update()
        {
            var keyring = PlayerKeyring.LocalPlayer;
            bool hasKey = string.IsNullOrWhiteSpace(requiredKeyId) || (keyring != null && keyring.HasKey(requiredKeyId));
            bool isMoving = _movementRoutine != null;

            if (!isMoving)
            {
                UpdatePlatformColor(hasKey);
            }
            else
            {
                UpdatePlatformColor(hasKey ? movingColor : inactiveColor);
            }

            if (!hasKey || isMoving)
            {
                return;
            }

            if (!PlayerKeyring.TryGetPlayerPosition(out var playerPosition))
            {
                return;
            }

            float sqrDistance = (playerPosition - transform.position).sqrMagnitude;
            if (sqrDistance > activationRange * activationRange)
            {
                return;
            }

            if (Input.GetKeyDown(activationKey))
            {
                _movementRoutine = StartCoroutine(MovePlatformRoutine());
            }
        }

        private IEnumerator MovePlatformRoutine()
        {
            CacheEndpoints();
            Vector3 from = _isAtStart ? _startPosition : _endPosition;
            Vector3 to = _isAtStart ? _endPosition : _startPosition;

            yield return MoveBetweenPoints(from, to);
            _isAtStart = !_isAtStart;

            if (returnAfterReachEnd && !_isAtStart)
            {
                yield return new WaitForSeconds(waitTimeAtEachEnd);
                yield return MoveBetweenPoints(_endPosition, _startPosition);
                _isAtStart = true;
            }

            yield return new WaitForSeconds(waitTimeAtEachEnd);
            _movementRoutine = null;
        }

        private IEnumerator MoveBetweenPoints(Vector3 from, Vector3 to)
        {
            float elapsed = 0f;
            float duration = Mathf.Max(0.01f, travelTime);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float curveT = travelCurve.Evaluate(t);
                Vector3 targetPosition = Vector3.LerpUnclamped(from, to, curveT);
                _rigidbody.MovePosition(targetPosition);
                yield return null;
            }

            _rigidbody.MovePosition(to);
        }

        private void CacheEndpoints()
        {
            _startPosition = startPoint ? startPoint.position : transform.position;
            _endPosition = endPoint ? endPoint.position : transform.position;
        }

        private void OnValidate()
        {
            if (travelTime < 0f)
            {
                travelTime = 0f;
            }

            if (activationRange < 0f)
            {
                activationRange = 0f;
            }

            if (waitTimeAtEachEnd < 0f)
            {
                waitTimeAtEachEnd = 0f;
            }
        }

        private void OnDrawGizmos()
        {
            Vector3 startPos = startPoint ? startPoint.position : transform.position;
            Vector3 endPos = endPoint ? endPoint.position : startPos;

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(startPos, 0.15f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(endPos, 0.15f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(startPos, endPos);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, activationRange);
        }

        private void UpdatePlatformColor()
        {
            var keyring = PlayerKeyring.LocalPlayer;
            bool hasKey = string.IsNullOrWhiteSpace(requiredKeyId) || (keyring != null && keyring.HasKey(requiredKeyId));
            UpdatePlatformColor(hasKey ? readyColor : inactiveColor);
        }

        private void UpdatePlatformColor(bool hasKey)
        {
            UpdatePlatformColor(hasKey ? readyColor : inactiveColor);
        }

        private void UpdatePlatformColor(Color color)
        {
            if (!platformRenderer)
            {
                return;
            }

            platformRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(ColorProperty, color);
            _propertyBlock.SetColor(BaseColorProperty, color);
            platformRenderer.SetPropertyBlock(_propertyBlock);
        }
    }
}
