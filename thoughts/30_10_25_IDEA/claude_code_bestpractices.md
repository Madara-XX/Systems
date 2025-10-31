w🎮 Unity Development Guidelines for Claude Code
Best Practices for RoombaRampage & Clean Unity Projects

📁 PROJECT STRUCTURE
Folder Hierarchy
Assets/
├── _Project/                          # All project-specific content
│   ├── Scenes/
│   │   ├── MainMenu.unity
│   │   ├── Arena_LivingRoom.unity
│   │   ├── ChargingStation.unity
│   │   └── TestScenes/                # Isolated test scenes
│   ├── Scripts/
│   │   ├── Core/                      # Essential systems
│   │   │   ├── GameManager.cs
│   │   │   ├── RunManager.cs
│   │   │   └── SaveSystem.cs
│   │   ├── Player/
│   │   │   ├── PlayerController.cs
│   │   │   ├── PlayerHealth.cs
│   │   │   └── PlayerStats.cs
│   │   ├── Combat/
│   │   │   ├── Weapon.cs
│   │   │   ├── Projectile.cs
│   │   │   ├── DamageSystem.cs
│   │   │   └── IDamageable.cs         # Interfaces here
│   │   ├── Enemies/
│   │   │   ├── EnemyBase.cs
│   │   │   ├── SwarmBug.cs
│   │   │   └── BossController.cs
│   │   ├── Progression/
│   │   │   ├── XPSystem.cs
│   │   │   ├── LevelUpManager.cs
│   │   │   └── SkillManager.cs
│   │   ├── UI/
│   │   │   ├── HUDController.cs
│   │   │   ├── LevelUpUI.cs
│   │   │   └── PauseMenu.cs
│   │   ├── Audio/
│   │   │   ├── AudioManager.cs
│   │   │   └── MusicController.cs
│   │   ├── Utilities/
│   │   │   ├── ObjectPool.cs
│   │   │   ├── Extensions.cs
│   │   │   └── Helpers.cs
│   │   └── Editor/                    # Editor-only scripts
│   │       ├── SkillEditor.cs
│   │       └── DebugTools.cs
│   ├── ScriptableObjects/
│   │   ├── Skills/                    # Skill definitions
│   │   ├── Weapons/                   # Weapon definitions
│   │   ├── Enemies/                   # Enemy definitions
│   │   ├── Waves/                     # Wave configurations
│   │   └── Config/                    # Game config
│   ├── Prefabs/
│   │   ├── Player/
│   │   ├── Enemies/
│   │   ├── Projectiles/
│   │   ├── VFX/
│   │   ├── UI/
│   │   └── Pickups/
│   ├── Materials/
│   ├── Textures/
│   ├── Models/
│   ├── Audio/
│   │   ├── SFX/
│   │   └── Music/
│   ├── VFX/
│   └── Animations/
├── Plugins/                           # Third-party assets
└── Settings/                          # Project settings
Key Principles:

