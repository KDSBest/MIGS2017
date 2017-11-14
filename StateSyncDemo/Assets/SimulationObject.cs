using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationObject : MonoBehaviour, IAccumulateableObject
{
    private float minRange = -5;
    private float maxRange = 5;
    public short Id { get; set; }

    public Vector3 TargetPosition = Vector3.zero;

    public Vector3 Offset = Vector3.zero;

    public bool IsServer = false;

    public float MinPrio = 0.1f;
    public float MaxPrio = 2.0f;

    public float Prio;
    // Use this for initialization
    void Start()
    {
        Prio = UnityEngine.Random.Range(MinPrio, MaxPrio);
        CurrentPrio = 0;
        this.transform.localPosition = Vector3.zero;
        SetColorBasedOnPrio();
    }

    public void SetColorBasedOnPrio()
    {
        this.gameObject.GetComponent<Renderer>().material.color =
            new Color(Prio / MaxPrio, Prio / MaxPrio, Prio / MaxPrio, 1.0f);
    }

    // Update is called once per frame
    void Update()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, TargetPosition, Time.deltaTime);
    }

    void FixedUpdate()
    {
        if (IsServer && Vector3.Distance(TargetPosition, transform.localPosition) < 0.03f)
        {
            TargetPosition = new Vector3(UnityEngine.Random.Range(minRange, maxRange), transform.position.y, UnityEngine.Random.Range(minRange, maxRange));
        }
    }

    public float CalculatePrio()
    {
        return Prio;
    }

    public float CurrentPrio { get; set; }

    public byte[] GetData()
    {
        Vector3 pos = TargetPosition;
        
        //Vector3 pos = this.transform.localPosition;
        // Slow Serialization but for showcase fine!
        List<byte> data = new List<byte>();
        data.AddRange(BitConverter.GetBytes(Id));
        data.AddRange(BitConverter.GetBytes(pos.x));
        data.AddRange(BitConverter.GetBytes(pos.y));
        data.AddRange(BitConverter.GetBytes(pos.z));
        data.AddRange(BitConverter.GetBytes(Prio));
        return data.ToArray();
    }
}