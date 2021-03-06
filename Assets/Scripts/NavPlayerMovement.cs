using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavPlayerMovement : MonoBehaviour
{
    public float speed = 40.0f;
    public float rotationSpeed = 30.0f;
    Rigidbody rgBody = null;
    float trans = 0;
    float rotate = 0;
    private Animator anim;
    private Transform lookTarget;

    public delegate void DropHive(Vector3 pos);
    public static event DropHive DroppedHive;

    private void Start()
    {
        rgBody = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        lookTarget = GameObject.Find("HeadAimTarget").transform;
    }
    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            DroppedHive?.Invoke(transform.position + (transform.forward * 10));
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

    private void FixedUpdate()
    {
        Vector3 rot = transform.rotation.eulerAngles;
        rot.y += rotate * rotationSpeed * Time.deltaTime;
        rgBody.MoveRotation(Quaternion.Euler(rot));
        rotate = 0;

        Vector3 move = transform.forward * trans * speed;
        move.y = rgBody.velocity.y;
        rgBody.velocity = move; // * Time.deltaTime;

        trans = 0;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.CompareTag("Hazard"))
        {
            anim.SetTrigger("died");
        }
        else
        {
            anim.SetTrigger("TwitchLeftEar");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Hazard"))
        {
            //lookTarget.position = other.transform.position;
            StartCoroutine(LookAndLookAway(lookTarget.position, other.transform.position));
        }
    }


    private IEnumerator LookAndLookAway(Vector3 targetPos, Vector3 hazardPos)
    {
        Vector3 targetDir = targetPos - transform.position;
        Vector3 hazardDir = hazardPos - transform.position;

        float angle = Vector2.SignedAngle(new Vector2(targetPos.x, targetPos.z), new Vector2(hazardPos.x, hazardPos.z));

        const int INTERVALS = 20;
        const float INTERVAL = 0.5f / INTERVALS;

        float angleInterval = angle / INTERVALS;

        for(int i = 0; i < INTERVALS; i++)
        {
            lookTarget.RotateAround(transform.position, Vector3.up, -angleInterval);
            yield return new WaitForSeconds(INTERVAL);
        }

        for (int i = 0; i < INTERVALS; i++)
        {
            lookTarget.RotateAround(transform.position, Vector3.up, angleInterval);
            yield return new WaitForSeconds(INTERVAL);
        }

    }

}
