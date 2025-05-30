using UnityEngine;
using Photon.Pun;
using System.Collections;
using UnityEngine.UI;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public InputField NickNameInput; // 닉네임 입력 UI
    public GameObject ConnectPanel; // 접속 패널
    public GameObject RespawnPanel; // 재시작 패널
    public Button ConnectButton; // 접속 버튼
    public Button RespawnButton; // 재시작 버튼

    private void Awake()
    {
        // 해상도 960, 540, 창모드
        Screen.SetResolution(960, 540, false);
        // 패킷을 초당 몇 번 전송하는지 설정
        PhotonNetwork.SendRate = 60;
        // PhotonView들이 OnPhotonSerialize를 초당 몇 회 호출하는지
        PhotonNetwork.SerializationRate = 30;
    }

    private void Start()
    {
        ConnectButton.onClick.AddListener(OnClickConnectButton);
        RespawnButton.onClick.AddListener(OnPlayerSpawn);
    }

    void OnClickConnectButton()
    {
        // 마스터 서버 접속 시도
        PhotonNetwork.ConnectUsingSettings();
    }

    // 마스터 서버 접속 시도 시 호출되는 함수
    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        // Room이 있다면 입장, 없다면 만듦
        PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions { MaxPlayers = 6 }, null);
    }

    // Room에 입장 시 호출
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        // 서버에 입장한 플레이어 닉네임 정보를 넣어줌
        PhotonNetwork.LocalPlayer.NickName = NickNameInput.text;
        ConnectPanel.SetActive(false); // 접속창 비활성화
        StartCoroutine(CoDestroyBullet()); // 접속 시 서버에 남아있는 총알 제거
        OnPlayerSpawn(); // 플레이어 생성
    }

    IEnumerator CoDestroyBullet()
    {
        yield return new WaitForSeconds(0.2f);
        foreach(GameObject go in GameObject.FindGameObjectsWithTag("Bullet"))
        {
            // 모든 네트워크 객체에 DestroyRPC라는 RPC 함수를 호출
            go.GetComponent<PhotonView>().RPC("DestroyRPC", RpcTarget.All);
        }
    }

    public void OnPlayerSpawn()
    {
        // Resources 폴더에 Wizard라는 이름을 가진 프리팹을 생성
        PhotonNetwork.Instantiate("Wizard", new Vector3(0, 5, 0), Quaternion.identity);
        //RespawnPanel.SetActive(false); // 리스폰 패널 비활성화
    }

    private void Update()
    {
        // ESC 키를 눌렀다면
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 서버와 연결 중이라면
            if (PhotonNetwork.IsConnected)
            {
                // 서버와 연결 해제
                PhotonNetwork.Disconnect();
                Application.Quit(0); // 게임 종료
            }
        }
    }

    // 서버와 연결이 해제된 경우
    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        ConnectPanel.SetActive(true); // 접속창 활성화
        RespawnPanel.SetActive(false); // 재시작창 비활성화
    }
}
