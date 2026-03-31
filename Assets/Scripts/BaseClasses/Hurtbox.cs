using UnityEngine;

public class Hurtbox : MonoBehaviour
{
    IDamageable parent;

    private Collider2D col;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void EnableHurtbox()
    {

    }

    private void DisableHurtbox()
    {

    }
}