✅ Single _Project folder for all game content (easy to find, easy to backup)
✅ Group by feature/system, not by asset type
✅ Test scenes in separate folder (don't clutter main scenes)
✅ ScriptableObjects in dedicated folder with subfolders by type


💻 CODING STANDARDS
Naming Conventions
C# Classes & Files
csharp// Classes: PascalCase
public class PlayerController { }
public class EnemySpawner { }

// Interfaces: I + PascalCase
public interface IDamageable { }
public interface IPoolable { }

// ScriptableObjects: PascalCase (no "SO" suffix needed)
[CreateAssetMenu(fileName = "New Skill", menuName = "RoombaRampage/Skill")]
public class Skill : ScriptableObject { }

// Enums: PascalCase (singular)
public enum WeaponType { Laser, Bullet, Missile }
public enum EnemyState { Idle, Chasing, Attacking, Dead }

// File names match class names exactly
// PlayerController.cs contains class PlayerController
Variables & Fields
csharp// Public fields: PascalCase (shown in Inspector)
public float MoveSpeed = 5f;
public int MaxHealth = 100;

// Private fields: camelCase with underscore
private float _currentSpeed;
private int _killCount;

// Serialized private: camelCase with underscore
[SerializeField] private Rigidbody _rigidbody;
[SerializeField] private Transform _firePoint;

// Properties: PascalCase
public int CurrentHealth { get; private set; }
public bool IsAlive => CurrentHealth > 0;

// Constants: SCREAMING_SNAKE_CASE
private const float MAX_FIRE_RATE = 10f;
private const int POOL_SIZE = 100;

// Local variables: camelCase
float deltaSpeed = acceleration * Time.deltaTime;
int enemyCount = enemies.Count;
Methods
csharp// Methods: PascalCase, verb-first
public void TakeDamage(int amount) { }
public void FireWeapon() { }
private void UpdateMovement() { }
private bool CanFire() { }

// Unity messages: Exactly as Unity defines them
private void Awake() { }
private void Start() { }
private void Update() { }
private void OnCollisionEnter(Collision collision) { }
```

### GameObjects & Prefabs (in Unity Editor)
```
// GameObjects in scene: PascalCase with descriptive names
Player
MainCamera
SpawnPoints
EnemySpawner_LivingRoom

// Prefabs: PascalCase, descriptive
Projectile_Laser
Enemy_SwarmBug
VFX_Explosion
UI_LevelUpPanel

// Avoid generic names like "GameObject", "Cube", "Sphere"

Code Structure & Patterns
1. Component-Based Architecture
csharp// ✅ GOOD: Single responsibility, composable
public class Health : MonoBehaviour
{
    [SerializeField] private int _maxHealth = 100;
    private int _currentHealth;
    
    public event Action OnDeath;
    public event Action<int> OnHealthChanged;
    
    private void Awake()
    {
        _currentHealth = _maxHealth;
    }
    
    public void TakeDamage(int amount)
    {
        _currentHealth -= amount;
        OnHealthChanged?.Invoke(_currentHealth);
        
        if (_currentHealth <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        OnDeath?.Invoke();
    }
}

// ✅ GOOD: Use this component
public class Player : MonoBehaviour
{
    [SerializeField] private Health _health;
    
    private void Awake()
    {
        _health.OnDeath += HandleDeath;
    }
    
    private void HandleDeath()
    {
        // Handle player death
    }
}

// ❌ BAD: Everything in one giant class
public class Player : MonoBehaviour
{
    public int maxHealth;
    private int currentHealth;
    public float moveSpeed;
    public GameObject weaponPrefab;
    // ... 500 more lines
}
2. Interfaces for Flexibility
csharp// ✅ GOOD: Define clear contracts
public interface IDamageable
{
    void TakeDamage(int amount);
    int CurrentHealth { get; }
}

public interface IPoolable
{
    void OnSpawnedFromPool();
    void OnReturnedToPool();
}

// Usage in systems
public class Projectile : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Works with ANY damageable object
        if (other.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(damage);
        }
    }
}
3. ScriptableObject-Based Design
csharp// ✅ GOOD: Data-driven, designer-friendly
[CreateAssetMenu(fileName = "New Skill", menuName = "RoombaRampage/Skill")]
public class Skill : ScriptableObject
{
    [Header("Identity")]
    public string SkillName;
    [TextArea(3, 5)] public string Description;
    public Sprite Icon;
    
    [Header("Rarity")]
    public SkillRarity Rarity;
    
    [Header("Effects")]
    public SkillEffectType EffectType;
    public float EffectValue;
    public bool CanStack = true;
    
    // Apply the skill effect
    public void ApplyToPlayer(PlayerStats stats)
    {
        switch (EffectType)
        {
            case SkillEffectType.FireRate:
                stats.FireRateMultiplier += EffectValue;
                break;
            case SkillEffectType.MoveSpeed:
                stats.MoveSpeedMultiplier += EffectValue;
                break;
            // ... etc
        }
    }
}

// ❌ BAD: Hardcoded values everywhere
public class SkillManager : MonoBehaviour
{
    private void ApplyFireRateSkill()
    {
        player.fireRate += 0.15f; // Magic number! Hard to balance!
    }
}
4. Event-Driven Communication
csharp// ✅ GOOD: Decoupled systems
public class XPSystem : MonoBehaviour
{
    public static event Action<int> OnXPGained;
    public static event Action OnLevelUp;
    
    private int _currentXP;
    private int _xpToNextLevel = 100;
    
    public void AddXP(int amount)
    {
        _currentXP += amount;
        OnXPGained?.Invoke(_currentXP);
        
        if (_currentXP >= _xpToNextLevel)
        {
            LevelUp();
        }
    }
    
    private void LevelUp()
    {
        OnLevelUp?.Invoke();
    }
}

