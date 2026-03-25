using UnityEngine;

public interface IStaggerable
{
    float staggerThreshold {  get; set; }
    float currentStagger {  get; set; }

    void IncrementStagger(HitData hit);
    void ApplyStagger();
}
