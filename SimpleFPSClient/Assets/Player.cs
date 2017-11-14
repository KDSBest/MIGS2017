using System;
using System.Collections;
using System.Collections.Generic;
using ReliableUdp;
using SimpleFPSShared;
using SimpleFPSShared.Client;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Player : MonoBehaviour
{
    private SFPSClient client;
    private bool sendCmds = false;
    public GameObject DummyPrefab;
    private Dictionary<string, GameObject> Dummies = new Dictionary<string, GameObject>();
    private float coolDown = 0.0f;
    public GameObject ShotPrefab;
    public Image HitIndicator;
    private SFPSShot Shot = new SFPSShot();

    public static string PlayerName = string.Empty;

    private bool HasInitialized = false;
    public static string Host = string.Empty;

    public InputField PNameField;

    public InputField HostField;
    public GameObject Panel;

    public void Connect()
    {
        PlayerName = PNameField.text;
        Host = HostField.text;
    }

    public void Start()
    {
        FactoryRegistrations.Register();
    }

    private float GetInterpolationTime()
    {
        float passedTimeNowToSnapshot = (float)(client.SnapshotManagement.GetEstimatedServerTime() - client.SnapshotManagement.GetInterpolationGoal().ServerTime)
            .TotalMilliseconds;
        float interpolationT = passedTimeNowToSnapshot / Time.fixedDeltaTime;
        interpolationT = Mathf.Clamp(interpolationT, 0, 1.0f);
        return interpolationT;
    }

    private void ExtrapolateMyself(Snapshot goalSnapshot, SFPSPlayerState state)
    {
        //var cmds = client.ExtrapolationDataManagement.GetAllCmdsAfterServerTime(goalSnapshot.ServerTime);

        //bool invalid = false;

        //SFPSVector2 pos = new SFPSVector2(state.Position);

        //foreach (var cmd in cmds)
        //{
        //    if (Mathf.Abs((float) SFPSVector2.Distance(pos, cmd.State.Position)) > 0.1f)
        //    {
        //        invalid = true;
        //    }

        //    pos = cmd.State.Position;
        //}

        //// This causes rubber banding but for showcase it's enough
        //if (invalid)
        //{
        //    this.transform.position = new Vector3(state.Position.X, 0.5f, state.Position.Y);
        //}
    }

    private void InitClient()
    {
        if (!HasInitialized && !string.IsNullOrEmpty(Host) && !string.IsNullOrEmpty(PlayerName))
        {
            Panel.SetActive(false);
            client = new SFPSClient(Host, 5667, PlayerName, 0,
                () =>
                {
                    if (client == null)
                        return;

                    var goal = client.SnapshotManagement.GetInterpolationGoal();
                    var origin = client.SnapshotManagement.GetInterpolationOrigin();

                    if (goal.HasHit)
                    {
                        this.HitIndicator.color = Color.green;
                    }
                    else
                    {
                        this.HitIndicator.color = Color.white;
                    }

                    var interpolationT = GetInterpolationTime();
                    foreach (var pKV in goal.Player)
                    {
                        if (pKV.Key.Equals(PlayerName))
                        {
                            ExtrapolateMyself(goal, pKV.Value);
                            continue;
                        }

                        if (!Dummies.ContainsKey(pKV.Key))
                        {
                            var go = GameObject.Instantiate(DummyPrefab);
                            go.name = "Dummy(" + pKV.Key + ")";
                            Dummies.Add(pKV.Key, go);
                        }

                        if (!origin.Player.ContainsKey(pKV.Key))
                        {
                            Dummies[pKV.Key].transform.position =
                                new Vector3(pKV.Value.Position.X, 0.5f, pKV.Value.Position.Y);
                            continue;
                        }
                        var orig = origin.Player[pKV.Key];
                        Dummies[pKV.Key].transform.position =
                            Vector3.Lerp(new Vector3(orig.Position.X, 0.5f, orig.Position.Y),
                                new Vector3(pKV.Value.Position.X, 0.5f, pKV.Value.Position.Y), interpolationT);
                        Dummies[pKV.Key].transform.rotation = Quaternion.Euler(0, pKV.Value.Rotation, 0);
                    }
                },
                () =>
                {
                    sendCmds = true;
                });
            client.Connect();
            HasInitialized = true;
        }
    }

    public void Update()
    {
        if (!HasInitialized)
        {
            InitClient();
            return;
        }

        coolDown -= Time.deltaTime;
        if (Input.GetMouseButtonDown(0) && coolDown <= 0.0000001f)
        {
            coolDown = 0.1f;
            GameObject shot = GameObject.Instantiate(ShotPrefab);
            shot.transform.position = this.transform.position;
            shot.transform.localRotation = Quaternion.Euler(Camera.main.transform.eulerAngles.x, this.transform.eulerAngles.y, 0);
            Shot = new SFPSShot()
            {
                Ray = new SFPSRay(new SFPSVector3(this.transform.position.x, 0, this.transform.position.z), new SFPSVector3(shot.transform.forward.normalized.x, shot.transform.forward.normalized.y, shot.transform.forward.normalized.z)),
                InterpolationDestinationId = client.SnapshotManagement.GetInterpolationGoal().Id,
                InterpolationOriginId = client.SnapshotManagement.GetInterpolationOrigin().Id,
                InterpolationPercentage = (byte)(GetInterpolationTime() * 100)
            };

            var enemies = GameObject.FindObjectsOfType<Enemy>();

            foreach (var enemy in enemies)
            {
                SFPSVector3 center = new SFPSVector3(enemy.transform.position.x, 0, enemy.transform.position.z);
                SFPSVector3 p;
                float t;
                if (Shot.Ray.IntersectSphere(center, 0.5f, out t, out p))
                {

                }
            }
        }

        if (client != null)
            client.Update();
    }

    public void FixedUpdate()
    {
        if (sendCmds)
        {
            var cmd = new SFPSPlayerCommand();
            cmd.State.Rotation = this.transform.rotation.eulerAngles.y;
            cmd.State.Position = new SFPSVector2(this.transform.position.x, this.transform.position.z);

            if (Shot.IsValid)
            {
                cmd.Shot = Shot;
                Shot = new SFPSShot();
            }

            client.SendCmd(cmd);

        }
    }
}