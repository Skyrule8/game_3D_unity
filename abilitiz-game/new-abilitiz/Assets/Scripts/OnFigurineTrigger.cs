using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnFigurineTrigger : MonoBehaviour
{
    GameObject thePlayer;
    OnTriggerEnterCollision printer;
    int globalNumber;
    bool isPut;
    bool isDone;
    // Start is called before the first frame update
    void Start()
    {
        isPut = false;
        isDone = false;
        thePlayer = GameObject.Find("printer");
    }

    // Update is called once per frame
    void Update()
    {
        printer = thePlayer.GetComponent<OnTriggerEnterCollision>();
        if(isPut == true && isDone == false)
        {
            printer.RPC_IncreaseNumber();
            isPut = false;
            isDone = true;
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "statuette" && isDone == false)
        {
            isPut = true;   
        }
    }
}
