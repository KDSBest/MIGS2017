using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Client : MonoBehaviour
{
    public GameObject Prefab;

    private Dictionary<short, SimulationObject> objects = new Dictionary<short, SimulationObject>();

    // Use this for initialization
    public void Awake()
    {
        this.GetComponent<FakeNetworkReceiver>().Action = Receive;
    }

    private void Receive(byte[] data)
    {
        for (int i = 0; i < data.Length;)
        {
            short id = BitConverter.ToInt16(data, i);
            i+=2;
            if (!objects.ContainsKey(id))
            {
                var newObj = GameObject.Instantiate(Prefab);
                newObj.transform.SetParent(this.transform);
                var simObj = newObj.GetComponent<SimulationObject>();
                simObj.Id = id;
                simObj.IsServer = false;
                if (this.GetComponent<FakeNetworkReceiver>().Receiver == FakeNetwork.Receiver.P1)
                {
                    simObj.Offset = new Vector3(-3, 0, 0);
                }
                else
                {
                    simObj.Offset = new Vector3(3, 0, 0);
                }

                objects.Add(id, simObj);
            }
            float x = BitConverter.ToSingle(data, i);
            i += 4;
            float y = BitConverter.ToSingle(data, i);
            i += 4;
            float z = BitConverter.ToSingle(data, i);
            i += 4;
            objects[id].TargetPosition = new Vector3(x, y, z);
            objects[id].Prio = BitConverter.ToSingle(data, i);
            objects[id].SetColorBasedOnPrio();
            i += 4;
        }
    }

    public void FixedUpdate()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }
}
