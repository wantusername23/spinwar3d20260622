using UnityEngine;

public class KillPlayer : MonoBehaviour
{
    [Header("스폰 지점 설정")]
    public Transform respawnPoint1;
    public Transform respawnPoint2;

    private void OnCollisionEnter(Collision other)
    {
        // 1. 부딪힌 물체의 최상위 부모(Root)를 찾습니다.
        // 복합적인 모델의 경우 자식 콜라이더가 먼저 닿기 때문입니다.
        GameObject rootObject = other.transform.root.gameObject;
        string rootTag = rootObject.tag;

        Debug.Log($"[충돌 발생] 부딪힌 물체: {other.gameObject.name}, 최상위 태그: {rootTag}");

        // 2. 최상위 부모의 태그를 기준으로 리스폰 판단
        if (rootTag == "Player")
        {
            Debug.Log("<color=green>Player 1 리스폰 실행</color>");
            Respawn(rootObject, respawnPoint1);
        }
        else if (rootTag == "Player2") // 이미지 설정대로 Player2 인식
        {
            Debug.Log("<color=green>Player 2 리스폰 실행</color>");
            Respawn(rootObject, respawnPoint2);
        }
    }

    private void Respawn(GameObject target, Transform spawnPoint)
    {
        if (spawnPoint == null)
        {
            Debug.LogError($"{target.name}의 리스폰 지점이 인스펙터에 할당되지 않았습니다!");
            return;
        }

        // Rigidbody를 가져옵니다 (최상위 오브젝트에 있어야 함)
        Rigidbody rb = target.GetComponent<Rigidbody>();

        if (rb != null)
        {
            // 물리 연산을 잠시 꺼서 텔레포트 시 발생하는 물리 버그 방지
            rb.isKinematic = true;

            // 위치와 회전값 설정 (인스펙터에 할당된 지점으로 이동)
            target.transform.position = spawnPoint.position;
            target.transform.rotation = spawnPoint.rotation;

            // 기존에 가지고 있던 속도 초기화
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // 다시 물리 연산 활성화
            rb.isKinematic = false;
        }
        else
        {
            // Rigidbody가 없는 경우 단순 위치 이동
            target.transform.position = spawnPoint.position;
            target.transform.rotation = spawnPoint.rotation;
        }
    }
}