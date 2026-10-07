using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [SerializeField] private AudioDatabaseSO audioDatabase;
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;
    [Space]
    private Player player;

    private AudioClip lastMusicPlayed;
    private string currentBgmGroupName;
    private Coroutine currentBgmCo;
    [SerializeField] private bool bgmShouldPlay;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (bgmSource.isPlaying == false && bgmShouldPlay)
        {
            if (string.IsNullOrEmpty(currentBgmGroupName))
                NextBGM(currentBgmGroupName);
        }

        if (bgmSource.isPlaying && bgmShouldPlay == false)
            StopBGM();
    }

    public void StartBGM(string musicGroup)
    {
        bgmShouldPlay = true;

        if (musicGroup == currentBgmGroupName)
            return;
        NextBGM(musicGroup);
    }

    public void NextBGM(string musicGroup)
    {
        bgmShouldPlay = true;
        currentBgmGroupName = musicGroup;


        if (currentBgmCo != null)
            StopCoroutine(currentBgmCo);

        currentBgmCo = StartCoroutine(SwitchMusicCo(musicGroup));

    }

    public void StopBGM()
    {
        bgmShouldPlay = false;

        StartCoroutine(FadeVolumeCo(bgmSource, 0, 1));

        if (currentBgmCo != null)
            StopCoroutine(currentBgmCo);

    }

    private IEnumerator SwitchMusicCo(string musicGroup)
    {
        AudioClipData data = audioDatabase.Get(musicGroup);
        AudioClip nextMusic = data.GetRandomClip();
        if (data == null || data.clips.Count == 0)
        {
            Debug.Log("No audio found for group " + musicGroup);
            yield break;
        }
        if (data.clips.Count > 1)
            while (nextMusic == lastMusicPlayed)
                nextMusic = data.GetRandomClip();

        if (bgmSource.isPlaying)
            yield return FadeVolumeCo(bgmSource, 0, 1f);


        lastMusicPlayed = nextMusic;
        bgmSource.clip = nextMusic;
        bgmSource.volume = 0;
        bgmSource.Play();
        StartCoroutine(FadeVolumeCo(bgmSource, data.maxVolume, 1f));

    }

    private IEnumerator FadeVolumeCo(AudioSource source, float targetVolumem, float duration)
    {
        float time = 0;
        float startVolume = source.volume;
        while (time < duration)
        {
            time += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, targetVolumem, time / duration);
            yield return null;
        }
        source.volume = targetVolumem;
    }

    public void PlaySFX(string soundName, AudioSource sfxSource, float minDistanceToHearSound = 10)
    {
        if (player == null)
            player = Player.instance;

        var data = audioDatabase.Get(soundName);
        if (data == null)
        {
            Debug.Log("Attempt to play sound - " + soundName);
            return;
        }

        var clip = data.GetRandomClip();
        if (clip == null) return;

        float maxVolume = data.maxVolume;
        float distance = Vector2.Distance(sfxSource.transform.position, player.transform.position);
        float t = Mathf.Clamp01(1 - (distance / minDistanceToHearSound));


        sfxSource.pitch = Random.Range(0.9f, 1.2f);
        sfxSource.volume = Mathf.Lerp(0, maxVolume, t * t);
        sfxSource.PlayOneShot(clip);

    }

    public void PlayGlobalSFX(string soundName)
    {
        var data = audioDatabase.Get(soundName);

        if (data == null) return;

        var clip = data.GetRandomClip();
        if (clip == null) return;


        sfxSource.pitch = Random.Range(0.9f, 1.2f);
        sfxSource.volume = data.maxVolume;
        sfxSource.PlayOneShot(clip);
    }


}
