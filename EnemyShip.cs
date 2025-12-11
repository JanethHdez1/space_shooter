using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyShip : MonoBehaviour
{
    // === CONFIGURACIÓN ===
    [Header("Movement")]
    [SerializeField]
    private float moveSpeed = 3f;

    [SerializeField]
    private float rotationSpeed = 120f;

    [Header("AI Settings")]
    [SerializeField]
    private float detectionRange = 8f; // Rango para detectar balas

    [SerializeField]
    private float evadeDistance = 3f; // Distancia para evadir

    [SerializeField]
    private float orbitRadius = 4f; // Radio para orbitar la torreta

    [SerializeField]
    private float shipAvoidanceRadius = 2.5f; // Distancia para evitar otras naves

    [Header("Combat")]
    [SerializeField]
    private int pointsOnDestroy = 10;

    [SerializeField]
    private int pointsLostOnHit = -10;

    [SerializeField]
    private float attackCooldown = 2f; // Tiempo entre ataques

    [SerializeField]
    private float retreatTime = 1.5f; // Tiempo retrocediendo después de chocar

    // === MÁQUINA DE ESTADOS PRINCIPAL ===
    private enum ShipState
    {
        Spawning, 
        Patrol, 
        Attack, 
        Evade, 
        Retreat, 
        Destroyed, 
    }

    private ShipState _currentState = ShipState.Spawning;

    // === MÁQUINA DE ESTADOS DE DECISIÓN (IA) ===
    private enum AIDecision
    {
        KeepCourse, 
        EvadeBullet, 
        AttackTurret,
        AvoidShips,
    }


    private Rigidbody _rb;
    private Transform _turret;


    private Vector3 _targetPosition;
    private float _orbitAngle;
    private float _stateTimer;
    private float _attackTimer;
    private Vector3 _retreatDirection;


    private GameObject _nearestBullet;
    private bool _hasAttackedRecently;
    private Vector3 _shipAvoidanceDirection;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        // Configurar Rigidbody
        _rb.useGravity = false;
        _rb.isKinematic = false;
        _rb.mass = 1f;
        _rb.linearDamping = 1f;
        _rb.angularDamping = 2f;
        _rb.constraints =
            RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;

        // Configurar capa de colisiones
        gameObject.layer = LayerMask.NameToLayer("Enemy");

        FindTurret();
    }

    private void FindTurret()
    {
        Health[] allHealths = FindObjectsOfType<Health>();
        foreach (var health in allHealths)
        {
            if (health.gameObject.name.ToLower().Contains("turret"))
            {
                _turret = health.transform;
                break;
            }
        }
    }

    public void Initialize(Vector3 spawnPosition)
    {
        // Calcular ángulo inicial de órbita con algo de variación
        if (_turret != null)
        {
            Vector3 dirFromTurret = (spawnPosition - _turret.position).normalized;
            _orbitAngle = Mathf.Atan2(dirFromTurret.z, dirFromTurret.x) * Mathf.Rad2Deg;
            _orbitAngle += Random.Range(-30f, 30f);
        }
    }

    private void Start()
    {
        TransitionToState(ShipState.Patrol);
    }

    private void Update()
    {
        _stateTimer += Time.deltaTime;
        _attackTimer += Time.deltaTime;

        // IA: Tomar decisiones cada frame
        AIDecision decision = MakeDecision();
        ExecuteDecision(decision);

        // Ejecutar comportamiento del estado actual
        ExecuteCurrentState();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
    }


    // MÁQUINA DE ESTADOS DE DECISIÓN (IA)
    private AIDecision MakeDecision()
    {
        // No tomar decisiones si está retrocediendo
        if (_currentState == ShipState.Retreat)
        {
            return AIDecision.KeepCourse;
        }

        // Prioridad 1: Evadir balas cercanas
        _nearestBullet = FindNearestBullet();
        if (_nearestBullet != null)
        {
            float distanceToBullet = Vector3.Distance(
                transform.position,
                _nearestBullet.transform.position
            );
            if (distanceToBullet < evadeDistance)
            {
                return AIDecision.EvadeBullet;
            }
        }

        // Prioridad 2: Evitar otras naves cercanas
        if (CheckForNearbyShips())
        {
            return AIDecision.AvoidShips;
        }

        // Prioridad 3: Atacar torreta si está lista
        if (_attackTimer >= attackCooldown && !_hasAttackedRecently)
        {
            return AIDecision.AttackTurret;
        }

        // default: mantener curso actual
        return AIDecision.KeepCourse;
    }

    private void ExecuteDecision(AIDecision decision)
    {
        switch (decision)
        {
            case AIDecision.EvadeBullet:
                if (_currentState != ShipState.Evade)
                {
                    TransitionToState(ShipState.Evade);
                }
                break;

            case AIDecision.AvoidShips:
                // Ajustar la órbita o target position para evitar naves
                AdjustPathToAvoidShips();
                break;

            case AIDecision.AttackTurret:
                if (_currentState != ShipState.Attack)
                {
                    TransitionToState(ShipState.Attack);
                    _attackTimer = 0f;
                }
                break;

            case AIDecision.KeepCourse:
                // Si está evadiendo y ya pasó el peligro, volver a patrullar
                if (_currentState == ShipState.Evade && _nearestBullet == null)
                {
                    TransitionToState(ShipState.Patrol);
                }
                // Si está atacando y pasó suficiente tiempo, volver a patrullar
                else if (_currentState == ShipState.Attack && _stateTimer > 3f)
                {
                    TransitionToState(ShipState.Patrol);
                    _hasAttackedRecently = true;
                    Invoke(nameof(ResetAttackFlag), attackCooldown);
                }
                break;
        }
    }

    private GameObject FindNearestBullet()
    {
        Bullet[] bullets = FindObjectsOfType<Bullet>();
        GameObject nearest = null;
        float minDistance = detectionRange;

        foreach (var bullet in bullets)
        {
            float distance = Vector3.Distance(transform.position, bullet.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = bullet.gameObject;
            }
        }

        return nearest;
    }

    private bool CheckForNearbyShips()
    {
        EnemyShip[] allShips = FindObjectsOfType<EnemyShip>();
        _shipAvoidanceDirection = Vector3.zero;

        foreach (var ship in allShips)
        {
            if (ship == this)
                continue;

            float distance = Vector3.Distance(transform.position, ship.transform.position);
            if (distance < shipAvoidanceRadius)
            {
                // Calcular dirección de separación
                Vector3 awayFromShip = (transform.position - ship.transform.position).normalized;
                _shipAvoidanceDirection += awayFromShip / distance; 
                return true;
            }
        }

        return false;
    }

    private void AdjustPathToAvoidShips()
    {
        // Ajustar el ángulo de órbita para evitar colisiones
        if (_currentState == ShipState.Patrol)
        {
            _orbitAngle += 45f * Time.deltaTime; // Acelerar la órbita temporalmente
        }

        // Aplicar corrección al target position
        _targetPosition += _shipAvoidanceDirection.normalized * 2f;
    }

    private void ResetAttackFlag()
    {
        _hasAttackedRecently = false;
    }

    
    private void TransitionToState(ShipState newState)
    {
        OnStateExit(_currentState);

        _currentState = newState;
        _stateTimer = 0f;

        OnStateEnter(newState);

        Debug.Log($"Nave: {_currentState}");
    }

    private void OnStateEnter(ShipState state)
    {
        switch (state)
        {
            case ShipState.Patrol:
                // Comenzar a orbitar
                break;

            case ShipState.Attack:
                // Calcular posición de la torreta
                if (_turret != null)
                {
                    _targetPosition = _turret.position;
                }
                break;

            case ShipState.Evade:
                // Calcular dirección de evasión
                CalculateEvadeDirection();
                break;

            case ShipState.Retreat:
                // Calcular dirección de retroceso
                CalculateRetreatDirection();
                break;
        }
    }

    private void OnStateExit(ShipState state)
    {
        
    }

    private void ExecuteCurrentState()
    {
        switch (_currentState)
        {
            case ShipState.Patrol:
                UpdatePatrolBehavior();
                break;

            case ShipState.Attack:
                UpdateAttackBehavior();
                break;

            case ShipState.Evade:
                UpdateEvadeBehavior();
                break;

            case ShipState.Retreat:
                UpdateRetreatBehavior();
                break;
        }
    }


    // COMPORTAMIENTOS DE CADA ESTADO
    private void UpdatePatrolBehavior()
    {
        if (_turret == null)
            return;

        // Orbitar alrededor de la torreta
        _orbitAngle += (rotationSpeed * 0.5f) * Time.deltaTime;

        float radians = _orbitAngle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(radians), 0, Mathf.Sin(radians)) * orbitRadius;
        _targetPosition = _turret.position + offset;
    }

    private void UpdateAttackBehavior()
    {
        if (_turret == null)
            return;

        // Ir directo hacia la torreta
        _targetPosition = _turret.position;
    }

    private void UpdateEvadeBehavior()
    {
        
        if (_stateTimer > 1f)
        {
            TransitionToState(ShipState.Patrol);
        }
    }

    private void UpdateRetreatBehavior()
    {
        // Mantener la dirección de retroceso por el tiempo especificado
        if (_stateTimer > retreatTime)
        {
            TransitionToState(ShipState.Patrol);
        }
    }

    private void CalculateEvadeDirection()
    {
        if (_nearestBullet != null && _turret != null)
        {
            // Calcular dirección perpendicular a la bala
            Vector3 bulletDirection = _nearestBullet.GetComponent<Bullet>().direction.normalized;
            Vector3 perpendicularDir = new Vector3(-bulletDirection.z, 0, bulletDirection.x);

            // Elegir la dirección perpendicular que NO vaya hacia la torreta
            Vector3 dirToTurret = (_turret.position - transform.position).normalized;
            if (Vector3.Dot(perpendicularDir, dirToTurret) > 0)
            {
                perpendicularDir = -perpendicularDir;
            }

            _targetPosition = transform.position + perpendicularDir * 3f;
        }
    }

    private void CalculateRetreatDirection()
    {
        if (_turret != null)
        {
            // Retroceder en dirección opuesta a la torreta
            _retreatDirection = (transform.position - _turret.position).normalized;
            _targetPosition = transform.position + _retreatDirection * 4f;
        }
    }

    private void ApplyMovement()
    {
        Vector3 targetVelocity;

        if (_currentState == ShipState.Retreat)
        {
            // Movimiento de retroceso más rápido
            targetVelocity = _retreatDirection * moveSpeed * 1.5f;
        }
        else
        {
            Vector3 direction = (_targetPosition - transform.position).normalized;
            direction.y = 0;
            targetVelocity = direction * moveSpeed;
        }

        // Aplicar velocidad
        _rb.linearVelocity = targetVelocity;


        // Rotar suavemente hacia la dirección de movimiento
        if (targetVelocity != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetVelocity);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            );
        }
    }

    
    private void OnCollisionEnter(Collision collision)
    {
        // Bala
        Bullet bullet = collision.gameObject.GetComponent<Bullet>();
        if (bullet != null)
        {
            DestroyShip();
            collision.gameObject.SetActive(false);
            return;
        }

        // Torreta
        Health health = collision.gameObject.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(10f);

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddScore(pointsLostOnHit);
            }

            TransitionToState(ShipState.Retreat);
            Debug.Log($"Nave chocó con torreta! {pointsLostOnHit} puntos");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Bullet bullet = other.GetComponent<Bullet>();
        if (bullet != null)
        {
            DestroyShip();
            other.gameObject.SetActive(false);
        }
    }

    private void DestroyShip()
    {
        if (_currentState == ShipState.Destroyed)
            return;

        TransitionToState(ShipState.Destroyed);

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(pointsOnDestroy);
            Debug.Log($"Nave destruida! +{pointsOnDestroy} puntos");
        }

        Destroy(gameObject);
    }

    public void SetPointsOnDestroy(int points)
    {
        pointsOnDestroy = points;
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shipAvoidanceRadius);

        if (_turret != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_turret.position, orbitRadius);
        }
    }
}
