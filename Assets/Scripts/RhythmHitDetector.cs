using UnityEngine;

public class RhythmHitDetector : MonoBehaviour
{
    public Conductor conductor;

    public float hitWindowLeniency = 0.4f;  // Leniency scale for hit thresshold. 0.25 -> strict. 0.5 -> lenient.
    public float hitZoneZ = 5.5f;   // Z position of the hit zone

    private float hitWindowUnits; // Calculated hit window in world units based on note size and leniency


    void Start()
    {
        hitWindowUnits = hitWindowLeniency * transform.Find("HitboxArrowUp").transform.localScale.x;    // Calculate hit window
    }


    public void CheckHit(int lane)
    {
        if (Time.timeScale == 0f) return; // Don't check for hits when the game is paused

        NoteController[] activeNotes = FindObjectsByType<NoteController>(); // Assign all note clones to array
        NoteController closestNote = null;
        float closestNoteDistance = float.MaxValue;

        // Find closest note to hit zone
        foreach (NoteController note in activeNotes)
        {
            if (note.lane != lane) continue; // Only check referenced lane's notes

            float distance = Mathf.Abs(note.transform.position.z - hitZoneZ);
            if (distance < closestNoteDistance)
            {
                closestNoteDistance = distance;
                closestNote = note;
            }
        }

        if (closestNote == null) return;

        // Check if note hit is inside the hit window
        if (closestNoteDistance <= hitWindowUnits)
        {
            closestNote.Hit();  // If yes, hit
            ScoreManager.Instance.RegisterHit();    // Register hit to score
        }
    }
}
