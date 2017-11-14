using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets;
using Messages;
using UnityEngine;

public class Server : MonoBehaviour
{
    public GameObject P1;
    public GameObject P2;

    private FakeNetwork network;
    public Simulation simulation = new Simulation();
    private int lastSendFrame = 0;
    private int sendFrame = 4;

    void Awake()
    {
        this.GetComponent<FakeNetworkReceiver>().Action = Receive;
        network = GameObject.FindObjectOfType<FakeNetwork>();
    }

    private void Receive(byte[] data)
    {
        var states = LockstepState.DeserializeAll(data);
        foreach (var state in states)
        {
            Debug.Log("Received " + state.Frame + " from " + state.ClickPosition.Keys.First());
            simulation.LockStep.Add(state);

        }
    }

    public void FixedUpdate()
    {
        if (simulation.LockStep.IsComplete(sendFrame) && lastSendFrame != sendFrame)
        {
            lastSendFrame = sendFrame;
            sendFrame++;
            var data = simulation.LockStep.GetState(lastSendFrame).Serialize();
            network.SendToReliable(FakeNetwork.Receiver.Server, FakeNetwork.Receiver.P1, data);
            network.SendToReliable(FakeNetwork.Receiver.Server, FakeNetwork.Receiver.P2, data);
            Debug.Log("Send " + lastSendFrame + " from server");
        }
        else
        {
            network.SendToReliable(FakeNetwork.Receiver.Server, FakeNetwork.Receiver.P1, new byte[0]);
            network.SendToReliable(FakeNetwork.Receiver.Server, FakeNetwork.Receiver.P2, new byte[0]);
        }

        simulation.Step();
        P1.transform.localPosition = simulation.P1NewPosition;
        P2.transform.localPosition = simulation.P2NewPosition;

        Debug.Log(simulation.CurrentFrame);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
