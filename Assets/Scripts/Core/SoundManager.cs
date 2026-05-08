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

        /// <summary>
        /// Ensures this object becomes the global SoundManager singleton (destroying the GameObject if another instance exists), makes it persist across scene loads, initializes the BGM and SFX AudioSource components (looping BGM, non-looping SFX), and applies initial volume settings.
        /// </summary>
        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;

            ApplyVolumes();
        }

        /// <summary>
        /// Starts the given audio clip as the background music if it differs from the currently assigned clip.
        /// </summary>
        /// <param name="clip">The BGM clip to play; if null or the same as the currently playing clip, no action is taken.</param>

        public void PlayBGM(AudioClip clip)
        {
            if (clip == null || bgmSource.clip == clip) return;
            bgmSource.clip = clip;
            bgmSource.Play();
        }

        /// <summary>
        /// Stops the currently playing background music and clears the assigned BGM clip.
        /// </summary>
        public void StopBGM()
        {
            bgmSource.Stop();
            bgmSource.clip = null;
        }

        /// <summary>
        /// Plays the given sound effect once using the current master and SFX volume settings; does nothing if the clip is null.
        /// </summary>
        /// <param name="clip">The audio clip to play; ignored if null.</param>
        public void PlaySFX(AudioClip clip)
        {
            if (clip == null) return;
            sfxSource.PlayOneShot(clip, masterVolume * sfxVolume);
        }

        /// <summary>
        /// Set the master volume level used to scale BGM and SFX playback.
        /// </summary>
        /// <param name="value">Desired master volume; values less than 0 are treated as 0 and values greater than 1 are treated as 1.</param>

        public void SetMasterVolume(float value)
        {
            masterVolume = Mathf.Clamp01(value);
            ApplyVolumes();
        }

        /// <summary>
        /// Sets the background music (BGM) volume and updates the active audio sources.
        /// </summary>
        /// <param name="value">Desired BGM volume; values less than 0 are treated as 0 and values greater than 1 are treated as 1.</param>
        public void SetBGMVolume(float value)
        {
            bgmVolume = Mathf.Clamp01(value);
            ApplyVolumes();
        }

        /// <summary>
        /// Sets the sound effects (SFX) volume level for future SFX playback.
        /// </summary>
        /// <param name="value">Desired SFX volume in the range 0 to 1; values outside this range are clamped. The change is applied immediately.</param>
        public void SetSFXVolume(float value)
        {
            sfxVolume = Mathf.Clamp01(value);
            ApplyVolumes();
        }

        /// <summary>
        /// Applies the current master and BGM volume settings to the background-music audio source.
        /// </summary>
        void ApplyVolumes()
        {
            bgmSource.volume = masterVolume * bgmVolume;
        }
    }
}
