using System;
using System.Collections;
using System.Collections.Generic;
using Assets;
using Messages;
using UnityEngine;

public class Client : MonoBehaviour
{
    public GameObject P1;
    public GameObject P2;

    private FakeNetwork network;
    public Vector2 newClickPosition = Vector2.negativeInfinity;
    public Simulation simulation = new Simulation();

    public bool IsP1 = false;
    private int lastSendFrame = 0;

    // Use this for initialization
    public void Awake()
    {
        this.GetComponent<FakeNetworkReceiver>().Action = Receive;
        network = GameObject.FindObjectOfType<FakeNetwork>();
        IsP1 = this.GetComponent<FakeNetworkReceiver>().Receiver == FakeNetwork.Receiver.P1;
    }

    private void Receive(byte[] data)
    {
        var states = LockstepState.DeserializeAll(data);
        foreach (var state in states)
            simulation.LockStep.Add(state);
    }

    public void FixedUpdate()
    {
        if (lastSendFrame != simulation.CurrentFrame + simulation.LockStep.SkipFrames)
        {
            lastSendFrame = simulation.CurrentFrame + simulation.LockStep.SkipFrames;
            LockstepState state = new LockstepState()
            {
                Frame = simulation.CurrentFrame + simulation.LockStep.SkipFrames,
                ClickPosition = new Dictionary<byte, Vector2>()
                {
                    {IsP1 ? (byte)0 : (byte)1, newClickPosition }
                }
            };

            simulation.LockStep.Add(state);
            network.SendToReliable(IsP1 ? FakeNetwork.Receiver.P1 : FakeNetwork.Receiver.P2, FakeNetwork.Receiver.Server, state.Serialize());
            newClickPosition = Vector2.negativeInfinity;
            Debug.Log("Send " + state.Frame + " from " + (IsP1 ? 0 : 1));
        }
        else
        {
            network.SendToReliable(IsP1 ? FakeNetwork.Receiver.P1 : FakeNetwork.Receiver.P2, FakeNetwork.Receiver.Server, new byte[0]);
        }
        simulation.Step();

        P1.transform.localPosition = simulation.P1NewPosition;
        P2.transform.localPosition = simulation.P2NewPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(0))
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo))
            {
                if (hitInfo.collider.gameObject.GetInstanceID() == this.gameObject.GetInstanceID())
                {
                    newClickPosition = new Vector2(hitInfo.point.x - this.transform.position.x, hitInfo.point.z - this.transform.position.z);
                    newClickPosition = new Vector2(newClickPosition.x * 4, newClickPosition.y * 4);
                }
            }
        }
    }
}
