using MiniTeam.Core;
using UnityEngine;

namespace MiniTeam.Pokemon
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("BGM")]
        public AudioClip bgmField;
        public AudioClip bgmBattle;
        public AudioClip bgmVictory;

        [Header("SFX")]
        public AudioClip sfxHeal;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Start()
        {
            PlayBGM(bgmField);
        }

        public void PlayFieldBGM()   => PlayBGM(bgmField);
        public void PlayBattleBGM()  => PlayBGM(bgmBattle);
        public void PlayVictoryBGM() => PlayBGM(bgmVictory);

        public void PlayBGM(AudioClip clip)                    => SoundManager.Instance?.PlayBGM(clip);
        public void StopBGM()                                  => SoundManager.Instance?.StopBGM();
        public void PlaySFX(AudioClip clip)                    => SoundManager.Instance?.PlaySFX(clip);
        public void PlaySFX(AudioClip clip, float volumeScale) => SoundManager.Instance?.PlaySFX(clip, volumeScale);
    }
}
