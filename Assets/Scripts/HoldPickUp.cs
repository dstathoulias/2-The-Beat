using UnityEngine;

public class HoldPickUp : MonoBehaviour
{
    public bool isHolding = false;

    public GameObject pickUpPrefab;

    public Vector3 holdingScale = new Vector3(0.5f, 0.5f, 0.5f);    // Orb scale when being help
    public Vector3 originalScale = new Vector3(3f, 3f, 3f); // Original orb scale

    public Transform hoverPoint;    // Holding position of orb
    public float throwForce = 70f;  // Orb launch force when thrown

    private GameObject currentPickUp;   // Current orb being help


    // Method to execute pick up orb logic
    public bool PickUpCollected()
    {
        if (!isHolding) // If not holding any orb
        {
            isHolding = true;

            // Instantiate orb prefab at hover position and set to holding scale
            currentPickUp = Instantiate(pickUpPrefab, hoverPoint.position, Quaternion.identity, hoverPoint);
            currentPickUp.transform.localScale = holdingScale;
            return true; // Successfully picked up
        }
        else
        {
            return false; // Already holding a pick up, cannot pick up another
        }
    }


    //Method to execute throw orb logic
    public void ThrowPickUp()
    {
        isHolding = false;
        currentPickUp.transform.SetParent(null);    // Set orb parent to null
        currentPickUp.transform.localScale = originalScale; // Set orb scale to original

        // Find orb rigidbody component
        Rigidbody rb = currentPickUp.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.AddForce(hoverPoint.forward * throwForce, ForceMode.Impulse);    // Add forward force to orb 
        }
        currentPickUp.GetComponent<PickUpController>().isThrown = true;
        currentPickUp = null;
    }
}
