using Photon.Pun;
using UnityEngine;

public class Bullet : MonoBehaviourPunCallbacks
{
    public PhotonView phoView;

    int dir; // �Ѿ��� ����

    void Start()
    {
        phoView = GetComponent<PhotonView>();
        phoView.RPC("DestroyWaitRPC", RpcTarget.All);
    }

    void Update()
    {
        transform.Translate(Vector3.right * 7 * Time.deltaTime * dir);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Ground�� �浹�ߴٸ�
        if (collision.gameObject.layer == LayerMask.GetMask("Ground"))
        {
            // DestroyRPC �Լ� ȣ��
            phoView.RPC("DestroyRPC", RpcTarget.AllBuffered);
        }

        // phoView.IsMine : �������� ����
        // �� �Ѿ��� �ƴϰ�, �÷��̾� �±��̸�, �� ĳ���Ͷ�� -> �ٸ� ����� �� �Ѿ˿� �� ĳ���Ͱ� �´´ٸ�
        if (!phoView.IsMine && collision.CompareTag("Player") && collision.GetComponent<PhotonView>().IsMine)
        {
            collision.GetComponent<WizardController>().OnHit();
            phoView.RPC("DestroyRPC", RpcTarget.AllBuffered);
        }
    }

    // PunRPC ��ŷ�� �� �Լ��� ȣ���Ϸ��� phoView.RPC ��ɾ �Ἥ �̸����� ȣ���ؾ���.
    [PunRPC]
    void DestroyWaitRPC() => Destroy(gameObject, 3.0f);

    [PunRPC]
    void DestroyRPC() => Destroy(gameObject);

    [PunRPC]
    void DirRPC(int _dir) => dir = _dir;
}
