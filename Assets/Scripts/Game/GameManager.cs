// --------------------------------------------------------------------------------------------------------------------
// <copyright file=GameManager.cs company=League of HTC Vive Developers>
/*
11111111111111111111111111111111111111001111111111111111111111111
11111111111111111111111111111111111100011111111111111111111111111
11111111111111111111111111111111100001111111111111111111111111111
11111111111111111111111111111110000111111111111111111111111111111
11111111111111111111111111111000000111111111111111111111111111111
11111111111111111111111111100000011110001100000000000000011111111
11111111111111111100000000000000000000000000000000011111111111111
11111111111111110111000000000000000000000000000011111111111111111
11111111111111111111111000000000000000000000000000000000111111111
11111111111111111110000000000000000000000000000000111111111111111
11111111111111111100011100000000000000000000000000000111111111111
11111111111111100000110000000000011000000000000000000011111111111
11111111111111000000000000000100111100000000000001100000111111111
11111111110000000000000000001110111110000000000000111000011111111
11111111000000000000000000011111111100000000000000011110001111111
11111110000000011111111111111111111100000000000000001111100111111
11111111000001111111111111111111110000000000000000001111111111111
11111111110111111111111111111100000000000000000000000111111111111
11111111111111110000000000000000000000000000000000000111111111111
11111111111111111100000000000000000000000000001100000111111111111
11111111111111000000000000000000000000000000111100000111111111111
11111111111000000000000000000000000000000001111110000111111111111
11111111100000000000000000000000000000001111111110000111111111111
11111110000000000000000000000000000000111111111110000111111111111
11111100000000000000000001110000001111111111111110001111111111111
11111000000000000000011111111111111111111111111110011111111111111
11110000000000000001111111111111111100111111111111111111111111111
11100000000000000011111111111111111111100001111111111111111111111
11100000000001000111111111111111111111111000001111111111111111111
11000000000001100111111111111111111111111110000000111111111111111
11000000000000111011111111111100011111000011100000001111111111111
11000000000000011111111111111111000111110000000000000011111111111
11000000000000000011111111111111000000000000000000000000111111111
11001000000000000000001111111110000000000000000000000000001111111
11100110000000000001111111110000000000000000111000000000000111111
11110110000000000000000000000000000000000111111111110000000011111
11111110000000000000000000000000000000001111111111111100000001111
11111110000010000000000000000001100000000111011111111110000001111
11111111000111110000000000000111110000000000111111111110110000111
11111110001111111100010000000001111100000111111111111111110000111
11111110001111111111111110000000111111100000000111111111111000111
11111111001111111111111111111000000111111111111111111111111100011
11111111101111111111111111111110000111111111111111111111111001111
11111111111111111111111111111110001111111111111111111111100111111
11111111111111111111111111111111001111111111111111111111001111111
11111111111111111111111111111111100111111111111111111111111111111
11111111111111111111111111111111110111111111111111111111111111111
*/
//   
// </copyright>
// <summary>
//  Chinese Chess VR
// </summary>
// <author>胡良云（CloudHu）</author>
//中文注释：胡良云（CloudHu） 3/28/2017
// --------------------------------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// FileName: GameManager.cs
/// Author: 胡良云（CloudHu）
/// Corporation: 
/// Description: 
/// DateTime: 3/28/2017
/// </summary>
public class GameManager : Photon.MonoBehaviour {
	
	#region Public Variables

	static public GameManager Instance;


	[Tooltip("玩家头盔预设")]
	public GameObject HeadPrefab;
	[Tooltip("玩家双手预设")]
	public GameObject LHandPrefab;
	[Tooltip("玩家双手预设")]
	public GameObject RHandPrefab;

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

    public void OnJoinedRoom()
    {
        if (CameraRigManager.LocalPlayerInstance == null && HeadPrefab!=null)
        {
            Debug.Log("We are Instantiating LocalPlayer from " + SceneManagerHelper.ActiveSceneName);

            // 我们在房间内.为本地玩家生成一个角色（这里是CameraRig）. 通过使用PhotonNetwork.Instantiate来再整个网络上同步
            GameObject Head = PhotonNetwork.Instantiate(this.HeadPrefab.name, Vector3.zero, Quaternion.identity, 0) as GameObject;
            GameObject RHand = PhotonNetwork.Instantiate(this.RHandPrefab.name, Vector3.zero, Quaternion.identity, 0) as GameObject;
			RHand.transform.SetParent (rightHand,false);
			RHand.transform.GetComponent<HandController> ().ViveControllerRoot = rightHand.gameObject;
            GameObject LHand = PhotonNetwork.Instantiate(this.LHandPrefab.name, Vector3.zero, Quaternion.identity, 0) as GameObject;
			LHand.transform.SetParent (leftHand,false);
			LHand.transform.GetComponent<HandController> ().ViveControllerRoot = leftHand.gameObject;
        }
        else
        {

            Debug.Log("Ignoring scene load for " + SceneManagerHelper.ActiveSceneName);
        }
    }

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

			//PhotonNetwork.LoadLevel("ChineseChessVR");   //加载关卡
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

			//PhotonNetwork.LoadLevel("ChineseChessVR");
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
