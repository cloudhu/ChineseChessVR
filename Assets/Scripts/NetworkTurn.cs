// --------------------------------------------------------------------------------------------------------------------
// <copyright file=NetworkTurn.cs company=League of HTC Vive Developers>
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
//中文注释：胡良云（CloudHu） 3/22/2017

// --------------------------------------------------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using Photon;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// FileName: NetworkTurn.cs
/// Author: 胡良云（CloudHu）
/// Corporation: 
/// Description: 这个脚本用于处理网络回合
/// DateTime: 3/22/2017
/// </summary>
public class NetworkTurn : PunBehaviour, IPunTurnManagerCallbacks {
	
	#region Public Variables  //公共变量区域

	/// <summary>  
	/// 被选中的棋子的ID，若没有被选中的棋子，则ID为-1  
	/// </summary>  
	[Tooltip("被选中的棋子的ID")]
	public int _selectedId=-1;  

	[Tooltip("是否是红子的回合,默认红子先行")]
	public bool isRedTurn=true;

	public struct step
	{
		public int moveId;
		public int killId;

		public float xFrom;
		public float yFrom;
		public float xTo;
		public float yTo;

		public step(int _moveId,int _killId,float _xFrom,float _yFrom,float _xTo,float _yTo){
			moveId = _moveId;
			killId = _killId;
			xFrom =_xFrom;
			yFrom =_yFrom;
			xTo=_xTo;
			yTo = _yTo;
		}
	}
	[Tooltip("保存每一步走棋")]
	public List<step> _steps = new List<step> ();

	[Tooltip("选中的音效,胜利，失败的音乐")]
	public AudioSource selectClap,winMusic,loseMusic;

	[Tooltip("德邦总管")]
	public ChessmanManager chessManManager;


	#endregion


	#region Private Variables   //私有变量区域

	// 追踪显示结果的时机来处理游戏逻辑.
	private bool IsShowingResults;

	[Tooltip("选中的棋子")]
	[SerializeField]
	private GameObject selectedChess;

	[Tooltip("连接UI视图")]
	[SerializeField]
	private RectTransform ConnectUiView;

	[Tooltip("游戏UI视图")]
	[SerializeField]
	private RectTransform GameUiView;

	[Tooltip("按钮幕布组")]
	[SerializeField]
	private CanvasGroup ButtonCanvasGroup;

	[Tooltip("计时器填充图")]
	[SerializeField]
	private RectTransform TimerFillImage;

	[Tooltip("回合文本")]
	[SerializeField]
	private Text TurnText;

	[Tooltip("时间文本")]
	[SerializeField]
	private Text TimeText;

	[Tooltip("远程玩家文本")]
	[SerializeField]
	private Text RemotePlayerText;

	[Tooltip("本地玩家文本")]
	[SerializeField]
	private Text LocalPlayerText;

	[Tooltip("输赢图片")]
	[SerializeField]
	private Image WinOrLossImage;

	[Tooltip("胜利精灵")]
	[SerializeField]
	private Sprite SpriteWin;

	[Tooltip("失败精灵")]
	[SerializeField]
	private Sprite SpriteLose;

	[Tooltip("平局精灵")]
	[SerializeField]
	private Sprite SpriteDraw;

	[Tooltip("断连面板")]
	[SerializeField]
	private RectTransform DisconnectedPanel;

	private ResultType result;//结果

	private PunTurnManager turnManager;//回合管家


	#endregion

	public enum ResultType	//结果类型枚举
	{
		None = 0,
		Draw,	//和
		LocalWin,	//赢
		LocalLoss	//输
	}

	
	#region Public Methods	//公共方法区域

	public void Judge(){
		if (ChessmanManager.chessman[0]._dead) {
			result = ResultType.LocalLoss;
			PlayLoseMusic ();
		}
		if (ChessmanManager.chessman[17]._dead) {
			result = ResultType.LocalWin;
			PlayWinMusic ();
		}
	}

	public void Start()
	{
		this.turnManager = this.gameObject.AddComponent<PunTurnManager>();	//添加组件并赋值
		this.turnManager.TurnManagerListener = this;	//为监听器赋值,从而触发下面的回调函数来完成游戏逻辑
		this.turnManager.TurnDuration = 120f;		//初始化回合持续时间

		RefreshUIViews();	//刷新UI视图
	}

