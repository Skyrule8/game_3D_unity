using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class OnTriggerEnterCollision : MonoBehaviour
{
    public static PhotonRoom room;
    private PhotonView PV;
    public Text txtNumber;
    public Text winText;
    public GameObject otherGameObject;

    public MeshRenderer computer1;
    public MeshRenderer computer2;
    public MeshRenderer computer3;
    public MeshRenderer computer4;
    public Transform transformObject;
    
    public Material material1;
    public Material material2;
    public Material material3;
    public Material material4;
    public Material material5;

    GameObject thePlayer;
    AudioScript playerScript;

    bool canpickup;
    bool displayMessage;
    bool telephone;
    bool computer;
    public int globalNumber; 
    int totalTaches = 4;

    void Start()
    {
        PV= GetComponent<PhotonView>();
        canpickup = false;
        displayMessage = false;
        computer = false;
        thePlayer = GameObject.Find("Telefone");
        playerScript = thePlayer.GetComponent<AudioScript>();
        telephone = false;
    }

    void Update()
    {
        CheckIfTimerIstrue();
        CheckIfComputerMeshAreOk();
        if(globalNumber < 1)
            txtNumber.text="Tâche accomplie "+ globalNumber;
        else 
            txtNumber.text="Tâches : "+globalNumber+"/"+totalTaches;

        if(globalNumber == totalTaches) 
            winText.text="Vous avez gagné !";
    }

    private void CheckIfComputerMeshAreOk()
    {
        if((computer1.material.name == "world-map (Instance)" && computer2.material.name == "world-map (Instance)" && computer3.material.name == "world-map (Instance)" && computer4.material.name == "world-map (Instance)" ) && computer == false)
        {
            computer = true;
            RPC_IncreaseNumber();
        }
    }

    private void CheckIfTimerIstrue()
    {
        if(playerScript.isFinish && telephone == false)
        {
            RPC_IncreaseNumber();
            telephone = true;
        }
    }


    private void OnTriggerEnter(Collider other) // to see when the player enters the collider
    {
        if(other.gameObject.tag == "grabbing" && canpickup == false) //on the object you want to pick up set the tag to be anything, in this case "object"
        {
            canpickup = true;
            otherGameObject = other.gameObject;
            transformObject = other.gameObject.transform;
            Destroy(otherGameObject);
            RPC_IncreaseNumber();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        canpickup = false; //when you leave the collider set the canpickup bool to false
    }

    public void IncreaseNumber(){
        PV.RPC("RPC_IncreaseNumber",RpcTarget.AllViaServer);
    }

    [PunRPC]
    public void RPC_IncreaseNumber(){
        globalNumber++;
    }
}

