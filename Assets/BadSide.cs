using System.Collections;
using UnityEngine;

public class BadSide : MonoBehaviour
{
    private bool iscolide = false;
    public GameObject player;
    public AudioSource[] playAudio;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Good"))
        {
            //Debug.Log("Gameover");
            GameManager.Instance.GameOVer();
            playAudio[1].Play();

        }
        if (collision.gameObject.CompareTag("Bad"))
        {
            GameManager.Instance.AddScore();
            collision.gameObject.SetActive(false);
            playAudio[0].Play();
            StartCoroutine(RotateObject());
            StartCoroutine(DeactivateObj(collision.gameObject));
        }
    }

    
    private IEnumerator RotateObject()
    {
        // Rotate by 7 degrees on Z-axis
        player.transform.Rotate(0f, 0f, 10f);
        iscolide = true;
        // Wait for a short period (you can adjust the time as needed)
        yield return new WaitForSeconds(0.5f); // Wait for 1 second

        // Reset rotation to 0 on the Z-axis
        player.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        iscolide = false;


    }

    private IEnumerator DeactivateObj(GameObject collidedObject)
    {
        yield return new WaitForSeconds(2f); // Wait for 2 seconds

        // Reactivate the object
        collidedObject.SetActive(true);

    }

        private IEnumerator ReactivateBadObject()
    {
        // Wait for the specified respawn delay
        yield return new WaitForSeconds(1f);

        // Reactivate the "Bad" object
        gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
