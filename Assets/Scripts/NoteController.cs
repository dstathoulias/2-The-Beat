using UnityEngine;

public class NoteController : MonoBehaviour
{
    public int  lane; // Rhythm minigame lane: 0 -> 1, 1 -> 2, 2 -> 3
    public float speed = 5f;    // Note movement speed

    public float missThreshold = 5.5f;  // note hit threshold

    private bool hasBeenHit = false;


    void Update()
    {
        if (Time.timeScale == 0f) return; // pause the note when the game is paused

        transform.position += speed * Time.deltaTime * Vector3.forward; // Update note position

        // If note has not been hit when exceeding the threshold, destroy it
        if (!hasBeenHit && transform.position.z > missThreshold)
        {
            Destroy(gameObject);
        }
    }


    // Hit note method
    public void Hit()
    {
        hasBeenHit = true;
        Destroy(gameObject);    // Destroy note
    }


    // Helper public method to destroy note
    public void DestroyNote()
    {
        Destroy(gameObject);
    }
}