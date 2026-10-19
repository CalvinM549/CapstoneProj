using UnityEngine;

public class MusicDirector : MonoBehaviour
{
    [SerializeField] private int enemiesForMaxIntensity = 8;
    [SerializeField][Range(0f, 1f)] private float calmIntensity;
    [SerializeField][Range(0f, 1f)] private float combatFloor;

    [SerializeField] private float calmDelay = 2f;
    [SerializeField] private float runEndFade = 3f;

    private bool inCombat;
    private float calmAt = -1f;

    private static AudioManager Audio => AudioManager.Instance;

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }

    private void Start()
    {
        if (Audio == null) return;
        // start playing music
    }

    private void Update()
    {
        if(Audio == null) return;

        if (inCombat)
        {
            int enemies = AIManager.Instance != null ? AIManager.Instance.ActiveEnemyCount : 0;
            float t = Mathf.Clamp01(enemies / (float)Mathf.Max(1, enemiesForMaxIntensity));
        }
        else if (calmAt >= 0f && Time.unscaledTime >= calmAt)
        {
            calmAt = -1f;
        }
    }

    private void HandleRoomActivated(bool hasEncounter)
    {

    }

    private void HandleRoomCompleted()
    {

    }

    private void HandleRunEnded(bool victory)
    {

    }
}
