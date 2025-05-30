using UnityEngine;
using Photon.Pun;
using System.Collections;
using UnityEngine.UI;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public InputField NickNameInput; // �г��� �Է� UI
    public GameObject ConnectPanel; // ���� �г�
    public GameObject RespawnPanel; // ����� �г�
    public Button ConnectButton; // ���� ��ư
    public Button RespawnButton; // ����� ��ư

    private void Awake()
    {
        // �ػ� 960, 540, â���
        Screen.SetResolution(960, 540, false);
        // ��Ŷ�� �ʴ� �� �� �����ϴ��� ����
        PhotonNetwork.SendRate = 60;
        // PhotonView���� OnPhotonSerialize�� �ʴ� �� ȸ ȣ���ϴ���
        PhotonNetwork.SerializationRate = 30;
    }

    private void Start()
    {
        ConnectButton.onClick.AddListener(OnClickConnectButton);
        RespawnButton.onClick.AddListener(OnPlayerSpawn);
    }

    void OnClickConnectButton()
    {
        // ������ ���� ���� �õ�
        PhotonNetwork.ConnectUsingSettings();
    }

    // ������ ���� ���� �õ� �� ȣ��Ǵ� �Լ�
    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        // Room�� �ִٸ� ����, ���ٸ� ����
        PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions { MaxPlayers = 6 }, null);
    }

    // Room�� ���� �� ȣ��
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        // ������ ������ �÷��̾� �г��� ������ �־���
        PhotonNetwork.LocalPlayer.NickName = NickNameInput.text;
        ConnectPanel.SetActive(false); // ����â ��Ȱ��ȭ
        StartCoroutine(CoDestroyBullet()); // ���� �� ������ �����ִ� �Ѿ� ����
        OnPlayerSpawn(); // �÷��̾� ����
    }

    IEnumerator CoDestroyBullet()
    {
        yield return new WaitForSeconds(0.2f);
        foreach(GameObject go in GameObject.FindGameObjectsWithTag("Bullet"))
        {
            // ��� ��Ʈ��ũ ��ü�� DestroyRPC��� RPC �Լ��� ȣ��
            go.GetComponent<PhotonView>().RPC("DestroyRPC", RpcTarget.All);
        }
    }

    public void OnPlayerSpawn()
    {
        // Resources ������ Wizard��� �̸��� ���� �������� ����
        PhotonNetwork.Instantiate("Wizard", new Vector3(0, 5, 0), Quaternion.identity);
        //RespawnPanel.SetActive(false); // ������ �г� ��Ȱ��ȭ
    }

    private void Update()
    {
        // ESC Ű�� �����ٸ�
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // ������ ���� ���̶��
            if (PhotonNetwork.IsConnected)
            {
                // ������ ���� ����
                PhotonNetwork.Disconnect();
                Application.Quit(0); // ���� ����
            }
        }
    }

    // ������ ������ ������ ���
    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        ConnectPanel.SetActive(true); // ����â Ȱ��ȭ
        RespawnPanel.SetActive(false); // �����â ��Ȱ��ȭ
    }
}
