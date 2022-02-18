using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class Change_Display : MonoBehaviour
{
    private PhotonView PV;
    public Material material1;
    public Material material2;
    public Material material3;
    public Material material4;
    public Material material5;
    private GameObject LCD;
    GameObject thePlayer;
    private OnTriggerEnterCollision playerScript;
    private int globalNumber;

    // Start is called before the first frame update
    void Start()
    {
        PV= GetComponent<PhotonView>();      
        thePlayer = GameObject.Find("printer");
    }

    // Update is called once per frame
    void Update()
    {
        playerScript = thePlayer.GetComponent<OnTriggerEnterCollision>();
        globalNumber = playerScript.globalNumber;
        getLCDChange();
    }

    [PunRPC]
    void ChangeNumber(int material) 
    {
        var hand = GameObject.Find("LCD_Number");
        if(material == 1)
            hand.gameObject.GetComponent<MeshRenderer>().material = material1;
        else if(material == 2)
            hand.gameObject.GetComponent<MeshRenderer>().material = material2;
        else if(material == 3)
            hand.gameObject.GetComponent<MeshRenderer>().material = material3;
        else if(material == 4)
            hand.gameObject.GetComponent<MeshRenderer>().material = material4;
        else if(material == 5)
            hand.gameObject.GetComponent<MeshRenderer>().material = material5;
    }

    public void getLCDChange()
    {
        if(globalNumber == 1)
            PV.RPC("ChangeNumber", RpcTarget.All, 1);
        else if(globalNumber == 2)
            PV.RPC("ChangeNumber", RpcTarget.All, 2);
        else if(globalNumber == 3)
            PV.RPC("ChangeNumber", RpcTarget.All, 3);
        else if(globalNumber == 4)
            PV.RPC("ChangeNumber", RpcTarget.All, 4);
        else if(globalNumber == 5)
            PV.RPC("ChangeNumber", RpcTarget.All, 5);   
    }

}
