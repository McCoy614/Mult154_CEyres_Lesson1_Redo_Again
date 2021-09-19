using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject[] lilyPadObjs = null;
    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("SpawnLilyPad", 2.0f, 5.0f);
    }

    // Update is called once per frame
    private void Update()
    {
        
    }


    void SpawnLilyPad()
    {
        foreach(GameObject lilyPad in lilyPadObjs)
        Instantiate(lilyPad);
    }
}