// UI listens to events
public class XPBarUI : MonoBehaviour
{
    private void OnEnable()
    {
        XPSystem.OnXPGained += UpdateBar;
    }
    
    private void OnDisable()
    {
        XPSystem.OnXPGained -= UpdateBar;
    }
    
    private void UpdateBar(int currentXP)
    {
        // Update UI
    }
}

// ❌ BAD: Tight coupling
public class XPSystem : MonoBehaviour
{
    public XPBarUI xpBarUI; // Now XPSystem depends on UI!
    
    public void AddXP(int amount)
    {
        xpBarUI.UpdateBar(amount); // What if UI is disabled?
    }
}
5. Proper Unity Component Caching
csharp// ✅ GOOD: Cache in Awake()
public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private Transform _firePoint;
    
    private void Awake()
    {
        // If not assigned in Inspector, try to get it
        if (_rigidbody == null)
            _rigidbody = GetComponent<Rigidbody>();
    }
    
    private void FixedUpdate()
    {
        _rigidbody.AddForce(moveDirection); // Fast! Already cached
    }
}

// ❌ BAD: GetComponent every frame
private void Update()
{
    GetComponent<Rigidbody>().AddForce(moveDirection); // SLOW!
}
6. Object Pooling Pattern
csharp// ✅ GOOD: Reusable pool system
public class ObjectPool<T> where T : Component
{
    private readonly T _prefab;
    private readonly Queue<T> _pool = new Queue<T>();
    private readonly Transform _parent;
    
    public ObjectPool(T prefab, int initialSize, Transform parent = null)
    {
        _prefab = prefab;
        _parent = parent;
        
        // Pre-warm pool
        for (int i = 0; i < initialSize; i++)
        {
            CreateNewObject();
        }
    }
    
    private T CreateNewObject()
    {
        T obj = Object.Instantiate(_prefab, _parent);
        obj.gameObject.SetActive(false);
        _pool.Enqueue(obj);
        return obj;
    }
    
    public T Get()
    {
        T obj = _pool.Count > 0 ? _pool.Dequeue() : CreateNewObject();
        obj.gameObject.SetActive(true);
        
        if (obj is IPoolable poolable)
            poolable.OnSpawnedFromPool();
        
        return obj;
    }
    
    public void Return(T obj)
    {
        if (obj is IPoolable poolable)
            poolable.OnReturnedToPool();
        
        obj.gameObject.SetActive(false);
        _pool.Enqueue(obj);
    }
}

// Usage
private ObjectPool<Projectile> _projectilePool;

private void Awake()
{
    _projectilePool = new ObjectPool<Projectile>(projectilePrefab, 100);
}

private void Fire()
{
    Projectile proj = _projectilePool.Get();
    proj.transform.position = firePoint.position;
    // proj.Initialize(...);
}

Unity-Specific Best Practices
1. SerializeField over Public
csharp// ✅ GOOD: Private with SerializeField
[SerializeField] private float _moveSpeed = 5f;
[SerializeField] private Rigidbody _rigidbody;

// ❌ BAD: Everything public
public float moveSpeed = 5f;
public Rigidbody rigidbody; // Name conflict with GetComponent<Rigidbody>()!
2. Use RequireComponent
csharp// ✅ GOOD: Automatically adds required components
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class PlayerController : MonoBehaviour
{
    private Rigidbody _rigidbody;
    
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }
}
3. Header & Tooltip Attributes
csharp// ✅ GOOD: Clear Inspector organization
public class Weapon : MonoBehaviour
{
    [Header("Weapon Stats")]
    [Tooltip("Damage per projectile")]
    [SerializeField] private int _damage = 10;
    
    [Tooltip("Shots per second")]
    [SerializeField] private float _fireRate = 2f;
    
    [Header("Projectile")]
    [SerializeField] private Projectile _projectilePrefab;
    [SerializeField] private Transform _firePoint;
    
    [Header("Audio")]
    [SerializeField] private AudioClip _fireSound;
}
4. Proper Layer & Tag Usage
csharp// ✅ GOOD: Use layer masks for physics checks
[SerializeField] private LayerMask _enemyLayer;

private void CheckForEnemies()
{
    Collider[] hits = Physics.OverlapSphere(transform.position, radius, _enemyLayer);
    foreach (var hit in hits)
    {
        // Process enemies
    }
}

