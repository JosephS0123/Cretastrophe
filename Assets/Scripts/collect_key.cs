using UnityEngine;
using System.Collections;

public class collect_key : MonoBehaviour
{
    public AudioClip drawSound; 
    public GameObject animation;
    private SpriteRenderer spriteRenderer; 
    private Collider2D collider2D; 

    void Start()
    {
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        collider2D = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            
            AudioSource.PlayClipAtPoint(drawSound, transform.position);

           
            spriteRenderer.enabled = false;
            collider2D.enabled = false;

            
            animation.SetActive(true);

            
            GameManager.addKey();

            
            StartCoroutine(DestroyAfterDelay(1.5f)); 
        }
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); 
        Destroy(gameObject); 
    }
}
