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

    public class Vector3Class
    {
        public float x;
        public float y;
        public float z;
    }
    Vector3Class vector3 = new Vector3Class();


    public IClient client;
    public ISession session;
    public ISocket socket;

    private IMatch match;
    private bool matchCreated;

    public PlayerMove otherPlayer; //이동 관련
    public Transform otherPlayerTransform; //rotation

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
        Debug.Log("매치가 생성되었습니다. " + matched.MatchId);

        match = await socket.JoinMatchAsync(matched);
        matchCreated = true;
        socket.ReceivedMatchState += OtherInfo;
    }
    public async void MatchMakingStart()
    {
        socket.ReceivedMatchmakerMatched += OnMatchMakerReceived;
        var ticket = await socket.AddMatchmakerAsync("*", 2, 2);

        Debug.Log("매치메이킹이 시작되었습니다. " + ticket.Ticket);
    }


    private void Update()
    {
        if (matchCreated)
        {
            matchCreated = false;
            SceneManager.LoadScene(1);
        }
    }

    public void Move(Vector3 dir)
    {
        //SendMatchStateAsync(매치아이디, 0, 보낼정보, 수신자 명단) : 현재 상태를 보내는 함수
        //수신자 명단은 아무것도 적지 않는 경우
        vector3.x = dir.x;
        vector3.y = dir.y;
        vector3.z = dir.z;
        socket.SendMatchStateAsync(match.Id, 100, JsonConvert.SerializeObject(vector3));
    }
    public void Look(Vector3 rotation)
    {
        vector3.x = rotation.x;
        vector3.y = rotation.y;
        vector3.z = rotation.z;
        socket.SendMatchStateAsync(match.Id, 101, JsonConvert.SerializeObject(vector3));
    }

    void OtherInfo(IMatchState state)
    {
        var aa = JsonConvert.DeserializeObject<Vector3Class>(Encoding.UTF8.GetString(state.State));
        switch (state.OpCode)
        {
            case 100:
                otherPlayer.Move(new Vector3(aa.x, aa.y, aa.z));
                break;
            case 101:
                UnityMainThreadDispatcher.Instance().Enqueue(() => otherPlayerTransform.localEulerAngles = new Vector3(aa.x, aa.y, aa.z));
                break;
        }
    }
}