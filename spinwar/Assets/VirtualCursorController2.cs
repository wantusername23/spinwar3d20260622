using UnityEngine;
using UnityEngine.UI;

public class VirtualCursorController2 : MonoBehaviour
{
    [Header("설정")]
    public RectTransform cursorTransform; // 커서 UI의 RectTransform
    public float cursorSpeed = 500f;      // 커서 이동 속도
    public Canvas parentCanvas;           // 현재 커서가 속한 캔버스

    [Header("입력 키 설정")]
    public string horizontalAxis = "Jump"; // 가로축 입력 이름
    //public string verticalAxis = "Vertical";     // 세로축 입력 이름

    private Vector2 canvasSize;

    void Start()
    {
        // 캔버스의 크기를 가져옵니다.
        RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();
        canvasSize = new Vector2(canvasRect.rect.width, canvasRect.rect.height);
    }

    void Update()
    {
        // 1. 입력값 받기 (Input Manager 기준)
        float h = Input.GetAxisRaw(horizontalAxis);
        //float v = Input.GetAxisRaw(verticalAxis);

        Vector2 move = new Vector2(h, 0).normalized * cursorSpeed * Time.deltaTime;

        // 2. 커서 위치 이동
        Vector2 newPos = cursorTransform.anchoredPosition + move;

        // 3. 화면 밖으로 나가지 않도록 제한 (Clamp)
        // Screen Space - Camera이므로 캔버스 크기 절반 범위 내로 제한합니다.
        float limitX = canvasSize.x / 2f;
        float limitY = canvasSize.y / 2f;

        newPos.x = Mathf.Clamp(newPos.x, -limitX, limitX);
        newPos.y = Mathf.Clamp(newPos.y, -limitY, limitY);

        cursorTransform.anchoredPosition = newPos;
    }
}
