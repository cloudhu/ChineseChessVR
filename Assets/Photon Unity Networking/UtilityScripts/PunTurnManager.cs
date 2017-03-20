// ----------------------------------------------------------------------------
// <copyright file="PunTurnManager.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2016 Exit Games GmbH
// </copyright>
// <summary>
//  使用PUN的回合制游戏管家
// </summary>
// <author>developer@exitgames.com</author>
// <interpreter>胡良云（CLoudHu）</interpreter>
// ----------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon;
using UnityEngine;
using ExitGames = ExitGames.Client.Photon.Hashtable;

/// <summary>
/// Pun回合制游戏管家.
/// 为玩家之间典型的回合流程和逻辑提供一个接口(IPunTurnManagerCallbacks)
/// 为PhotonPlayer、Room和RoomInfo提供了扩展来满足回合制游戏需求而专门制作的API
/// </summary>
public class PunTurnManager : PunBehaviour
{
	/// <summary>
	/// 包装了对房间的自定义属性"turn"的访问.
	/// </summary>
	/// <value>回合的索引</value>
    public int Turn	//回合
    {
        get { return PhotonNetwork.room.GetTurn(); }
        private set {

			_isOverCallProcessed = false;

			PhotonNetwork.room.SetTurn(value, true);
		}
    }


	/// <summary>
	/// 回合的持续时间（单位：秒）.
	/// </summary>
    public float TurnDuration = 20f;

	/// <summary>
	/// 获取当前回合过去的时间（秒）
	/// </summary>
	/// <value>回合流逝的时间.</value>
	public float ElapsedTimeInTurn
	{
		get { return ((float)(PhotonNetwork.ServerTimestamp - PhotonNetwork.room.GetTurnStart()))/1000.0f; }
	}


	/// <summary>
	/// 获取当前回合剩余的时间. 范围从0到TurnDuration
	/// </summary>
	/// <value>当前回合剩余的时间（秒）</value>
	public float RemainingSecondsInTurn
	{
		get { return Mathf.Max(0f,this.TurnDuration - this.ElapsedTimeInTurn); }
	}


	/// <summary>
	/// 获取表明回合是否被全部玩家完成的布尔值.
	/// </summary>
	/// <value><c>true</c> 如果该回合被所有玩家完成则返回真; 否则返回假, <c>false</c>.</value>
    public bool IsCompletedByAll
    {
        get { return PhotonNetwork.room != null && Turn > 0 && this.finishedPlayers.Count == PhotonNetwork.room.PlayerCount; }
    }

	/// <summary>
	/// 获取表明当前回合是否被我完成的布尔值.
	/// </summary>
	/// <value><c>true</c> 如果当前回合被我完成返回真; 否则返回假, <c>false</c>.</value>
    public bool IsFinishedByMe
    {
        get { return this.finishedPlayers.Contains(PhotonNetwork.player); }
    }

	/// <summary>
	/// 获取表明当前回合是否完成的布尔值.即是ElapsedTimeinTurn>=TurnDuration 或 RemainingSecondsInTurn <= 0f
	/// </summary>
	/// <value><c>true</c> 如果当前回合完了返回真; 否则返回假, <c>false</c>.</value>
    public bool IsOver
    {
		get { return this.RemainingSecondsInTurn <= 0f; }
    }

	/// <summary>
	/// 回合管家监听器. 设置该监听器到你自己的脚本实例来捕捉回调函数
	/// </summary>
    public IPunTurnManagerCallbacks TurnManagerListener;


	/// <summary>
	/// 完成回合的玩家哈希集.
	/// </summary>
    private readonly HashSet<PhotonPlayer> finishedPlayers = new HashSet<PhotonPlayer>();

	/// <summary>
	/// 回合管家事件偏移事件消息字节. 内部用于定义房间自定义属性中的数据
	/// </summary>
    public const byte TurnManagerEventOffset = 0;

	/// <summary>
	/// 移动事件消息字节. 内部用于保存房间自定义属性中的数据
	/// </summary>
    public const byte EvMove = 1 + TurnManagerEventOffset;

	/// <summary>
	/// 最终移动事件消息字节. 内部用于保存房间自定义属性中的数据
	/// </summary>
    public const byte EvFinalMove = 2 + TurnManagerEventOffset;

	// 追踪消息调用
	private bool _isOverCallProcessed = false;

	#region MonoBehaviour CallBack

	/// <summary>
	/// 注册来自PhotonNetwork的事件调用.
	/// </summary>
    void Start()
    {
        PhotonNetwork.OnEventCall = OnEvent;
    }

