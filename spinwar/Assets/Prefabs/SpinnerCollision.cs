using UnityEngine;

public class SpinnerCollision : MonoBehaviour
{
    public float repulsionForce = 15f; // 튕겨나가는 힘의 세기
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 충돌한 대상도 팽이(Rigidbody)인지 확인
        Rigidbody otherRb = collision.gameObject.GetComponent<Rigidbody>();

        if (otherRb != null)
        {
            // 1. 내 위치에서 상대방 위치로 향하는 방향 계산
            Vector3 pushDirection = (collision.transform.position - transform.position).normalized;

            // 2. Y축 값은 무시 (위로 튀지 않고 수평으로만 튕기게 설정)
            pushDirection.y = 0;

            // 3. 상대방을 내 반대 방향으로 밀어냄
            otherRb.AddForce(pushDirection * repulsionForce, ForceMode.Impulse);

            // 4. (선택 사항) 나도 반대 방향으로 튕겨나가고 싶다면:
            // rb.AddForce(-pushDirection * repulsionForce, ForceMode.Impulse);
        }
    }
}