// --------------------------------------------------------------------------------------------------------------------
// <copyright file=PunLauncher.cs company=League of HTC Vive Developers>
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
using UnityEngine.UI;
using UnityEngine.SceneManagement; 

using Random = UnityEngine.Random;
/// <summary>
/// FileName: PunLauncher.cs
/// Author: 胡良云（CloudHu）
/// Corporation: 
/// Description: 
/// DateTime: 3/28/2017
/// </summary>
public class PunLauncher  : Photon.PunBehaviour {

		#region Public Variables  //公共变量区域

		[Tooltip("让用户输入姓名、连接和开始游戏的UI面板")]
		public GameObject controlPanel;

		[Tooltip("告知用户连接进程的UI文本")]
		public Text feedbackText;

		[Tooltip("每间房间的最大玩家数量")]
		public byte maxPlayersPerRoom = 10;

		[Tooltip("UI加载动画")]
		public Loader loaderAnime;

		#endregion

		#region Private Variables   //私有变量区域
		/// <summary>
		/// 跟踪当前进程。因为连接是异步的，且是基于来自Photon的几个回调，
		/// 我们需要跟踪这一点，以在我们收到Photon回调时适当地调整该行为。
		/// 这通常是用于OnConnectedToMaster()回调。
		/// </summary>
		bool isConnecting;

		/// <summary>
		/// 此客户端的版本号。用户通过gameversion彼此分离 (这让你可以做出突破性的改变).
		/// </summary>
		string _gameVersion = "1";

		#endregion

		#region MonoBehaviour CallBacks //回调函数区域

		/// <summary>
		/// 在早期初始化阶段里被Unity在游戏对象上调用的MonoBehaviour方法。
		/// </summary>
		void Awake()
		{
			if (loaderAnime==null)
			{
				Debug.LogError("<Color=Red><b>缺少</b></Color> loaderAnime引用.",this);
			}

			// #NotImportant
			// Force LogLevel
			PhotonNetwork.logLevel = PhotonLogLevel.ErrorsOnly;

			// #Critical | 极重要
			//我们不加入大厅。没有必要加入一个大厅来获得房间列表。
			PhotonNetwork.autoJoinLobby = false;

			// #Critical | 极重要
			//这样可以确保我们可以在主客户端上使用PhotonNetwork.LoadLevel()方法，并且在相同房间里的所有客户端都会自动同步它们的关卡。
			PhotonNetwork.automaticallySyncScene = true;


		}

		#endregion


		#region Public Methods //公共方法

		/// <summary>
		/// 启动连接进程。
		/// - 如果已经连接，我们试图加入一个随机的房间
		/// - 如果尚未连接，请将此应用程序实例连接到Photon云网络
		/// </summary>
		public void Connect()
		{
			// 我们想要确保每次连接的时候记录都被清空，如果连接失败我们可能会有几次失败的尝试。
			feedbackText.text = "";

			// 跟踪玩家加入一个房间的意愿，因为当我们从游戏中回来时，我们会得到一个我们已连接的回调，所以我们需要知道那个时候该怎么做
			isConnecting = true;

			// hide the Play button for visual consistency
			controlPanel.SetActive(false);

			// start the loader animation for visual effect.
			if (loaderAnime!=null)
			{
				loaderAnime.StartLoaderAnimation();
			}

			// 我们检查是否连接，如果我们已连接则加入，否则我们启动连接到服务器。
			if (PhotonNetwork.connected)
			{
				LogFeedback("Joining Room...");
				// #Critical | 极重要 -我们需要在这个点上企图加入一个随机房间。如果失败，我们将在OnPhotonRandomJoinFailed()里面得到通知，这样我们将创建一个房间。
				PhotonNetwork.JoinRandomRoom();
			}else{

				LogFeedback("Connecting...");


				// #Critical | 极重要 -我们必须首先连接到Photon在线服务器。
				PhotonNetwork.ConnectUsingSettings(_gameVersion);
			}
		}

		/// <summary>
		/// Logs the feedback in the UI view for the player, as opposed to inside the Unity Editor for the developer.
		/// </summary>
		/// <param name="message">Message.</param>
		void LogFeedback(string message)
		{
			// we do not assume there is a feedbackText defined.
			if (feedbackText == null) {
				return;
			}

			// add new messages as a new line and at the bottom of the log.
			feedbackText.text += System.Environment.NewLine+message;
		}

		#endregion


