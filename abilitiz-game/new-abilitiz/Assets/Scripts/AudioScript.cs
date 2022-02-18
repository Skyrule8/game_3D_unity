using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class AudioScript : MonoBehaviour
{
    private PhotonView PV;
    public AudioSource audioRPC;
    public AudioClip Voice;
    public AudioClip Call;
    private bool timer;
    public bool isFinish;
    private bool isNearPhone;
    private float timerTotal;

    // Start is called before the first frame update
    void Start()
    {
        PV= GetComponent<PhotonView>();
        timerTotal = 15;

        if(PhotonNetwork.PlayerList.Length >= 5)
            StartCoroutine(ExampleCoroutine());        
        isFinish = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(timer && timerTotal > 0 && isFinish == false)
            timerTotal -= Time.deltaTime;

        if(timerTotal < 0)
        {
            isFinish = true;
            audioRPC.mute = true;
        }
    }

    public void ForPhoneRing()
    {
        PV.RPC("_PhoneRing",RpcTarget.AllViaServer);
    }

    public void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            audioRPC.clip = Voice;
            audioRPC.loop = true;
            audioRPC.Play();
            isNearPhone = true;
            timer = true;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        timerTotal = timerTotal+5;
        timer = false;
        isNearPhone = false;
        audioRPC.clip = Call;
        audioRPC.loop = true;
        audioRPC.Play();
    }

    [PunRPC]
    public void _PhoneRing()
    {
        audioRPC.spatialBlend = 1;
        audioRPC.mute = false;
        audioRPC.loop = true;
        audioRPC.minDistance = 2;
        audioRPC.maxDistance = 15;
        audioRPC.Play();
    }

    IEnumerator ExampleCoroutine()
    {
        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSeconds(50);
        ForPhoneRing();
    }

    void OnGUI()
    {
        if(isNearPhone && isFinish == false)
            GUI.Label(new Rect(Screen.width / 2 - 75, Screen.height - 100, 200, 200), "Restez prêt du téléphone : "+timerTotal+" \n Attention à vous si vous partez ! ");
    }
}