// ✅ GOOD: CompareTag instead of == for tags
if (other.CompareTag("Player"))
{
    // Do something
}

// ❌ BAD: String comparison
if (other.tag == "Player") { } // Slower, typo-prone
5. Coroutines Best Practices
csharp// ✅ GOOD: Cache WaitForSeconds
private WaitForSeconds _wait = new WaitForSeconds(1f);

private IEnumerator SpawnRoutine()
{
    while (true)
    {
        SpawnEnemy();
        yield return _wait; // Reuse cached wait
    }
}

// ✅ GOOD: Store coroutine reference for stopping
private Coroutine _spawnCoroutine;

private void StartSpawning()
{
    _spawnCoroutine = StartCoroutine(SpawnRoutine());
}

private void StopSpawning()
{
    if (_spawnCoroutine != null)
    {
        StopCoroutine(_spawnCoroutine);
        _spawnCoroutine = null;
    }
}

// ❌ BAD: Create new WaitForSeconds every loop
private IEnumerator SpawnRoutine()
{
    while (true)
    {
        SpawnEnemy();
        yield return new WaitForSeconds(1f); // Creates garbage!
    }
}
6. Physics Best Practices
csharp// ✅ GOOD: Physics in FixedUpdate
private void FixedUpdate()
{
    _rigidbody.AddForce(moveDirection * moveSpeed);
}

// ✅ GOOD: Use ForceMode appropriately
_rigidbody.AddForce(jumpForce, ForceMode.Impulse); // Instant velocity change

// ✅ GOOD: Raycast with layer mask
if (Physics.Raycast(origin, direction, out RaycastHit hit, maxDistance, _enemyLayer))
{
    // Hit something on enemy layer
}

// ❌ BAD: Physics operations in Update
private void Update()
{
    transform.position += moveDirection; // Not physics-based!
}

Editor Scripting Guidelines
1. Custom Inspector Example
csharp#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Skill))]
public class SkillEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Skill skill = (Skill)target;
        
        // Custom layout
        EditorGUILayout.LabelField("Skill Configuration", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // Use default for most fields
        DrawDefaultInspector();
        
        EditorGUILayout.Space();
        
        // Custom buttons
        if (GUILayout.Button("Test Apply Effect"))
        {
            Debug.Log($"Testing {skill.SkillName} effect...");
        }
        
        // Preview
        EditorGUILayout.HelpBox($"Effect: {skill.EffectType} +{skill.EffectValue}", MessageType.Info);
    }
}
#endif
2. Menu Items for Quick Actions
csharp#if UNITY_EDITOR
using UnityEditor;

public static class GameDevTools
{
    [MenuItem("RoombaRampage/Create All Folders")]
    private static void CreateProjectFolders()
    {
        string[] folders = {
            "Assets/_Project/Scripts/Core",
            "Assets/_Project/Scripts/Player",
            "Assets/_Project/Scripts/Enemies",
            // ... etc
        };
        
        foreach (string folder in folders)
        {
            if (!AssetDatabase.IsValidFolder(folder))
            {
                Directory.CreateDirectory(folder);
            }
        }
        
        AssetDatabase.Refresh();
        Debug.Log("Project folders created!");
    }
    
    [MenuItem("RoombaRampage/Clear PlayerPrefs")]
    private static void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("PlayerPrefs cleared!");
    }
}
#endif
3. Scene Validation
csharp#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class SceneValidator
{
    static SceneValidator()
    {
        EditorSceneManager.sceneOpened += OnSceneOpened;
    }
    
    private static void OnSceneOpened(UnityEngine.SceneManagement.Scene scene, OpenSceneMode mode)
    {
        // Check for required objects
        if (GameObject.FindObjectOfType<GameManager>() == null)
        {
            Debug.LogWarning($"Scene '{scene.name}' is missing a GameManager!");
        }
    }
}
#endif

Performance Guidelines
1. Avoid Common Pitfalls
csharp// ❌ BAD: FindObjectOfType every frame
private void Update()
{
    Player player = FindObjectOfType<Player>(); // VERY SLOW!
}

// ✅ GOOD: Cache on Awake
private Player _player;

private void Awake()
{
    _player = FindObjectOfType<Player>();
}

// ❌ BAD: GameObject.Find with strings
private void Start()
{
    GameObject player = GameObject.Find("Player"); // Slow + typo-prone
}

