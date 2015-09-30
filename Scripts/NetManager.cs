using UnityEngine;
using FutureCode.Net;
using FutureCode.Net.SocketClient;
using FutureCode.ObjectPool;
using FutureCode.ObjectPool.Converter;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;
using FutureCode.Game;
using FutureCode.Game.Command;

public class NetManager : MonoBehaviour {
    public static NetManager Instance;
    public GameClock Clock;
    public int Ping = 9999; 
    public bool Connected = false;

    GameSettings settings;
    Queue<Action> unprocessActions;
	// Use this for initialization
    void Awake()
    {
        Instance = this;
    }
	void Start () {
        settings = GameSettings.Instance;
        Clock = new GameClock(DateTime.Now);
        unprocessActions = new Queue<Action>();
        InitializeClient();
    }
	
	// Update is called once per frame
	void Update () {
        lock(unprocessActions)
        {
            if (unprocessActions.Count > 0)
            {
                Action a = unprocessActions.Dequeue();
                a.Invoke();
            }
        }


        if(client.Connected && Time.frameCount % 100 == 0)
        {
            TimeSynchronization();
        }
	}

    void OnApplicationQuit()
    {
        if (client.Connected)
            client.Dispose();
    }

    public void InvokeAtMainThread(Action action)
    {
        lock(unprocessActions)
            unprocessActions.Enqueue(action);
    }

    SocketClient client;
    BufferManager<byte> bufferManager;

    void InitializeClient()
    {
        bufferManager = new BufferManager<byte>(1 << 8);
        client = new SocketClient(bufferManager);
        client.HostConnected += Client_HostConnected;
        client.HostDisconnected += Client_HostDisconnected;
        client.SendCompleted += Client_SendCompleted;
        client.NewDataReceived += Client_NewDataReceived;

        client.Connect("127.0.0.1", 1316);
    }

    private void Client_NewDataReceived(object sender, NewDataReceivedEventArgs e)
    {
        CommandType cmdType = (CommandType)e.Data.PackageData.GetInt16(4);
        Debug.Log("*" + cmdType.ToString() + " command received!");
        switch (cmdType)
        {
            case CommandType.TimeSynchronization:
                TimeSynchronizationCommand tsc = CommandSerializer.Deserialize<TimeSynchronizationCommand>(e.Data.PackageData);
                int ping = (int)((DateTime.Now - tsc.ClientTime).TotalMilliseconds);
                ping /= 2;
                DateTime serverTime = tsc.ServerTime.AddMilliseconds(ping);
                this.Ping = ping;
                Clock = new GameClock(serverTime);
                break;
            case CommandType.Connected:
                ConnectedCommand cc = CommandSerializer.Deserialize<ConnectedCommand>(e.Data.PackageData);
                GameSettings.Instance.IsPlayerClient = cc.IsPlayer;
                this.Connected = true;
                break;
            case CommandType.PlayerStateChanged:
                PlayerStateChangedCommand pscc = CommandSerializer.Deserialize<PlayerStateChangedCommand>(e.Data.PackageData);
                InvokeAtMainThread(() => {
                    settings.Player.SendMessage("PlayerStateChanged", pscc);
                });
                break;
            case CommandType.MonsterStateChanged:
                MonsterStateChangedCommand mscc = CommandSerializer.Deserialize<MonsterStateChangedCommand>(e.Data.PackageData);
                InvokeAtMainThread(() =>
                {
                    MonsterManager.Instance.MonsterStateChanged(mscc);
                });
                break;
        }
    }

    private void Client_SendCompleted(object sender, EventArgs e)
    {
        InvokeAtMainThread(() =>
        {
            Debug.Log("send completed!" + sender.ToString() + "   "  + Time.frameCount);
        });
    }

    private void Client_HostDisconnected(object sender, EventArgs e)
    {
    }

    private void Client_HostConnected(object sender, EventArgs e)
    {
        Debug.Log("connected");
        TimeSynchronization();
    }

    public void Send(ICommand cmd)
    {
        var buffer = cmd.Serialize(bufferManager);
        client.Send(buffer);
    }

    private void TimeSynchronization()
    {
        TimeSynchronizationCommand cmd = new TimeSynchronizationCommand();
        cmd.ClientTime = DateTime.Now;
        Send(cmd);
    }

    public IEnumerator SyncSmoothDrag(GameObject obj, Vector3 targetPos, float timeDuration, Action callBack = null)
    {
        float startTime = Time.time;
        float endTime = Time.time + timeDuration;
        float usedTime = 0f;
        while (Time.time < endTime)
        {
            float percentage = usedTime / timeDuration;
            obj.transform.position = Vector3.Lerp(obj.transform.position, targetPos, percentage);
            yield return new WaitForEndOfFrame();
            usedTime = Time.time - startTime;
        }
        if (callBack != null)
            callBack.Invoke();
    }

    public Vector3 PositionPrediction(Vector3 startPosition, Vector3 speed, float startTime, float predictTime)
    {
        float duartion = predictTime - startTime;
        Vector3 position = startPosition + speed * duartion;
        return position;
    }
}
