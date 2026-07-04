using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    public Conductor conductor;
    public GameObject noteLeftPrefab;
    public GameObject noteRightPrefab;
    public GameObject noteUpPrefab;

    public float[] laneXPositions = { 3f, 0f, -3f };    // Lanes X positions
    public float spawnZ = 5.5f; // note spawn Z location

    private float lastBeatSpawned = 0f;

    void Update()
    {
        if (Time.timeScale == 0f) return; // pause the note spawner when the game is paused
        if (conductor.isPlaying == false) return; // only spawn notes when a track is playing
                                                  // to prevent note spawning before the rhythm minigame starts

        // Calculate the current conductor beat and if note not spawned yet this beat, spawn
        int currentBeat = Mathf.FloorToInt(conductor.songPositionInBeats);
        if (currentBeat > lastBeatSpawned)
        {
            lastBeatSpawned = currentBeat;
            SpawnNote();
        }
    }


    // Spawn note method
    void SpawnNote()
    {
        int lane = Random.Range(0, laneXPositions.Length);  // Pick random lane to spawn note in
        // Set spawn position based on lane picked
        Vector3 spawnPos = transform.position + new Vector3(laneXPositions[lane], 0.3f, spawnZ);
        
        // Instantiate corresponding note prefab to lane picked:
        // Lane 1 -> Left Arrow
        // Lane 2 -> Up Arrow
        // Lane 3 -> Right Arrow
        GameObject notePrefab = lane switch
        {
            0 => noteLeftPrefab,
            1 => noteUpPrefab,
            2 => noteRightPrefab,
            _ => throw new System.Exception("Invalid lane index")
        };
        GameObject note = Instantiate(notePrefab, spawnPos, notePrefab.transform.rotation);

        note.GetComponent<NoteController>().lane = lane;    // Set spawned note's lane
    }


    // Reset spawner beats counter
    public void ResetSpawner()
    {
        lastBeatSpawned = 0f;
    }
}