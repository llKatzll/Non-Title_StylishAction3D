using Nakama;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Network : MonoBehaviour
{


    public IClient client;
    public ISession session;
    public ISocket socket;

    private IMatch match;
    private IMatchmakerTicket matchmakerTicket; //��ġ����ŷ�� �ʿ��� Ƽ��
    private bool matchCreated;

    public PlayerMove otherPlayer; //�̵� ����
    public Transform otherPlayerTransform; //rotation

    public int UserIndex = 0;

    public enum NetworkOrder
    {
        PlayerMove = 100, //������

        PlayerRunStart, //
        PlayerRunEnd, //

        PlayerRotation, //�ٶ󺸴� ����

        PlayerAttackStart, //���� ���� (�ٸ� ���� > ���� ����)
        PlayerAttack, //���� ���϶�, ���� ���� (���ݿ��� > ���� ����)

        PlayerSkill, //��ų ���
        PlayerJump,

        PlayerGuard, //���� ��ư
        PlayerGuardEnd, //���� ��ư ������

        PingPong
    }

    public struct NetworkPacket
    {
        public NetworkOrder packetType; //������ ����
        public string packetBody;
    }


    private void Awake()
    {
        DontDestroyOnLoad(gameObject); //���� ��ȯ�Ǿ �ı����� ����
    }

    // Start is called before the first frame update
    async void Start()
    {
        client = new Client("http", "222.116.135.178", 7350, "defaultkey", UnityWebRequestAdapter.Instance);


        var expiredDate = DateTime.UtcNow.AddDays(1);
        if (session == null || session.HasExpired(expiredDate)) //������ ����Ǿ��°�?
        {
            var deviceId = PlayerPrefs.GetString("deviceId", SystemInfo.deviceUniqueIdentifier);

            if (deviceId == SystemInfo.unsupportedIdentifier)
            {
                deviceId = Guid.NewGuid().ToString();
            }

            PlayerPrefs.SetString("deviceId", deviceId);

            try
            {
                session = await client.AuthenticateDeviceAsync(deviceId, name, true);
                PlayerPrefs.SetString("sessionToken", session.AuthToken);
                Debug.Log(session);
                //SocketInit();

                socket = client.NewSocket();
                await socket.ConnectAsync(session);
            }
            catch (Exception ex)
            {
                Debug.LogAssertion(ex);
            }
        }
    }

    public async void OnMatchMakerReceived(IMatchmakerMatched matched) //��ġ����ŷ ������ ȣ��Ǵ� �Լ�
    {
        socket.ReceivedMatchmakerMatched -= OnMatchMakerReceived;
        socket.ReceivedMatchPresence += MatchPresence;
        Debug.Log("��ġ�� �����Ǿ����ϴ�. " + matched.MatchId);

        match = await socket.JoinMatchAsync(matched);
        matchCreated = true;

        socket.ReceivedMatchState += OtherInfo;
    }

    public void MatchPresence(IMatchPresenceEvent presenceEvent)
    {
        var users = presenceEvent.Joins.GetEnumerator();
        var i = 0;
        while (users.MoveNext())
        {
            if (users.Current.UserId == session.UserId) //��ȸ�ϸ鼭 ���� �α��ε� ������ ���̵�� ��
            {
                break;
            }
            i++;
        }

        UserIndex = i;
    }
    public async void MatchMakingStart()
    {
        socket.ReceivedMatchmakerMatched += OnMatchMakerReceived;
        matchmakerTicket = await socket.AddMatchmakerAsync("*", 2, 2);

        Debug.Log("��ġ����ŷ�� ���۵Ǿ����ϴ�. " + matchmakerTicket.Ticket);
    }

    public async void MatchMakingStop()
    {
        socket.ReceivedMatchmakerMatched -= OnMatchMakerReceived;
        await socket.RemoveMatchmakerAsync(matchmakerTicket);
        Debug.Log("��ġ����ŷ�� �ߴܵǾ����ϴ�. ");
    }


    private void Update()
    {
        if (matchCreated)
        {
            matchCreated = false;
            SceneManager.LoadScene(1);
        }
    }

    public void Move(float[] dir)
    {
        //SendMatchStateAsync(��ġ���̵�, 0, ��������, ������ ���) : ���� ���¸� ������ �Լ�
        //������ ����� �ƹ��͵� ���� �ʴ� ���
        NetworkPacket packet = new NetworkPacket()
        {
            packetType = NetworkOrder.PlayerMove,
            packetBody = JsonConvert.SerializeObject(dir)
        };

        socket.SendMatchStateAsync(match.Id, (int)NetworkOrder.PlayerMove, JsonConvert.SerializeObject(packet));
    }
    public void Look(Vector3 rotation)
    {
        NetworkPacket packet = new NetworkPacket()
        {
            packetType = NetworkOrder.PlayerMove,
            packetBody = JsonConvert.SerializeObject(new float[] { rotation.x, rotation.y, rotation.z })
        };

        socket.SendMatchStateAsync(match.Id, (int)NetworkOrder.PlayerRotation, JsonConvert.SerializeObject(packet));
    }

    public void Ping()
    {
        NetworkPacket packet = new NetworkPacket()
        {
            packetType = NetworkOrder.PlayerMove,
            packetBody = DateTimeOffset.Now.ToUnixTimeMilliseconds() + ""
        };

        socket.SendMatchStateAsync(match.Id, (int)NetworkOrder.PingPong, JsonConvert.SerializeObject(packet));
    }

    public void SendPacket(NetworkOrder order, string msg)
    {
        NetworkPacket packet = new NetworkPacket()
        {
            packetType = order,
            packetBody = msg
        };

        socket.SendMatchStateAsync(match.Id, (int)order, JsonConvert.SerializeObject(packet));
    }

    void OtherInfo(IMatchState state)
    {
        var aa = JsonConvert.DeserializeObject<NetworkPacket>(Encoding.UTF8.GetString(state.State));
        switch ((NetworkOrder)state.OpCode)
        {
            case NetworkOrder.PlayerMove:
                float[] dirF = JsonConvert.DeserializeObject<float[]>(aa.packetBody);
                otherPlayer.Move(dirF);
                break;
            case NetworkOrder.PlayerRotation:
                float[] rot = JsonConvert.DeserializeObject<float[]>(aa.packetBody);
                UnityMainThreadDispatcher.Instance().Enqueue(() => otherPlayerTransform.localEulerAngles = new Vector3(rot[0], rot[1], rot[2]));
                break;
            case NetworkOrder.PlayerGuard:
                UnityMainThreadDispatcher.Instance().Enqueue(() => { otherPlayer.GuardStart(); });
                break;
            case NetworkOrder.PlayerGuardEnd:
                UnityMainThreadDispatcher.Instance().Enqueue(() => { otherPlayer.GuardEnd(); });
                break;
            case NetworkOrder.PlayerAttackStart:
                Debug.LogAssertion("Other Player Attacked!");
                UnityMainThreadDispatcher.Instance().Enqueue(() => { otherPlayer.AttackStart(); });
                break;
            case NetworkOrder.PlayerAttack:
                Debug.LogAssertion("Other Player Attacked!");
                UnityMainThreadDispatcher.Instance().Enqueue(() => { otherPlayer.Attack(bool.Parse(aa.packetBody)); });
                break;
            case NetworkOrder.PlayerJump:
                UnityMainThreadDispatcher.Instance().Enqueue(() => { otherPlayer.Jump(); });
                break;
            case NetworkOrder.PingPong:
                Debug.Log("Ping Pong Diff : " + (DateTimeOffset.Now.ToUnixTimeMilliseconds() - long.Parse(aa.packetBody)));
                break;
            case NetworkOrder.PlayerSkill:
                Dictionary<string, int> map = JsonConvert.DeserializeObject<Dictionary<string, int>>(aa.packetBody);
                UnityMainThreadDispatcher.Instance().Enqueue(() => { otherPlayer.SkillUse((SkillCommand.Skill)map["skill"]); });
                break;
            case NetworkOrder.PlayerRunStart:
                UnityMainThreadDispatcher.Instance().Enqueue(() => { otherPlayer.RunStart(); });
                break;
            case NetworkOrder.PlayerRunEnd:
                UnityMainThreadDispatcher.Instance().Enqueue(() => { otherPlayer.RunEnd(); });
                break;
        }
    }
}