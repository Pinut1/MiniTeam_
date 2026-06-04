using UnityEngine;

namespace MiniTeam.Core
{
    // Hub 씬 전용 오디오 관리자
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("BGM")]
        public AudioClip bgmHub;

        [Header("효과음 (UI)")]
        public AudioClip sfxClick;
        public AudioClip sfxCancel;
        public AudioClip sfxPopupOpen;
        public AudioClip sfxSmallJudangchiAppear; // 작은 주댕치 등장음
        
        [Header("효과음 (상호작용)")]
        public AudioClip sfxInteract;
        public AudioClip sfxGetItem;

      

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
        public void PlaySFX(AudioClip clip, float volumeScale) => SoundManager.Instance?.PlaySFX(clip, volumeScale);
    
        public void PlayVoice(AudioClip clip)                  => SoundManager.Instance?.PlayVoice(clip);
    }
}
