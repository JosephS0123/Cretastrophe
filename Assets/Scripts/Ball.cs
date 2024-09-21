using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class Ball : MonoBehaviour
{
    [SerializeField] private float SPEED = 5.0f;
    [SerializeField] private Ball _redBallPrefab;
    [SerializeField] private Rigidbody2D _rigidBody;
    private SpriteRenderer _spriteRenderer;
    private bool toBeDestroyed = false;
    int numRedCollisions = 0;

    void Start()
    {
        _rigidBody.velocity = new Vector3(0, SPEED * -1, 0);
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }


    void Update()
    {
        //transform.Translate(Vector3.down * Time.deltaTime * SPEED);
        //_rigidBody.velocity = new Vector3(0, SPEED * -1, 0);
    }

    private IEnumerator OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Red" && gameObject.tag != "Red")
        {
            if(!toBeDestroyed)
            {
                numRedCollisions++;
                float colorOffset = 0.6f * numRedCollisions / 10;
                _spriteRenderer.color = new Color(1, 1-colorOffset, 1-colorOffset);
                
                if(numRedCollisions > 10)
                {
                    Ball redball = Instantiate(_redBallPrefab, gameObject.transform.position, Quaternion.identity);
                    redball.setSpeed(SPEED);
                    toBeDestroyed = true;
                    Destroy(gameObject);
                }
            }
        }
        else if (collision.gameObject.tag == "White")
        {
            if (SPEED > 0)
            {
                SPEED = SPEED - 0.5f;
            }

            if (SPEED < 1.0f)
            {
                toBeDestroyed = true;
                yield return new WaitForSeconds(0.2f);
                Destroy(gameObject);
            }
            _rigidBody.velocity = new Vector3(0, SPEED * -1, 0);
        }
    }

    public void setSpeed(float speed)
    {
        SPEED = speed;
        _rigidBody.velocity = new Vector3(0, SPEED * -1, 0);
    }
}