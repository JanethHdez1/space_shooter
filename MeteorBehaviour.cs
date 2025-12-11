using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class MeteorBehaviour : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float minSpeed = 3f;
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float speedMultiplier = 0.5f;

    [Header("Score Configuration")]
    [SerializeField] private int pointsOnDestroy = 10;
    [SerializeField] private int pointsLostOnHit = -10;

    public Vector3 Direction { get; set; }

    private Rigidbody _rb;
    private SphereCollider _col;

    private float _speed;
    private MeteorData[] _children;
    private bool _exploded;
    private float _timer;
    private bool _hasHitTurret;

    private void Awake()
    {
        _speed = Random.Range(minSpeed, maxSpeed) * speedMultiplier;
        _rb = GetComponent<Rigidbody>();
        _col = GetComponent<SphereCollider>();
        _rb.maxLinearVelocity = _speed;
        SetupExplodingAnimation();
    }

    private void SetupExplodingAnimation()
    {
        var childrenTransforms = GetComponentsInChildren<Transform>();
        _children = new MeteorData[childrenTransforms.Length];
        for (var i = 0; i < childrenTransforms.Length; i++)
        {
            var child = childrenTransforms[i];
            _children[i] = new MeteorData
            {
                Transform = child,
                ExplodeDirection = child.localPosition.normalized
            };
        }
    }

    private void Start()
    {
        _rb.linearVelocity = Direction * _speed;
    }

    private void Update()
    {
        MeteorExplosionAnimation();
    }

    private void MeteorExplosionAnimation()
    {
        if (!_exploded) return;

        _timer += Time.deltaTime;
        var isTimerFirstStop = _timer >= 2f;
        var isTimerSecondStop = _timer >= 4f;

        foreach (var child in _children)
        {
            var movementDirection = Direction * (_speed * 0.5f) + child.ExplodeDirection;
            child.Transform.Translate(movementDirection * Time.deltaTime, Space.World);
            child.Transform.Rotate(
                Random.insideUnitSphere,
                Random.Range(15f, 720f) * Time.deltaTime,
                Space.World);

            if (isTimerFirstStop)
            {
                var scale = child.Transform.localScale;
                child.Transform.localScale = Vector3.Lerp(scale, Vector3.zero, Time.deltaTime * 5f);
            }
        }

        if (isTimerSecondStop) DestroyAll();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (_exploded) return;

        Bullet bullet = other.gameObject.GetComponent<Bullet>();
        if (bullet != null)
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddScore(pointsOnDestroy);
            }

            other.gameObject.SetActive(false);
            Debug.Log($"Meteorito destruido por bala! +{pointsOnDestroy} puntos");
        }

        Health health = other.gameObject.GetComponent<Health>();
        if (health != null && !_hasHitTurret)
        {
            health.TakeDamage(damage);
            _hasHitTurret = true;

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddScore(pointsLostOnHit);
            }

            Debug.Log($"Meteorito chocó con torreta! -{damage} HP, {pointsLostOnHit} puntos");
        }

        _col.enabled = false;
        _exploded = true;
        foreach (var child in _children)
        {
            child.Transform.parent = transform.parent;
        }
    }

    private void DestroyAll()
    {
        foreach (var child in _children)
        {
            if (child.Transform != null)
            {
                Destroy(child.Transform.gameObject);
            }
        }
        Destroy(gameObject);
    }
}

public struct MeteorData
{
    public Transform Transform;
    public Vector3 ExplodeDirection;
}
