using UnityEngine;

/// <summary>
/// Manages sound and music
/// </summary>
public class AudioManager : MonoBehaviour {

    public static AudioManager instance; //only one instance will be used

    //music
    [SerializeField] private AudioSource musicPlayer;

    //sound effects
    [SerializeField] private AudioSource soundPlayer;
    public AudioClip menuSelection;
    public AudioClip recovery;

    public void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != null)
            DestroyObject(gameObject);
    }

    /// <summary>
    /// Plays a sound effect once.
    /// </summary>
    /// <param name="soundEffect">The sound effect to be played</param>
    public void PlaySoundEffect(AudioClip soundEffect)
    {
        soundPlayer.clip = soundEffect;
        soundPlayer.Play();
    }

    /// <summary>
    /// Checks to see if a sound effect is currently playing
    /// </summary>
    /// <returns>Returns true if a sound is playing, otherwise it returns false</returns>
    public bool SoundEffectPlaying()
    {
        return soundPlayer.isPlaying;
    }
}
