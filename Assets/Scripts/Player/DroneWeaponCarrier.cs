using UnityEngine;

/// <summary>
/// Drone-like weapon carrier that hovers near the player. The fire point is
/// always exactly on the player's current aim ray (no lag, ever) - that's
/// the hard guarantee that keeps shots lined up with the mouse. The drone's
/// visible body is a fully separate value that physically chases the fire
/// point with inertia, so it trails/swings/catches up like a real object
/// instead of snapping to a computed position every frame.
/// </summary>
public class DroneWeaponCarrier : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public Player player;
    [SerializeField] private Transform visualRoot;   // the sprite - what actually lags, stays upright
    [SerializeField] private Transform turret;       // optional - the part that rotates to show aim, if any
    [SerializeField] private Transform firePoint;    // exact spawn point, never lags

    [Header("Orbit")]
    [Tooltip("Distance from the player along the aim direction.")]
    [SerializeField] private float restDistance = 1.4f;
    [Tooltip("Extra distance while the player is actively moving.")]
    [SerializeField] private float movingDistanceBonus = 0.25f;
    [Tooltip("How fast the target distance itself settles (radial only).")]
    [SerializeField] private float distanceSmoothTime = 0.1f;

    [Header("Body Lag (this is what makes it feel like a trailing drone)")]
    [Tooltip("How long the visual body takes to catch up to the fire point. Higher = floatier/laggier.")]
    [SerializeField] private float bodySmoothTime = 0.22f;
    [Tooltip("Clamp on how fast the body can move, so it doesn't overshoot wildly on big snaps.")]
    [SerializeField] private float bodyMaxSpeed = 25f;

    [Header("Rotation")]
    [SerializeField] private float maxRotationSpeed = 480f;

    [Header("Idle Bob (layered on top of the body's own lagged position)")]
    [SerializeField] private float bobAmplitude = 0.05f;
    [SerializeField] private float bobFrequency = 2.2f;

    private float currentDistance;
    private float distanceVelocity;
    private Vector2 currentAimDir = Vector2.right;
    private Vector2 bodyVelocity;      // SmoothDamp velocity for the visual body
    private Vector2 bodyPosition;      // the body's actual, laggy, physical position
    private float bobTimer;

    private void Awake()
    {
        currentDistance = restDistance;
        if (player == null) player = GetComponentInParent<Player>();
        bodyPosition = transform.position;
    }

    private void LateUpdate()
    {
        if (player == null) return;
        if(TimescaleManager.IsPaused) return;

        // --- Exact math, no lag: this is the line shots travel along ---
        currentAimDir = -player.GetTargetDirection();
        if (currentAimDir.sqrMagnitude < 0.0001f) currentAimDir = Vector2.right;

        bool isMoving = player.Movement != null && player.Movement.IsMoving;
        float targetDistance = restDistance + (isMoving ? movingDistanceBonus : 0f);
        currentDistance = Mathf.SmoothDamp(
            currentDistance, targetDistance, ref distanceVelocity, distanceSmoothTime);

        Vector2 playerPos = player.transform.position;
        Vector2 exactFirePos = playerPos + currentAimDir * currentDistance;

        if (firePoint != null)
            firePoint.position = exactFirePos;   // always exact - never touched by body lag

        // --- Separate, laggy chase: this is what gives it physical presence ---
        bodyPosition = Vector2.SmoothDamp(
            bodyPosition, exactFirePos, ref bodyVelocity, bodySmoothTime, bodyMaxSpeed);

        transform.position = bodyPosition;

        // Small bob layered on top of the already-lagging body, not on firePoint.
        bobTimer += Time.deltaTime * bobFrequency;
        Vector2 perpendicular = new Vector2(-currentAimDir.y, currentAimDir.x);
        Vector2 bobOffset = perpendicular * (Mathf.Sin(bobTimer * Mathf.PI * 2f) * bobAmplitude);

        // Body stays upright - only its position moves/bobs, rotation is untouched.
        if (visualRoot != null)
            visualRoot.position = bodyPosition + bobOffset;
        else
            transform.position = bodyPosition + bobOffset; // fallback if no separate sprite child

        // If there's a turret/indicator child, that's what shows aim direction.
        if (turret != null)
        {
            turret.position = bodyPosition; // or bodyPosition + bobOffset if you want it to bob too
            float targetAngle = Mathf.Atan2(currentAimDir.y, currentAimDir.x) * Mathf.Rad2Deg;
            float newAngle = Mathf.MoveTowardsAngle(
                turret.eulerAngles.z, targetAngle, maxRotationSpeed * Time.deltaTime);
            turret.rotation = Quaternion.Euler(0f, 0f, newAngle);
        }
    }

    public Vector2 AimDirection => currentAimDir;

    public Vector2 FireOrigin => firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position;
}