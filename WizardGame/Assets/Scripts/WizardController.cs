using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class WizardController : MonoBehaviourPunCallbacks, IPunObservable
{
    // * Network 객체
    // - MonoBehaviorPunCallbacks 를 상속하면 Network 객체가 된다.

    // * PhotonView 컴포넌트
    // - 네트워크 객체를 동기화 시켜주기 위해 필요한 컴포넌트
    // - 네트워크 객체들의 행동은 모두 PhotonView를 통해 관찰 후 동기화 됨

    // * PhotonView - Syncronization
    // - 어떻게 동기화 할 것인지 정하는 옵션
    // - off : RPC 함수만 사용(동기화 안함)
    // - Reliable Delta Compressed : 받은 데이터를 비교해 같으면 보내지 않음
    // - Unreliable : 계속 데이터를 보냄(손실 가능성 있음)
    // - Unreliable OnChange : 변경이 있을 때 계속 보냄

    public Rigidbody2D Rigidbody2D;
    public Animator Animator;
    public SpriteRenderer SpriteRenderer;
    public PhotonView PhoView;

    public Text NickNameText;
    public Image HealthImage;

    GameObject RespawnPanel;

    bool isGround;
    Vector3 curPos;
    int currentHp = 10;
    int maxHp = 10;

    void Awake()
    {
        Rigidbody2D = GetComponent<Rigidbody2D>();
        Animator = GetComponent<Animator>();
        SpriteRenderer = GetComponent<SpriteRenderer>();
        PhoView = GetComponent<PhotonView>();
    }

    void Start()
    {
        // 내꺼라면 내 닉네임, 아니라면 상대의 닉네임
        NickNameText.text = PhoView.IsMine ? PhotonNetwork.NickName : PhoView.Owner.NickName;
        // 내꺼라면 파란색, 상대꺼라면 빨간색
        NickNameText.color = PhoView.IsMine ? Color.blue : Color.red;

        RespawnPanel = GameObject.Find("RespawnPanel");
        RespawnPanel.SetActive(false);
    }

    void Update()
    {
        // 내꺼라면
        if (PhoView.IsMine)
        {
            Move();
            Jump();
            Shot();
        }
        else // 상대꺼람녀
        {
            transform.position = curPos;
        }
    }

    void Move()
    {
        float axis = Input.GetAxisRaw("Horizontal");
        Rigidbody2D.linearVelocity = new Vector2(4 * axis, Rigidbody2D.linearVelocity.y);

        if (axis != 0)
        {
            Animator.SetBool("Run", true);
            PhoView.RPC("FlipXRPC", RpcTarget.AllBuffered, axis);

            // * RpcTaget
            // 1. All : 자신은 함수를 바로 실행하고 다른 모두에게 전달하고 사라짐
            // 2. AllBuffered
            // - 자신과 다른 모두에게 함수를 전달하고 서버에 정보가 남아있음
            // - 뒤늦게 들어온 사람에게도 정보를 전달(총알 같이 사라져야 하는 것들에 사용)
            // 3. AllViaServer : 모두에게 서버를 거쳐서 동시 호출
            // 4. MasterClient : 방장에게만 호출
            // 5. Others : 나를 제외한 모두에게 호출
            // 6. OthersBuffered : 나를 제외한 모두에게 버퍼와 함께 전달
        }
        else
        {
            Animator.SetBool("Run", false);
        }
    }

    void Jump()
    {
        // 땅과 충돌여부 검사
        isGround = Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(0, -1.3f), 0.07f, LayerMask.GetMask("Ground"));

        Animator.SetBool("Jump", !isGround);
        if (Input.GetKeyDown(KeyCode.Space) && isGround)
        {
            PhoView.RPC("JumpRPC", RpcTarget.All);
        }
    }

    void Shot()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GameObject go = PhotonNetwork.Instantiate("Bullet", transform.position + new Vector3(0, -0.25f, 0), Quaternion.identity);
            go.GetComponent<PhotonView>().RPC("DirRPC", RpcTarget.All, SpriteRenderer.flipX ? -1 : 1);
            Animator.SetTrigger("Attack");
        }
    }

    public void OnHit()
    {
        currentHp--;
        HealthImage.fillAmount = (float)currentHp / maxHp;
        if (currentHp <= 0)
        {
            RespawnPanel.SetActive(true);
            PhoView.RPC("DestroyRPC", RpcTarget.AllBuffered);
        }
    }

    // 서버에 의해서 주기적으로 호출되는 함수
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // * PhotonStream
        // - 특정 메세지, RPC 또는 정보 업데이트를 위한 클래스

        // 내꺼라면 Writing이다.
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(HealthImage.fillAmount);
        }
        // 상대 컴퓨터에 있는 내 복제품이라면 Reading이다.
        if (stream.IsReading)
        {
            // SendNext로 보낸 순서대로 받아와진다.
            curPos = (Vector3)stream.ReceiveNext();
            HealthImage.fillAmount = (float)stream.ReceiveNext();
        }
    }

    [PunRPC]
    void FlipXRPC(float axis) => SpriteRenderer.flipX = (axis == -1);

    [PunRPC]
    void JumpRPC()
    {
        Rigidbody2D.linearVelocity = Vector2.zero;
        Rigidbody2D.AddForce(Vector2.up * 300);
    }

    [PunRPC]
    void DestroyRPC() => Destroy(gameObject);
}