	public void Update()
	{
		// 检查我们是否脱离了环境, 这意味着我们有可能回到演示中枢（演示中枢是用来总控所有案例的）.
		if (this.DisconnectedPanel ==null)
		{
			Destroy(this.gameObject);
		}

		// 为了方便调试, 弄一些快捷键是很有用的:
		if (Input.GetKeyUp(KeyCode.L)) 	//L键离开房间
		{
			PhotonNetwork.LeaveRoom();
		}
		if (Input.GetKeyUp(KeyCode.C)) //C键连接
		{
			PhotonNetwork.ConnectUsingSettings(null);
			PhotonHandler.StopFallbackSendAckThread();
		}


		if ( ! PhotonNetwork.inRoom)	//不在房间则退出
		{
			return;
		}

		// 如果PUN已连接或正在连接则禁用"reconnect panel"（重连面板）
		if (PhotonNetwork.connected && this.DisconnectedPanel.gameObject.GetActive())
		{
			this.DisconnectedPanel.gameObject.SetActive(false);
		}
		if (!PhotonNetwork.connected && !PhotonNetwork.connecting && !this.DisconnectedPanel.gameObject.GetActive())
		{
			this.DisconnectedPanel.gameObject.SetActive(true);
		}


		if (PhotonNetwork.room.PlayerCount>1)
		{
			if (this.turnManager.IsOver)
			{
				return;	//回合结束
			}

			if (this.TurnText != null)
			{
				this.TurnText.text = this.turnManager.Turn.ToString();	//更新回合数
			}

			if (this.turnManager.Turn > 0 && this.TimeText != null && ! IsShowingResults)
			{

				this.TimeText.text = this.turnManager.RemainingSecondsInTurn.ToString("F1") + " 秒";	//更新回合剩余时间

				TimerFillImage.anchorMax = new Vector2(1f- this.turnManager.RemainingSecondsInTurn/this.turnManager.TurnDuration,1f);
			}


		}

		this.UpdatePlayerTexts();	//更新玩家文本信息



		// 远程玩家的选择只在回合结束时（双方都完成回合）展示
		if (this.turnManager.IsCompletedByAll)
		{

		}
		else
		{
			ButtonCanvasGroup.interactable = PhotonNetwork.room.PlayerCount > 1;	//玩家数量大于1才可以触发按钮

			if (PhotonNetwork.room.PlayerCount < 2)
			{

			}

			// 如果所有玩家都没有完成该回合,我们为远程玩家的手势使用一个随机图片
			else if (this.turnManager.Turn > 0 && !this.turnManager.IsCompletedByAll)
			{
				// 远程玩家手势图片的阿尔法值（透明度）被用于表明远程玩家是否“活跃”以及“完成回合”
				PhotonPlayer remote = PhotonNetwork.player.GetNext();
				float alpha = 0.5f;
				if (this.turnManager.GetPlayerFinishedTurn(remote))
				{
					alpha = 1;	//完成回合为1
				}
				if (remote != null && remote.IsInactive)
				{
					alpha = 0.1f;
				}
					

			}
		}

	}
	#endregion

	#region TurnManager Callbacks	//回调区域

	/// <summary>
	/// 发起回合开始事件.
	/// </summary>
	/// <param name="turn">回合.</param>
	public void OnTurnBegins(int turn)
	{
		Debug.Log("OnTurnBegins() turn: "+ turn);


		this.WinOrLossImage.gameObject.SetActive(false);	//关闭输赢的图片


		IsShowingResults = false;	//不展示结果
		ButtonCanvasGroup.interactable = true;	//可以与按钮交互
	}

	/// <summary>
	/// 当回合完成时调用(被所有玩家完成)
	/// </summary>
	/// <param name="turn">回合索引</param>
	/// <param name="obj">Object.</param>
	public void OnTurnCompleted(int obj)
	{
		Debug.Log("OnTurnCompleted: " + obj);

		this.Judge();	//计算输赢
		this.UpdateScores();	//更新得分
		this.OnEndTurn();	//结束回合
	}


	/// <summary>
	/// 当玩家移动时调用(但是没有完成该回合)
	/// </summary>
	/// <param name="player">玩家引用</param>
	/// <param name="turn">回合索引</param>
	/// <param name="move">移动对象数据</param>
	/// <param name="photonPlayer">Photon player.</param>
	public void OnPlayerMove(PhotonPlayer photonPlayer, int turn, object move)
	{
		Debug.Log("OnPlayerMove: " + photonPlayer + " turn: " + turn + " action: " + move);
	}


