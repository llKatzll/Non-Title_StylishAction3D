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
    private IMatchmakerTicket matchmakerTicket; //매치메이킹에 필요한 티켓
    private bool matchCreated;

    public PlayerMove otherPlayer; //이동 관련
    public Transform otherPlayerTransform; //rotation

    public int UserIndex = 0;

    public enum NetworkOrder
    {
        PlayerMove = 100, //움직임

        PlayerRunStart, //
        PlayerRunEnd, //

        PlayerRotation, //바라보는 방향

        PlayerAttackStart, //공격 시작 (다른 동작 > 공격 진입)
        PlayerAttack, //공격 중일때, 다음 공격 (공격에서 > 다음 공격)

        PlayerSkill, //스킬 사용
        PlayerJump,

        PlayerGuard, //가드 버튼
        PlayerGuardEnd, //가드 버튼 뗏을때

        PingPong
    }

    public struct NetworkPacket
    {
        public NetworkOrder packetType; //정보의 종류
        public string packetBody;
    }


    private void Awake()
    {
        DontDestroyOnLoad(gameObject); //씬이 전환되어도 파괴되지 않음
    }

    // Start is called before the first frame update
    async void Start()
    {
        client = new Client("http", "222.116.135.178", 7350, "defaultkey", UnityWebRequestAdapter.Instance);


        var expiredDate = DateTime.UtcNow.AddDays(1);
        if (session == null || session.HasExpired(expiredDate)) //세션이 만료되었는가?
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

    public async void OnMatchMakerReceived(IMatchmakerMatched matched) //매치메이킹 성공시 호출되는 함수
    {
        socket.ReceivedMatchmakerMatched -= OnMatchMakerReceived;
        socket.ReceivedMatchPresence += MatchPresence;
        Debug.Log("매치가 생성되었습니다. " + matched.MatchId);

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
            if (users.Current.UserId == session.UserId) //순회하면서 현재 로그인된 세션의 아이디와 비교
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

        Debug.Log("매치메이킹이 시작되었습니다. " + matchmakerTicket.Ticket);
    }

    public async void MatchMakingStop()
    {
        socket.ReceivedMatchmakerMatched -= OnMatchMakerReceived;
        await socket.RemoveMatchmakerAsync(matchmakerTicket);
        Debug.Log("매치메이킹이 중단되었습니다. ");
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
        //SendMatchStateAsync(매치아이디, 0, 보낼정보, 수신자 명단) : 현재 상태를 보내는 함수
        //수신자 명단은 아무것도 적지 않는 경우
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