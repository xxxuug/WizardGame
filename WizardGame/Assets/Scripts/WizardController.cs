using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class WizardController : MonoBehaviourPunCallbacks, IPunObservable
{
    // * Network ��ü
    // - MonoBehaviorPunCallbacks �� ����ϸ� Network ��ü�� �ȴ�.

    // * PhotonView ������Ʈ
    // - ��Ʈ��ũ ��ü�� ����ȭ �����ֱ� ���� �ʿ��� ������Ʈ
    // - ��Ʈ��ũ ��ü���� �ൿ�� ��� PhotonView�� ���� ���� �� ����ȭ ��

    // * PhotonView - Syncronization
    // - ��� ����ȭ �� ������ ���ϴ� �ɼ�
    // - off : RPC �Լ��� ���(����ȭ ����)
    // - Reliable Delta Compressed : ���� �����͸� ���� ������ ������ ����
    // - Unreliable : ��� �����͸� ����(�ս� ���ɼ� ����)
    // - Unreliable OnChange : ������ ���� �� ��� ����

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
        // ������� �� �г���, �ƴ϶�� ����� �г���
        NickNameText.text = PhoView.IsMine ? PhotonNetwork.NickName : PhoView.Owner.NickName;
        // ������� �Ķ���, ��벨��� ������
        NickNameText.color = PhoView.IsMine ? Color.blue : Color.red;

        RespawnPanel = GameObject.Find("RespawnPanel");
        RespawnPanel.SetActive(false);
    }

    void Update()
    {
        // �������
        if (PhoView.IsMine)
        {
            Move();
            Jump();
            Shot();
        }
        else // ��벨����
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
            // 1. All : �ڽ��� �Լ��� �ٷ� �����ϰ� �ٸ� ��ο��� �����ϰ� �����
            // 2. AllBuffered
            // - �ڽŰ� �ٸ� ��ο��� �Լ��� �����ϰ� ������ ������ ��������
            // - �ڴʰ� ���� ������Ե� ������ ����(�Ѿ� ���� ������� �ϴ� �͵鿡 ���)
            // 3. AllViaServer : ��ο��� ������ ���ļ� ���� ȣ��
            // 4. MasterClient : ���忡�Ը� ȣ��
            // 5. Others : ���� ������ ��ο��� ȣ��
            // 6. OthersBuffered : ���� ������ ��ο��� ���ۿ� �Բ� ����
        }
        else
        {
            Animator.SetBool("Run", false);
        }
    }

    void Jump()
    {
        // ���� �浹���� �˻�
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

    // ������ ���ؼ� �ֱ������� ȣ��Ǵ� �Լ�
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // * PhotonStream
        // - Ư�� �޼���, RPC �Ǵ� ���� ������Ʈ�� ���� Ŭ����

        // ������� Writing�̴�.
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(HealthImage.fillAmount);
        }
        // ��� ��ǻ�Ϳ� �ִ� �� ����ǰ�̶�� Reading�̴�.
        if (stream.IsReading)
        {
            // SendNext�� ���� ������� �޾ƿ�����.
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
