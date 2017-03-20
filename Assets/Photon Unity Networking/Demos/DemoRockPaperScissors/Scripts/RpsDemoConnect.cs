using Photon;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RpsDemoConnect : PunBehaviour
{
    public InputField InputField;
    public string UserId;
    public string previousRoom;
    private const string MainSceneName = "DemoRPS-Scene";

	const string NickNamePlayerPrefsKey = "NickName";


	void Start()
	{
		InputField.text = PlayerPrefs.HasKey(NickNamePlayerPrefsKey)?PlayerPrefs.GetString(NickNamePlayerPrefsKey):"";  //三元运算，如果PlayerPrefs中有玩家的昵称则获取之，否则为空字符串
    }

    /// <summary>
    /// 应用用户Id并连接
    /// </summary>
    public void ApplyUserIdAndConnect()
    {
		string nickName = "DemoNick";
        //设置玩家的昵称
        if (this.InputField != null && !string.IsNullOrEmpty(this.InputField.text))
        {
            nickName = this.InputField.text;
			PlayerPrefs.SetString(NickNamePlayerPrefsKey,nickName);
        }
        //if (string.IsNullOrEmpty(UserId))
        //{
        //    this.UserId = nickName + "ID";
        //}
        Debug.Log("Nickname: " + nickName + " userID: " + this.UserId,this);


        if (PhotonNetwork.AuthValues == null)
        {
            PhotonNetwork.AuthValues = new AuthenticationValues();
        }
        //else
        //{
        //    Debug.Log("Re-using AuthValues. UserId: " + PhotonNetwork.AuthValues.UserId);
        //}

        PhotonNetwork.playerName = nickName;
        PhotonNetwork.ConnectUsingSettings("0.5");
        
        // this way we can force timeouts by pausing the client (in editor)
        PhotonHandler.StopFallbackSendAckThread();
    }

	/// <summary>
	/// 在到主服务器连接被建立和认证后调用，但是只有当PhotonNetwork.autoJoinLobby是false时才调用.
	/// </summary>
	/// <remarks>如果你设置PhotonNetwork.autoJoinLobby为true，取而代之调用的是OnJoinedLobby().
	/// 
	/// 即使没有在游戏大厅内，你也可以加入房间和创建房间。在这种情况下使用了默认的大厅。
	/// 可用房间列表将不可用，除非你通过PhotonNetwork.joinLobby加入一个游戏大厅.</remarks>
    public override void OnConnectedToMaster()
    {
        // 连接之后 
        this.UserId = PhotonNetwork.player.UserId;
        ////Debug.Log("UserID " + this.UserId);


        // 超时之后: 重新加入之前的房间（如果有的话）
        if (!string.IsNullOrEmpty(this.previousRoom))
        {
            Debug.Log("ReJoining previous room: " + this.previousRoom);
            PhotonNetwork.ReJoinRoom(this.previousRoom);
            this.previousRoom = null;       // we only will try to re-join once. if this fails, we will get into a random/new room
        }
        else
        {
            // 否则加入随机房间
            PhotonNetwork.JoinRandomRoom();
        }
    }

	/// <summary>
	/// 在主服务器上进入一个大厅时调用。实际的房间列表的更新会调用OnReceivedRoomListUpdate()。
	/// </summary>
	/// <remarks>注意：当PhotonNetwork.autoJoinLobby是false时，OnConnectedToMaster()将会被调用并且房间列表将不可用。
	/// 
	/// 而在大堂的房间列表是在固定的时间间隔内自动更新（这是你不能修改的）。
	/// 当OnReceivedRoomListUpdate()在OnJoinedLobby()之后被调用后，房间列表变得可用.</remarks>
    public override void OnJoinedLobby()
    {
        OnConnectedToMaster(); // 这样我们是否加入游戏大厅都无所谓了
    }

	/// <summary>
	/// 在一个JoinRandom()请求失败后调用。参数提供ErrorCode错误代码和消息。
	/// </summary>
	/// <param name="codeAndMsg">Code and message.</param>
    public override void OnPhotonRandomJoinFailed(object[] codeAndMsg)
    {
        PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = 2, PlayerTtl = 5000 }, null);
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
		Debug.Log("Joined room: " + PhotonNetwork.room.Name);
        this.previousRoom = PhotonNetwork.room.Name;

    }

	/// <summary>
	/// 当一个JoinRoom()调用失败时被调用。参数以数组的方式提供ErrorCode和消息。
	/// </summary>
	/// <remarks>最有可能是因为房间的名称已经在使用（其他客户端比你更快）。
	/// 如果PhotonNetwork.logLevel >= PhotonLogLevel.Informational为真，PUN会记录一些信息。</remarks>
	/// <param name="codeAndMsg">codeAndMsg[0]是short ErrorCode，codeAndMsg[1]是调试消息字符串.</param>
    public override void OnPhotonJoinRoomFailed(object[] codeAndMsg)
    {
        this.previousRoom = null;
    }

	/// <summary>
	/// 当未知因素导致连接失败（在建立连接之后）时调用，接着调用OnDisconnectedFromPhoton()。
	/// </summary>
	/// <remarks>如果服务器不能一开始就被连接，就会调用OnFailedToConnectToPhoton。错误的原因会以DisconnectCause的形式提供。</remarks>
	/// <param name="cause">Cause.</param>
    public override void OnConnectionFail(DisconnectCause cause)
    {
        Debug.Log("Disconnected due to: " + cause + ". this.previousRoom: " + this.previousRoom);
    }
}
