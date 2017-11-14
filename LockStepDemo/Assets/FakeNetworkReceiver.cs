using System;
using UnityEngine;
using UnityEngine.Events;

public class FakeNetworkReceiver : MonoBehaviour
{
    public FakeNetwork.Receiver Receiver;
    public Action<byte[]> Action;
} 