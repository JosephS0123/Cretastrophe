using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; //NEW
public class PlayerController : MonoBehaviour
{
[SerializeField] private float BASE_SPEED = 5;
private Rigidbody2D rb;
float currentSpeed;
//NEW
[SerializeField] private float JUMP_FORCE = 5f;
//private bool isGrounded = false;
// Start is called before the first frame update
void Start()
{
rb = GetComponent<Rigidbody2D>();
currentSpeed = BASE_SPEED;
//DontDestroyOnLoad(this.gameObject); //NEW
//SceneManager.sceneLoaded += OnSceneLoaded; //NEW
}
//NEW(for spawn point). Use if ALSO using DontDestroyOnLoad. Otherwise, see
//relevant code in GameManager
//void OnSceneLoaded(Scene scene, LoadSceneMode mode)
//{
//GameObject spawner = GameObject.FindGameObjectWithTag("Spawn");
//if (spawner)
//{
// this.transform.position = spawner.transform.position;
//}
//}
//NEW attempt 1
//public void SetSpeed(float newSpeed)
//{
// currentSpeed = newSpeed;
//}
//NEW attempt 2
public IEnumerator SpeedChange(float newSpeed, float timeInSecs)
{
currentSpeed = newSpeed;
yield return new WaitForSeconds(timeInSecs);
currentSpeed = BASE_SPEED;
}
//NEW
//private void OnCollisionStay2D(Collision2D collision)
//{
// isGrounded = true;
//}
// Update is called once per frame
void Update()
{
float horizontal = Input.GetAxis("Horizontal");
float vertical = Input.GetAxis("Vertical");
Vector3 dir = new Vector3(horizontal, 0, 0);
//rb.velocity = dir * currentSpeed;
//NEW
rb.velocity = new Vector2((dir * currentSpeed).x, rb.velocity.y);
if (horizontal < 0)
{
this.transform.rotation = new Quaternion(0, -1, 0, 0);
}
else
{
this.transform.rotation = new Quaternion(0, 0, 0, 0);
}
//NEW
//jumping 1
if (vertical > 0 && Mathf.Approximately(rb.velocity.y, 0))
{
rb.AddRelativeForce(new Vector2(0, JUMP_FORCE),
ForceMode2D.Impulse);
}
//jumping 2 -- what is the problem?
//if (vertical > 0 && isGrounded)
//{
// isGrounded = false;
// rb.AddRelativeForce(new Vector2(0, JUMP_FORCE),
//ForceMode2D.Impulse);
//}D
}
}