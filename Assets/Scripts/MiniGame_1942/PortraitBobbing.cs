using UnityEngine;

namespace MiniTeam.Shooting1942
{
    public class PortraitBobbing : MonoBehaviour
    {
        [System.Serializable]
        public struct PartSettings
        {
            public Transform part;
            public float     amplitude; // 흔들림 폭 (px 또는 unit)
            public float     speed;     // 흔들림 속도
            public float     phase;     // 위상 오프셋 (다른 부위와 엇박자)
        }

        public PartSettings body;
        public PartSettings leftHand;
        public PartSettings rightHand;
        public PartSettings head;

        Vector3 bodyOrigin, leftHandOrigin, rightHandOrigin, headOrigin;

        void Start()
        {
            if (body.part      != null) bodyOrigin      = body.part.localPosition;
            if (leftHand.part  != null) leftHandOrigin  = leftHand.part.localPosition;
            if (rightHand.part != null) rightHandOrigin = rightHand.part.localPosition;
            if (head.part      != null) headOrigin      = head.part.localPosition;
        }

        void Update()
        {
            float t = Time.time;
            if (body.part      != null)
                body.part.localPosition      = bodyOrigin      + Vector3.up * Mathf.Sin(t * body.speed      + body.phase)      * body.amplitude;
            if (leftHand.part  != null)
                leftHand.part.localPosition  = leftHandOrigin  + Vector3.up * Mathf.Sin(t * leftHand.speed  + leftHand.phase)  * leftHand.amplitude;
            if (rightHand.part != null)
                rightHand.part.localPosition = rightHandOrigin + Vector3.up * Mathf.Sin(t * rightHand.speed + rightHand.phase) * rightHand.amplitude;
            if (head.part      != null)
                head.part.localPosition      = headOrigin      + Vector3.up * Mathf.Sin(t * head.speed      + head.phase)      * head.amplitude;
        }
    }
}
