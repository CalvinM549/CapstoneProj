using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public SpawnTag tag = SpawnTag.Any;
    [SerializeField] private ParticleSystem telegraphParticles;

    public bool Accepts(SpawnTag required) => required == SpawnTag.Any || tag == SpawnTag.Any || tag == required;

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