	/// <summary>
	/// 当玩家完成回合时调用(包括该玩家的动作/移动)
	/// </summary>
	/// <param name="player">玩家引用</param>
	/// <param name="turn">回合索引</param>
	/// <param name="move">移动对象数据</param>
	/// <param name="photonPlayer">Photon player.</param>
	public void OnPlayerFinished(PhotonPlayer photonPlayer, int turn, object move)
	{
		Debug.Log("OnTurnFinished: " + photonPlayer + " turn: " + turn + " action: " + move);

		if (photonPlayer.IsLocal)
		{
			//this.localSelection = (Hand)(byte)move;
		}
		else
		{
			//this.remoteSelection = (Hand)(byte)move;
		}
	}


	/// <summary>
	/// 当回合由于时间限制完成时调用(回合超时)
	/// </summary>
	/// <param name="turn">回合索引</param>
	/// <param name="obj">Object.</param>
	public void OnTurnTimeEnds(int obj)
	{
		if (!IsShowingResults)
		{
			Debug.Log("OnTurnTimeEnds: Calling OnTurnCompleted");
			OnTurnCompleted(-1);
		}
	}

	/// <summary>
	/// 更新得分
	/// </summary>
	private void UpdateScores()
	{
		if (this.result == ResultType.LocalWin)
		{
			PhotonNetwork.player.AddScore(1);   //这是PhotonPlayer的扩展方法.就是给玩家加分
		}
	}
	#endregion

	#region Core Gameplay Methods	//核心玩法

	/// <summary>
	/// 开始对局.
	/// </summary>
	public void StartGame(){
		chessManManager.ChessmanInit ();
	}

	/// <summary>
	/// 重新开始对局.
	/// </summary>
	public void RestartGame(){
		chessManManager.DestroyAllChessman ();
		chessManManager.ChessmanInit ();
	}

	/// <summary>调用来开始回合 (只有主客户端会发送).</summary>
	public void StartTurn()
	{
		if (PhotonNetwork.isMasterClient)
		{
			this.turnManager.BeginTurn();
		}
	}

	/// <summary>
	/// 回合中的选择
	/// </summary>
	/// <param name="selection">选择.</param>
	public void MakeTurn()
	{

	}

	/// <summary>
	/// 回合结束
	/// </summary>
	public void OnEndTurn()
	{
		this.StartCoroutine("ShowResultsBeginNextTurnCoroutine");
	}

	/// <summary>
	/// 显示结果并开始下一回合的协程
	/// </summary>
	/// <returns>.</returns>
	public IEnumerator ShowResultsBeginNextTurnCoroutine()
	{
		ButtonCanvasGroup.interactable = false;	//禁用按钮交互
		IsShowingResults = true; 
		// yield return new WaitForSeconds(1.5f);

		if (this.result == ResultType.Draw)	//根据结果展示不同的图片
		{
			this.WinOrLossImage.sprite = this.SpriteDraw;
		}
		else
		{
			this.WinOrLossImage.sprite = this.result == ResultType.LocalWin ? this.SpriteWin : SpriteLose;
		}
		this.WinOrLossImage.gameObject.SetActive(true);

		yield return new WaitForSeconds(2.0f);

		this.StartTurn();
	}

	/// <summary>
	/// 结束游戏
	/// </summary>
	public void EndGame()
	{
		Debug.Log("EndGame");
	}
		
	/// <summary>
	/// 选择精灵
	/// </summary>
	/// <returns>返回对应手势的精灵.</returns>
	/// <param name="hand">手势.</param>
	private Sprite SelectionToSprite()
	{


		return null;
	}

	/// <summary>
	/// 更新玩家文本信息
	/// </summary>
	private void UpdatePlayerTexts()
	{
		PhotonPlayer remote = PhotonNetwork.player.GetNext();
		PhotonPlayer local = PhotonNetwork.player;

		if (remote != null)
		{
			// 应该是这种格式: "name        00"
			this.RemotePlayerText.text = remote.NickName + "        " + remote.GetScore().ToString("D2");
		}
		else
		{

			TimerFillImage.anchorMax = new Vector2(0f,1f);
			this.TimeText.text = "";
			this.RemotePlayerText.text = "等待其他玩家        00";
		}

		if (local != null)
		{
			// 应该是这种样式: "YOU   00"
			this.LocalPlayerText.text = "YOU   " + local.GetScore().ToString("D2");
		}
	}
		
	/// <summary>
	/// 播放选择音效.
	/// </summary>
	public void PlaySelectSound(){
		if (!selectClap.isPlaying) {
			selectClap.Play ();
		}
	}

	/// <summary>
	/// 播放胜利音乐.
	/// </summary>
	public void PlayWinMusic(){
		if (!winMusic.isPlaying) {
			winMusic.Play ();
		}
	}

