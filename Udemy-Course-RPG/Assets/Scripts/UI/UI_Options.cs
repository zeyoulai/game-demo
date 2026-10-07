using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class UI_Options : MonoBehaviour
{
    private Player player;
    [SerializeField] private Toggle healthBarToggle;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private float mixerMultiplier = 25f;

    [Header("BGM Volume Settings")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private string bgmParametr;

    [Header("SFX Volume Settings")]
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private string sfxParametr;

    private void Start()
    {
        player = FindAnyObjectByType<Player>();


        healthBarToggle?.onValueChanged.AddListener(OnHealBarToggleChanged);
    }

    public void BGMSliderValue(float value)
    {

        float newValue = Mathf.Log10(value) * mixerMultiplier;
        audioMixer.SetFloat(bgmParametr, newValue);
    }

    public void SFXSliderValue(float value)
    {
        float safeValue = Mathf.Max(value, 0.0001f);
        float newValue = Mathf.Log10(safeValue) * mixerMultiplier;
        bool isok = audioMixer.SetFloat(sfxParametr, newValue);

    }

    private void OnHealBarToggleChanged(bool isOn)
    {
        player.health.EnableHealthBar(isOn);
    }

    public void GoMainMenuBTN() => GameManager.instance.ChangeScene("MainMenu",RespawnType.NonSpecific);

    private void OnEnable()
    {
        sfxSlider.value = PlayerPrefs.GetFloat(sfxParametr, 0.6f);
        bgmSlider.value = PlayerPrefs.GetFloat(bgmParametr, 0.6f);
    }


    private void OnDisable()
    {
   
        PlayerPrefs.SetFloat(sfxParametr, sfxSlider.value);
        PlayerPrefs.SetFloat(bgmParametr, bgmSlider.value);
        PlayerPrefs.Save();
     
    }

    public void LoadUpVolume()
    {
        sfxSlider.value = PlayerPrefs.GetFloat(sfxParametr, 0.6f);
        bgmSlider.value = PlayerPrefs.GetFloat(bgmParametr, 0.6f);
    }
}
