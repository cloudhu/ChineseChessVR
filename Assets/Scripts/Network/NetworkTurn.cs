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
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using VRTK;

/// <summary>
/// FileName: NetworkTurn.cs
/// Author: 胡良云（CloudHu）
/// Corporation: 
/// Description: 这个脚本用于处理网络回合
/// DateTime: 3/22/2017
/// </summary>
public class NetworkTurn : PunBehaviour, IPunTurnManagerCallbacks {
	
	#region Public Variables  //公共变量区域
	[Tooltip("棋局一回合的时间")]
	public float TurnTime=120f;
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
	public int _selectedId=-1;  

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

	[Tooltip("选中的音效,胜利，失败的音乐")]
	public AudioClip selectClap,winMusic,loseMusic,welcomMusic,DrawMusic,hurryUp,JoinClip,LeaveClip;

	[Tooltip("德邦总管")]
	public ChessmanManager chessManManager;
    [Tooltip("之前选中的棋子")]
    public GameObject Selected;
    [Tooltip("路径")]
    public GameObject Path;
    [Tooltip("棋盘总管")]
    public BoardManager boardManager;

    static public NetworkTurn Instance;
    #endregion


    #region Private Variables   //私有变量区域

	//本地玩家选择
	private bool localSelection;
	//远程玩家选择
	private bool remoteSelection;

    // 追踪显示结果的时机来处理游戏逻辑.
    private bool IsShowingResults;
	

	[Tooltip("语音UI视图")]
	[SerializeField]
	private RectTransform VoiceUiView;

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

    [Tooltip("游戏状态文本")]
    [SerializeField]
    private Text GameStatusText;

	[Tooltip("游戏提示信息")]
	[SerializeField]
	private Tips Tip;

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

    [Tooltip("断连面板")]
	[SerializeField]
	private RectTransform DisconnectedPanel;

	[Tooltip("请求面板")]
	[SerializeField]
	private RectTransform RequestPanel;

	private ResultType result;//结果

	private PunTurnManager turnManager;//回合管家

    private GameObject instance;

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


		if (PhotonNetwork.room.PlayerCount>1)
		{
			if (this.turnManager.IsOver)
			{
				return;	//回合结束
			}

			if (this.TurnText != null)
			{
				this.TurnText.text = (this.turnManager.Turn*0.5f).ToString();	//更新回合数
			}

			if (this.turnManager.Turn > 0 && this.TimeText != null && ! IsShowingResults)
			{
				float leftTime = this.turnManager.RemainingSecondsInTurn;


				TimerFillImage.anchorMax = new Vector2(1f- leftTime/TurnTime,1f);

				if (leftTime==0) {	//超时判负
					if (localPlayerType==ChessPlayerType.Red) {
						result = ResultType.LocalLoss;
					}
					if (localPlayerType==ChessPlayerType.Black) {
						result = ResultType.LocalWin;
					}
					if (_isRedTurn) {
						//红方超时，判输
						this.turnManager.SendMove ("RedTimeOut",true);
						return;
					} else {
						//黑方超时，判输
						this.turnManager.SendMove ("BlackTimeOut",true);
						return;
					}
				}
				if (_isRedTurn) {
					Tip.UpdateText ("红方剩余时间：" +leftTime.ToString("F1") +"秒", "1");
					this.TimeText.text = "红方本回合剩余时间："+leftTime.ToString("F1") + " 秒";	//更新回合剩余时间
				} else {
					this.TimeText.text = "黑方本回合剩余时间："+leftTime.ToString("F1") + " 秒";	//更新回合剩余时间
					Tip.UpdateText ("黑方剩余时间：" +leftTime.ToString("F1") +"秒", "1");
				}
			}


		}

