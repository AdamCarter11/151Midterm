using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//OSC package
using UnityOSC;

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

    //OSC stuff
    Dictionary<string, ServerLog> servers = new Dictionary<string, ServerLog>();
    public Text countText;
    private int count=0, powerUpCount = 0;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        extraJumps = resetJumps;
        respawnPoint = transform.position;

        //OSC stuff
        OSCHandler.Instance.Init();
        //OSCHandler.Instance.SendMessageToClient("pd","/unity/trigger", "ready");
        //OSCHandler.Instance.SendMessageToClient("pd","/unity/bg", "ready");
        OSCHandler.Instance.SendMessageToClient("pd","/unity/bg", count);
        //setCountText ();
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

        //OSC stuff
        OSCHandler.Instance.UpdateLogs();
        Dictionary<string, ServerLog> servers = new Dictionary<string, ServerLog>();
        servers = OSCHandler.Instance.Servers;

        foreach(KeyValuePair<string, ServerLog> item in servers){
            if(item.Value.log.Count > 0){
                int lastPacketIndex = item.Value.packets.Count-1;
                countText.text = item.Value.packets [lastPacketIndex].Address.ToString ();
				countText.text += item.Value.packets [lastPacketIndex].Data [0].ToString ();
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("checkPoint")){
            Debug.Log("CHECKPOINT!");
            respawnPoint = other.gameObject.transform.position;
        }
        if(other.CompareTag("PowerUp")){
            //used for triggering OSC in PD
            //count = count+1;
            setCountText();


            Destroy(other.gameObject);
        }
    }

    //used for OSC example (refer to lines 99 and 104 in OSCHandler script for flags)
    void setCountText()
	{
        countText.text = "Count: " + count.ToString();

        //************* Send the message to the client...
        //OSCHandler.Instance.SendMessageToClient ("pd", "/unity/trigger", count);
        OSCHandler.Instance.SendMessageToClient("pd","/unity/powerup", powerUpCount);
        StartCoroutine(powerUpDelay());
        //OSCHandler.Instance.SendMessageToClient("pd","/unity/bg", count);
        //*************


    }
    IEnumerator powerUpDelay(){
        yield return new WaitForSeconds(.1f);
        powerUpCount += 1;
        OSCHandler.Instance.SendMessageToClient("pd","/unity/powerup", powerUpCount);
        powerUpCount = 0;
        StopAllCoroutines();
    }
}
