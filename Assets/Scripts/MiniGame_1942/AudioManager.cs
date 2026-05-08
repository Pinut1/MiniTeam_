using MiniTeam.Core;
using UnityEngine;

namespace MiniTeam.Shooting1942
{
    // 1942 전용 오디오 클립 슬롯 — 실제 재생은 Core.SoundManager에 위임
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("BGM")]
        public AudioClip bgmWave1;
        public AudioClip bgmWave2;
        public AudioClip bgmBoss;
        public AudioClip bgmClear;
        public AudioClip bgmGameOver;

        [Header("효과음")]
        public AudioClip sfxPlayerShoot;
        public AudioClip sfxEnemyDie;
        public AudioClip sfxBossHit;
        public AudioClip sfxPlayerHit;

        /// <summary>
        /// Ensures a single active AudioManager instance for the scene by enforcing the singleton pattern.
        /// </summary>
        /// <remarks>
        /// If another AudioManager instance already exists, the current GameObject is destroyed; otherwise this instance becomes the singleton.
        /// </remarks>
        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        /// <summary>
/// Plays the provided audio clip as the current background music.
/// </summary>
/// <param name="clip">The audio clip to play as background music. If the audio subsystem is unavailable, the call is ignored.</param>
public void PlayBGM(AudioClip clip) => SoundManager.Instance?.PlayBGM(clip);
        /// <summary>
/// Stops any currently playing background music.
/// </summary>
public void StopBGM()              => SoundManager.Instance?.StopBGM();
        /// <summary>
/// Plays the provided sound effect through the project's global SoundManager; does nothing if no SoundManager is available.
/// </summary>
/// <param name="clip">The audio clip to play as a sound effect.</param>
public void PlaySFX(AudioClip clip) => SoundManager.Instance?.PlaySFX(clip);
    }
}
