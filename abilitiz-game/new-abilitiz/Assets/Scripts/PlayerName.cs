using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class PlayerName : MonoBehaviourPun, IPunObservable
{
    public Text nameText;
    private string photonName;
    public PhotonView pv;

    private void Start()
    {
        if(!photonView.IsMine)
        {
            photonName = PhotonNetwork.NickName;
            nameText.text = photonName;
        }

        nameText.text = pv.Owner.NickName;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(photonName);
        }
        else if (stream.IsReading)
        {
            photonName = (string)stream.ReceiveNext();
        }
    }
}