using Photon.Pun;
using UnityEngine;

public class Bullet : MonoBehaviourPunCallbacks
{
    public PhotonView phoView;

    int dir; // 총알의 방향

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
        // Ground와 충돌했다면
        if (collision.gameObject.layer == LayerMask.GetMask("Ground"))
        {
            // DestroyRPC 함수 호출
            phoView.RPC("DestroyRPC", RpcTarget.AllBuffered);
        }

        // phoView.IsMine : 내것인지 여부
        // 내 총알이 아니고, 플레이어 태그이며, 내 캐릭터라면 -> 다른 사람이 쏜 총알에 내 캐릭터가 맞는다면
        if (!phoView.IsMine && collision.CompareTag("Player") && collision.GetComponent<PhotonView>().IsMine)
        {
            collision.GetComponent<WizardController>().OnHit();
            phoView.RPC("DestroyRPC", RpcTarget.AllBuffered);
        }
    }

    // PunRPC 마킹을 한 함수를 호출하려면 phoView.RPC 명령어를 써서 이름으로 호출해야함.
    [PunRPC]
    void DestroyWaitRPC() => Destroy(gameObject, 3.0f);

    [PunRPC]
    void DestroyRPC() => Destroy(gameObject);

    [PunRPC]
    void DirRPC(int _dir) => dir = _dir;
}
