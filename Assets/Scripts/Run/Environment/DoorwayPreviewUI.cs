using TMPro;
using UnityEngine;

public class DoorwayPreviewUI : MonoBehaviour
{
    private Doorway door;
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
    }

    public void InitializeWithDestination(MapNode destination)
    {
        typeName.text = $"[{destination.type.GetRoomTypeName()}]";

        if (destination.type != RoomType.Rest || destination.type != RoomType.End)
        {
            string rewardText = destination.rewards.Equals(default) ? "" : $"<{destination.rewards.category.ToString()}>";
            rewardName.text = rewardText;
        }
        else
            rewardName.text = "";

        flavourName.text = $"Sector {GenerateRandomCode(Random.Range(3, 7))}";
    }

    private const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ01233456789";
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
        if (door.IsLocked)
        {
            cg.alpha = 0f;
            return;
        }

        float distance = Vector2.Distance(door.transform.position, RunManager.Instance.activePlayer.transform.position);
        float alpha = Mathf.InverseLerp(maxDistance, minDistance, distance);

        cg.alpha = alpha;
    }
}
