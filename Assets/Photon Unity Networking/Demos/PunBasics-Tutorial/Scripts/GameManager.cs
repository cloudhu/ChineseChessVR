// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Launcher.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Networking Demos
// </copyright>
// <summary>
//  Used in "PUN Basic tutorial" to handle typical game management requirements
// </summary>
// <author>developer@exitgames.com</author>
//中文注释：胡良云（CloudHu）
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections;

using UnityEngine;
using UnityEngine.SceneManagement; 


	/// <summary>
	/// Game manager.
	/// Connects and watch Photon Status, Instantiate Player
	/// Deals with quiting the room and the game
	/// Deals with level loading (outside the in room synchronization)
	/// </summary>
	public class GameManager : Photon.MonoBehaviour {

		#region Public Variables

		static public GameManager Instance;


		[Tooltip("玩家头盔预设")]
		public GameObject HeadPrefab;

		[Tooltip("玩家双手预设")]
		public GameObject HandPrefab;

        public Transform head;
        public Transform leftHand;
        public Transform rightHand;
        //[Tooltip("VRTK")]
        // public GameObject vrtk;

        #endregion

        #region Private Variables

        private GameObject instance;

		#endregion

		#region MonoBehaviour CallBacks

		/// <summary>
		/// MonoBehaviour method called on GameObject by Unity during initialization phase.
		/// </summary>
		void Start()
		{
			Instance = this;

			// in case we started this demo with the wrong scene being active, simply load the menu scene
			if (!PhotonNetwork.connected)
			{
				SceneManager.LoadScene("PunLauncher");

				return;
			}

			if (HeadPrefab == null) { // #Tip Never assume public properties of Components are filled up properly, always check and inform the developer of it.
				
				Debug.LogError("<Color=Red><b>Missing</b></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'",this);
			} else {
				

				if (CameraRigManager.LocalPlayerInstance==null)
				{
					Debug.Log("We are Instantiating LocalPlayer from "+SceneManagerHelper.ActiveSceneName);

					// 我们在房间内.为本地玩家生成一个角色（这里是CameraRig）. 通过使用PhotonNetwork.Instantiate来再整个网络上同步


                    
                    GameObject RHand= PhotonNetwork.Instantiate(this.HandPrefab.name, Vector3.zero, Quaternion.identity, 0) as GameObject;
					RHand.transform.SetParent(rightHand,false);
					GameObject LHand = PhotonNetwork.Instantiate(this.HandPrefab.name, Vector3.zero, Quaternion.identity, 0) as GameObject;
					LHand.transform.SetParent(leftHand,false);
					GameObject Head= PhotonNetwork.Instantiate(this.HeadPrefab.name, Vector3.zero, Quaternion.identity, 0) as GameObject;
					Head.transform.SetParent(head,false);

                }
                else{

					Debug.Log("Ignoring scene load for "+ SceneManagerHelper.ActiveSceneName);
				}

				
			}

		}

		/// <summary>
		/// MonoBehaviour method called on GameObject by Unity on every frame.
		/// </summary>
		void Update()
		{
			// "back" button of phone equals "Escape". quit app if that's pressed
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				QuitApplication();
			}
		}

        #endregion

        #region Photon Messages //Photon消息区域

        /// <summary>
        /// 当Photon玩家已连接时调用。我们需要在那时加载更大的场景。
        /// </summary>
        /// <param name="other">Other.</param>
        public void OnPhotonPlayerConnected( PhotonPlayer other  )
		{
			Debug.Log( "OnPhotonPlayerConnected() " + other.NickName); //如果你是正在连接的玩家则看不到

            if ( PhotonNetwork.isMasterClient ) 
			{
				Debug.Log( "OnPhotonPlayerConnected isMasterClient " + PhotonNetwork.isMasterClient ); // 在OnPhotonPlayerDisconnected之前调用

                PhotonNetwork.LoadLevel("ChineseChessVR");   //加载关卡
            }
		}

		/// <summary>
		/// 当Photon玩家断连时调用。我们需要加载小一点的场景。
		/// </summary>
		/// <param name="other">Other.</param>
		public void OnPhotonPlayerDisconnected( PhotonPlayer other  )
		{
			Debug.Log( "OnPhotonPlayerDisconnected() " + other.NickName ); // 当其他玩家断连时可见

			if ( PhotonNetwork.isMasterClient ) 
			{
				Debug.Log( "OnPhotonPlayerConnected isMasterClient " + PhotonNetwork.isMasterClient ); // 在OnPhotonPlayerDisconnected之前调用

                 PhotonNetwork.LoadLevel("ChineseChessVR");
            }
		}

        /// <summary>
        /// 当本地玩家离开房间时被调用。我们需要加载Launcher场景。
        /// </summary>
        public virtual void OnLeftRoom()
		{
			SceneManager.LoadScene("PunLauncher");
		}

		#endregion

		#region Public Methods

		public void LeaveRoom()
		{
			PhotonNetwork.LeaveRoom();
		}

		public void QuitApplication()
		{
			Application.Quit();
		}

		#endregion

		#region Private Methods



		#endregion

	}
	