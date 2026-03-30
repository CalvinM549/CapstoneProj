using UnityEngine;

public interface IHitSource
{
    public string Name { get; set; }
    public Transform Position { get; set; }
}
