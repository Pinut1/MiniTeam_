using UnityEngine;

namespace MiniTeam.Core
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [Header("볼륨 (0~1)")]
        [Range(0f, 1f)] public float masterVolume = 1f;
        [Range(0f, 1f)] public float bgmVolume    = 0.6f;
        [Range(0f, 1f)] public float sfxVolume     = 1f;

        private AudioSource bgmSource;
        private AudioSource sfxSource;
        private AudioSource voiceSource; // 대사 음성 전용 (이전 대사를 끊기 위함)

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;

            voiceSource = gameObject.AddComponent<AudioSource>();
            voiceSource.loop = false;

            // 저장된 볼륨 값 로드 (저장된 값이 없으면 기본값 사용)
            masterVolume = PlayerPrefs.GetFloat("SavedMasterVolume", 1f);
            bgmVolume    = PlayerPrefs.GetFloat("SavedBGMVolume", 0.6f);
            sfxVolume    = PlayerPrefs.GetFloat("SavedSFXVolume", 1f);

            ApplyVolumes();
        }

        // ── 재생 ─────────────────────────────────

        public void PlayBGM(AudioClip clip)
        {
            if (clip == null || bgmSource.clip == clip) return;
            bgmSource.clip = clip;
            bgmSource.Play();
        }

        public void StopBGM()
        {
            bgmSource.Stop();
            bgmSource.clip = null;
            bgmSource.pitch = 1f; // 정지 시 기본 피치로 원상복구
        }

        public void SetBGMPitch(float pitchValue)
        {
            bgmSource.pitch = pitchValue;
        }

        public void PlaySFX(AudioClip clip)
        {
            if (clip == null) return;
            sfxSource.PlayOneShot(clip, masterVolume * sfxVolume);
        }

        public void PlaySFX(AudioClip clip, float volumeScale)
        {
            if (clip == null) return;
            sfxSource.PlayOneShot(clip, masterVolume * sfxVolume * volumeScale);
        }

        public void PlayVoice(AudioClip clip)
        {
            if (clip == null) return;
            // 이전 대사 음성이 재생 중이면 끊고 새 음성 재생 (다른 효과음은 영향 안 받음)
            voiceSource.Stop();
            voiceSource.clip = clip;
            voiceSource.Play();
        }

        // ── 볼륨 조절 ─────────────────────────────

        public void SetMasterVolume(float value)
        {
            masterVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat("SavedMasterVolume", masterVolume);
            ApplyVolumes();
        }

        public void SetBGMVolume(float value)
        {
            bgmVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat("SavedBGMVolume", bgmVolume);
            ApplyVolumes();
        }

        public void SetSFXVolume(float value)
        {
            sfxVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat("SavedSFXVolume", sfxVolume);
            ApplyVolumes();
        }

        void ApplyVolumes()
        {
            bgmSource.volume = masterVolume * bgmVolume;
            voiceSource.volume = masterVolume * sfxVolume; // 음성도 기본적으로 효과음 볼륨을 따름
        }
    }
}
