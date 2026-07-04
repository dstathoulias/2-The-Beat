using UnityEngine;

public class Conductor : MonoBehaviour
{
    // IMPORTANT:
    // While the song's beats start at 1,
    // the song's position and loop's position start at 0


    // song's Beats Per Minute
    public float songBPM;
    // the number of seconds in each beat
    public float secPerBeat;
    // the current position of the song in seconds
    public float songPosition;
    // the current position of the song in beats
    public float songPositionInBeats;
    // the time when the music starts
    public float dspSongTime;
    // the audio source of the music
    public AudioClip track1;
    public AudioClip track2;
    public AudioSource musicSource;
    // the number of beats in each loop of the song
    public float beatsPerLoop;
    // the number of completed loops of the song
    public int completeLoops = 0;
    // the current position of the song in beats within the loop
    public float loopPositionInBeats;

    public bool isPlaying;  // Track is currently playing flag

    private float pauseTime;

    public AudioSource ambientSource;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // get audio source, calculate the number of seconds in each beat
        // and record the time when the music starts
        musicSource = GetComponent<AudioSource>();
        ambientSource.Pause();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale == 0f) return; // pause the conductor when the game is paused

        // calculate the song position in beats
        songPosition = (float)(AudioSettings.dspTime - dspSongTime);
        songPositionInBeats = songPosition / secPerBeat;

        // we want the song to be able to loop
        // calculate the song position in the loop and the number of completed loops
        if (songPositionInBeats >= (completeLoops + 1) * beatsPerLoop)
        {
            completeLoops++;
            loopPositionInBeats = songPositionInBeats - beatsPerLoop * completeLoops;
        }
    }


    // Method to switch the currently playing music track, update the BPM and restart the timing
    public void SwitchTrack(AudioClip newClip, float newBPM)
    {
        musicSource.Stop();
        ambientSource.Pause();
        musicSource.clip = newClip;
        songBPM = newBPM;
        secPerBeat = 60f / songBPM;
        completeLoops = 0;
        songPosition = 0f;
        songPositionInBeats = 0f;
        loopPositionInBeats = 0f;
        dspSongTime = (float)AudioSettings.dspTime;
        musicSource.Play();
        isPlaying = true;
    }


    // Stop currently playing track
    public void StopTrack()
    {
        musicSource.Stop();
        isPlaying = false;
    }


    // Pause currently playing track and freeze dspTimer
    // Called when game is paused
    public void Pause()
    {
        pauseTime = (float)AudioSettings.dspTime;
        musicSource.Pause();
        ambientSource.UnPause();    // Play ambient sound
    }


    // Resume paused track and unfreeze dspTimer
    // Called when game is resumed
    public void Resume()
    {
        musicSource.Play();
        dspSongTime += (float)(AudioSettings.dspTime - pauseTime);
        ambientSource.Pause();  // Pause ambient sound
    }

    // sources:
    //https://www.gamedeveloper.com/audio/coding-to-the-beat---under-the-hood-of-a-rhythm-game-in-unity
    //https://www.gamedeveloper.com/programming/music-syncing-in-rhythm-games
}
