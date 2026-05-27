using UnityEngine;

namespace MiniTeam.Tetris
{
    public class WallDissolveController : MonoBehaviour
    {
        private Renderer targetRenderer;
        private MaterialPropertyBlock propBlock;

        [Header("단계별 디졸브 수치 (0.75, 0.8, 0.9, 1.0)")]
        private readonly float[] dissolveStages = { 0.75f, 0.8f, 0.9f, 1.0f };
         
        private void Awake()
        {
            targetRenderer = GetComponent<Renderer>();
            propBlock = new MaterialPropertyBlock();
        }

        /// <summary>
        /// 타격 횟수(1~4)를 받아 디졸브 단계를 설정합니다.
        /// </summary>
        /// <param name="hitCount">현재 타격 횟수</param>
        public void SetDissolveStage(int hitCount)
        {
            if (targetRenderer == null) return;

            // 1~4의 숫자를 받아서 0~3 인덱스로 보정
            int index = Mathf.Clamp(hitCount - 1, 0, dissolveStages.Length - 1);
            float amount = dissolveStages[index];

            // MaterialPropertyBlock을 사용하여 개별 수치 적용 (최적화 방식)
            targetRenderer.GetPropertyBlock(propBlock);
            propBlock.SetFloat("_DissolveAmount", amount);
            targetRenderer.SetPropertyBlock(propBlock);

            Debug.Log($"[WallDissolve] {gameObject.name} 단계 변경: {amount} (타격: {hitCount}회)");
        }

        /// <summary>
        /// 디졸브 효과를 초기화(0)합니다.
        /// </summary>
        public void ResetDissolve()
        {
            if (targetRenderer == null) return;

            targetRenderer.GetPropertyBlock(propBlock);
            propBlock.SetFloat("_DissolveAmount", 0f);
            targetRenderer.SetPropertyBlock(propBlock);
        }
        
        /// <summary>
        /// 타격 지점 월드 좌표를 셰이더에 전달합니다.
        /// </summary>
        public void SetImpactPosition(Vector3 worldPos)
        {
            if (targetRenderer == null) return;

            targetRenderer.GetPropertyBlock(propBlock);
            propBlock.SetVector("_ImpactPos", worldPos);
            targetRenderer.SetPropertyBlock(propBlock);
        }
    }
}
