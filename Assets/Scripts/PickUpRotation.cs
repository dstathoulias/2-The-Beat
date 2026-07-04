using UnityEngine;

public class PickUpRotation : MonoBehaviour
{
    public float xSpeed = 0f;
    public float ySpeed = 90f;  // Orb rotates horizontally
    public float zSpeed = 0f;

    private Space rotationSpace = Space.Self;

    void Update()
    {
        if (Time.timeScale == 0f) return; // Pause the rotation when the game is paused

        // Set orb rotation vector and apply it
        Vector3 rotationAmount = new Vector3(xSpeed, ySpeed, zSpeed) * Time.deltaTime;
        transform.Rotate(rotationAmount, rotationSpace);
    }
}