		#region Photon.PunBehaviour CallBacks   //PUN回调区域
		// below, we implement some callbacks of PUN
		// you can find PUN's callbacks in the class PunBehaviour or in enum PhotonNetworkingMessage


		/// <summary>
		/// Called after the connection to the master is established and authenticated but only when PhotonNetwork.autoJoinLobby is false.
		/// </summary>
		public override void OnConnectedToMaster()
		{

			Debug.Log("Region:"+PhotonNetwork.networkingPeer.CloudRegion);

			// we don't want to do anything if we are not attempting to join a room. 
			// this case where isConnecting is false is typically when you lost or quit the game, when this level is loaded, OnConnectedToMaster will be called, in that case
			// we don't want to do anything.
			if (isConnecting)
			{
				LogFeedback("OnConnectedToMaster: Next -> try to Join Random Room");
				Debug.Log("Launcher: OnConnectedToMaster() was called by PUN. Now this client is connected and could join a room.\n Calling: PhotonNetwork.JoinRandomRoom(); Operation will fail if no room found");

				//#Critical | 极重要: 我们首先尝试要做的就是加入一个潜在现有房间。如果有，很好，否则，我们将调用回调OnPhotonRandomJoinFailed()  
				PhotonNetwork.JoinRandomRoom();
			}
		}

		/// <summary>
		/// Called when a JoinRandom() call failed. The parameter provides ErrorCode and message.
		/// </summary>
		/// <remarks>
		/// Most likely all rooms are full or no rooms are available. <br/>
		/// </remarks>
		/// <param name="codeAndMsg">codeAndMsg[0] is short ErrorCode. codeAndMsg[1] is string debug msg.</param>
		public override void OnPhotonRandomJoinFailed(object[] codeAndMsg)
		{
			LogFeedback("<Color=Red>OnPhotonRandomJoinFailed</Color>: Next -> Create a new Room");
			Debug.Log("DemoAnimator/Launcher:OnPhotonRandomJoinFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom(null, new RoomOptions() {maxPlayers = 4}, null);");

			// #Critical | 极重要: 我们加入一个随机房间失败，也许没有房间存在或房间已满。别担心，我们创建一个新的房间即可。
			PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = this.maxPlayersPerRoom}, null);
		}


		/// <summary>
		/// Called after disconnecting from the Photon server.
		/// </summary>
		/// <remarks>
		/// In some cases, other callbacks are called before OnDisconnectedFromPhoton is called.
		/// Examples: OnConnectionFail() and OnFailedToConnectToPhoton().
		/// </remarks>
		public override void OnDisconnectedFromPhoton()
		{
			LogFeedback("<Color=Red>OnDisconnectedFromPhoton</Color>");
			Debug.LogError("DemoAnimator/Launcher:Disconnected");

			// #Critical: we failed to connect or got disconnected. There is not much we can do. Typically, a UI system should be in place to let the user attemp to connect again.
			loaderAnime.StopLoaderAnimation();

			isConnecting = false;
			controlPanel.SetActive(true);

		}

		/// <summary>
		/// 当进入一个房间时（通过创建或加入该房间）在所有客户端上调用（包括主客户端）。
		/// </summary>
		/// <remarks>
		/// 该方法常被用于实例化玩家角色。
		/// 如果匹配必须被玩家主动启动，你可以调用一个[PunRPC](@ref PhotonView.RPC)由用户按钮按下或定时器触发。
		/// 
		/// 当该方法被调用时，你通常已经可以通过PhotonNetwork.playerList访问到房间里存在的玩家了。
		/// 同样的，所有自定义属性应该已经作为Room.customProperties可供使用了。检查Room.PlayerCount来查看房间里是否有足够的玩家来开始游戏了。
		/// </remarks>
		public override void OnJoinedRoom()
		{
			LogFeedback("<Color=Green>OnJoinedRoom</Color> with "+PhotonNetwork.room.PlayerCount+" Player(s)");
			Debug.Log("DemoAnimator/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.\nFrom here on, your game would be running. For reference, all callbacks are listed in enum: PhotonNetworkingMessage");

			// #Critical | 极重要：只有第一个玩家才加载，否则我们依赖PhotonNetwork.automaticallySyncScene来同步我们的场景实例
			// if (PhotonNetwork.room.PlayerCount == 1)
			//{
			Debug.Log("We load the 'ChineseChessVR' ");

			//  #Critical | 极重要：加载房间关卡
			if (PhotonNetwork.room.PlayerCount > 1)
			{
				PhotonNetwork.LoadLevel("ChineseChessVR");

			}
		}

		#endregion

	}