		this.UpdatePlayerTexts();	//更新玩家文本信息


	}

	#endregion

    #region Public Methods	//公共方法区域

	/// <summary>
	/// 尝试移动棋子.
	/// </summary>
	/// <param name="killId">击杀棋子ID.</param>
	/// <param name="x">The x coordinate坐标.</param>
	/// <param name="z">The z coordinate坐标.</param>
    public void TryMoveChessman(int killId, float x, float z,int occupyId)
    {
        if (killId != -1 && SameColor(killId, _selectedId))
        {
            TrySelectChessman(killId);
            return;
        }

        bool ret = CanMove(_selectedId, killId, new Vector3(x, 1f, z));

        if (ret)
        {
            MoveStone(_selectedId, killId, new Vector3(x, 1f, z),occupyId);
            OnMoveChessman(_selectedId, killId, x, z,occupyId);
        }
        else
        {
            MoveError(_selectedId, new Vector3(x, 1f, z));
        }
    }

	/// <summary>
	/// 判断输赢.
	/// </summary>
    public void Judge(){
		
        if (ChessmanManager.chessman[0]._dead)  //红方：帅死则输，将死则赢
        {
			if (localPlayerType == ChessPlayerType.Red) {
				result = ResultType.LocalLoss;
				PlayMusic (loseMusic);
			}
			if (localPlayerType == ChessPlayerType.Black) {
				result = ResultType.LocalWin;
				PlayMusic(winMusic);
			}
            GameStatusText.text = "恭喜黑方获胜！Black Win！";
        }

        if (ChessmanManager.chessman[16]._dead)
        {
			if (localPlayerType == ChessPlayerType.Black) {
				result = ResultType.LocalLoss;
				PlayMusic (loseMusic);
			}
			if (localPlayerType == ChessPlayerType.Red) {
				result = ResultType.LocalWin;
				PlayMusic(winMusic);
			}
            GameStatusText.text = "恭喜红方获胜！Red Win！";
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
        _selectedId = -1;
		localSelection = false;
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
	/// 当玩家有动作时调用(但是没有完成该回合)
	/// </summary>
	/// <param name="player">玩家引用</param>
	/// <param name="turn">回合索引</param>
	/// <param name="move">移动对象数据</param>
	/// <param name="photonPlayer">Photon player.</param>
	public void OnPlayerMove(PhotonPlayer photonPlayer, int turn, object move)
	{
		Debug.Log("OnPlayerMove: " + photonPlayer + " turn: " + turn + " action: " + move);
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
				else
					localSelection = true;
				break;
			case "Draw":
				if (!photonPlayer.IsLocal)
					PopRequest ("和棋");
				else
					localSelection = true;
				break;
			case "Back":
				if (!photonPlayer.IsLocal)
					PopRequest ("悔棋");
				else
					localSelection = true;
				break;
			case "重新开始对局Yes":
				Restart ();
				break;
			case "重新开始对局No":
				GameStatusText.text = "重新开局失败";
				break;
			case "和棋Yes":
				turnManager.SendMove ("Draw", true);
				break;
			case "和棋No":
				GameStatusText.text = "和棋失败";
				break;
			case "悔棋Yes":
				BackOne ();
				break;
			case "悔棋No":
				GameStatusText.text = "悔棋失败";
				break;
			default:
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
		Debug.Log("OnTurnFinished: " + photonPlayer + " turn: " + turn + " action: " + move);
		string tmpStr = move.ToString ();
        if (tmpStr.Contains("s"))
        {
            if (!photonPlayer.IsLocal)
            {
                string[] strArr = tmpStr.Split(char.Parse("s"));
                MoveStone(int.Parse(strArr[0]), int.Parse(strArr[1]), new Vector3(float.Parse(strArr[4]), 1f, float.Parse(strArr[5])),int.Parse(strArr[6]));
            }
        }
        else
        {
            switch (tmpStr)
            {
                case "RedTimeOut":
                    Tip.UpdateText("红方超时,判输！", "0");
                    break;
                case "BlackTimeOut":
                    Tip.UpdateText("黑方超时,判输！", "0");
                    break;
                case "BlackDefeat":
                    Tip.UpdateText("黑方认输！", "0");
                    if (localPlayerType == ChessPlayerType.Red)
                    {
                        result = ResultType.LocalWin;
                    }
                    break;
                case "RedDefeat":
                    Tip.UpdateText("红方认输！", "0");
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


	/// <summary>调用来开始回合 (只有主客户端会发送).</summary>
	public void StartTurn()
	{
		if (PhotonNetwork.isMasterClient)
		{
			this.turnManager.BeginTurn();
		}

		if (_isRedTurn) {
			GameStatusText.text = "请红方走棋！触摸棋子按下扳机即可 (*^__^*)";
		} else {
			GameStatusText.text = "请黑方走棋！触摸棋子按下扳机即可 (*^__^*)";
		}
    }


    /// <summary>
    /// 行棋
    /// </summary>
    /// <param name="moveId">选中的棋子</param>
    /// <param name="killId">击杀的棋子</param>
    /// <param name="targetPosition">目标位置</param>
    public void MoveStone(int moveId, int killId, Vector3 targetPosition,int occupyId)
    {
        boardManager.leavePoint(ChessmanManager.chessman[moveId]._x, ChessmanManager.chessman[moveId]._z);
        boardManager.occupyPoint(occupyId);
        // 0.保存记录到列表
        SaveStep(moveId, killId, targetPosition.x, targetPosition.z);
		// 1.若移动到的位置上有棋子，将其吃掉  
        KillChessman(killId);
		// 2.将移动棋子的路径显示出来  
        ShowPath(new Vector3(ChessmanManager.chessman[moveId]._x, 1f, ChessmanManager.chessman[moveId]._z), targetPosition);
		// 3.将棋子移动到目标位置  
        MoveChessman(moveId, targetPosition);
        
    }

    /// <summary>
    /// 回合结束时调用
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

        switch (result) //根据结果展示不同的图片
        {
            case ResultType.None:
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
            default:
				
                break;
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

	public void SendMassage(string massage){
		this.turnManager.SendMove(massage, false);
		PlayMusic(selectClap);
	}


	/// <summary>
	/// 同意.
	/// </summary>
	/// <param name="t">T.</param>
	public void OnAgree(Text t){
		localSelection = true;
		this.turnManager.SendMove(t.text+"Yes", false);
		PlayMusic(selectClap);
	}

	/// <summary>
	/// 拒绝.
	/// </summary>
	/// <param name="t">T.</param>
	public void OnDisagree(Text t){
		localSelection = false;
		this.turnManager.SendMove(t.text+"No", false);
		PlayMusic(selectClap);
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
		

    public void OnSelectChessman(int selectId,float x,float z,int occupyId)
    {
        if (_selectedId == -1)
        {
            TrySelectChessman(selectId);
        }
        else
        {
            TryMoveChessman(selectId, x, z,occupyId);
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
    bool CanMove(int moveId, int killId, Vector3 targetPosition)
    {
        if (SameColor(moveId, killId)) {    //如果是同阵营的棋子，则取消原来的选择，选择新的棋子
            OnCancelSelected(moveId);
            TrySelectChessman(killId);
            return false;
        }
        float row = targetPosition.x;
        float col = targetPosition.z;
        switch (ChessmanManager.chessman[moveId]._type)
        {
            case ChessmanManager.Chessman.TYPE.KING:
                return PositionManager.canMoveKing(moveId, row, col, killId);
            case ChessmanManager.Chessman.TYPE.GUARD:
                return PositionManager.canMoveGuard(moveId, row, col, killId);
            case ChessmanManager.Chessman.TYPE.ELEPHANT:
                return PositionManager.canMoveElephant(moveId, row, col, killId);
            case ChessmanManager.Chessman.TYPE.HORSE:
                return PositionManager.canMoveHorse(moveId, row, col, killId);
            case ChessmanManager.Chessman.TYPE.ROOK:
                return PositionManager.canMoveRook(moveId, row, col, killId);
            case ChessmanManager.Chessman.TYPE.CANNON:
                return PositionManager.canMoveCannon(moveId, row, col, killId);
            case ChessmanManager.Chessman.TYPE.PAWN:
                return PositionManager.canMovePawn(moveId, row, col, killId);
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
        PhotonPlayer local = PhotonNetwork.player;
        if (PhotonNetwork.isMasterClient) {

			localPlayerType = ChessPlayerType.Red;
			GameStatusText.text = "您是红方棋手……";
			Debug.Log ("OnJoinedRoom+您是红方棋手");
            ExitGames.Client.Photon.Hashtable playerType = new ExitGames.Client.Photon.Hashtable();
            playerType.Add("playerType", "红方选手");
            local.SetCustomProperties(playerType, null, false);
        }

        if (PhotonNetwork.room.PlayerCount == 2)
        {
            if (!PhotonNetwork.isMasterClient) {
                ExitGames.Client.Photon.Hashtable playerType = new ExitGames.Client.Photon.Hashtable();
                playerType.Add("playerType", "黑方选手");
                local.SetCustomProperties(playerType, null, false);
                localPlayerType = ChessPlayerType.Black;
                GameStatusText.text = "您是黑方棋手……";
            }

            if (this.turnManager.Turn == 0)
            {
                
                // 当房间内有两个玩家,则开始首回合
                this.StartTurn();
				PlayMusic (welcomMusic);
            }
        }

        if (PhotonNetwork.room.PlayerCount > 2)
        {
            if (localPlayerType == ChessPlayerType.Guest)
            {
                GameStatusText.text = "棋局已开始，正在进入观棋模式……";
            }
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
		Debug.Log("Other player arrived");
		GameStatusText.text = "欢迎"+newPlayer.NickName+"加入游戏！";
		if (PhotonNetwork.room.PlayerCount == 2)
		{
			if (this.turnManager.Turn == 0)
			{

				this.StartTurn();
			}
		}
		PlayMusic (JoinClip);
	}

	/// <summary>
	/// 当一个远程玩家离开房间时调用。这个PhotonPlayer 此时已经从playerlist玩家列表删除.
	/// </summary>
	/// <remarks>当你的客户端调用PhotonNetwork.leaveRoom时，PUN将在现有的客户端上调用此方法。当远程客户端关闭连接或被关闭时，这个回调函数会在经过几秒钟的暂停后被执行.</remarks>
	/// <param name="otherPlayer">Other player.</param>
	public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
	{
		PlayMusic (LeaveClip);
		Debug.Log("Other player disconnected! isInactive: " + otherPlayer.IsInactive);
		GameStatusText.text ="玩家"+ otherPlayer.NickName+"已离开游戏";
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
	private void UpdatePlayerTexts()
	{
		PhotonPlayer remote = PhotonNetwork.player.GetNext();
		PhotonPlayer local = PhotonNetwork.player;

		Tip.UpdateText (GameStatusText.text,"0");

		if (remote != null)
		{
			// 应该是这种格式: "name        00"
			if (PhotonNetwork.isMasterClient) {
				this.RemotePlayerText.text = remote.NickName + "—黑方 | Black   " + remote.GetScore ().ToString ("D2");
			} else {

				this.RemotePlayerText.text = remote.NickName + "—红方 | Red   " + remote.GetScore ().ToString ("D2");
			}
		}
		else
		{

			TimerFillImage.anchorMax = new Vector2(0f, 1f);
			this.TimeText.text = "";
			this.RemotePlayerText.text = "等待其他玩家        00";
		}

		if (local != null)
		{
			// 应该是这种样式: "YOU   00"
			if (PhotonNetwork.isMasterClient)
			{
				this.LocalPlayerText.text = local.NickName+":红方 | Red   " + local.GetScore().ToString("D2");
				//Debug.Log ("MasterClient");
			}else{

				this.LocalPlayerText.text = local.NickName + ":黑方 | Black   " + local.GetScore().ToString("D2");
			}


			if (localPlayerType == ChessPlayerType.Guest)
			{

				Debug.Log ("ChessPlayerType.Guest");
			}


		}
	}

	void MoveError(int moveId, Vector3 position)
	{
		GameObject chessman = chessManManager.transform.FindChild(moveId.ToString()).gameObject;
		Vector3 oldPosition = new Vector3(ChessmanManager.chessman[moveId]._x, 1f, ChessmanManager.chessman[moveId]._z);
		HidePath ();
		ShowPath (oldPosition,position);
		GameStatusText.text = "MoveError:"+chessman.name+"不能移动到目标位置:"+position;
	}

	void TrySelectChessman(int selectId)
	{
		if (selectId==-1)
		{
			return;
		}

		if (localPlayerType == ChessPlayerType.Guest) return;   //游客只能观看

		if (selectId>=16)    //黑子无法被红方或红色回合内选定
		{
			if (localPlayerType==ChessPlayerType.Red || _isRedTurn)
			{
				return;
			}
		}
		else    //红子同样无法被其他阵营选定
		{
			if (localPlayerType == ChessPlayerType.Black || !_isRedTurn)
			{
				return;
			}
		}

		if (!CanSelect(selectId)) return;
		this.turnManager.SendMove ("+ConfirmedSelect "+selectId.ToString(),false);
		ConfirmedSelect (selectId);
	}

	void ConfirmedSelect(int selectId){
		_selectedId = selectId;

		HidePath ();
		boardManager.hidePossibleWay ();
		boardManager.showPossibleWay(selectId);
	}

	/// <summary>  
	/// 设置棋子死亡  
	/// </summary>  
	/// <param name="id"></param>  
	void KillChessman(int id)
	{
		if (id == -1) return;

		CameraRigManager.LocalPlayerInstance.GetComponent<CameraRigManager> ().ApplyDamage ();
		ChessmanManager.chessman[id]._dead = true;
		Transform chessman=chessManManager.transform.FindChild(id.ToString());
		chessman.GetComponent<ChessmanController> ().SwitchDead ();
	}

	/// <summary>  
	/// 复活棋子  
	/// </summary>  
	/// <param name="id"></param>  
	void ReliveChess(int id)
	{
		if (id == -1) return;

		//因GameObject.Find();函数不能找到active==false的物体，故先找到其父物体，再找到其子物体才可以找到active==false的物体  
		ChessmanManager.chessman[id]._dead = false;
		GameObject Stone = chessManManager.transform.Find(id.ToString()).gameObject;
		Stone.SetActive(true);
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
	void ShowPath(Vector3 oldPosition, Vector3 newPosition)
	{
		Selected.transform.localPosition = newPosition;
		if (!Selected.activeSelf) {
			Selected.SetActive(true);
		}

		Path.transform.localPosition = oldPosition;
		if (!Path.activeSelf) {
			Path.SetActive(true);
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
	void MoveChessman(int moveId, Vector3 targetPosition)
	{
		Transform chessman = chessManManager.transform.FindChild(moveId.ToString());
		boardManager.hidePossibleWay ();
		chessman.GetComponent<ChessmanController>().SetTarget(targetPosition);
		_isRedTurn = !_isRedTurn;
	}

	/// <summary>  
	/// 通过记录的步骤结构体来返回上一步  
	/// </summary>  
	/// <param name="_step"></param>  
	void Back(step _step)
	{
		if (_step.killId != -1) {
			ReliveChess (_step.killId);
		}
		MoveChessman(_step.moveId, new Vector3(_step.xFrom,1f, _step.zFrom));

		// this.turnManager.SendMove(_step, true);
		HidePath();
		if (_selectedId != -1)
		{      
			_selectedId = -1;
		}
	}

	/// <summary>
	/// 正在移动棋子.
	/// </summary>
	/// <param name="selectedId">选择的棋子ID.</param>
	/// <param name="killId">要击杀的棋子ID.</param>
	/// <param name="toX">目标To x.</param>
	/// <param name="toZ">目标To z.</param>
	void OnMoveChessman(int selectedId,int killId,float toX,float toZ,int occupyId)
	{
		float fromX = ChessmanManager.chessman[selectedId]._x;
		float fromZ = ChessmanManager.chessman[selectedId]._z;
		string tmpStr = selectedId.ToString() +"s"+ killId.ToString()+"s"+fromX.ToString()+"s"+fromZ.ToString()+"s"+toX.ToString()+"s"+toZ.ToString() + "s" +occupyId.ToString();
		this.turnManager.SendMove(tmpStr, true);	//弃用step结构体来传递信息的原因是Photon不能序列化,所以采用字符串来同步信息
	}

	/// <summary>
	/// 刷新UI视图
	/// </summary>
	void RefreshUIViews()
	{
		TimerFillImage.anchorMax = new Vector2(0f, 1f);

		GameUiView.gameObject.SetActive(PhotonNetwork.inRoom);
		VoiceUiView.gameObject.SetActive(PhotonNetwork.inRoom);
		ButtonCanvasGroup.interactable = PhotonNetwork.room != null ? PhotonNetwork.room.PlayerCount > 1 : false;
	}

	bool IsRed(int id)
	{
		return ChessmanManager.chessman[id]._red;
	}

	bool IsDead(int id)
	{
		if (id == -1) return true;
		return ChessmanManager.chessman[id]._dead;
	}

	bool SameColor(int id1, int id2)
	{
		if (id1 == -1 || id2 == -1) return false;

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
		if (PhotonNetwork.isMasterClient) {
			PhotonNetwork.LoadLevel ("ChineseChessVR0");
		}

	}

	void CancelSelected(int cancelId){
		if (_selectedId==cancelId)
		{
			_selectedId = -1;
		}
	}

	#endregion


}
