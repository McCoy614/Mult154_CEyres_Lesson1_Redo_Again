using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerMovement : NetworkBehaviour
{
    private Rigidbody rbPlayer;
    private Vector3 direction = Vector3.zero;
    public float speed = 10.0f;
    public GameObject[] respawnPoints = null;
    private Dictionary<Item.VegetableType, int> ItemInventory = new Dictionary<Item.VegetableType, int>();


    // Start is called before the first frame update
    void Start()
    {
        if(!isLocalPlayer)
        {
            return;
        }

        rbPlayer = GetComponent<Rigidbody>();
        respawnPoints = GameObject.FindGameObjectsWithTag("Respawn");

        
    }

    

    private void Update()
    {
        if(!isLocalPlayer)
        {
            return;
        }

        float horMove = Input.GetAxis("Horizontal");
        float verMove = Input.GetAxis("Vertical");

        direction = new Vector3(horMove, 0, verMove);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        rbPlayer.AddForce(direction * speed, ForceMode.Force);

        if(transform.position.z > 40)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, 40);
        }
        else if(transform.position.z < -40)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, -40);
        }
    }

    private void Respawn()
    {
        int index = 0;
        while(Physics.CheckBox(respawnPoints[index].transform.position, new Vector3(1.5f, 1.5f, 1.5f)))
        {
            index++;
        }
        rbPlayer.MovePosition(respawnPoints[index].transform.position);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isLocalPlayer)
        {
            return;
        }

        if (other.CompareTag("Hazard"))
        {
            Respawn();
        }
    }


}