// ✅ GOOD: Assign in Inspector or use singleton
[SerializeField] private Player _player;

// ❌ BAD: Using Camera.main in Update
private void Update()
{
    Camera.main.transform.LookAt(target); // Camera.main uses FindObjectWithTag!
}

// ✅ GOOD: Cache the camera
private Camera _mainCamera;

private void Awake()
{
    _mainCamera = Camera.main;
}
2. String Optimization
csharp// ✅ GOOD: Use hashed strings for Animator
private static readonly int SpeedHash = Animator.StringToHash("Speed");

private void UpdateAnimator()
{
    animator.SetFloat(SpeedHash, currentSpeed);
}

// ❌ BAD: String every frame
animator.SetFloat("Speed", currentSpeed);
3. Update Optimization
csharp// ✅ GOOD: Only update when necessary
public class Enemy : MonoBehaviour
{
    private bool _isActive = true;
    
    private void Update()
    {
        if (!_isActive) return; // Early exit
        
        // ... rest of update logic
    }
    
    public void SetActive(bool active)
    {
        _isActive = active;
        enabled = active; // Disable Update entirely if inactive
    }
}

Documentation Standards
1. XML Documentation for Public APIs
csharp/// <summary>
/// Manages player health, damage, and death.
/// </summary>
public class Health : MonoBehaviour
{
    /// <summary>
    /// Invoked when health reaches zero.
    /// </summary>
    public event Action OnDeath;
    
    /// <summary>
    /// Applies damage to this entity.
    /// </summary>
    /// <param name="amount">Amount of damage to apply.</param>
    public void TakeDamage(int amount)
    {
        // Implementation
    }
}
2. Comment Complex Logic
csharp// ✅ GOOD: Explain WHY, not WHAT
private void CalculateMovement()
{
    // Normalize input to prevent faster diagonal movement
    Vector3 input = new Vector3(horizontal, 0, vertical).normalized;
    
    // Apply acceleration curve for smoother feel
    float speed = _accelerationCurve.Evaluate(_currentSpeed / _maxSpeed);
}

// ❌ BAD: Obvious comments
private void CalculateMovement()
{
    // Get input
    Vector3 input = new Vector3(horizontal, 0, vertical);
    // Normalize it
    input.normalized;
}
3. TODO Comments
csharp// TODO: Add recoil effect when firing
// FIXME: Enemy sometimes gets stuck on corners
// HACK: Temporary fix for null reference, needs proper solution
// NOTE: This value is carefully balanced, test thoroughly before changing

Testing & Debugging
1. Debug-Friendly Code
csharppublic class Enemy : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool _showDebugGizmos = true;
    
    private void OnDrawGizmosSelected()
    {
        if (!_showDebugGizmos) return;
        
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
2. Assertions for Sanity Checks
csharpusing UnityEngine.Assertions;

private void Awake()
{
    Assert.IsNotNull(_rigidbody, "Rigidbody is required!");
    Assert.IsTrue(_maxHealth > 0, "Max health must be positive!");
}
3. Conditional Debug Logs
csharp// ✅ GOOD: Compile out in release builds
[System.Diagnostics.Conditional("UNITY_EDITOR")]
private void DebugLog(string message)
{
    Debug.Log($"[{GetType().Name}] {message}");
}

// Usage
DebugLog("Player took damage"); // Only logs in Editor

Key Principles Summary
For Claude Code:

✅ Always use the folder structure above
✅ Follow naming conventions exactly
✅ Component-based, not monolithic classes
✅ ScriptableObjects for all data/config
✅ Events for decoupling systems
✅ Cache all component references in Awake()
✅ Use SerializeField instead of public fields
✅ Add Header/Tooltip attributes for clarity
✅ Document public APIs with XML comments
✅ Physics in FixedUpdate, logic in Update
✅ Object pooling for frequently spawned objects
✅ Layer masks for collision filtering
✅ Interfaces for flexible systems
✅ No magic numbers—use constants or ScriptableObjects
✅ Early returns for readability

For You (Human):

✅ Organize by feature, not asset type
✅ Use prefabs liberally—nest them
✅ Tag and layer everything properly
✅ Keep scenes minimal—reference prefabs
✅ Use ScriptableObjects in Inspector, not hardcoded values


This guide ensures clean, maintainable, performant Unity code that both you and Claude Code can work with efficiently! 🚀