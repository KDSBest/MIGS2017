using UnityEngine;

public interface IExtrapolationStrategy
{
    void Extrapolate(Transform transform);

    void Interpolate(Transform transform);

    void Click(GameObject clickIndicator, Transform transform, Vector3 extrapolationLocation);

    void Reset();
}