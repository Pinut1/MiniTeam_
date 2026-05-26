using UnityEngine;

[ExecuteInEditMode]
public class EyeOpeningEffect : MonoBehaviour
{
    [Range(0.001f, 1.0f)] public float openAmount = 0.001f;
    [Range(0.0f, 2.0f)] public float expand = 0.0f;
    public float smoothness = 0.1f;

    public Shader shader;
    private Material effectMaterial;

    // 1. 머티리얼을 안전하게 가져오는 프로퍼티 (Lazy Initialization)
    private Material Material
    {
        get
        {
            if (effectMaterial == null && shader != null)
            {
                effectMaterial = new Material(shader);
                // 에디터에서 머티리얼이 에셋으로 저장되는 것을 방지
                effectMaterial.hideFlags = HideFlags.HideAndDontSave;
            }
            return effectMaterial;
        }
    }

    private void OnDisable()
    {
        // 2. 컴포넌트가 꺼지거나 삭제될 때 메모리 누수 방지를 위해 머티리얼 파괴
        if (effectMaterial != null)
        {
            if (Application.isPlaying) Destroy(effectMaterial);
            else DestroyImmediate(effectMaterial);
            effectMaterial = null;
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        // 3. 셰이더가 연결되지 않았거나 머티리얼 생성에 실패하면 원본 출력
        if (shader == null || Material == null)
        {
            Graphics.Blit(source, destination);
            return;
        }

        Material.SetFloat("_OpenAmount", openAmount);
        Material.SetFloat("_Expand", expand);
        Material.SetFloat("_Smoothness", smoothness);

        Graphics.Blit(source, destination, Material);
    }
}