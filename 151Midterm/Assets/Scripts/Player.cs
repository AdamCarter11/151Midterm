using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private float speed;
    [SerializeField]
    private float jumpForce;
    [SerializeField]
    private Transform groundCheck;
    [SerializeField]
    private float checkRadius;
    [SerializeField]
    private LayerMask whatIsGround;
    private bool isGrounded;
    [SerializeField]
    private int resetJumps;
    private int extraJumps;

    private float moveInput;
    private Rigidbody2D rb;
    private Vector3 respawnPoint;
    private int direction = 1;

    [SerializeField]
    private GameObject bulletPref;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        extraJumps = resetJumps;
        respawnPoint = transform.position;
    }
    private void FixedUpdate() {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);
    }
    // Update is called once per frame
    void Update()
    {
        moveInput = Input.GetAxis("Horizontal");
        rb.velocity = new Vector2(moveInput * speed, rb.velocity.y);
        if(isGrounded){
            extraJumps = resetJumps;
        }
        if(Input.GetKeyDown(KeyCode.Space) && extraJumps > 0){
            rb.velocity = Vector2.up * jumpForce;
            extraJumps--;
        }
        else if(Input.GetKeyDown(KeyCode.Space) && extraJumps == 0 && isGrounded){
            rb.velocity = Vector2.up * jumpForce;
        }
        if(transform.position.y < -5){
            transform.position = respawnPoint;
        }
        if(moveInput > 0){
            direction = 1;
        }
        else if(moveInput < 0){
            direction = -1;
        }
        if(Input.GetKeyDown(KeyCode.E)){
            GameObject spawnedBullet = Instantiate(bulletPref, transform.position, Quaternion.identity);
            spawnedBullet.GetComponent<Rigidbody2D>().velocity = new Vector2(direction, 0) * 50;
            Destroy(spawnedBullet, 3f);
        }
    }
    private void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("checkPoint")){
            Debug.Log("CHECKPOINT!");
            respawnPoint = other.gameObject.transform.position;
        }
        if(other.CompareTag("PowerUp")){
            Destroy(other.gameObject);
        }
    }
}
