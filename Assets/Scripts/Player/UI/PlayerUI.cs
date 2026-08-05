using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private float lagValue;

    private void LateUpdate()
    {
        transform.localPosition = Vector3.Lerp(
            transform.localPosition, 
            Vector3.zero, 
            lagValue * Time.deltaTime);
    }
}
