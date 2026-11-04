using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private static SoundManager instance;
    private AudioSource audioSource;
    [SerializeField]
    private AudioClip[] soundList;
    private void Awake()
    {
        instance = this;

        audioSource = GetComponent<AudioSource>();
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
}
