using System.Collections;
using UnityEngine;

public class Server : MonoBehaviour
{
    public GameObject Prefab;

    private FakeNetwork network;

    public int ObjectCount = 100;

    public AccumulatedPriority P1AccumulatedPriority = new AccumulatedPriority();
    public AccumulatedPriority P2AccumulatedPriority = new AccumulatedPriority();

    public int SendObjectMaxCount = 10;

    void Awake()
    {
        this.GetComponent<FakeNetworkReceiver>().Action = Receive;
        network = GameObject.FindObjectOfType<FakeNetwork>();

        for (short i = 0; i < ObjectCount; i++)
        {
            var newObj = GameObject.Instantiate(Prefab);
            newObj.transform.SetParent(this.transform);
            var simObj = newObj.GetComponent<SimulationObject>();
            simObj.Id = i;
            simObj.IsServer = true;
            P1AccumulatedPriority.Add(simObj);
            P2AccumulatedPriority.Add(simObj);
        }
    }

    private void Receive(byte[] data)
    {

    }

    public void FixedUpdate()
    {
        network.SendTo(FakeNetwork.Receiver.P1, P1AccumulatedPriority.GetAndAccumulate(SendObjectMaxCount));
        network.SendTo(FakeNetwork.Receiver.P2, P2AccumulatedPriority.GetAndAccumulate(SendObjectMaxCount));
    }

    // Update is called once per frame
    void Update()
    {

    }
}
