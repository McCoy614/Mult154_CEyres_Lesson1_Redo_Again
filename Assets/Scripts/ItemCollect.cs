using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ItemCollect : NetworkBehaviour
{
    private Dictionary<Item.VegetableType, int> ItemInventory = new Dictionary<Item.VegetableType, int>();

    public delegate void CollectItem(Item.VegetableType item);
    public static event CollectItem ItemCollected;

    // Start is called before the first frame update
    void Start()
    {
        foreach (Item.VegetableType item in System.Enum.GetValues(typeof(Item.VegetableType)))
        {
            ItemInventory.Add(item, 0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isLocalPlayer)
        {
            return;
        }

        if (other.CompareTag("Item") && Input.GetKey(KeyCode.Space))
        {
            Item item = other.gameObject.GetComponent<Item>();
            AddToInventory(item);
            ItemCollected?.Invoke(item.typeOfVeggie);
            PrintInventory();
        }
    }
    private void AddToInventory(Item item)
    {
        ItemInventory[item.typeOfVeggie]++;
    }

    private void PrintInventory()
    {
        string output = "";

        foreach (KeyValuePair<Item.VegetableType, int> kvp in ItemInventory)
        {
            output += string.Format("{0}: {1} ", kvp.Key, kvp.Value);
        }
        Debug.Log(output);
    }

}
