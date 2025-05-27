using UnityEngine;

[System.Serializable]
public struct SoundEffect
{
    public string groupID;    // Назва групи звуків
    public AudioClip[] clips; // Масив аудіокліпів для цієї групи
}

public class SoundLibrary : MonoBehaviour
{
    public SoundEffect[] soundEffects; // Масив усіх звукових груп

    public AudioClip GetClipFromName(string name)
    {
        foreach (var soundEffect in soundEffects)
        {
            if (soundEffect.groupID == name)
            {
                return soundEffect.clips[Random.Range(0, soundEffect.clips.Length)];
            }
        }
        return null;
    }
}