	void Update()
	{
		if (Turn > 0 && this.IsOver && !_isOverCallProcessed)
		{
			_isOverCallProcessed = true;
			this.TurnManagerListener.OnTurnTimeEnds(this.Turn);
		}

	}


	#endregion

	#region Public Methods

	/// <summary>
	/// 告诉TurnManager开始一个新的回合.
	/// </summary>
    public void BeginTurn()
    {
        Turn = this.Turn + 1; // 注意: 这将设置房间里的一个属性,该属性对于其他玩家可用.
    }


    /// <summary>
	/// 调用来发送一个动作. 也可以选择结束该回合.
	/// 移动对象可以是任何事物. 尝试去优化,只发送严格的最小化信息集来定义回合移动.
	/// </summary>
    /// <param name="move">回合移动</param>
    /// <param name="finished">是否完成</param>
    public void SendMove(object move, bool finished)
    {
        if (IsFinishedByMe)
        {
            UnityEngine.Debug.LogWarning("不能SendMove. 该玩家已经完成了这回合.");
            return;
        }

        // 与实际移动一起,我们不得不发送该移动属于哪一个回合
        Hashtable moveHt = new Hashtable();
        moveHt.Add("turn", Turn);
        moveHt.Add("move", move);

        byte evCode = (finished) ? EvFinalMove : EvMove;
        PhotonNetwork.RaiseEvent(evCode, moveHt, true, new RaiseEventOptions() { CachingOption = EventCaching.AddToRoomCache });
        if (finished)
        {
            PhotonNetwork.player.SetFinishedTurn(Turn);
        }

        // 服务器不会把该事件发送回源头 (默认). 要获取该事件,本地调用即可
		// (注意: 事件的顺序可能会混淆，因为我们在本地做这个调用)
        OnEvent(evCode, moveHt, PhotonNetwork.player.ID);
    }

	/// <summary>
	/// 获取该玩家是否完成了当前回合.
	/// </summary>
	/// <returns><c>true</c>, 如果传入的玩家完成了当前回合则返回真, <c>false</c> 否则返回假.</returns>
	/// <param name="player">The Player to check for</param>
    public bool GetPlayerFinishedTurn(PhotonPlayer player)
    {
        if (player != null && this.finishedPlayers != null && this.finishedPlayers.Contains(player))
        {
            return true;
        }

        return false;
    }
	#endregion

	#region Callbacks

	/// <summary>
	/// 被PhotonNetwork.OnEventCall的注册调用（Start方法中注册了该事件）
	/// </summary>
	/// <param name="eventCode">事件代码.</param>
	/// <param name="content">内容.</param>
	/// <param name="senderId">发送者Id.</param>
    public void OnEvent(byte eventCode, object content, int senderId)
    {
        PhotonPlayer sender = PhotonPlayer.Find(senderId);
        switch (eventCode)
        {
            case EvMove:
            {
                Hashtable evTable = content as Hashtable;
                int turn = (int)evTable["turn"];
                object move = evTable["move"];
                this.TurnManagerListener.OnPlayerMove(sender, turn, move);

                break;
            }
            case EvFinalMove:
            {
                Hashtable evTable = content as Hashtable;
                int turn = (int)evTable["turn"];
                object move = evTable["move"];

                if (turn == this.Turn)
                {
                    this.finishedPlayers.Add(sender);

                        this.TurnManagerListener.OnPlayerFinished(sender, turn, move);

                }

                if (IsCompletedByAll)
                {
                    this.TurnManagerListener.OnTurnCompleted(this.Turn);
                }
                break;
            }
        }
    }

	/// <summary>
	/// 当一个房间的自定义属性更改时被调用。propertiesThatChanged改变的属性包含所有通过Room.SetCustomProperties设置的.
	/// </summary>
	/// <param name="propertiesThatChanged">Properties that changed.</param>
    public override void OnPhotonCustomRoomPropertiesChanged(Hashtable propertiesThatChanged)
    {

     	//   Debug.Log("OnPhotonCustomRoomPropertiesChanged: "+propertiesThatChanged.ToStringFull());

        if (propertiesThatChanged.ContainsKey("Turn"))
        {
			_isOverCallProcessed = false;
            this.finishedPlayers.Clear();
            this.TurnManagerListener.OnTurnBegins(this.Turn);
        }
    }

	#endregion
}


