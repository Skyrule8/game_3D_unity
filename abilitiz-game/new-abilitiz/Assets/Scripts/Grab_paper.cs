using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grab_paper : MonoBehaviour
{
    public GameObject Paper; 
    public GameObject Figurine; 
    private bool paperGrab;
    // Start is called before the first frame update
    void Start()
    {
        Paper.gameObject.transform.localScale = new Vector3(50, 50, 50);
        Figurine.gameObject.transform.localScale = new Vector3(4, 4, 4);
        paperGrab = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(Paper != null)
            Paper.gameObject.transform.localScale = new Vector3(50, 50, 50);
        if(Paper != null)
            Figurine.gameObject.transform.localScale = new Vector3(4, 4, 4);
    }   

    public void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
            paperGrab = true;
            StartCoroutine(ExampleCoroutine());
    }

    public void OnTriggerExit(Collider other)
    {
    }

    private void OnGUI()
    {
        if(paperGrab)
            GUI.Label(new Rect(Screen.width / 2 - 75, Screen.height - 100, 200, 200), "Mettez le papier dans l'imprimante");
    }

    IEnumerator ExampleCoroutine()
    {
        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSeconds(5);
        paperGrab = false;
    }
}
