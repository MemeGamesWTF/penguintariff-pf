using System.Collections;
using UnityEngine;

public class BasePlayer : MonoBehaviour
{
    public AudioSource[] playAudio;
   
    private bool iscolide = false;
    void Start()
    {

    }

    void Update()
    {
        if (!GameManager.Instance.GameState)
            return;

        //UPDATE LOGIC
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Good")){
            //Debug.Log("Good");
            playAudio[0].Play();
            GameManager.Instance.AddScore();
            collision.gameObject.SetActive(false);
           // StartCoroutine(ReactivateBadObject());
            StartCoroutine(DeactivateObj(collision.gameObject));
            if (!iscolide) {
                StartCoroutine(RotateObjectRight());

            }
            

        }
        if (collision.gameObject.CompareTag("Bad")){
            // Debug.Log("GameOver");
            playAudio[1].Play();
            GameManager.Instance.GameOVer();
        }
    }

    
    public IEnumerator RotateObjectRight()
    {
        // Rotate by 7 degrees on Z-axis
        transform.Rotate(0f, 0f, -10f);
        iscolide = true;
        // Wait for a short period (you can adjust the time as needed)
        yield return new WaitForSeconds(0.5f); // Wait for 1 second

        // Reset rotation to 0 on the Z-axis
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        iscolide = false;


    }

    private IEnumerator DeactivateObj(GameObject collidedObject)
    {
        yield return new WaitForSeconds(2f); // Wait for 2 seconds

        // Reactivate the object
        collidedObject.SetActive(true);

    }


   /* private IEnumerator ReactivateBadObject()
    {
        // Wait for the specified respawn delay
        yield return new WaitForSeconds(1f);

        // Reactivate the "Bad" object
        gameObject.SetActive(true);
    }
*/

    public void GameOver()
    {
       // GameManager.Instance.GameOVer();
    }

    public void Reset()
    {

    }
}
