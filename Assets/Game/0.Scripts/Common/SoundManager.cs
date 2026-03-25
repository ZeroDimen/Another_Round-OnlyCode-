using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class SoundManager : Singleton<SoundManager>
{
    [Header("BGM Audio 설정")]
    [Tooltip("메인화면 BGM, 게임화면 BGM 순서로 할당을 가정합니다.")]
    public AudioClip[] bgmClips;
    public float bgmVolume;
    public AudioSource bgmAudioSource;
    public AudioMixerGroup bgmGroup;

    [Header("SFX Audio 설정")]
    public AudioClip[] sfxClips;
    public float[] sfxClipVolumes; // 클립별 볼륨 배열
    public int sfxChannels;
    public AudioSource[] sfxAudioSource;
    public AudioMixerGroup sfxGroup;

    public AudioMixer mainMixer;

    [Header("적 사망 사운드 중복 제한 설정")]
    public int maxEnemyDieSounds = 3;
    private int currentEnemyDieCount = 0;

    public enum BGM
    {
        MainScreen = 0,
        GamePlay = 1
    }

    public enum SFX
    {
        BulletFire = 0,
        EnemyHit = 1,
        EnemyDie = 2, 
        RewardScreen = 3,
        MenuMove = 4,
        BeamCharge = 5,
        BeamFire = 6,
        Blackhole = 7,
        CooldownDone = 8,
        ForceField = 9,
        Laser = 10,
        Shard = 11,
        Spray = 12,
        Tornado = 13,
        Wave = 14
    }

    // ----------------------
    // 초기화 및 생명 주기
    // ----------------------

    protected override void Awake()
    {
        base.Awake();
        if (Instance == this)
        {
            SoundInit();
        }
    }

    /// <summary>
    /// 사운드 시스템을 초기화하고 AudioSource 컴포넌트(채널)를 생성합니다.
    /// </summary>
    public void SoundInit()
    {
        //BGM AudioSource 초기화
        GameObject bgmObject = new GameObject("BGM_Player");
        bgmObject.transform.parent = this.transform;

        bgmAudioSource = bgmObject.AddComponent<AudioSource>();
        bgmAudioSource.loop = true;
        bgmAudioSource.playOnAwake = false;
        bgmAudioSource.outputAudioMixerGroup = bgmGroup;
        bgmAudioSource.volume = bgmVolume;


        //SFX AudioSource 초기화
        GameObject sfxObject = new GameObject("SFX_Player");
        sfxObject.transform.parent = this.transform;
        sfxAudioSource = new AudioSource[sfxChannels];

        for (int index = 0; index < sfxAudioSource.Length; index++)
        {
            sfxAudioSource[index] = sfxObject.AddComponent<AudioSource>();
            sfxAudioSource[index].loop = false;
            sfxAudioSource[index].playOnAwake = false;
            sfxAudioSource[index].outputAudioMixerGroup = sfxGroup;
            sfxAudioSource[index].volume = 1.0f;
        }
    }

    public void PlayBGM(BGM bgmIndex, int channel = 0)
    {
        if (channel != 0) return;

        int index = (int)bgmIndex;

        if (index >= bgmClips.Length)
        {
            Debug.LogError($"BGM 재생 오류: 클립({bgmIndex}) 인덱스가 범위를 벗어났습니다.");
            return;
        }

        AudioSource source = bgmAudioSource;
        source.clip = bgmClips[index];
        source.volume = bgmVolume;

        if (!source.isPlaying || source.clip != bgmClips[index])
        {
            source.Play();
        }
    }

    /// <summary>
    /// 지정된 SFX를 재생합니다. (클립별 볼륨 사용)
    /// </summary>
    public void PlaySFX(SFX sfxIndex)
    {
        int index = (int)sfxIndex;

        if (sfxIndex == SFX.EnemyDie)
        {
            if (currentEnemyDieCount >= maxEnemyDieSounds)
            {
                // 최대 횟수를 초과했으므로 재생하지 않고 종료
                return;
            }
        }

        //인덱스 및 볼륨 배열 길이 체크
        if (index >= sfxClips.Length || index >= sfxClipVolumes.Length)
        {
            Debug.LogError($"SFX 재생 오류: 클립({sfxIndex}) 인덱스가 범위를 벗어났거나 sfxClipVolumes 배열에 할당된 값이 부족합니다.");
            return;
        }

        AudioClip clip = sfxClips[index];
        float volume = sfxClipVolumes[index]; // 클립별 볼륨 사용

        //현재 재생 중이 아닌 AudioSource(채널)를 찾아 사용
        AudioSource availableSource = null;
        for (int i = 0; i < sfxChannels; i++)
        {
            if (sfxAudioSource[i] != null && !sfxAudioSource[i].isPlaying)
            {
                availableSource = sfxAudioSource[i];
                break;
            }
        }

        //사용 가능한 채널이 있으면 PlayOneShot으로 재생
        if (availableSource != null)
        {
            availableSource.PlayOneShot(clip, volume);
            if (sfxIndex == SFX.EnemyDie)
            {
                // PlayOneShot의 재생 시간을 추적하여 카운트를 감소시킬 코루틴 시작
                StartCoroutine(EnemyDieCountDown(clip.length));
            }
        }
        else
        {
            //모든 채널이 사용 중이면, 첫 번째 채널을 사용하여 강제로 재생
            sfxAudioSource[0].PlayOneShot(clip, volume);
            
            if (sfxIndex == SFX.EnemyDie)
            {
                StartCoroutine(EnemyDieCountDown(clip.length));
            }
        }
    }

    private IEnumerator EnemyDieCountDown(float duration)
    {
        // 카운트 증가 (최대 횟수 검사를 통과한 후 여기에 도달)
        currentEnemyDieCount++;

        // 클립 재생 시간만큼 대기
        yield return new WaitForSeconds(duration);

        // 재생이 끝났으므로 카운트 감소 (0 이하로 내려가지 않도록 안전 장치 추가)
        if (currentEnemyDieCount > 0)
        {
            currentEnemyDieCount--;
        }
    }

    /// <summary>
    /// AudioMixer의 Master 볼륨을 설정합니다.
    /// </summary>
    public void SetMasterVolume(float normalizedVolume)
    {
        // 선형 볼륨(0.0~1.0)을 로그 스케일 dB 값(-80dB~0dB)으로 변환합니다.
        float dB = VolumeToDecibel(normalizedVolume);

        if (mainMixer != null)
        {
            // 에디터에서 노출된 BGM_Volume 파라미터에 dB 값을 설정합니다.
            mainMixer.SetFloat("MasterVolume", dB);
        }
    }

    /// <summary>
    /// AudioMixer의 BGM 그룹 볼륨을 설정합니다.
    /// </summary>
    public void SetBGMVolume(float normalizedVolume)
    {
        // 선형 볼륨(0.0~1.0)을 로그 스케일 dB 값(-80dB~0dB)으로 변환합니다.
        float dB = VolumeToDecibel(normalizedVolume);

        if (mainMixer != null)
        {
            // 에디터에서 노출된 BGM_Volume 파라미터에 dB 값을 설정합니다.
            mainMixer.SetFloat("BGMVolume", dB);
        }
    }

    /// <summary>
    /// AudioMixer의 SFX 그룹 볼륨을 설정합니다.
    /// </summary>
    /// <param name="normalizedVolume">0.0 (음소거) ~ 1.0 (최대)</param>
    public void SetSFXVolume(float normalizedVolume)
    {
        float dB = VolumeToDecibel(normalizedVolume);

        if (mainMixer != null)
        {
            // 에디터에서 노출된 SFX_Volume 파라미터에 dB 값을 설정합니다.
            mainMixer.SetFloat("SFXVolume", dB);
        }
    }

    /// <summary>
    /// 0.0~1.0의 선형 볼륨 값을 AudioMixer의 dB 스케일 값으로 변환합니다.
    /// </summary>
    public float VolumeToDecibel(float linearVolume)
    {
        // 볼륨이 0.0이면 -80dB (거의 음소거)로 설정
        if (linearVolume <= 0.0001f)
        {
            return -80f;
        }
        // 0.0001 ~ 1.0 범위의 볼륨을 -80 ~ 0 dB로 변환
        return Mathf.Log10(linearVolume) * 20;
    }

    public float DecibelToVolume(float decibel)
    {
        // SoundManager에서 0.0001f 이하를 -80f로 처리했으므로,
        // -80f 이하이면 0.0f로 처리하여 오차를 방지합니다.
        if (decibel <= -80f)
        {
            return 0.0f;
        }

        // 10^(dB/20) 공식 적용 (역변환 공식)
        return Mathf.Pow(10f, decibel / 20f);
    }

    private void OnEnable()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }
    }

    private void OnDisable()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (Instance != this || bgmAudioSource == null)
        {
            return;
        }

        // 씬 로드 시 BGM 정지 로직이 누락되어 추가합니다.
        if (bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Stop();
        }

        if (scene.name == Constants.MENUSCENE_NAME)
        {
            PlayBGM(BGM.MainScreen);
            // 일반 상태로 즉시 전환
        }
        else if (scene.name == Constants.GAMESCENE_NAME)
        {
            PlayBGM(BGM.GamePlay);
            // 일반 상태로 부드럽게 전환 (예: 1초 동안)
        }
    }

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode) { }
    protected override void OnSceneUnloaded(Scene scene) { }
}