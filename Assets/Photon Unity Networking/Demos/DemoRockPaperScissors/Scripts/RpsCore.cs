using System;
using System.Collections;
using Photon;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

// Photon服务器为每个玩家指派一个ActorNumber (player.ID),从1开始
// 至于这个游戏,我们不需要实际的数字
// 这个游戏使用0和1,这样客户端需要自己计算出自己的号码
public class RpsCore : PunBehaviour, IPunTurnManagerCallbacks
{
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

	[Tooltip("本地选择图片")]
    [SerializeField]
    private Image localSelectionImage;
	[Tooltip("本地选择")]
    public Hand localSelection;

	[Tooltip("远程选择图片")]
    [SerializeField]
    private Image remoteSelectionImage;
	[Tooltip("远程选择")]
    public Hand remoteSelection;

	[Tooltip("已选石头")]
    [SerializeField]
    private Sprite SelectedRock;

	[Tooltip("已选纸")]
    [SerializeField]
    private Sprite SelectedPaper;

	[Tooltip("已选剪刀")]
    [SerializeField]
    private Sprite SelectedScissors;

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

	[Tooltip("随机手势")]
    public Hand randomHand;    // 用于当本地玩家没有选择任何手势时显示远程玩家的手势

	// 追踪显示结果的时机来处理游戏逻辑.
	private bool IsShowingResults;

    public enum Hand	//手势枚举
    {
        None = 0,
        Rock,	//石头
        Paper,	//纸|布
        Scissors //剪刀
    }
		
    public enum ResultType	//结果类型枚举
    {
        None = 0,
        Draw,	//和
        LocalWin,	//赢
        LocalLoss	//输
    }

    public void Start()
    {
		this.turnManager = this.gameObject.AddComponent<PunTurnManager>();	//添加组件并赋值
        this.turnManager.TurnManagerListener = this;	//为监听器赋值,从而触发下面的回调函数来完成游戏逻辑
        this.turnManager.TurnDuration = 5f;		//初始化回合持续时间
        

        this.localSelectionImage.gameObject.SetActive(false);	//激活本地选择图片
        this.remoteSelectionImage.gameObject.SetActive(false);	//激活远程选择图片
        this.StartCoroutine("CycleRemoteHandCoroutine");	//启动协程，间隔0.5秒随机一个手势

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

        // 展示本地玩家的选择手势
        Sprite selected = SelectionToSprite(this.localSelection);
        if (selected != null)
        {
            this.localSelectionImage.gameObject.SetActive(true);
            this.localSelectionImage.sprite = selected;
        }

        // 远程玩家的选择只在回合结束时（双方都完成回合）展示
        if (this.turnManager.IsCompletedByAll)
        {
            selected = SelectionToSprite(this.remoteSelection);
            if (selected != null)
            {
                this.remoteSelectionImage.color = new Color(1,1,1,1);
                this.remoteSelectionImage.sprite = selected;
            }
        }
        else
        {
			ButtonCanvasGroup.interactable = PhotonNetwork.room.PlayerCount > 1;	//玩家数量大于1才可以触发按钮

            if (PhotonNetwork.room.PlayerCount < 2)
            {
                this.remoteSelectionImage.color = new Color(1, 1, 1, 0);
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

                this.remoteSelectionImage.color = new Color(1, 1, 1, alpha);
                this.remoteSelectionImage.sprite = SelectionToSprite(randomHand);
            }
        }

    }

    #region TurnManager Callbacks	//回调区域

    /// <summary>
    /// 发起回合开始事件.
    /// </summary>
    /// <param name="turn">回合.</param>
    public void OnTurnBegins(int turn)
    {
        Debug.Log("OnTurnBegins() turn: "+ turn);
        this.localSelection = Hand.None;
        this.remoteSelection = Hand.None;

        this.WinOrLossImage.gameObject.SetActive(false);	//关闭输赢的图片

        this.localSelectionImage.gameObject.SetActive(false);	//关闭本地选择图片
        this.remoteSelectionImage.gameObject.SetActive(true);	//关闭远程选择图片

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

        this.CalculateWinAndLoss();	//计算输赢
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
        throw new NotImplementedException();
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
            this.localSelection = (Hand)(byte)move;
        }
        else
        {
            this.remoteSelection = (Hand)(byte)move;
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
    }

	/// <summary>
	/// 回合中的选择
	/// </summary>
	/// <param name="selection">选择.</param>
    public void MakeTurn(Hand selection)
    {
        this.turnManager.SendMove((byte)selection, true);
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
	/// 计算输赢
	/// </summary>
    private void CalculateWinAndLoss()
    {
        this.result = ResultType.Draw;
        if (this.localSelection == this.remoteSelection)	//如果双方的手势一样,则为和局
        {
            return;
        }

		if (this.localSelection == Hand.None)	//如果本地玩家没有选择,弃权为输
		{
			this.result = ResultType.LocalLoss;
			return;
		}

		if (this.remoteSelection == Hand.None)	//远程玩家没有选择也为输
		{
			this.result = ResultType.LocalWin;
		}
        
        if (this.localSelection == Hand.Rock)	//根据石头剪刀布的游戏规则判断
        {
            this.result = (this.remoteSelection == Hand.Scissors) ? ResultType.LocalWin : ResultType.LocalLoss;
        }
        if (this.localSelection == Hand.Paper)
        {
            this.result = (this.remoteSelection == Hand.Rock) ? ResultType.LocalWin : ResultType.LocalLoss;
        }

        if (this.localSelection == Hand.Scissors)
        {
            this.result = (this.remoteSelection == Hand.Paper) ? ResultType.LocalWin : ResultType.LocalLoss;
        }
    }

	/// <summary>
	/// 选择精灵
	/// </summary>
	/// <returns>返回对应手势的精灵.</returns>
	/// <param name="hand">手势.</param>
    private Sprite SelectionToSprite(Hand hand)
    {
        switch (hand)
        {
            case Hand.None:
                break;
            case Hand.Rock:
                return this.SelectedRock;
            case Hand.Paper:
                return this.SelectedPaper;
            case Hand.Scissors:
                return this.SelectedScissors;
        }

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

    public IEnumerator CycleRemoteHandCoroutine()
    {
        while (true)
        {
            // 循环可用的图像
            this.randomHand = (Hand)Random.Range(1, 4);
            yield return new WaitForSeconds(0.5f);
        }
    }

    #endregion


    #region Handling Of Buttons	//处理按钮

	/// <summary>
	/// 点击石头按钮就是选择石头,下同
	/// </summary>
    public void OnClickRock()
    {
        this.MakeTurn(Hand.Rock);
    }

    public void OnClickPaper()
    {
       this.MakeTurn(Hand.Paper);
    }

    public void OnClickScissors()
    {
        this.MakeTurn(Hand.Scissors);
    }

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
