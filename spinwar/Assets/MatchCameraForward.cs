using UnityEngine;

public class MatchCameraForward : MonoBehaviour
{
    public Transform cameraTransform; // 따라갈 가상 카메라 또는 메인 카메라

    void LateUpdate()
    {
        if (cameraTransform == null) return;

        // 1. 카메라의 앞방향(Forward) 벡터를 가져옵니다.
        Vector3 forward = cameraTransform.forward;

        // 2. 수직 방향(Y축)은 무시하고 수평 방향만 남깁니다. (물체가 땅을 보거나 하늘을 보지 않게 함)
        forward.y = 0;

        // 3. 벡터의 길이가 0이 아닐 때만 회전값을 적용합니다.
        if (forward.sqrMagnitude > 0.01f)
        {
            // 물체의 정면을 카메라의 수평 정면과 일치시킵니다.
            transform.rotation = Quaternion.LookRotation(forward);
        }
    }
}
