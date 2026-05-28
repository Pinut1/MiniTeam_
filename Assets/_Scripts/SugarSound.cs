using UnityEngine;
using System.Collections;

public class SugarSound : MonoBehaviour
{
    public static SugarSound Instance;
    
    private AudioSource openingSource;
    private AudioSource gameplaySource;
    private AudioSource pierrePhaseSource;
    private AudioSource banillaPhaseSource;
    private AudioSource cutsceneSource;
    private AudioSource sfxSource;
    private AudioSource backAttackSource;

    [Header("BGM ?¤ì •")]
    public AudioClip openingBgm;
    public AudioClip gameplayBgm; 
    public AudioClip pierrePhaseBgm;  
    public AudioClip banillaPhaseBgm; 
    public AudioClip magicStickGrowingBgm; 
    public AudioClip magicStickTransformBgm; 

    [Header("?¨ê³¼??SFX) ?¤ì •")]
    public AudioClip heartCollectSfx;
    public AudioClip backAttackSfx; 
    public AudioClip knockbackSfx;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 1. ?¤í”„?ìš© ?¤ë””???ŒìŠ¤ ?ì„± ë°?ë¯¸ë¦¬ ë¡œë“œ
        openingSource = gameObject.AddComponent<AudioSource>();
        openingSource.loop = true;
        openingSource.clip = openingBgm;
        if (openingBgm != null) openingBgm.LoadAudioData();

        // 2. ê²Œì„?Œë ˆ?´ìš© ?¤ë””???ŒìŠ¤ ?ì„± ë°?ë¯¸ë¦¬ ë¡œë“œ
        gameplaySource = gameObject.AddComponent<AudioSource>();
        gameplaySource.loop = true;
        gameplaySource.clip = gameplayBgm;
        if (gameplayBgm != null) gameplayBgm.LoadAudioData();

        // 3. ?¼ì—ë¥??˜ì´ì¦ˆìš© ?¤ë””???ŒìŠ¤
        pierrePhaseSource = gameObject.AddComponent<AudioSource>();
        pierrePhaseSource.loop = true;
        pierrePhaseSource.clip = pierrePhaseBgm;
        if (pierrePhaseBgm != null) pierrePhaseBgm.LoadAudioData();

        // 4. ë°”ë‹???˜ì´ì¦ˆìš© ?¤ë””???ŒìŠ¤
        banillaPhaseSource = gameObject.AddComponent<AudioSource>();
        banillaPhaseSource.loop = true;
        if (banillaPhaseBgm != null) { banillaPhaseSource.clip = banillaPhaseBgm; banillaPhaseBgm.LoadAudioData(); }

        cutsceneSource = gameObject.AddComponent<AudioSource>();
        cutsceneSource.loop = true;

        // 5. ?¨ê³¼??SFX) ?ŒìŠ¤ ?ì„±
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        
        // ?¨ê³¼??ë¯¸ë¦¬ ë©”ëª¨ë¦¬ì— ë¡œë“œ?˜ì—¬ ?¬ìƒ ?œë ˆ???„ë²½ ?œê±°
        if (heartCollectSfx != null) heartCollectSfx.LoadAudioData();
        if (knockbackSfx != null) knockbackSfx.LoadAudioData();

        // 6. BackAttack ?„ìš© ?ŒìŠ¤ ?ì„±
        backAttackSource = gameObject.AddComponent<AudioSource>();
        backAttackSource.loop = true; // ê³µê²© ì¤‘ì¼ ??ë£¨í”„???˜ë„ ?ˆìœ¼??trueë¡??˜ë˜, ë©ˆì¶”ë©?êº¼ì§
        backAttackSource.clip = backAttackSfx;
        if (backAttackSfx != null) backAttackSfx.LoadAudioData();

        // ?œì‘?˜ìë§ˆì ?¤í”„???Œì•… ?¬ìƒ
        PlayOpeningBGM();
    }

    private void StopAllBGM()
    {
        if (openingSource != null) openingSource.Stop();
        if (gameplaySource != null) gameplaySource.Stop();
        if (pierrePhaseSource != null) pierrePhaseSource.Stop();
        if (banillaPhaseSource != null) banillaPhaseSource.Stop();
        if (cutsceneSource != null) cutsceneSource.Stop();
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

    public void PlayMagicStickGrowingBGM()
    {
        StopAllBGM();
        if (cutsceneSource != null && magicStickGrowingBgm != null)
        {
            cutsceneSource.clip = magicStickGrowingBgm;
            cutsceneSource.Play();
        }
    }

    public void PlayMagicStickTransformBGM()
    {
        StopAllBGM();
        if (cutsceneSource != null && magicStickTransformBgm != null)
        {
            cutsceneSource.clip = magicStickTransformBgm;
            cutsceneSource.Play();
        }
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
    }}

