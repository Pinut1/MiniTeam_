using MiniTeam.Core;
using UnityEngine;

namespace MiniTeam.Sponge
{
    public class SpongeAudioManager : MonoBehaviour
    {
        public static SpongeAudioManager Instance { get; private set; }

        [Header("BGM")]
        public AudioClip bgmOpening;     // AceSponge_Opening    — 오프닝(처음~opening_27)
        public AudioClip bgmCourtroom;   // AceSponge_Courtroom  — 일반 법정 대화
        public AudioClip bgmExamination; // AceSponge_Examination — 증언 시작 / 심문 시작
        public AudioClip bgmTheTruth;    // AceSponge_TheTruth   — evidence_02_22~24 → 엔딩 전환
        public AudioClip bgmEnding;      // AceSponge_Ending     — 엔딩 대사

        [Header("효과음")]
        public AudioClip sfxObjec;       // AceSpong_Objection!  — 증거 제시 성공

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void PlayBGM(AudioClip clip)                    => SoundManager.Instance?.PlayBGM(clip);
        public void StopBGM()                                  => SoundManager.Instance?.StopBGM();
        public void PlaySFX(AudioClip clip)                    => SoundManager.Instance?.PlaySFX(clip);
        // public void PlaySFX(AudioClip clip, float volumeScale) => SoundManager.Instance?.PlaySFX(clip, volumeScale);
    }
}
