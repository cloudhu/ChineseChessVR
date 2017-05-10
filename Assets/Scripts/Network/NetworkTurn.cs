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

using Photon;
using System.Collections.Generic;
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
    [Tooltip("人机模式AI接口")]
    public GameInterface AI;
	[Tooltip("棋局一回合的时间")]
	public float TurnTime=120f;
	[Tooltip("声源组件")]
    public AudioSource source;
    public enum ChessPlayerType	//棋手类型
    {
        Red,    //红方
        Black,  //黑方
        Guest   //游客
    }
    [Tooltip("玩家类型")]
    public ChessPlayerType localPlayerType = ChessPlayerType.Guest;

	/// <summary>  
	/// 被选中的棋子的ID，若没有被选中的棋子，则ID为-1  
	/// </summary>  
	[Tooltip("被选中的棋子的ID")]
	public int _selectedId = GlobalConst.NOCHESS;  

	[Tooltip("是否是红子的回合,默认红子先行")]
	public bool _isRedTurn=true;

	public struct step	//每步棋的结构
	{
		public int moveId;
		public int killId;

		public float xFrom;
		public float zFrom;
		public float xTo;
		public float zTo;

		public step(int _moveId,int _killId,float _xFrom,float _zFrom,float _xTo,float _zTo){
			moveId = _moveId;
			killId = _killId;
			xFrom =_xFrom;
			zFrom =_zFrom;
			xTo=_xTo;
			zTo = _zTo;
		}
	}
		
	[Tooltip("保存每一步走棋")]
	public List<step> _steps = new List<step> ();

	[Tooltip("音效:胜利,失败...")]
	public AudioClip selectClap,winMusic,loseMusic,welcomMusic,DrawMusic,hurryUp,JoinClip,LeaveClip;

	[Tooltip("德邦总管")]
	public ChessmanManager chessManManager;
    [Tooltip("之前选中的棋子")]
    public GameObject Selected;
    [Tooltip("路径")]
    public GameObject Path;
    static public NetworkTurn Instance;

	public delegate void OnConfirmSelectChessman(int SelectedId);

	public static event OnConfirmSelectChessman OnConfirmedSelect;

	public delegate void OnChessmanDead();

	public static event OnChessmanDead OnPureDead;
    #endregion


    #region Private Variables   //私有变量区域

	[Header("游戏面板")]
	[Tooltip("游戏UI视图")]
	[SerializeField]
	private RectTransform GameUiView;

	[Tooltip("按钮幕布组")]
	[SerializeField]
	private CanvasGroup ButtonCanvasGroup;

	[Tooltip("断连面板")]
	[SerializeField]
	private RectTransform DisconnectedPanel;

	[Tooltip("请求面板")]
	[SerializeField]
	private RectTransform RequestPanel;

	[Header("本地玩家")]
	[Tooltip("本地玩家文本")]
	[SerializeField]
	private Text LocalPlayerNameText;
	[Tooltip("本地玩家时间,得分,回合文本")]
	[SerializeField]
	private Text LocalPlayerTimeText,LocalScoreText,LocalTurnText;
	[Tooltip("本地游戏状态文本")]
	[SerializeField]
	private Text LocalGameStatusText;

	[Header("远程玩家")]
	[Tooltip("远程玩家文本")]
	[SerializeField]
	private Text RemotePlayerNameText;

    [Tooltip("远程玩家时间,得分,回合文本")]
    [SerializeField]
	private Text RemotePlayerTimeText,RemoteScoreText,RemoteTurnText;

    [Tooltip("远程游戏状态文本")]
    [SerializeField]
	private Text RemoteGameStatusText;
	[Header("图片精灵")]
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

    [Tooltip("下一回合")]
    [SerializeField]
    private Sprite Next;

	private ResultType result=ResultType.None;	//结果

	private PunTurnManager turnManager;	//回合管家

	private bool remoteSelection;	//远程玩家选择

	private bool IsShowingResults;	//追踪显示结果的时机来处理游戏逻辑.

    private GameObject instance;

	private PhotonPlayer local;
	private PhotonPlayer remote;
    int nFlag = 1;  //0:人机模式; 2:PVP模式

    #endregion

    public enum ResultType	//结果类型枚举
	{
		None = 0,
		Draw,	//和
		LocalWin,	//赢
		LocalLoss	//输
	}
	#region Mono Callbacks //Unity的回调函数

	public void Start()
	{
		this.turnManager = this.gameObject.AddComponent<PunTurnManager>();	//添加组件并赋值
		this.turnManager.TurnManagerListener = this;	//为监听器赋值,从而触发下面的回调函数来完成游戏逻辑
		this.turnManager.TurnDuration = TurnTime;		//初始化回合持续时间
		if (this.source == null) this.source = FindObjectOfType<AudioSource>();
		Instance = this;

		RefreshUIViews();	//刷新UI视图
	}

	public void Update()
	{
		// 检查我们是否脱离了环境.
		if (this.DisconnectedPanel ==null)
		{
			Destroy(this.gameObject);
		}


		if ( ! PhotonNetwork.inRoom)	//不在房间则退出更新
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


		if (PhotonNetwork.room.PlayerCount>1 || nFlag==0)
		{
			if (this.turnManager.IsOver)
			{
				return;	//回合结束
			}

			if (this.turnManager.Turn > 0  && !IsShowingResults)
			{

				if (_isRedTurn) {
                    if (localPlayerType == ChessPlayerType.Red)
                    {
                        LocalPlayerTimeText.text = this.turnManager.RemainingSecondsInTurn.ToString("F1") + "秒";
                        RemotePlayerTimeText.text = "00:00";
                    }
                    if (localPlayerType == ChessPlayerType.Black)
                    {
                        RemotePlayerTimeText.text = this.turnManager.RemainingSecondsInTurn.ToString("F1") + "秒";
                        LocalPlayerTimeText.text = "00:00";
                    }
                } else {
                    if (localPlayerType == ChessPlayerType.Red)
                    {
                        RemotePlayerTimeText.text = this.turnManager.RemainingSecondsInTurn.ToString("F1") + "秒";
                        LocalPlayerTimeText.text = "00:00";
                    }
                    if (localPlayerType == ChessPlayerType.Black)
                    {
                        LocalPlayerTimeText.text = this.turnManager.RemainingSecondsInTurn.ToString("F1") + "秒";
                        RemotePlayerTimeText.text = "00:00";
                    }

                }
			}
		}

	}

	#endregion

    #region Public Methods	//公共方法区域

	/// <summary>
	/// 尝试移动棋子.
	/// </summary>
	/// <param name="killId">击杀棋子ID.</param>
	/// <param name="x">The x coordinate坐标.</param>
	/// <param name="z">The z coordinate坐标.</param>
    public void TryMoveChessman(int killId, float x, float z)
    {
        if (localPlayerType == ChessPlayerType.Guest) {
            Debug.Log(localPlayerType + "游客只能观看");
            return;   //游客只能观看
        }
        if (killId == GlobalConst.NOCHESS)
        {
            if (_selectedId >= 16)    //黑子无法被红方或红色回合内选定
            {
                if (localPlayerType == ChessPlayerType.Red || _isRedTurn)
                {
                    Debug.Log(_selectedId + "黑子无法被红方或红色回合内选定");
                    return;
                }
            }
            else    //红子同样无法被其他阵营选定
            {
                if (localPlayerType == ChessPlayerType.Black || !_isRedTurn)
                {
                    Debug.Log(_selectedId + "红子同样无法被其他阵营选定");
                    return;
                }
            }
        }


		bool ret = IsValidMove(_selectedId, killId,x,z);

        if (ret)
        {
			MovingChessman(_selectedId, killId, x,z);
            OnMoveChessman(_selectedId, killId, x, z);
        }
    }

	/// <summary>
	/// 判断输赢.
	/// </summary>
    public void Judge(){
		
        if (ChessmanManager.chessman[GlobalConst.R_KING]._dead)  //红方：帅死则输，将死则赢
        {
			if (localPlayerType == ChessPlayerType.Red) {
				result = ResultType.LocalLoss;
				PlayMusic (loseMusic);
			}
			if (localPlayerType == ChessPlayerType.Black) {
				result = ResultType.LocalWin;
				PlayMusic(winMusic);
			}
            LocalGameStatusText.text = "恭喜黑方获胜！Black Win！";
        }

        if (ChessmanManager.chessman[GlobalConst.B_KING]._dead)
        {
			if (localPlayerType == ChessPlayerType.Black) {
				result = ResultType.LocalLoss;
				PlayMusic (loseMusic);
			}
			if (localPlayerType == ChessPlayerType.Red) {
				result = ResultType.LocalWin;
				PlayMusic(winMusic);
			}
            LocalGameStatusText.text = "恭喜红方获胜！Red Win！";
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
        CancelSelected(_selectedId);
        this.LocalTurnText.text = (this.turnManager.Turn).ToString();	//更新回合数
		RemoteTurnText.text = LocalTurnText.text;
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

		this.Judge();	//计算输赢
		this.UpdateScores();	//更新得分
		this.OnEndTurn();	//结束回合
	}


	/// <summary>
	/// 当玩家有动作时调用(但是没有完成该回合)
	/// </summary>
	/// <param name="player">玩家引用</param>
	/// <param name="turn">回合索引</param>
	/// <param name="move">移动对象数据</param>
	/// <param name="photonPlayer">Photon player.</param>
	public void OnPlayerMove(PhotonPlayer photonPlayer, int turn, object move)
	{
		//Debug.Log("OnPlayerMove: " + photonPlayer + " turn: " + turn + " action: " + move);
		string strMove = move.ToString ();
		if (strMove.StartsWith ("+C")) {
			
			if (!photonPlayer.IsLocal) {
				string[] strArr = strMove.Split (char.Parse (" "));
				if (strArr [0] == "+ConfirmedSelect") {				
					ConfirmedSelect (int.Parse (strArr [1]));

				} else {
					CancelSelected (int.Parse (strArr [1]));
				}
			}
		} else {
			switch (strMove) {
			case "Hurry":	//催棋
				PlayMusic (hurryUp);
				break;
			case "Restart":
				if (!photonPlayer.IsLocal)
					PopRequest ("重新开始对局");
				break;
			case "Draw":
				if (!photonPlayer.IsLocal)
					PopRequest ("和棋");
				break;
			case "Back":
				if (!photonPlayer.IsLocal)
					PopRequest ("悔棋");
				break;
			case "重新开始对局Yes":
				Restart ();
				break;
			case "重新开始对局No":
				LocalGameStatusText.text = "重新开局失败";
				break;
			case "和棋Yes":
				turnManager.SendMove ("Draw", true);
				break;
			case "和棋No":
				LocalGameStatusText.text = "和棋失败";
				break;
			case "悔棋Yes":
				BackOne ();
				break;
			case "悔棋No":
				LocalGameStatusText.text = "悔棋失败";
				break;
			}
		}

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
		//Debug.Log("OnTurnFinished: " + photonPlayer + " turn: " + turn + " action: " + move);
		string tmpStr = move.ToString ();
		if (tmpStr.Contains("_"))
        {
            if (!photonPlayer.IsLocal)
            {
				string[] strArr = tmpStr.Split(char.Parse("_"));
                MovingChessman(int.Parse(strArr[0]), int.Parse(strArr[1]), float.Parse(strArr[4]),float.Parse(strArr[5]));
            }
        }
        else
        {
            switch (tmpStr)
            {
                case "BlackDefeat":
                    RemoteGameStatusText.text = "黑方认输！";
                    if (localPlayerType == ChessPlayerType.Red)
                    {
                        result = ResultType.LocalWin;
                    }
                    break;
                case "RedDefeat":
                    RemoteGameStatusText.text = "红方认输！";
                    if (localPlayerType == ChessPlayerType.Black)
                    {
                        result = ResultType.LocalWin;
                    }
                    break;
                case "Draw":
                    result = ResultType.Draw;
                    break;
                default:
                    break;
            }
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
			//Debug.Log("OnTurnTimeEnds: Calling OnTurnCompleted");
			if (_isRedTurn) {
				
				//红方超时，判输
				if (localPlayerType==ChessPlayerType.Red) {
					result = ResultType.LocalLoss;
					LocalGameStatusText.text = "您已超时，判负！";
					RemoteGameStatusText.text = "胜利！";
				}
				if (localPlayerType==ChessPlayerType.Black) {
					result = ResultType.LocalWin;
					LocalGameStatusText.text = "对方超时，胜利！";
					RemoteGameStatusText.text = "失败！";
				}

			} else {
				//黑方超时，判输
				if (localPlayerType==ChessPlayerType.Red) {
					result = ResultType.LocalWin;
					LocalGameStatusText.text = "对方超时，胜利！";
					RemoteGameStatusText.text = "失败！";
				}
				if (localPlayerType==ChessPlayerType.Black) {
					result = ResultType.LocalLoss;
					LocalGameStatusText.text = "您已超时，判负！";
					RemoteGameStatusText.text = "胜利！";
				}
			}
				
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
            UpdatePlayerScoreTexts();
        }
	}
	#endregion

	#region Core Gameplay Methods	//核心玩法


	/// <summary>调用来开始回合 (只有主客户端会发送).</summary>
	public void StartTurn()
	{
		if (PhotonNetwork.isMasterClient)
		{
			this.turnManager.BeginTurn();
		}
        this.turnManager.isTurnStarted = true;

        if (_isRedTurn) {

			if (localPlayerType==ChessPlayerType.Red) {
				LocalGameStatusText.text = "您的回合开始！";
				RemoteGameStatusText.text = "等待...";
				return;
			}
			if (localPlayerType==ChessPlayerType.Black) {
				LocalGameStatusText.text = "等待对方走棋！";
				RemoteGameStatusText.text = "思考中...";
			}

		} else {
			if (localPlayerType==ChessPlayerType.Red) {
				LocalGameStatusText.text = "等待对方走棋！";
				RemoteGameStatusText.text = "思考中...";
				return;
			}
			if (localPlayerType==ChessPlayerType.Black) {
				LocalGameStatusText.text = "您的回合开始！";
				RemoteGameStatusText.text = "等待...";
			}
		}
    }


    /// <summary>
    /// 行棋
    /// </summary>
    /// <param name="moveId">选中的棋子</param>
    /// <param name="killId">击杀的棋子</param>
    /// <param name="targetPosition">目标位置</param>
	public void MovingChessman(int moveId, int killId,float x,float z)
    {
        //Debug.Log("红方回合:" + _isRedTurn + "++移动棋子ID:" + moveId + "++目标id:" +killId+"++目标x: "+x+"++目标z:"+z);
        float _x = ChessmanManager.chessman[moveId]._x;
        float _z = ChessmanManager.chessman[moveId]._z;
        // 0.保存记录到列表
        SaveStep(moveId, killId, x, z);
		// 1.若移动到的位置上有棋子，将其吃掉  
        KillChessman(killId);
		// 2.将移动棋子的路径显示出来  
		ShowPath(new Vector3(_x, 1f,_z), x,z);
		// 3.将棋子移动到目标位置  
		MoveChessman(moveId, x,z,killId);
        if (killId==GlobalConst.B_KING || killId==GlobalConst.R_KING)
        {
            return;
        }
        //换算成AI棋盘矩阵的坐标
        _x = _x == 0 ? 0 : _x / 3;
        x = x == 0 ? 0 : x / 3;
        _z = _z == 0 ? 0 : _z / 3;
        z = z == 0 ? 0 : z / 3;

        AI.OnMoveChessman(nFlag, (int)_x, (int)_z,(int)x, (int)z);
        
    }

    /// <summary>
    /// 回合结束时调用
    /// </summary>
    public void OnEndTurn()
	{
		ButtonCanvasGroup.interactable = false;	//禁用按钮交互
		IsShowingResults = true;
		this.turnManager.isTurnStarted = false;
		switch (result) //根据结果展示不同的图片
		{
		case ResultType.None:
			this.StartTurn();
			break;
		case ResultType.Draw:
			this.WinOrLossImage.sprite = this.SpriteDraw;
			break;
		case ResultType.LocalWin:
			this.WinOrLossImage.sprite = this.SpriteWin;
			break;
		case ResultType.LocalLoss:
			this.WinOrLossImage.sprite = this.SpriteLose;
			break;
		}
		if (result!=ResultType.None) {
			this.WinOrLossImage.gameObject.SetActive(true);
            if (nFlag == 2)
            {
                ButtonCanvasGroup.interactable = true;
            }
		}

	}
		

	/// <summary>
	/// 结束游戏
	/// </summary>
	public void EndGame()
	{
		Debug.Log("EndGame");
		Application.Quit ();
	}

    /// <summary>  
    /// 悔棋，退回一步  
    /// </summary>  
    public void BackOne()
    {
        if (_steps.Count == 0) return;

        step tmpStep = _steps[_steps.Count - 1];
        _steps.RemoveAt(_steps.Count - 1);
        Back(tmpStep);
    }

    #endregion

    
    #region Handling Of Buttons	//处理按钮

    /// <summary>
    /// 离线模式
    /// </summary>
    public void OfflineModeAct(int nPly)
    {
        HidePath();
        chessManManager.hidePointer();
        _isRedTurn = true;
        PhotonNetwork.offlineMode = true;
        nFlag = 0;
        PlayMusic(welcomMusic);
        AI.OnNewGame(1, nPly);
        chessManManager.ChessmanInit();
        LocalGameStatusText.text = "开局！";
        RemoteGameStatusText.text = "开局！";
        localPlayerType = ChessPlayerType.Red;
        LocalGameStatusText.text = "您是红方棋手……";
        if (PlayerPrefs.HasKey("PlayerName"))
        {
            this.LocalPlayerNameText.text =  PlayerPrefs.GetString("PlayerName") + ":红方";
        }
        RemotePlayerNameText.text = "AI:黑方";
        UpdatePlayerScoreTexts();
        this.StartTurn();
    }

    /// <summary>
    /// 发送消息给对方
    /// </summary>
    /// <param name="massage"></param>
	public void SendMassage(string massage){
		this.turnManager.SendMove(massage, false);
		PlayMusic(selectClap);
	}


	/// <summary>
	/// 同意.
	/// </summary>
	/// <param name="t">T.</param>
	public void OnAgree(Text t){
		this.turnManager.SendMove(t.text+"Yes", false);
		PlayMusic(selectClap);
		RequestPanel.gameObject.SetActive (false);
	}

	/// <summary>
	/// 拒绝.
	/// </summary>
	/// <param name="t">T.</param>
	public void OnDisagree(Text t){
		this.turnManager.SendMove(t.text+"No", false);
		PlayMusic(selectClap);
		RequestPanel.gameObject.SetActive (false);
	}

	/// <summary>
	/// 认输.
	/// </summary>
	public void OnDefeat(){
		result = ResultType.LocalLoss;
		PlayMusic(selectClap);
		if (localPlayerType==ChessPlayerType.Black) {
			this.turnManager.SendMove("BlackDefeat", true);
		}
		if (localPlayerType==ChessPlayerType.Red) {
			this.turnManager.SendMove("RedDefeat", true);
		}
	}
		
    public void OnCancelSelected(int targetId)
    {
		this.turnManager.SendMove("+CancelSelected "+targetId.ToString(), false);

		CancelSelected (targetId);
    }
		

    public void OnSelectChessman(int selectId,float x,float z)
    {
		if (this.turnManager.isTurnStarted) {
            if (_selectedId == GlobalConst.NOCHESS)
	        {
                //Debug.Log(selectId + "  OnSelectChessman->TrySelect");
                TrySelectChessman(selectId);
	        }
	        else
	        {
                //Debug.Log(selectId+ "  OnSelectChessman->trymove");
	            TryMoveChessman(selectId, x, z);
	        }
		}
    }

    /// <summary>
    /// 连接
    /// </summary>
    public void OnClickConnect()
	{
		PlayMusic(selectClap);
		PhotonNetwork.ConnectUsingSettings(null);
		PhotonHandler.StopFallbackSendAckThread();  // 这在案例中被用于后台超时!
	}

	/// <summary>
	/// 重新连接并重新加入
	/// </summary>
	public void OnClickReConnectAndRejoin()
	{
		PlayMusic(selectClap);
		PhotonNetwork.ReconnectAndRejoin();
		PhotonHandler.StopFallbackSendAckThread();  // this is used in the demo to timeout in background!
	}

    #endregion

    #region Call PositionManager //调用棋子规则


    /// <summary>  
    /// 判断走棋是否符合走棋的规则  
    /// </summary>  
    /// <param name="selectedId">选中的棋子</param>  
    /// <param name="killId">击杀的棋子</param>
    /// <param name="targetPosition">目标位置</param>
    /// <returns></returns>
	bool IsValidMove(int moveId, int killId,float x,float z)
    {
        //Debug.Log(moveId + "CanMove");
        if (killId!= GlobalConst.NOCHESS) {    //如果是同阵营的棋子，则取消原来的选择，选择新的棋子
			if (SameColor (moveId, killId)) {
				OnCancelSelected (moveId);
				TrySelectChessman (killId);
                //Debug.Log(killId + "SameColor");
				return false;
			}
            return isObstacle(killId);
        }
			
        return true;
    }

    /// <summary>  
    /// 判断点击的棋子是否可以被选中，即点击的棋子是否在它可以移动的回合  
    /// </summary>  
    /// <param name="id">棋子ID</param>  
    /// <returns></returns>  
    bool CanSelect(int id)
    {
        return _isRedTurn == ChessmanManager.chessman[id]._red;
    }

	/// <summary>
	/// Is the obstacle. | 判断目标棋子是否可以击杀，通过障碍来判断，如果该目标是障碍中的一员，则可以击杀
	/// </summary>
	/// <returns><c>true</c>, if obstacle was ised, <c>false</c> otherwise.</returns>
	/// <param name="killId">Kill identifier.</param>
	bool isObstacle(int killId){
		if (chessManManager.DetectedObstacles.Count>0) {
			for (int i = 0; i < chessManManager.DetectedObstacles.Count; i++) {
				if (chessManManager.DetectedObstacles[i].name==killId.ToString()) {
					return true;
				}
			}
		}
		return false;
	}
	 
    #endregion

    #region PUN Callbacks   //重新PUN回调函数


    /// <summary>
    /// 当本地用户/客户离开房间时调用。
    /// </summary>
    /// <remarks>当离开一个房间时，PUN将你带回主服务器。
    /// 在您可以使用游戏大厅和创建/加入房间之前，OnJoinedLobby()或OnConnectedToMaster()会再次被调用。</remarks>
    public override void OnLeftRoom()
    {
        Debug.Log("OnLeftRoom()");
		PlayMusic (LeaveClip);
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
        local = PhotonNetwork.player;
		remote = PhotonNetwork.player.GetNext();
        if (PhotonNetwork.isMasterClient && local != null)
        {

            localPlayerType = ChessPlayerType.Red;
            LocalGameStatusText.text = "您是红方棋手……";
            this.LocalPlayerNameText.text = local.NickName + ":红方";
            ExitGames.Client.Photon.Hashtable playerType = new ExitGames.Client.Photon.Hashtable();
            playerType.Add("playerType", "红方选手");
            local.SetCustomProperties(playerType, null, false);
        }
        else
        {
            this.LocalPlayerNameText.text = local.NickName + ":黑方";
            ExitGames.Client.Photon.Hashtable playerType = new ExitGames.Client.Photon.Hashtable();
            playerType.Add("playerType", "黑方选手");
            local.SetCustomProperties(playerType, null, false);
            localPlayerType = ChessPlayerType.Black;
            LocalGameStatusText.text = "您是黑方棋手……";
        }

        if (PhotonNetwork.room.PlayerCount == 2 && this.turnManager.Turn == 0)
        {
             // 当房间内有两个玩家,则开始首回合
			Play();
        }

		if (PhotonNetwork.room.PlayerCount > 2)
        {
			localPlayerType = ChessPlayerType.Guest;
			LocalGameStatusText.text = "棋局已开始，正在进入观棋模式……";
        }

		if (remote != null)
		{
			RemoteGameStatusText.text = "已匹配！";
			// 应该是这种格式: "name        00"
			if (PhotonNetwork.isMasterClient) {
				this.RemotePlayerNameText.text = remote.NickName + "—黑方";
			} else {
				this.RemotePlayerNameText.text = remote.NickName + "—红方" ;
			}
		}
		else
		{
			this.RemotePlayerNameText.text = "匹配...";
			LocalGameStatusText.text="等待其他玩家...";
			RemoteGameStatusText.text = "正在匹配...";
		}

		RefreshUIViews();
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

	/// <summary>
	/// 当一个远程玩家进入房间时调用。这个PhotonPlayer在这个时候已经被添加playerlist玩家列表.
	/// </summary>
	/// <remarks>如果你的游戏开始时就有一定数量的玩家，这个回调在检查Room.playerCount并发现你是否可以开始游戏时会很有用.</remarks>
	/// <param name="newPlayer">New player.</param>
	public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
	{
		//Debug.Log("Other player arrived");
		LocalGameStatusText.text = "欢迎"+newPlayer.NickName+"加入游戏！";
		PlayMusic (JoinClip);
		if (PhotonNetwork.room.PlayerCount == 2 && this.turnManager.Turn == 0)
		{
            // when the room has two players, start the first turn (later on, joining players won't trigger a turn)
			Play();
        
        }
    }

	/// <summary>
	/// 当一个远程玩家离开房间时调用。这个PhotonPlayer 此时已经从playerlist玩家列表删除.
	/// </summary>
	/// <remarks>当你的客户端调用PhotonNetwork.leaveRoom时，PUN将在现有的客户端上调用此方法。当远程客户端关闭连接或被关闭时，这个回调函数会在经过几秒钟的暂停后被执行.</remarks>
	/// <param name="otherPlayer">Other player.</param>
	public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
	{
		PlayMusic (LeaveClip);
		//Debug.Log("Other player disconnected! isInactive: " + otherPlayer.IsInactive);
		LocalGameStatusText.text ="玩家"+ otherPlayer.NickName+"已离开游戏";
        if (!otherPlayer.IsLocal)
        {
            result = ResultType.LocalWin;
            UpdateScores();
        }
        Restart();
	}
    #endregion


	#region Private Methods //私有方法

	/// <summary>
	/// Play this instance.初始化棋子和地图并开始回合
	/// </summary>
	void Play(){
        nFlag = 2;
		PlayMusic (welcomMusic);
		chessManManager.ChessmanInit ();
		LocalGameStatusText.text = "开局！";
		RemoteGameStatusText.text = "开局！";
		UpdatePlayerScoreTexts();
		this.StartTurn();
	}

	/// <summary>
	/// 播放指定音效.
	/// </summary>
	/// <param name="targetAudio">目标音效</param>
	void PlayMusic(AudioClip targetAudio)
	{

		if (targetAudio != null && !source.isPlaying)
		{
			this.source.PlayOneShot(targetAudio);
		}
	}

	/// <summary>
	/// 更新玩家文本信息
	/// </summary>
	private void UpdatePlayerScoreTexts()
	{
        if (remote != null)
		{
			// 应该是这种格式: "00"
            RemoteScoreText.text= remote.GetScore().ToString("D2");
		}

		if (local != null)
		{
            LocalScoreText.text= local.GetScore().ToString("D2");
		}
	}

	void MoveError(int moveId, float x,float z)
	{
		GameObject chessman = chessManManager.transform.FindChild(moveId.ToString()).gameObject;
		Vector3 oldPosition = new Vector3(ChessmanManager.chessman[moveId]._x, 1f, ChessmanManager.chessman[moveId]._z);
		HidePath ();
		ShowPath (oldPosition,x,z);
		LocalGameStatusText.text = "MoveError:"+chessman.name+"不能移动到目标位置:"+x;
	}

	void TrySelectChessman(int selectId)
	{
		if (selectId== GlobalConst.NOCHESS)
		{
			return;
		}

        if (localPlayerType == ChessPlayerType.Guest)
        {
            Debug.Log(localPlayerType + "游客只能观看");
            return;   //游客只能观看
        }

        if (selectId >= 16)    //黑子无法被红方或红色回合内选定
        {
            if (localPlayerType == ChessPlayerType.Red || _isRedTurn)
            {
                Debug.Log(selectId + "黑子无法被红方或红色回合内选定");
                return;
            }
        }
        else    //红子同样无法被其他阵营选定
        {
            if (localPlayerType == ChessPlayerType.Black || !_isRedTurn)
            {
                Debug.Log(selectId + "红子同样无法被其他阵营选定");
                return;
            }
        }

        if (!CanSelect(selectId)) return;
		this.turnManager.SendMove ("+ConfirmedSelect "+selectId.ToString(),false);
		ConfirmedSelect (selectId);
	}

	void ConfirmedSelect(int SelectedId){
		_selectedId = SelectedId;
		OnConfirmedSelect (SelectedId);
		ChessmanManager.chessman [SelectedId].go.GetComponent<ChessmanController> ().SelectedChessman ();
		HidePath ();
	}

	/// <summary>  
	/// 设置棋子死亡  
	/// </summary>  
	/// <param name="id"></param>  
	void KillChessman(int id)
	{
		if (id == GlobalConst.NOCHESS) return;
		if (_isRedTurn && localPlayerType==ChessPlayerType.Black) {	//红方回合被击杀的必然是黑方减分
			OnPureDead();
		}
		if (!_isRedTurn && localPlayerType==ChessPlayerType.Red) {
			OnPureDead();
		}
		ChessmanManager.chessman[id]._dead = true;
		ChessmanManager.chessman[id].go.GetComponent<ChessmanController> ().SwitchDead ();
	}

	/// <summary>  
	/// 复活棋子  
	/// </summary>  
	/// <param name="id"></param>  
	void ReliveChess(int id)
	{
		if (id == GlobalConst.NOCHESS) return;

		//因GameObject.Find();函数不能找到active==false的物体，故先找到其父物体，再找到其子物体才可以找到active==false的物体  
		ChessmanManager.chessman[id]._dead = false;
		ChessmanManager.chessman [id].go.SetActive (true);
		ChessMap.chessman [id].go.SetActive (true);
	}

	/// <summary>  
	/// 将移动的棋子ID、吃掉的棋子ID以及棋子从A点的坐标移动到B点的坐标都记录下来  
	/// </summary>  
	/// <param name="moveId">选中的棋子ID</param>  
	/// <param name="killId">击杀的棋子ID</param>  
	/// <param name="toX">目标X坐标</param>  
	/// <param name="toZ">目标Z坐标</param>  
	void SaveStep(int moveId, int killId, float toX, float toZ)
	{
		step tmpStep = new step();
		//当前棋子的位置
		float fromX = ChessmanManager.chessman[moveId]._x;
		float fromZ = ChessmanManager.chessman[moveId]._z;

		tmpStep.moveId = moveId;
		tmpStep.killId = killId;
		tmpStep.xFrom = fromX;
		tmpStep.zFrom = fromZ;
		tmpStep.xTo = toX;
		tmpStep.zTo = toZ;

		_steps.Add(tmpStep);

	}

	/// <summary>  
	/// 设置上一步棋子走过的路径，即将上一步行动的棋子的位置留下标识，并标识该棋子  
	/// </summary>  
	void ShowPath(Vector3 oldPosition, float x,float z)
	{
		Vector3 newPosition = new Vector3 (x, 0.57f, z);
		if (!Selected.activeSelf) {
			Selected.SetActive(true);
            Selected.transform.localPosition = newPosition;
        }

		if (!Path.activeSelf) {
			Path.SetActive(true);
            Path.transform.localPosition = oldPosition;
        }

	}

	/// <summary>  
	/// 隐藏路径  
	/// </summary>  
	void HidePath()
	{
		if (Selected.activeSelf)
		{
			Selected.SetActive(false);
			Path.SetActive(false);
		}

	}

	/// <summary>  
	/// 移动棋子到目标位置  
	/// </summary>  
	/// <param name="targetPosition">目标位置</param>  
	void MoveChessman(int moveId,float x,float z,int killId)
	{
		ChessmanManager.chessman[moveId].go.GetComponent<ChessmanController>().SetTarget(x,z,killId);
		_isRedTurn = !_isRedTurn;
	}

	/// <summary>  
	/// 通过记录的步骤结构体来返回上一步  
	/// </summary>  
	/// <param name="_step"></param>  
	void Back(step _step)
	{
		if (_step.killId != GlobalConst.NOCHESS) {
			ReliveChess (_step.killId);
		}
		MoveChessman(_step.moveId, _step.xFrom, _step.zFrom, GlobalConst.NOCHESS);

		HidePath();
		if (_selectedId != GlobalConst.NOCHESS)
		{      
			_selectedId = GlobalConst.NOCHESS;
		}
	}

	/// <summary>
	/// 正在移动棋子.
	/// </summary>
	/// <param name="selectedId">选择的棋子ID.</param>
	/// <param name="killId">要击杀的棋子ID.</param>
	/// <param name="toX">目标To x.</param>
	/// <param name="toZ">目标To z.</param>
	void OnMoveChessman(int selectedId,int killId,float toX,float toZ)
	{
		float fromX = ChessmanManager.chessman[selectedId]._x;
		float fromZ = ChessmanManager.chessman[selectedId]._z;
		string tmpStr = selectedId.ToString() +"_"+ killId.ToString()+"_"+fromX.ToString()+"_"+fromZ.ToString()+"_"+toX.ToString()+"_"+toZ.ToString();
		this.turnManager.SendMove(tmpStr, true);	//弃用step结构体来传递信息的原因是Photon不能序列化,所以采用字符串来同步信息
	}

	/// <summary>
	/// 刷新UI视图
	/// </summary>
	void RefreshUIViews()
	{
		GameUiView.gameObject.SetActive(PhotonNetwork.inRoom);
		ButtonCanvasGroup.interactable = PhotonNetwork.room != null ? PhotonNetwork.room.PlayerCount > 1 : false;
	}

	bool IsRed(int id)
	{
		return ChessmanManager.chessman[id]._red;
	}

	bool IsDead(int id)
	{
		if (id == GlobalConst.NOCHESS) return true;
		return ChessmanManager.chessman[id]._dead;
	}

	bool SameColor(int id1, int id2)
	{
		if (id1 == GlobalConst.NOCHESS || id2 == GlobalConst.NOCHESS) return false;

		return IsRed(id1) == IsRed(id2);
	}

	void PopRequest(string title){
		if (RequestPanel == null && RequestPanel.gameObject.activeSelf) {
			return;
		} else {
			RequestPanel.gameObject.SetActive (true);
			RequestPanel.transform.FindChild ("Title/Text").GetComponent<Text> ().text=title;
		}

	}

	void Restart(){
        if (nFlag==0)   //人机模式
        {
            OfflineModeAct(3);
            return;
        }
		if (PhotonNetwork.isMasterClient && PhotonNetwork.room.PlayerCount == 2 ) {
			this.turnManager.RestartTurn ();
		}
        this.turnManager.isTurnStarted = true;
        _isRedTurn = true;
        chessManManager.ChessmanInit();
	}

	void CancelSelected(int cancelId){
		if (_selectedId==cancelId)
		{
			_selectedId = GlobalConst.NOCHESS;
            OnConfirmedSelect(GlobalConst.NOCHESS);
        }
	}

	#endregion


}
