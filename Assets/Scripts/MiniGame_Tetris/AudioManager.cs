using MiniTeam.Core;
using UnityEngine;

namespace MiniTeam.Tetris
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("BGM")]
        public AudioClip bgmMain;

        [Header("효과음")]
        public AudioClip sfxMove;
        public AudioClip sfxRotate;
        public AudioClip sfxHardDrop;
        public AudioClip sfxClearLine;
        public AudioClip sfxGameOver;
        public AudioClip sfxHold;

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
