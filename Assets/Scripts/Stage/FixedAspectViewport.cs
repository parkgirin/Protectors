using UnityEngine;

// 스테이지 전체를 고정된 9:16 영역 안에 렌더링한다.
// 남는 화면은 검은 여백으로 두어 모든 해상도에서 월드와 UI의 배치가 같게 보인다.
public sealed class FixedAspectViewport : MonoBehaviour
{
    private const float TargetAspect = 9f / 16f;

    private Camera stageCamera;
    private Camera backgroundCamera;
    private int lastScreenWidth = -1;
    private int lastScreenHeight = -1;

    public void Configure(Camera camera)
    {
        stageCamera = camera;
        if (stageCamera == null) return;
        CreateBackgroundCamera();
        ApplyViewport(true);
    }

    private void LateUpdate()
    {
        ApplyViewport(false);
    }

    private void CreateBackgroundCamera()
    {
        if (backgroundCamera != null || stageCamera == null) return;
        GameObject background = new GameObject("고정 게임 영역 여백");
        background.transform.SetParent(transform, false);
        backgroundCamera = background.AddComponent<Camera>();
        backgroundCamera.clearFlags = CameraClearFlags.SolidColor;
        backgroundCamera.backgroundColor = Color.black;
        backgroundCamera.cullingMask = 0;
        backgroundCamera.depth = stageCamera.depth - 1f;
        backgroundCamera.rect = new Rect(0f, 0f, 1f, 1f);
        backgroundCamera.orthographic = true;
        backgroundCamera.targetDisplay = stageCamera.targetDisplay;
    }

    private void ApplyViewport(bool force)
    {
        if (stageCamera == null || Screen.width <= 0 || Screen.height <= 0) return;
        if (!force && Screen.width == lastScreenWidth && Screen.height == lastScreenHeight) return;
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;

        float screenAspect = (float)Screen.width / Screen.height;
        Rect viewport;
        if (screenAspect > TargetAspect)
        {
            float width = TargetAspect / screenAspect;
            viewport = new Rect((1f - width) * 0.5f, 0f, width, 1f);
        }
        else
        {
            float height = screenAspect / TargetAspect;
            viewport = new Rect(0f, (1f - height) * 0.5f, 1f, height);
        }
        stageCamera.rect = viewport;
    }

    private void OnDestroy()
    {
        if (backgroundCamera != null) Destroy(backgroundCamera.gameObject);
    }
}
