using TMPro;
using UnityEngine;

public class DoorwayPreviewUI : MonoBehaviour
{
    private Doorway door;
    private Transform playerTransform;

    [SerializeField] private CanvasGroup cg;
    [SerializeField] private TextMeshProUGUI typeName;
    [SerializeField] private TextMeshProUGUI rewardName;
    [SerializeField] private TextMeshProUGUI flavourName;
    // icon?

    [SerializeField] private float minDistance;
    [SerializeField] private float maxDistance;

    private void Awake()
    {
        door = GetComponentInParent<Doorway>();
        playerTransform = RunManager.Instance.activePlayer.transform;
    }

    public void InitializeWithDestination(MapNode destination)
    {
        typeName.text = $"[{destination.type.displayName}]";

        bool showReward = destination.type.displayRewardType && destination.offers.Length > 0;
        rewardName.text = showReward ? $"<{destination.offers[0].category}>" : "";

        flavourName.text = $"Sector {GenerateRandomCode(Random.Range(3, 7))}";
    }

    private const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private string GenerateRandomCode(int length)
    {
        char[] stringChars = new char[length];

        for (int i = 0; i < length; i++)
        {
            stringChars[i] = validChars[Random.Range(0, validChars.Length)];
        }

        return new string(stringChars);
    }

    private void Update()
    {
        float distance = Vector2.Distance(door.transform.position, playerTransform.position);
        float alpha = Mathf.InverseLerp(maxDistance, minDistance, distance);

        cg.alpha = alpha;
    }
}
