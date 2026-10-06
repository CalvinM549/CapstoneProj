using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public SpawnTag allowedTag = SpawnTag.Any;
    [SerializeField] private ParticleSystem telegraphParticles;

    public bool Accepts(SpawnTag required) => required == SpawnTag.Any || allowedTag == SpawnTag.Any || allowedTag == required;

    public void PlayTelegraph()
    {
        if(telegraphParticles != null)
            telegraphParticles.Play();
    }
}

public enum SpawnTag
{
    Any,
    Ranged,
    Melee,
    Flying
}