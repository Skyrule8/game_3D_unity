// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlayerManager.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Networking Demos
// </copyright>
// <summary>
//  Used in PUN Basics Tutorial to deal with the networked player instance
// </summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

namespace Photon.Pun.Demo.PunBasics
{
	#pragma warning disable 649

    /// <summary>
    /// Player manager.
    /// Handles fire Input and Beams.
    /// </summary>
    public class PlayerManager : MonoBehaviourPunCallbacks, IPunObservable
    {
        #region Public Fields

        [Tooltip("The current Health of our player")]
        public float Health = 1f;

        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;

        public static Camera CameraRobot;

        public TextMesh LocalGameObject; 

        public Material material1;

        public Material material2;

        #endregion

        #region Private Fields

        [Tooltip("The Player's UI GameObject Prefab")]
        [SerializeField]
        private GameObject playerUiPrefab;

        [Tooltip("The Beams GameObject to control")]
        [SerializeField]
        private GameObject beams;

        private bool isNearComputer;
        //True, when the user is firing
        bool IsFiring;

        bool isNearFigurine;

        #endregion

        #region MonoBehaviour CallBacks

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
        /// </summary>
        public void Awake()
        {
            if (this.beams == null)
                Debug.LogError("<Color=Red><b>Missing</b></Color> Beams Reference.", this);
            else
                this.beams.SetActive(false);

            // #Important
            // used in GameManager.cs: we keep track of the localPlayer instance to prevent instanciation when levels are synchronized
            if (photonView.IsMine)
                LocalPlayerInstance = gameObject;

            // #Critical
            // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during initialization phase.
        /// </summary>
        public void Start()
        {
            //if(photonView.IsMine)
                // LocalGameObject.text = PhotonNetwork.LocalPlayer.NickName;

            CameraWork _cameraWork = gameObject.GetComponent<CameraWork>();
            Camera[] allCameras = FindObjectsOfType<Camera>();

            foreach(Camera cam in allCameras)
            {
                if(cam.name == "CameraRobot")
                    CameraRobot = cam;
            }

            if (_cameraWork != null)
            {
                Debug.Log(true);
                if (photonView.IsMine)
                        if(!PhotonNetwork.IsMasterClient)
                            _cameraWork.OnStartFollowing();
                    
            }
            else
            {
                Debug.LogError("<Color=Red><b>Missing</b></Color> CameraWork Component on player Prefab.", this);
            }

            // Create the UI
            if (this.playerUiPrefab != null)
            {
                GameObject _uiGo = Instantiate(this.playerUiPrefab);
                _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
            }
            else
            {
                Debug.LogWarning("<Color=Red><b>Missing</b></Color> PlayerUiPrefab reference on player Prefab.", this);
            }

            #if UNITY_5_4_OR_NEWER
            // Unity 5.4 has a new scene management. register a method to call CalledOnLevelWasLoaded.
			UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
            #endif
        }


		public override void OnDisable()
		{
			// Always call the base to remove callbacks
			base.OnDisable ();

			#if UNITY_5_4_OR_NEWER
			UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
			#endif
		}


        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity on every frame.
        /// Process Inputs if local player.
        /// Show and hide the beams
        /// Watch for end of game, when local player health is 0.
        /// </summary>
        public void Update()
        {

            // we only process Inputs and check health if we are the local player
            if (photonView.IsMine)
            {
                this.InputCode();

                if (this.Health <= 0f)
                    GameManager.Instance.LeaveRoom();
            }

            /*if (this.beams != null && this.IsFiring != this.beams.activeInHierarchy)
                this.beams.SetActive(this.IsFiring);*/
        }

        /// <summary>
        /// MonoBehaviour method called when the Collider 'other' enters the trigger.
        /// Affect Health of the Player if the collider is a beam
        /// Note: when jumping and firing at the same, you'll find that the player's own beam intersects with itself
        /// One could move the collider further away to prevent this or check if the beam belongs to the player.
        /// </summary>
        public void OnTriggerEnter(Collider other)
        {
            if(other.gameObject.tag == "computer")
                isNearComputer = true;
            if(other.gameObject.tag == "statuette")
                isNearFigurine = true;
            if (!photonView.IsMine)
                return;

            // We are only interested in Beamers
            // we should be using tags but for the sake of distribution, let's simply check by name.
            if (!other.name.Contains("Beam"))
                return;

            this.Health -= 0.1f;
        }

        public void OnTriggerExit(Collider other)
        {
            isNearComputer = false;
            isNearFigurine = false;
        }

        /// <summary>
        /// MonoBehaviour method called once per frame for every Collider 'other' that is touching the trigger.
        /// We're going to affect health while the beams are interesting the player
        /// </summary>
        /// <param name="other">Other.</param>
        public void OnTriggerStay(Collider other)
        {
            // we dont' do anything if we are not the local player.
            if (!photonView.IsMine)
                return;

            // We are only interested in Beamers
            // we should be using tags but for the sake of distribution, let's simply check by name.
            if (!other.name.Contains("Beam"))
                return;

            // we slowly affect health when beam is constantly hitting us, so player has to move to prevent death.
            this.Health -= 0.1f*Time.deltaTime;
        }


        #if !UNITY_5_4_OR_NEWER
        /// <summary>See CalledOnLevelWasLoaded. Outdated in Unity 5.4.</summary>
        void OnLevelWasLoaded(int level)
        {
            this.CalledOnLevelWasLoaded(level);
        }

        #endif


        /// <summary>
        /// MonoBehaviour method called after a new level of index 'level' was loaded.
        /// We recreate the Player UI because it was destroy when we switched level.
        /// Also reposition the player if outside the current arena.
        /// </summary>
        /// <param name="level">Level index loaded</param>
        void CalledOnLevelWasLoaded(int level)
        {
            // check if we are outside the Arena and if it's the case, spawn around the center of the arena in a safe zone
            if (!Physics.Raycast(transform.position, -Vector3.up, 5f))
                transform.position = new Vector3(0f, 5f, 0f);

            GameObject _uiGo = Instantiate(this.playerUiPrefab);
            _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
        }

        #endregion

        #region Private Methods


		#if UNITY_5_4_OR_NEWER
		void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadingMode)
		{
			this.CalledOnLevelWasLoaded(scene.buildIndex);
		}

