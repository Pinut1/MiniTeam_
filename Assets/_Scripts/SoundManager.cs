using UnityEngine;
using System.Collections;

public class BgmManager : MonoBehaviour
{
    public static BgmManager Instance;
    
    private AudioSource openingSource;
    private AudioSource gameplaySource;
    private AudioSource pierrePhaseSource;
    private AudioSource banillaPhaseSource;
    private AudioSource sfxSource;
    private AudioSource backAttackSource;

    [Header("BGM 설정")]
    public AudioClip openingBgm;    // 인스펙터에서 할당
    public AudioClip gameplayBgm;  // 인스펙터에서 할당
    public AudioClip pierrePhaseBgm;  // 인스펙터에서 할당
    public AudioClip banillaPhaseBgm;  // 인스펙터에서 할당

    [Header("효과음(SFX) 설정")]
    public AudioClip heartCollectSfx; // 인스펙터에서 할당
    public AudioClip backAttackSfx; // 인스펙터에서 할당
    public AudioClip knockbackSfx; // 인스펙터에서 할당

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 1. 오프닝용 오디오 소스 생성 및 미리 로드
        openingSource = gameObject.AddComponent<AudioSource>();
        openingSource.loop = true;
        openingSource.clip = openingBgm;
        if (openingBgm != null) openingBgm.LoadAudioData();

        // 2. 게임플레이용 오디오 소스 생성 및 미리 로드
        gameplaySource = gameObject.AddComponent<AudioSource>();
        gameplaySource.loop = true;
        gameplaySource.clip = gameplayBgm;
        if (gameplayBgm != null) gameplayBgm.LoadAudioData();

        // 3. 피에르 페이즈용 오디오 소스
        pierrePhaseSource = gameObject.AddComponent<AudioSource>();
        pierrePhaseSource.loop = true;
        pierrePhaseSource.clip = pierrePhaseBgm;
        if (pierrePhaseBgm != null) pierrePhaseBgm.LoadAudioData();

        // 4. 바닐라 페이즈용 오디오 소스
        banillaPhaseSource = gameObject.AddComponent<AudioSource>();
        banillaPhaseSource.loop = true;
        banillaPhaseSource.clip = banillaPhaseBgm;
        if (banillaPhaseBgm != null) banillaPhaseBgm.LoadAudioData();

        // 5. 효과음(SFX) 소스 생성
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        
        // 효과음 미리 메모리에 로드하여 재생 딜레이 완벽 제거
        if (heartCollectSfx != null) heartCollectSfx.LoadAudioData();
        if (knockbackSfx != null) knockbackSfx.LoadAudioData();

        // 6. BackAttack 전용 소스 생성
        backAttackSource = gameObject.AddComponent<AudioSource>();
        backAttackSource.loop = true; // 공격 중일 때 루프될 수도 있으니 true로 하되, 멈추면 꺼짐
        backAttackSource.clip = backAttackSfx;
        if (backAttackSfx != null) backAttackSfx.LoadAudioData();

        // 시작하자마자 오프닝 음악 재생
        PlayOpeningBGM();
    }

    private void StopAllBGM()
    {
        if (openingSource != null) openingSource.Stop();
        if (gameplaySource != null) gameplaySource.Stop();
        if (pierrePhaseSource != null) pierrePhaseSource.Stop();
        if (banillaPhaseSource != null) banillaPhaseSource.Stop();
    }

    public void PlayOpeningBGM()
    {
        StopAllBGM();
        if (openingSource != null && openingSource.clip != null) openingSource.Play();
    }

    public void PlayGameBGM()
    {
        StopAllBGM();
        if (gameplaySource != null && gameplaySource.clip != null) gameplaySource.Play();
    }

    public void PlayPierrePhaseBGM()
    {
        StopAllBGM();
        if (pierrePhaseSource != null && pierrePhaseSource.clip != null) pierrePhaseSource.Play();
    }

    public void PlayBanillaPhaseBGM()
    {
        StopAllBGM();
        if (banillaPhaseSource != null && banillaPhaseSource.clip != null) banillaPhaseSource.Play();
    }

    public void ChangeBGM(AudioClip newClip)
    {
        PlayGameBGM();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlayBackAttackSFX()
    {
        if (backAttackSource != null && !backAttackSource.isPlaying)
        {
            backAttackSource.Play();
        }
    }

    public void StopBackAttackSFX()
    {
        if (backAttackSource != null && backAttackSource.isPlaying)
        {
            backAttackSource.Stop();
        }
    }
}