	/// <summary>
	/// 播放失败音乐.
	/// </summary>
	public void PlayLoseMusic(){
		if (!loseMusic.isPlaying) {
			loseMusic.Play ();
		}
	}

	#endregion


	#region Handling Of Buttons	//处理按钮


	/// <summary>
	/// 连接
	/// </summary>
	public void OnClickConnect()
	{
		PhotonNetwork.ConnectUsingSettings(null);
		PhotonHandler.StopFallbackSendAckThread();  // 这在案例中被用于后台超时!
	}

	/// <summary>
	/// 重新连接并重新加入
	/// </summary>
	public void OnClickReConnectAndRejoin()
	{
		PhotonNetwork.ReconnectAndRejoin();
		PhotonHandler.StopFallbackSendAckThread();  // this is used in the demo to timeout in background!
	}

	#endregion

	/// <summary>
	/// 刷新UI视图
	/// </summary>
	void RefreshUIViews()
	{
		TimerFillImage.anchorMax = new Vector2(0f,1f);

		ConnectUiView.gameObject.SetActive(!PhotonNetwork.inRoom);
		GameUiView.gameObject.SetActive(PhotonNetwork.inRoom);

		ButtonCanvasGroup.interactable = PhotonNetwork.room!=null?PhotonNetwork.room.PlayerCount > 1:false;
	}

	/// <summary>
	/// 当本地用户/客户离开房间时调用。
	/// </summary>
	/// <remarks>当离开一个房间时，PUN将你带回主服务器。
	/// 在您可以使用游戏大厅和创建/加入房间之前，OnJoinedLobby()或OnConnectedToMaster()会再次被调用。</remarks>
	public override void OnLeftRoom()
	{
		Debug.Log("OnLeftRoom()");



		RefreshUIViews();
	}

	/// <summary>
	/// 当进入一个房间（通过创建或加入）时被调用。在所有客户端（包括主客户端）上被调用.
	/// </summary>
	/// <remarks>这种方法通常用于实例化玩家角色。
	/// 如果一场比赛必须“积极地”被开始，你也可以调用一个由用户的按键或定时器触发的PunRPC 。
	/// 
	/// 当这个被调用时，你通常可以通过PhotonNetwork.playerList访问在房间里现有的玩家。
	/// 同时，所有自定义属性Room.customProperties应该已经可用。检查Room.playerCount就知道房间里是否有足够的玩家来开始游戏.</remarks>
	public override void OnJoinedRoom()
	{
		RefreshUIViews();

		if (PhotonNetwork.room.PlayerCount == 2)
		{
			if (this.turnManager.Turn == 0)
			{
				// 当房间内有两个玩家,则开始首回合
				this.StartTurn();
			}
		}
		else
		{
			Debug.Log("Waiting for another player");
		}
	}

	/// <summary>
	/// 当一个远程玩家进入房间时调用。这个PhotonPlayer在这个时候已经被添加playerlist玩家列表.
	/// </summary>
	/// <remarks>如果你的游戏开始时就有一定数量的玩家，这个回调在检查Room.playerCount并发现你是否可以开始游戏时会很有用.</remarks>
	/// <param name="newPlayer">New player.</param>
	public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
	{
		Debug.Log("Other player arrived");

		if (PhotonNetwork.room.PlayerCount == 2)
		{
			if (this.turnManager.Turn == 0)
			{

				this.StartTurn();
			}
		}
	}


	/// <summary>
	/// 当一个远程玩家离开房间时调用。这个PhotonPlayer 此时已经从playerlist玩家列表删除.
	/// </summary>
	/// <remarks>当你的客户端调用PhotonNetwork.leaveRoom时，PUN将在现有的客户端上调用此方法。当远程客户端关闭连接或被关闭时，这个回调函数会在经过几秒钟的暂停后被执行.</remarks>
	/// <param name="otherPlayer">Other player.</param>
	public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
	{
		Debug.Log("Other player disconnected! isInactive: " + otherPlayer.IsInactive);
	}

	/// <summary>
	/// 当未知因素导致连接失败（在建立连接之后）时调用，接着调用OnDisconnectedFromPhoton()。
	/// </summary>
	/// <remarks>如果服务器不能一开始就被连接，就会调用OnFailedToConnectToPhoton。错误的原因会以DisconnectCause的形式提供。</remarks>
	/// <param name="cause">Cause.</param>
	public override void OnConnectionFail(DisconnectCause cause)
	{
		this.DisconnectedPanel.gameObject.SetActive(true);
	}



}
