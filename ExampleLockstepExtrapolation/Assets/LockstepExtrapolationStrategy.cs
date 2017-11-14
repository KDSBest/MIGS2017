using System;
using UnityEngine;

public class LockstepExtrapolationStrategy : IExtrapolationStrategy
{
    private Vector3 newPosition = Vector3.negativeInfinity;
    private Vector3 oldPosition = Vector3.negativeInfinity;
    private Quaternion newRotation = Quaternion.identity;
    private Quaternion oldRotation = Quaternion.identity;
    private float time = 0;
    private float extrapolationTime = 0;
    public void Extrapolate(Transform transform)
    {
        if (newPosition.x == Single.NegativeInfinity)
            return;

        extrapolationTime += Time.deltaTime * 2;
        if (extrapolationTime > 1)
        {
            extrapolationTime = 1;
        }

        transform.rotation = Quaternion.Lerp(oldRotation, newRotation, extrapolationTime);
    }

    public void Interpolate(Transform transform)
    {
        if (newPosition.x == Single.NegativeInfinity)
            return;

        time += Time.deltaTime;
        if (time > 1)
        {
            time = 1;
        }

        float fullTime = extrapolationTime + time;
        if (fullTime > 1)
        {
            fullTime = 1;
        }

        transform.position = Vector3.Lerp(oldPosition, newPosition, time);
        transform.rotation = Quaternion.Lerp(oldRotation, newRotation, fullTime);
    }

    public void Click(GameObject clickIndicator, Transform transform, Vector3 extrapolationLocation)
    {
        time = 0;
        extrapolationTime = 0;
        oldPosition = transform.position;
        oldRotation = transform.rotation;
        newPosition = new Vector3(extrapolationLocation.x, transform.position.y, extrapolationLocation.z);
        newRotation = Quaternion.LookRotation(newPosition - oldPosition, Vector3.up);
        clickIndicator.transform.position = newPosition;
        clickIndicator.SetActive(true);
        clickIndicator.GetComponent<Animation>().Stop();
        clickIndicator.GetComponent<Animation>().Play();
    }

    public void Reset()
    {
        newPosition = Vector3.negativeInfinity;
    }
}