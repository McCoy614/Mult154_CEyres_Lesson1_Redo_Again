using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BearBrain : MonoBehaviour
{

    private Bot bot;
    private Vector3 hivePos;
    private bool hiveDropped = false;

    // Start is called before the first frame update
    void Start()
    {
        bot = GetComponent<Bot>();
        NavPlayerMovement.DroppedHive += HiveReady;
    }

    void HiveReady(Vector3 pos)
    {
        hivePos = pos;
        hiveDropped = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (hiveDropped)
        {
            bot.Seek(hivePos);
        }
        else
        {

            if (bot.CanTargetSeeMe())
            {
                bot.Evade();
            }
            else if (bot.CanSeeTarget())
            {
                bot.Pursue();
            }
            else
            {
                bot.Wander();
            }
        }
    }
}