public interface IPunTurnManagerCallbacks
{
	/// <summary>
	/// 发起回合开始事件.
	/// </summary>
	/// <param name="turn">回合.</param>
    void OnTurnBegins(int turn);

	/// <summary>
	/// 当回合完成时调用(被所有玩家完成)
	/// </summary>
	/// <param name="turn">回合索引</param>
    void OnTurnCompleted(int turn);

	/// <summary>
	/// 当玩家移动时调用(但是没有完成该回合)
	/// </summary>
	/// <param name="player">玩家引用</param>
	/// <param name="turn">回合索引</param>
	/// <param name="move">移动对象数据</param>
    void OnPlayerMove(PhotonPlayer player, int turn, object move);

	/// <summary>
	/// 当玩家完成回合时调用(包括该玩家的动作/移动)
	/// </summary>
	/// <param name="player">玩家引用</param>
	/// <param name="turn">回合索引</param>
	/// <param name="move">移动对象数据</param>
    void OnPlayerFinished(PhotonPlayer player, int turn, object move);


	/// <summary>
	/// 当回合由于时间限制完成时调用(回合超时)
	/// </summary>
	/// <param name="turn">回合索引</param>
    void OnTurnTimeEnds(int turn);
}


public static class TurnExtensions
{
	/// <summary>
	/// 当前进行的回合数
	/// </summary>
    public static readonly string TurnPropKey = "Turn";

	/// <summary>
	/// 当前进行的回合开始（服务器）时间（用于计算结束）
	/// </summary>
    public static readonly string TurnStartPropKey = "TStart";

	/// <summary>
	/// 完成回合的演员 (后面接数字)
	/// </summary>
    public static readonly string FinishedTurnPropKey = "FToA";

	/// <summary>
	/// 设置该回合.
	/// </summary>
	/// <param name="room">房间引用</param>
	/// <param name="turn">回合索引</param>
	/// <param name="setStartTime">如果设置为真 <c>true</c> 则设置开始时间.</param>
    public static void SetTurn(this Room room, int turn, bool setStartTime = false)
    {
        if (room == null || room.CustomProperties == null)
        {
            return;
        }

        Hashtable turnProps = new Hashtable();
        turnProps[TurnPropKey] = turn;
        if (setStartTime)
        {
            turnProps[TurnStartPropKey] = PhotonNetwork.ServerTimestamp;
        }

        room.SetCustomProperties(turnProps);
    }

	/// <summary>
	/// 从RoomInfo获取当前回合
	/// </summary>
	/// <returns>返回回合索引 </returns>
	/// <param name="room">RoomInfo引用</param>
    public static int GetTurn(this RoomInfo room)
    {
        if (room == null || room.CustomProperties == null || !room.CustomProperties.ContainsKey(TurnPropKey))
        {
            return 0;
        }

        return (int)room.CustomProperties[TurnPropKey];
    }


	/// <summary>
	/// 返回回合开始的时间. 可用于计算回合进行的时间.
	/// </summary>
	/// <returns>返回回合开始时间.</returns>
	/// <param name="room">房间信息.</param>
    public static int GetTurnStart(this RoomInfo room)
    {
        if (room == null || room.CustomProperties == null || !room.CustomProperties.ContainsKey(TurnStartPropKey))
        {
            return 0;
        }

        return (int)room.CustomProperties[TurnStartPropKey];
    }

	/// <summary>
	/// 获取玩家完成的回合 (从房间属性中)
	/// </summary>
	/// <returns>返回已完成的回合索引</returns>
	/// <param name="player">玩家引用</param>
    public static int GetFinishedTurn(this PhotonPlayer player)
    {
        Room room = PhotonNetwork.room;
        if (room == null || room.CustomProperties == null || !room.CustomProperties.ContainsKey(TurnPropKey))
        {
            return 0;
        }

        string propKey = FinishedTurnPropKey + player.ID;
        return (int)room.CustomProperties[propKey];
    }

	/// <summary>
	/// 设置玩家完成的回合 (在房间属性中)
	/// </summary>
	/// <param name="player">玩家引用</param>
	/// <param name="turn">回合索引</param>
    public static void SetFinishedTurn(this PhotonPlayer player, int turn)
    {
        Room room = PhotonNetwork.room;
        if (room == null || room.CustomProperties == null)
        {
            return;
        }

        string propKey = FinishedTurnPropKey + player.ID;
        Hashtable finishedTurnProp = new Hashtable();
        finishedTurnProp[propKey] = turn;

        room.SetCustomProperties(finishedTurnProp);
    }
}