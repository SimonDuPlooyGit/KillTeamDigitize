using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private static SoundManager instance;
    private AudioSource audioSource;
    private AudioSource bgmSource1;
    private AudioSource bgmSource2;
    [SerializeField]
    private AudioClip[] soundList;
    [SerializeField] 
    private AudioClip mainTrack;
    [SerializeField] 
    private AudioClip ambiance;
    private void Awake()
    {
        instance = this;

        AudioSource[] sources = GetComponents<AudioSource>();

        if (sources.Length >= 3)
        {
            audioSource = sources[0];
            bgmSource1 = sources[1];
            bgmSource2 = sources[2];

            StartBackingTracks();

        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public static void PlaySound(int index, float volume = 1f, bool randomizePitch = false)
    {

        if (instance == null)
        {
            Debug.LogWarning("No SoundManager found in the current scene!");
            return;
        }

        if (index < 0 || index >= instance.soundList.Length)
        {
            Debug.LogWarning($"Sound index {index} out of range on this scene's SoundManager!");
            return;
        }

        AudioClip clip = instance.soundList[index];

        if (randomizePitch)
        {
            //Create temp instance to play alt pitch sfx
            GameObject tempGO = new GameObject("TempAudioSource");
            tempGO.transform.position = instance.transform.position;

            AudioSource tempSource = tempGO.AddComponent<AudioSource>();
            tempSource.clip = clip;
            tempSource.volume = volume;
            tempSource.pitch = Random.Range(0.7f, 1.2f);
            tempSource.Play();

            Destroy(tempGO, clip.length / tempSource.pitch);
        }
        else
        {
            //default playing method
            instance.audioSource.pitch = 1f;
            instance.audioSource.PlayOneShot(clip, volume);
        }
    }

    public static void StartBackingTracks()
    {
        if (instance == null) return;

        if (instance.mainTrack != null)
        {
            instance.bgmSource1.clip = instance.mainTrack;
            instance.bgmSource1.loop = true;
            instance.bgmSource1.volume = 0.4f;
            instance.bgmSource1.Play();
        }

        if (instance.ambiance != null)
        {
            instance.bgmSource2.clip = instance.ambiance;
            instance.bgmSource2.loop = true;
            instance.bgmSource2.volume = 0.2f;
            instance.bgmSource2.Play();
        }
    }
    public static void SetMusicVolumes(float track1Volume, float track2Volume)
    {
        if (instance == null) return;

        instance.bgmSource1.volume = Mathf.Clamp01(track1Volume);
        instance.bgmSource2.volume = Mathf.Clamp01(track2Volume);
    }
}
