using UnityEngine;
using UnityEngine.UI;

public class VolumeUIController : MonoBehaviour
{
    [SerializeField] private Scrollbar masterVolumn;
    [SerializeField] private Scrollbar bgmVolumn;
    [SerializeField] private Scrollbar sfxVolumn;

    private void Start()
    {
        InitializeVolumes();
        // ⭐ 시작 시 리스너 등록 ⭐
        RegisterListeners();
    }

    private void OnDisable()
    {
        // ⭐ 비활성화 시 리스너 해제 ⭐
        UnregisterListeners();
    }

    private void InitializeVolumes()
    {
        if (SoundManager.Instance != null && SoundManager.Instance.mainMixer != null)
        {
            // AudioMixer에서 dB 값을 가져와 선형 볼륨(0.0 ~ 1.0)으로 변환 후 할당
            float db;

            if (SoundManager.Instance.mainMixer.GetFloat("MasterVolume", out db))
            {
                if (masterVolumn != null)
                    masterVolumn.value = SoundManager.Instance.DecibelToVolume(db);
            }

            if (SoundManager.Instance.mainMixer.GetFloat("BGMVolume", out db))
            {
                if (bgmVolumn != null)
                    bgmVolumn.value = SoundManager.Instance.DecibelToVolume(db);
            }

            if (SoundManager.Instance.mainMixer.GetFloat("SFXVolume", out db))
            {
                if (sfxVolumn != null)
                    sfxVolumn.value = SoundManager.Instance.DecibelToVolume(db);
            }
        }
    }

    private void RegisterListeners()
    {
        if (SoundManager.Instance != null)
        {
            // SoundManager의 Set...Volume 함수는 Scrollbar.value 변경 시 호출됩니다.
            masterVolumn?.onValueChanged.AddListener(SoundManager.Instance.SetMasterVolume);
            bgmVolumn?.onValueChanged.AddListener(SoundManager.Instance.SetBGMVolume);
            sfxVolumn?.onValueChanged.AddListener(SoundManager.Instance.SetSFXVolume);
        }
    }

    private void UnregisterListeners()
    {
        if (SoundManager.Instance != null)
        {
            // OnDisable에서 리스너를 해제하여 중복 등록과 MissingReferenceException을 방지합니다.
            masterVolumn?.onValueChanged.RemoveListener(SoundManager.Instance.SetMasterVolume);
            bgmVolumn?.onValueChanged.RemoveListener(SoundManager.Instance.SetBGMVolume);
            sfxVolumn?.onValueChanged.RemoveListener(SoundManager.Instance.SetSFXVolume);
        }
    }
}