		#endif

        /// <summary>
        /// Processes the inputs. This MUST ONLY BE USED when the player has authority over this Networked GameObject (photonView.isMine == true)
        /// </summary>
        void ProcessInputs()
        {
            if (Input.GetButtonDown("Fire1"))
            {
                // we don't want to fire when we interact with UI buttons for example. IsPointerOverGameObject really means IsPointerOver*UI*GameObject
                // notice we don't use on on GetbuttonUp() few lines down, because one can mouse down, move over a UI element and release, which would lead to not lower the isFiring Flag.
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    //	return;
                }

                if (!this.IsFiring)
                    this.IsFiring = true;
            }

            if (Input.GetButtonUp("Fire1"))
            {
                if (this.IsFiring)
                    this.IsFiring = false;
            }
        }

        void InputCode()
        {
            RaycastHit hit;
            LocalPlayerInstance.transform.position = new Vector3(LocalPlayerInstance.transform.position.x, 1.5f, LocalPlayerInstance.transform.position.z) ;
            Ray ray = new Ray(LocalPlayerInstance.transform.position, LocalPlayerInstance.transform.forward);
            if (Physics.Raycast(ray, out hit, 1)) 
            {   
                if (Input.GetKeyDown(KeyCode.E) && IsFiring == false)
                {
                    if(hit.collider.name == "Computer")
                        photonView.RPC("SomeCoolAction", RpcTarget.All, 1);
                    else if(hit.collider.name == "Computer-2")
                        photonView.RPC("SomeCoolAction2", RpcTarget.All, 1);
                    else if(hit.collider.name == "Computer-3")
                        photonView.RPC("SomeCoolAction3", RpcTarget.All, 1);
                    else if(hit.collider.name == "Computer-4")
                        photonView.RPC("SomeCoolAction4", RpcTarget.All, 1);
                    this.IsFiring = true;

                    // we don't want to fire when we interact with UI buttons for example. IsPointerOverGameObject really means IsPointerOver*UI*GameObject
                    // notice we don't use on on GetbuttonUp() few lines down, because one can mouse down, move over a UI element and release, which would lead to not lower the isFiring Flag.
                    if (EventSystem.current.IsPointerOverGameObject())
                    {
                        
                    }

                    if (!this.IsFiring)
                    {    

                    }
                } 
                else if (Input.GetKeyDown(KeyCode.E) && IsFiring == true)
                {
                    if (this.IsFiring)
                    {                    
                        if(hit.collider.name == "Computer")
                            photonView.RPC("SomeCoolAction", RpcTarget.All, 2);
                        else if(hit.collider.name == "Computer-2")
                            photonView.RPC("SomeCoolAction2", RpcTarget.All, 2);
                        else if(hit.collider.name == "Computer-3")
                            photonView.RPC("SomeCoolAction3", RpcTarget.All, 2);
                        else if(hit.collider.name == "Computer-4")
                            photonView.RPC("SomeCoolAction4", RpcTarget.All, 2);                       
                        this.IsFiring = false;
                    }
                }
            }
        }

        [PunRPC]
        public void SomeCoolAction(int material)
        {
            var hand = GameObject.Find("ComputerPlane1");
            if(material == 1)
                hand.gameObject.GetComponent<MeshRenderer>().material = material2;
            else if(material == 2)
                hand.gameObject.GetComponent<MeshRenderer>().material = material1;
        }

        [PunRPC]
        public void SomeCoolAction2(int material)
        {
            var hand = GameObject.Find("ComputerPlane2");
            if(material == 1)
                hand.gameObject.GetComponent<MeshRenderer>().material = material2;
            else if(material == 2)
                hand.gameObject.GetComponent<MeshRenderer>().material = material1;
        }
        
        [PunRPC]
        public void SomeCoolAction3(int material)
        {
            var hand = GameObject.Find("ComputerPlane3");
            if(material == 1)
                hand.gameObject.GetComponent<MeshRenderer>().material = material2;
            else if(material == 2)
                hand.gameObject.GetComponent<MeshRenderer>().material = material1;
        }

        [PunRPC]
        public void SomeCoolAction4(int material)
        {
            var hand = GameObject.Find("ComputerPlane4");
            if(material == 1)
                hand.gameObject.GetComponent<MeshRenderer>().material = material2;
            else if(material == 2)
                hand.gameObject.GetComponent<MeshRenderer>().material = material1;
        }

        #endregion

        #region IPunObservable implementation

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // We own this player: send the others our data
                stream.SendNext(this.IsFiring);
                stream.SendNext(this.Health);
            }
            else
            {
                // Network player, receive data
                this.IsFiring = (bool)stream.ReceiveNext();
                this.Health = (float)stream.ReceiveNext();
            }
        }

        #endregion

        void OnGUI()
        {
            if(isNearFigurine)
                GUI.Label(new Rect(Screen.width / 2 - 75, Screen.height - 100, 160, 40), "Vous avez trouvé l'objet secret ! Déposez le à l'acceuil");
            if(isNearComputer)
                GUI.Label(new Rect(Screen.width / 2 - 75, Screen.height - 100, 160, 40), "Appuyez sur 'E' pour " + (IsFiring ? "fermer" : "ouvrir") + " le PC");
        }
    }
}