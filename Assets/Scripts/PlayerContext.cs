using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public abstract class PlayerState
{
    protected NetworkBehaviour thisObject;
    protected string stateName;
    protected GameObject player;

    protected PlayerState(NetworkBehaviour thisObj)
    {
        thisObject = thisObj;
        player = thisObject.gameObject;
    }



    public abstract void Start();
    public abstract void Update();
    public abstract void FixedUpdate();
    public abstract void OnCollisionEnter(Collision collision);
    public abstract void OnTriggerEnter(Collider other);
    public abstract void OnTriggerExit(Collider other);

}

public class RiverState : PlayerState
{
    private Rigidbody rbPlayer;
    private Vector3 direction = Vector3.zero;
    public float speed = 10.0f;
    public GameObject[] respawnPoints = null;
    private Dictionary<Item.VegetableType, int> ItemInventory = new Dictionary<Item.VegetableType, int>();

    public RiverState(NetworkBehaviour thisObj) : base(thisObj)
    {
        stateName = "RiverHop";
    }


    public override void Start()
    {
        rbPlayer = player.GetComponent<Rigidbody>();
        respawnPoints = GameObject.FindGameObjectsWithTag("Respawn");    
    }



    public override void Update()
    { 

        float horMove = Input.GetAxis("Horizontal");
        float verMove = Input.GetAxis("Vertical");

        direction = new Vector3(horMove, 0, verMove);
    }

   /* private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(player.transform.position, direction * 10);
        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(player.transform.position, rbPlayer.velocity * 5);
    } */

    // Update is called once per frame
    public override void FixedUpdate()
    {
        rbPlayer.AddForce(direction * speed, ForceMode.Force);

        if (player.transform.position.z > 40)
        {
            player.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, 40);
        }
        else if (player.transform.position.z < -40)
        {
            player.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, -40);
        }
    }

    private void Respawn()
    {
        int index = 0;
        while (Physics.CheckBox(respawnPoints[index].transform.position, new Vector3(1.5f, 1.5f, 1.5f)))
        {
            index++;
        }
        //Debug.Log("Index of spawn point" + index);
        rbPlayer.MovePosition(respawnPoints[index].transform.position);
        rbPlayer.velocity = Vector3.zero;
    }

    public override void OnCollisionEnter(Collision collision)
    {
       
    }

    public override void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Exit"))
        {
            NetworkManager networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
            networkManager.ServerChangeScene("ForestLevel");
        }
    }

    public override void OnTriggerExit(Collider other)
    {

        if (other.CompareTag("Hazard"))
        {
            Respawn();
        }
    }
}

public class ForestState : PlayerState
{
    public float speed = 40.0f;
    public float rotationSpeed = 30.0f;
    Rigidbody rgBody = null;
    float trans = 0;
    float rotate = 0;
    private Animator anim;
    private Camera camera;
    private Transform lookTarget;

    public delegate void DropHive(Vector3 pos);
    public static event DropHive DroppedHive;
    public ForestState(NetworkBehaviour thisObj) : base(thisObj)
    {
        stateName = "ForestLevel";
    }

    public override void Start()
    {
        player.transform.position = new Vector3(-20, 0.5f, -10);

        Transform rabbit = player.transform.Find("Rabbit");
        rabbit.transform.localEulerAngles = Vector3.zero;
        rabbit.transform.localScale = Vector3.one;


        rgBody = player.GetComponent<Rigidbody>();
        anim = player.GetComponentInChildren<Animator>();
        lookTarget = GameObject.Find("HeadAimTarget").transform;
        camera = player.GetComponentInChildren<Camera>(); 
        camera.enabled = true;
    }
    public override void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            DroppedHive?.Invoke(player.transform.position + (player.transform.forward * 10));
        }
        // Get the horizontal and vertical axis.
        // By default they are mapped to the arrow keys.
        // The value is in the range -1 to 1
        float translation = Input.GetAxis("Vertical");
        float rotation = Input.GetAxis("Horizontal");

        anim.SetFloat("speed", translation);

        trans += translation;
        rotate += rotation;
    }

    public override void FixedUpdate()
    {
        Vector3 rot = player.transform.rotation.eulerAngles;
        rot.y += rotate * rotationSpeed * Time.deltaTime;
        rgBody.MoveRotation(Quaternion.Euler(rot));
        rotate = 0;

        Vector3 move = player.transform.forward * trans * speed;
        move.y = rgBody.velocity.y;
        rgBody.velocity = move; // * Time.deltaTime;

        trans = 0;
    }

    public override void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Hazard"))
        {
            anim.SetTrigger("died");
        }
        else
        {
            anim.SetTrigger("TwitchLeftEar");
        }
    }

    public override void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hazard"))
        {
            //lookTarget.position = other.transform.position;
            thisObject.StartCoroutine(LookAndLookAway(lookTarget.position, other.transform.position));
        }
    }

    public override void OnTriggerExit(Collider other)
    {
      //  throw new System.NotImplementedException();
    }


    private IEnumerator LookAndLookAway(Vector3 targetPos, Vector3 hazardPos)
    {
        Vector3 targetDir = targetPos - player.transform.position;
        Vector3 hazardDir = hazardPos - player.transform.position;

        float angle = Vector2.SignedAngle(new Vector2(targetPos.x, targetPos.z), new Vector2(hazardPos.x, hazardPos.z));

        const int INTERVALS = 20;
        const float INTERVAL = 0.5f / INTERVALS;

        float angleInterval = angle / INTERVALS;

        for (int i = 0; i < INTERVALS; i++)
        {
            lookTarget.RotateAround(player.transform.position, Vector3.up, -angleInterval);
            yield return new WaitForSeconds(INTERVAL);
        }

        for (int i = 0; i < INTERVALS; i++)
        {
            lookTarget.RotateAround(player.transform.position, Vector3.up, angleInterval);
            yield return new WaitForSeconds(INTERVAL);
        }

    }

}

public class PlayerContext : NetworkBehaviour
{
    PlayerState currentState;
    private void Start()
    {
        if (!isLocalPlayer) return;

        if(SceneManager.GetActiveScene().name == "RiverHop")
        {
            currentState = new RiverState(this);
        }

        if (SceneManager.GetActiveScene().name == "ForestLevel")
        {
            currentState = new ForestState(this);
        }

        currentState.Start();
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        currentState.Update();
    }
    void FixedUpdate()
    {
        if (!isLocalPlayer) return;

        currentState.FixedUpdate();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!isLocalPlayer) return;

        currentState.OnCollisionEnter(collision);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isLocalPlayer) return;

        currentState.OnTriggerEnter(other);
    }

    void OnTriggerExit(Collider other)
    {
        if (!isLocalPlayer) return;

        currentState.OnTriggerExit(other);
    }
}
