// ----------------------------------------------------------------------------
// <copyright file="PhotonClasses.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2011 Exit Games GmbH
// </copyright>
// <summary>
//
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------

#pragma warning disable 1587
/// \file
/// <summary>包装相对较小的不需要单独成文件的类 </summary>
///
///
/// \defgroup publicApi Public API
/// \brief Groups the most important classes that you need to understand early on.
///
/// \defgroup optionalGui Optional Gui Elements
/// \brief Useful GUI elements for PUN.
#pragma warning restore 1587

#if UNITY_5 && !UNITY_5_0 && !UNITY_5_1 && !UNITY_5_2
#define UNITY_MIN_5_3
#endif

using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;

using Hashtable = ExitGames.Client.Photon.Hashtable;
using SupportClassPun = ExitGames.Client.Photon.SupportClass;


/// <summary>定义OnPhotonSerializeView 方法来更容易地正确实现可观察脚本。</summary>
/// \ingroup publicApi
public interface IPunObservable
{
    /// <summary>
	/// 每秒被PUN调用几次，这样你的脚本可以为PhotonView读写同步数据。
    /// </summary>
    /// <remarks>
	/// 这个方法将在那些被指派作为PhotonView的观察组件的脚本里被调用<br/>
    /// PhotonNetwork.sendRateOnSerialize影响该方法被调用的频率.<br/>
    /// PhotonNetwork.sendRate影响由这个客户端发包的频率.<br/>
    ///
	/// 实现该方法，你可以自定义PhotonView定期同步哪些数据。
    /// 你的代码定义那些内容是正在被发送的以及你的数据如何被接收的客户端使用。
    ///
	/// 不像其他回调函数, <i>OnPhotonSerializeView只在被指派给PhotonView作为PhotonView.observed脚本时才被调用</i>.
    ///
	/// 要利用该方法，PhotonStream是必备的。它在控制PhotonView的客户端上将是“写入模式”(PhotonStream.isWriting == true)
	/// 且在接收控制客户端发送数据的远程客户端上是“读取模式”(PhotonStream.isWriting == false)
    ///
    /// 如果你跳过写入任何值到数据流，PUN将跳过该更新。谨慎使用，这可以节省宽带和消息（这在每个房间/秒是有限制的）。
    ///
	/// 注意当发送者没有发送任何更新时，OnPhotonSerializeView没有在远程客户端上被调用。
	/// 这个不能被作为"x-次/秒 Update()"使用。
    /// </remarks>
    /// \ingroup publicApi
    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info);
}

/// <summary>
/// 这个接口是被用作PUN所有回调方法的定义，除了OnPhotonSerializeView。最好单独实现它们。
/// </summary>
/// <remarks>
/// 这个接口还有待完善，但对于一个游戏的实际实现已经足够了。
/// 你可以无需实现IPunCallbacks的情况下在任何MonoMehaviour中单独实现每一个方法。
///
/// PUN通过名称来调用所有的回调函数。不要使用全名来实现回调。
/// 例如：IPunCallbacks.OnConnectedToPhoton不会被Unity的SendMessage()方法调用。
///
/// PUN将会调用那些在任何脚本上实现了回调函数的方法，类似于Unity的事件和回调。
/// 触发调用的情况在每一个方法中都有描述。
///
/// OnPhotonSerializeView不像其他回调函数一样被调用！它的使用频率比较高并且实现了 : IPunObservable。
/// </remarks>
/// \ingroup publicApi
public interface IPunCallbacks
{
    /// <summary>
	/// 当初始连接已被建立时调用，但在您可以使用服务器之前。当PUN准备好时调用OnJoinedLobby()或OnConnectedToMaster()。
    /// </summary>
    /// <remarks>
	/// 这个回调函数只在检测服务器是否可以被完全连接时有用（技术上）。
	/// 通常，实现OnFailedToConnectToPhoton()和OnDisconnectedFromPhoton()就够了。
    ///
	/// <i>当PUN准备好时调用OnJoinedLobby()或OnConnectedToMaster()。</i>
    ///
	/// 当被调用时，低层次的连接被建立起来并且PUN会在后台发送你的AppId，用户信息等。
	/// 从主服务器向游戏服务器转换时不会被调用。
    /// </remarks>
    void OnConnectedToPhoton();

    /// <summary>
	/// 当本地用户/客户离开房间时调用。
    /// </summary>
    /// <remarks>
	/// 当离开一个房间时，PUN将你带回主服务器。
	/// 在您可以使用游戏大厅和创建/加入房间之前，OnJoinedLobby()或OnConnectedToMaster()会再次被调用。
    /// </remarks>
    void OnLeftRoom();

    /// <summary>
	/// 在当前主客户端离开时切换到一个新的主客户端后调用。
    /// </summary>
    /// <remarks>
	/// 当这个客户进入某个房间时，这是不被调用的。
	/// 当这个方法被调用时之前的主客户端仍在玩家列表。
    /// </remarks>
    void OnMasterClientSwitched(PhotonPlayer newMasterClient);

    /// <summary>
	/// 当一个CreateRoom()方法调用失败时调用。参数以数组的方式提供ErrorCode和消息。
    /// </summary>
    /// <remarks>
	/// 最有可能是因为房间的名称已经在使用（其他客户端比你更快）。
	/// 如果PhotonNetwork.logLevel >= PhotonLogLevel.Informational为真，PUN会记录一些信息。
    /// </remarks>
	/// <param name="codeAndMsg">codeAndMsg[0]是short ErrorCode，codeAndMsg[1]是调试消息字符串.</param>
    void OnPhotonCreateRoomFailed(object[] codeAndMsg);

    /// <summary>
	/// 当一个JoinRoom()调用失败时被调用。参数以数组的方式提供ErrorCode和消息。
    /// </summary>
    /// <remarks>
	/// 最有可能是因为房间的名称已经在使用（其他客户端比你更快）。
	/// 如果PhotonNetwork.logLevel >= PhotonLogLevel.Informational为真，PUN会记录一些信息。
    /// </remarks>
	/// <param name="codeAndMsg">codeAndMsg[0]是short ErrorCode，codeAndMsg[1]是调试消息字符串.</param>
    void OnPhotonJoinRoomFailed(object[] codeAndMsg);

    /// <summary>
	/// 当这个客户端创建了一个房间并进入它时调用。OnJoinedRoom()也会被调用。
    /// </summary>
    /// <remarks>
	/// 这个回调只在创建房间的客户端调用（详见PhotonNetwork.CreateRoom）。
    ///
	/// 由于任何客户端在任何时候都可能会关闭（或断开连接），一个房间的创造者有一定的几率不执行OnCreatedRoom.
    ///
	/// 如果你需要特定的房间属性或一个“开始信号”，实现OnMasterClientSwitched()并使新的主客户端检查房间的状态更加安全。
    /// </remarks>
    void OnCreatedRoom();

    /// <summary>
	/// 在主服务器上进入一个大厅时调用。实际的房间列表的更新会调用OnReceivedRoomListUpdate()。
    /// </summary>
    /// <remarks>
	/// 注意：当PhotonNetwork.autoJoinLobby是false时，OnConnectedToMaster()将会被调用并且房间列表将不可用。
    ///
	/// 而在大堂的房间列表是在固定的时间间隔内自动更新（这是你不能修改的）。
	/// 当OnReceivedRoomListUpdate()在OnJoinedLobby()之后被调用后，房间列表变得可用.
    /// </remarks>
    void OnJoinedLobby();

    /// <summary>
	/// 离开游戏大厅后被调用。
    /// </summary>
    /// <remarks>
    /// 当你离开游戏大厅, [CreateRoom](@ref PhotonNetwork.CreateRoom)和[JoinRandomRoom](@ref PhotonNetwork.JoinRandomRoom)
    /// 自动指向默认的游戏大厅.
    /// </remarks>
    void OnLeftLobby();

    /// <summary>
	/// 如果到Photon服务器的连接请求失败了，在连接被建立前调用该方法，接着调用OnDisconnectedFromPhoton()。
    /// </summary>
    /// <remarks>
    /// 当完全没有连接可以被建立时该回调被调用。
	/// 它与OnConnectionFail不同，OnConnectionFail在一个存在的连接失败时被调用。
    /// </remarks>
    void OnFailedToConnectToPhoton(DisconnectCause cause);

    /// <summary>
	/// 当未知因素导致连接失败（在建立连接之后）时调用，接着调用OnDisconnectedFromPhoton()。
    /// </summary>
    /// <remarks>
	/// 如果服务器不能一开始就被连接，就会调用OnFailedToConnectToPhoton。错误的原因会以DisconnectCause的形式提供。
    /// </remarks>
    void OnConnectionFail(DisconnectCause cause);

    /// <summary>
	/// 从Photon服务器断开连接后被调用。
    /// </summary>
    /// <remarks>
	/// 在某些情况下，其他回调函数在OnDisconnectedFromPhoton被调用之前被调用。
	/// 例如：OnConnectionFail()和OnFailedToConnectToPhoton()。
    /// </remarks>
    void OnDisconnectedFromPhoton();

    /// <summary>
	/// 在一个通过使用PhotonNetwork.Instantiate方法被实例化出来的游戏对象（及其子类）上的任何脚本里被调用。 
    /// </summary>
    /// <remarks>
	/// PhotonMessageInfo参数提供关于谁创建的该对象以及什么时候创建的（基于PhotonNetworking.time）这些信息。
    /// </remarks>
    void OnPhotonInstantiate(PhotonMessageInfo info);

    /// <summary>
	/// 在主服务器上的大厅内（PhotonNetwork.insideLobby）房间列表的任何更新都会调用该函数。
    /// </summary>
    /// <remarks>
	/// PUN通过PhotonNetwork.GetRoomList()方法提供房间列表.<br/>
	/// 每一项都是一个RoomInfo，其中可能包括自定义属性（提供你在创建一个房间时定义的那些lobby-listed）。
    ///
	/// 不是所有类型的游戏大厅都会提供一系列的房间给客户端。有些游戏大厅是沉默的并且专门做服务器端的匹配（例如英雄联盟的游戏大厅就没有房间列表，所有的房间都是通过服务器匹配的，自定义则是会提供房间列表）.
    /// </remarks>
    void OnReceivedRoomListUpdate();

    /// <summary>
	/// 当进入一个房间（通过创建或加入）时被调用。在所有客户端（包括主客户端）上被调用.
    /// </summary>
    /// <remarks>
	/// 这种方法通常用于实例化玩家角色。
	/// 如果一场比赛必须“积极地”被开始，你也可以调用一个由用户的按键或定时器触发的PunRPC 。
    ///
	/// 当这个被调用时，你通常可以通过PhotonNetwork.playerList访问在房间里现有的玩家。
    /// 同时，所有自定义属性Room.customProperties应该已经可用。检查Room.playerCount就知道房间里是否有足够的玩家来开始游戏.
    /// </remarks>
    void OnJoinedRoom();

    /// <summary>
	/// 当一个远程玩家进入房间时调用。这个PhotonPlayer在这个时候已经被添加playerlist玩家列表.
    /// </summary>
    /// <remarks>
    /// 如果你的游戏开始时就有一定数量的玩家，这个回调在检查Room.playerCount并发现你是否可以开始游戏时会很有用.
    /// </remarks>
    void OnPhotonPlayerConnected(PhotonPlayer newPlayer);

    /// <summary>
	/// 当一个远程玩家离开房间时调用。这个PhotonPlayer 此时已经从playerlist玩家列表删除.
    /// </summary>
    /// <remarks>
	/// 当你的客户端调用PhotonNetwork.leaveRoom时，PUN将在现有的客户端上调用此方法。当远程客户端关闭连接或被关闭时，这个回调函数会在经过几秒钟的暂停后被执行.
    /// </remarks>
    void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer);

    /// <summary>
	/// 在一个JoinRandom()请求失败后调用。参数提供ErrorCode错误代码和消息。
    /// </summary>
    /// <remarks>
	/// 最有可能所有的房间都是满的或没有房间是可用的. <br/>
	/// 当使用多个大厅（通过JoinLobby或TypedLobby），另一个大厅可能有更多/合适的房间.<br/>
	/// 如果PhotonNetwork.logLevel >= PhotonLogLevel.Informational为真，PUN记录一些信息.
    /// </remarks>
	/// <param name="codeAndMsg">codeAndMsg[0] 是short类型的ErrorCode， codeAndMsg[1] 是string类型的调试信息debug msg.</param>
    void OnPhotonRandomJoinFailed(object[] codeAndMsg);

    /// <summary>
	/// 在到主服务器连接被建立和认证后调用，但是只有当PhotonNetwork.autoJoinLobby是false时才调用.
    /// </summary>
    /// <remarks>
	/// 如果你设置PhotonNetwork.autoJoinLobby为true，取而代之调用的是OnJoinedLobby().
    ///
	/// 即使没有在游戏大厅内，你也可以加入房间和创建房间。在这种情况下使用了默认的大厅。
	/// 可用房间列表将不可用，除非你通过PhotonNetwork.joinLobby加入一个游戏大厅.
    /// </remarks>
    void OnConnectedToMaster();

    /// <summary>
	/// 由于并发用户限制（暂时）到达了，该客户端会被服务器拒绝并断连.
    /// </summary>
    /// <remarks>
	/// 当这种情况发生时，用户可能会等一会儿再试一次。在OnPhotonMaxCcuReached()触发时你不能创建或加入房间，因为客户端会断开连接.
	/// 你可以用一个新的许可证提高CCU[CCU在上文中提过，是高并发用户的缩写。]的限制（当你使用自己的主机时）或扩展订阅（当使用Photon云服务时）.
	/// Photon云会在达到CCU极限时给你发邮件。这也是在仪表板（网页）中可见的.
    /// </remarks>
    void OnPhotonMaxCccuReached();

    /// <summary>
	/// 当一个房间的自定义属性更改时被调用。propertiesThatChanged改变的属性包含所有通过Room.SetCustomProperties设置的.
    /// </summary>
    /// <remarks>
	/// 自从v1.25版本这个方法就有一个参数：Hashtable propertiesThatChanged.<br/>
	/// 更改属性必须由Room.SetCustomProperties完成，导致这个回调函数局限在本地.
    /// </remarks>
    /// <param name="propertiesThatChanged">改变了的属性</param>
    void OnPhotonCustomRoomPropertiesChanged(Hashtable propertiesThatChanged);

    /// <summary>
	/// 当自定义玩家属性更改时调用。玩家和更改的属性被传递为对象数组object[].
    /// </summary>
    /// <remarks>
	/// 自从v1.25版本这个方法就有一个参数：object[] playerAndUpdatedProps，其中包含两个条目.<br/>
	/// [0]是被影响的PhotonPlayer玩家.<br/>
	/// [1]是那些改变了的属性的哈希表.<br/>
    ///
	/// 我们使用一个对象数组object[]是由于Unity的GameObject.SendMessage方法的限制（该方法只有一个可选的参数）.
    ///
	/// 更改属性必须由PhotonPlayer.SetCustomProperties完成，导致这个回调局限在本地.
    ///
    /// 案例:<pre>
    /// void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps) {
    ///     PhotonPlayer player = playerAndUpdatedProps[0] as PhotonPlayer;
    ///     Hashtable props = playerAndUpdatedProps[1] as Hashtable;
    ///     //...
    /// }</pre>
    /// </remarks>
    /// <param name="playerAndUpdatedProps">包含PhotonPlayer和已更改的属性.</param>
    void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps);

    /// <summary>
	/// 当服务器发送一个对FindFriends请求的响应并更新PhotonNetwork.Friends时调用.
    /// </summary>
    /// <remarks>
	/// 好友列表可作为PhotonNetwork.Friends、列出的姓名、在线状态和玩家所在的房间（如果有）.
    /// </remarks>
    void OnUpdatedFriendList();

    /// <summary>
	/// 当自定义身份验证失败时调用。接下来就会断开连接!
    /// </summary>
    /// <remarks>
	/// 自定义身份验证可能会失败，由于用户输入、坏令牌/密匙.
	/// 如果身份验证是成功的，这种方法不被调用。实现OnJoinedLobby()或OnConnectedToMaster()（像往常一样）.
    ///
	/// 在游戏开发过程中，它也可能由于服务器端的配置错误而失败.
	/// 在这些情况下，记录debugMessage调试消息是非常重要的.
    ///
	/// 除非您为应用程序（在仪表板[Dashboard](https://www.photonengine.com/dashboard)）设置一个自定义的身份验证服务，否则将不会被调用!
    /// </remarks>
    /// <param name="debugMessage">包含一个为什么授权失败的调试消息.这个必须在开放期间解决.</param>
    void OnCustomAuthenticationFailed(string debugMessage);

    /// <summary>
	/// 当您的自定义身份验证服务用附加数据响应时调用.
    /// </summary>
    /// <remarks>
	/// 自定义身份验证服务可以在响应中包含一些自定义数据.
	/// 当存在时，该数据是在回调函数中作为字典使其可用.
	/// 而你的数据的键值必须是字符串，值类型可以是字符串或数字（以JSON形式）.
	/// 你需要额外确定，这个值类型是你期望的。数字成为（目前）Int64.
    ///
    /// 案例: void OnCustomAuthenticationResponse(Dictionary&lt;string, object&gt; data) { ... }
    /// </remarks>
    /// <see cref="https://doc.photonengine.com/en/realtime/current/reference/custom-authentication"/>
    void OnCustomAuthenticationResponse(Dictionary<string, object> data);

    /// <summary>
	/// 当一个WebRPC的回应可用时被PUN调用。详见PhotonNetwork.WebRPC.
    /// </summary>
    /// <remarks>
	/// 重要：如果Photon可以使用你的网页服务response.ReturnCode是0.<br/>
	/// 回应的内容是你的网页服务发送的。你可以从中创建一个WebResponse实例.<br/>
    /// 案例: WebRpcResponse webResponse = new WebRpcResponse(operationResponse);<br/>
    ///
	/// 请注意:OperationResponse类是在需要被使用的一个命名空间里:<br/>
	/// using ExitGames. Client.Photon; //包含OperationResponse（和其他的类）
    ///
	/// Photon的OperationResponse.ReturnCode返回代码:<pre>
	///  0代表“OK”
	/// -3代表“网页服务没有配置”（详见仪表盘/ 网页钩子）
	/// -5代表“网页服务现在有RPC路径/名称”（至少是Azure）</pre>
    /// </remarks>
    void OnWebRpcResponse(OperationResponse response);

    /// <summary>
	/// 当另一个玩家从你（现在的所有者）这里请求一个PhotonView的所有权时调用.
    /// </summary>
    /// <remarks>
	/// 参数viewAndPlayer包含：
    ///
    /// PhotonView view = viewAndPlayer[0] as PhotonView;
    ///
    /// PhotonPlayer requestingPlayer = viewAndPlayer[1] as PhotonPlayer;
    /// </remarks>
    /// <param name="viewAndPlayer">PhotonView是viewAndPlayer[0]且请求的玩家是viewAndPlayer[1].</param>
    void OnOwnershipRequest(object[] viewAndPlayer);

    /// <summary>
	/// 当主服务器发送一个游戏大厅统计更新、更新PhotonNetwork.LobbyStatistics时调用.
    /// </summary>
    /// <remarks>
	/// 这个回调有两个前提条件:
	/// EnableLobbyStatistics必须设置为true，在客户端连接之前.
	/// 并且客户端必须连接到主服务器，提供关于大堂的信息.
    /// </remarks>
    void OnLobbyStatisticsUpdate();
}

/// <summary>
/// Defines all the methods that a Object Pool must implement, so that PUN can use it.
/// </summary>
/// <remarks>
/// To use a Object Pool for instantiation, you can set PhotonNetwork.ObjectPool.
/// That is used for all objects, as long as ObjectPool is not null.
/// The pool has to return a valid non-null GameObject when PUN calls Instantiate.
/// Also, the position and rotation must be applied.
///
/// Please note that pooled GameObjects don't get the usual Awake and Start calls.
/// OnEnable will be called (by your pool) but the networking values are not updated yet
/// when that happens. OnEnable will have outdated values for PhotonView (isMine, etc.).
/// You might have to adjust scripts.
///
/// PUN will call OnPhotonInstantiate (see IPunCallbacks). This should be used to
/// setup the re-used object with regards to networking values / ownership.
/// </remarks>
public interface IPunPrefabPool
{
    /// <summary>
    /// This is called when PUN wants to create a new instance of an entity prefab. Must return valid GameObject with PhotonView.
    /// </summary>
    /// <param name="prefabId">The id of this prefab.</param>
    /// <param name="position">The position we want the instance instantiated at.</param>
    /// <param name="rotation">The rotation we want the instance to take.</param>
    /// <returns>The newly instantiated object, or null if a prefab with <paramref name="prefabId"/> was not found.</returns>
    GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation);

    /// <summary>
    /// This is called when PUN wants to destroy the instance of an entity prefab.
    /// </summary>
	/// <remarks>
	/// A pool needs some way to find out which type of GameObject got returned via Destroy().
	/// It could be a tag or name or anything similar.
	/// </remarks>
    /// <param name="gameObject">The instance to destroy.</param>
    void Destroy(GameObject gameObject);
}


namespace Photon
{
    using Hashtable = ExitGames.Client.Photon.Hashtable;

    /// <summary>
    /// This class adds the property photonView, while logging a warning when your game still uses the networkView.
    /// </summary>
    public class MonoBehaviour : UnityEngine.MonoBehaviour
    {
        /// <summary>Cache field for the PhotonView on this GameObject.</summary>
        private PhotonView pvCache = null;

        /// <summary>A cached reference to a PhotonView on this GameObject.</summary>
        /// <remarks>
        /// If you intend to work with a PhotonView in a script, it's usually easier to write this.photonView.
        ///
        /// If you intend to remove the PhotonView component from the GameObject but keep this Photon.MonoBehaviour,
        /// avoid this reference or modify this code to use PhotonView.Get(obj) instead.
        /// </remarks>
        public PhotonView photonView
        {
            get
            {
                if (pvCache == null)
                {
                    pvCache = PhotonView.Get(this);
                }
                return pvCache;
            }
        }

        #if !UNITY_MIN_5_3
        /// <summary>
        /// This property is only here to notify developers when they use the outdated value.
        /// </summary>
        /// <remarks>
        /// If Unity 5.x logs a compiler warning "Use the new keyword if hiding was intended" or
        /// "The new keyword is not required", you may suffer from an Editor issue.
        /// Try to modify networkView with a if-def condition:
        ///
        /// #if UNITY_EDITOR
        /// new
        /// #endif
        /// public PhotonView networkView
        /// </remarks>
        [Obsolete("Use a photonView")]
        public new PhotonView networkView
        {
            get
            {
                Debug.LogWarning("Why are you still using networkView? should be PhotonView?");
                return PhotonView.Get(this);
            }
        }
        #endif
    }


    /// <summary>
    /// This class provides a .photonView and all callbacks/events that PUN can call. Override the events/methods you want to use.
    /// </summary>
    /// <remarks>
    /// By extending this class, you can implement individual methods as override.
    ///
    /// Visual Studio and MonoDevelop should provide the list of methods when you begin typing "override".
    /// <b>Your implementation does not have to call "base.method()".</b>
    ///
    /// This class implements IPunCallbacks, which is used as definition of all PUN callbacks.
    /// Don't implement IPunCallbacks in your classes. Instead, implent PunBehaviour or individual methods.
    /// </remarks>
    /// \ingroup publicApi
    // the documentation for the interface methods becomes inherited when Doxygen builds it.
    public class PunBehaviour : Photon.MonoBehaviour, IPunCallbacks
    {
        /// <summary>
        /// Called when the initial connection got established but before you can use the server. OnJoinedLobby() or OnConnectedToMaster() are called when PUN is ready.
        /// </summary>
        /// <remarks>
        /// This callback is only useful to detect if the server can be reached at all (technically).
        /// Most often, it's enough to implement OnFailedToConnectToPhoton() and OnDisconnectedFromPhoton().
        ///
        /// <i>OnJoinedLobby() or OnConnectedToMaster() are called when PUN is ready.</i>
        ///
        /// When this is called, the low level connection is established and PUN will send your AppId, the user, etc in the background.
        /// This is not called for transitions from the masterserver to game servers.
        /// </remarks>
        public virtual void OnConnectedToPhoton()
        {
        }

        /// <summary>
        /// Called when the local user/client left a room.
        /// </summary>
        /// <remarks>
        /// When leaving a room, PUN brings you back to the Master Server.
        /// Before you can use lobbies and join or create rooms, OnJoinedLobby() or OnConnectedToMaster() will get called again.
        /// </remarks>
        public virtual void OnLeftRoom()
        {
        }

        /// <summary>
        /// Called after switching to a new MasterClient when the current one leaves.
        /// </summary>
        /// <remarks>
        /// This is not called when this client enters a room.
        /// The former MasterClient is still in the player list when this method get called.
        /// </remarks>
        public virtual void OnMasterClientSwitched(PhotonPlayer newMasterClient)
        {
        }

        /// <summary>
        /// Called when a CreateRoom() call failed. The parameter provides ErrorCode and message (as array).
        /// </summary>
        /// <remarks>
        /// Most likely because the room name is already in use (some other client was faster than you).
        /// PUN logs some info if the PhotonNetwork.logLevel is >= PhotonLogLevel.Informational.
        /// </remarks>
        /// <param name="codeAndMsg">codeAndMsg[0] is a short ErrorCode and codeAndMsg[1] is a string debug msg.</param>
        public virtual void OnPhotonCreateRoomFailed(object[] codeAndMsg)
        {
        }

        /// <summary>
        /// Called when a JoinRoom() call failed. The parameter provides ErrorCode and message (as array).
        /// </summary>
        /// <remarks>
        /// Most likely error is that the room does not exist or the room is full (some other client was faster than you).
        /// PUN logs some info if the PhotonNetwork.logLevel is >= PhotonLogLevel.Informational.
        /// </remarks>
        /// <param name="codeAndMsg">codeAndMsg[0] is short ErrorCode. codeAndMsg[1] is string debug msg.</param>
        public virtual void OnPhotonJoinRoomFailed(object[] codeAndMsg)
        {
        }

        /// <summary>
        /// Called when this client created a room and entered it. OnJoinedRoom() will be called as well.
        /// </summary>
        /// <remarks>
        /// This callback is only called on the client which created a room (see PhotonNetwork.CreateRoom).
        ///
        /// As any client might close (or drop connection) anytime, there is a chance that the
        /// creator of a room does not execute OnCreatedRoom.
        ///
        /// If you need specific room properties or a "start signal", it is safer to implement
        /// OnMasterClientSwitched() and to make the new MasterClient check the room's state.
        /// </remarks>
        public virtual void OnCreatedRoom()
        {
        }

        /// <summary>
        /// Called on entering a lobby on the Master Server. The actual room-list updates will call OnReceivedRoomListUpdate().
        /// </summary>
        /// <remarks>
        /// Note: When PhotonNetwork.autoJoinLobby is false, OnConnectedToMaster() will be called and the room list won't become available.
        ///
        /// While in the lobby, the roomlist is automatically updated in fixed intervals (which you can't modify).
        /// The room list gets available when OnReceivedRoomListUpdate() gets called after OnJoinedLobby().
        /// </remarks>
        public virtual void OnJoinedLobby()
        {
        }

        /// <summary>
        /// Called after leaving a lobby.
        /// </summary>
        /// <remarks>
        /// When you leave a lobby, [CreateRoom](@ref PhotonNetwork.CreateRoom) and [JoinRandomRoom](@ref PhotonNetwork.JoinRandomRoom)
        /// automatically refer to the default lobby.
        /// </remarks>
        public virtual void OnLeftLobby()
        {
        }

        /// <summary>
        /// Called if a connect call to the Photon server failed before the connection was established, followed by a call to OnDisconnectedFromPhoton().
        /// </summary>
        /// <remarks>
        /// This is called when no connection could be established at all.
        /// It differs from OnConnectionFail, which is called when an existing connection fails.
        /// </remarks>
        public virtual void OnFailedToConnectToPhoton(DisconnectCause cause)
        {
        }

        /// <summary>
        /// Called after disconnecting from the Photon server.
        /// </summary>
        /// <remarks>
        /// In some cases, other callbacks are called before OnDisconnectedFromPhoton is called.
        /// Examples: OnConnectionFail() and OnFailedToConnectToPhoton().
        /// </remarks>
        public virtual void OnDisconnectedFromPhoton()
        {
        }

        /// <summary>
        /// Called when something causes the connection to fail (after it was established), followed by a call to OnDisconnectedFromPhoton().
        /// </summary>
        /// <remarks>
        /// If the server could not be reached in the first place, OnFailedToConnectToPhoton is called instead.
        /// The reason for the error is provided as DisconnectCause.
        /// </remarks>
        public virtual void OnConnectionFail(DisconnectCause cause)
        {
        }

        /// <summary>
        /// Called on all scripts on a GameObject (and children) that have been Instantiated using PhotonNetwork.Instantiate.
        /// </summary>
        /// <remarks>
        /// PhotonMessageInfo parameter provides info about who created the object and when (based off PhotonNetworking.time).
        /// </remarks>
        public virtual void OnPhotonInstantiate(PhotonMessageInfo info)
        {
        }

        /// <summary>
        /// Called for any update of the room-listing while in a lobby (PhotonNetwork.insideLobby) on the Master Server.
        /// </summary>
        /// <remarks>
        /// PUN provides the list of rooms by PhotonNetwork.GetRoomList().<br/>
        /// Each item is a RoomInfo which might include custom properties (provided you defined those as lobby-listed when creating a room).
        ///
        /// Not all types of lobbies provide a listing of rooms to the client. Some are silent and specialized for server-side matchmaking.
        /// </remarks>
        public virtual void OnReceivedRoomListUpdate()
        {
        }

        /// <summary>
        /// Called when entering a room (by creating or joining it). Called on all clients (including the Master Client).
        /// </summary>
        /// <remarks>
        /// This method is commonly used to instantiate player characters.
        /// If a match has to be started "actively", you can call an [PunRPC](@ref PhotonView.RPC) triggered by a user's button-press or a timer.
        ///
        /// When this is called, you can usually already access the existing players in the room via PhotonNetwork.playerList.
        /// Also, all custom properties should be already available as Room.customProperties. Check Room.playerCount to find out if
        /// enough players are in the room to start playing.
        /// </remarks>
        public virtual void OnJoinedRoom()
        {
        }

        /// <summary>
        /// Called when a remote player entered the room. This PhotonPlayer is already added to the playerlist at this time.
        /// </summary>
        /// <remarks>
        /// If your game starts with a certain number of players, this callback can be useful to check the
        /// Room.playerCount and find out if you can start.
        /// </remarks>
        public virtual void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
        {
        }

        /// <summary>
        /// Called when a remote player left the room. This PhotonPlayer is already removed from the playerlist at this time.
        /// </summary>
        /// <remarks>
        /// When your client calls PhotonNetwork.leaveRoom, PUN will call this method on the remaining clients.
        /// When a remote client drops connection or gets closed, this callback gets executed. after a timeout
        /// of several seconds.
        /// </remarks>
        public virtual void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
        {
        }

        /// <summary>
        /// Called when a JoinRandom() call failed. The parameter provides ErrorCode and message.
        /// </summary>
        /// <remarks>
        /// Most likely all rooms are full or no rooms are available. <br/>
        /// When using multiple lobbies (via JoinLobby or TypedLobby), another lobby might have more/fitting rooms.<br/>
        /// PUN logs some info if the PhotonNetwork.logLevel is >= PhotonLogLevel.Informational.
        /// </remarks>
        /// <param name="codeAndMsg">codeAndMsg[0] is short ErrorCode. codeAndMsg[1] is string debug msg.</param>
        public virtual void OnPhotonRandomJoinFailed(object[] codeAndMsg)
        {
        }

        /// <summary>
        /// Called after the connection to the master is established and authenticated but only when PhotonNetwork.autoJoinLobby is false.
        /// </summary>
        /// <remarks>
        /// If you set PhotonNetwork.autoJoinLobby to true, OnJoinedLobby() will be called instead of this.
        ///
        /// You can join rooms and create them even without being in a lobby. The default lobby is used in that case.
        /// The list of available rooms won't become available unless you join a lobby via PhotonNetwork.joinLobby.
        /// </remarks>
        public virtual void OnConnectedToMaster()
        {
        }

        /// <summary>
        /// Because the concurrent user limit was (temporarily) reached, this client is rejected by the server and disconnecting.
        /// </summary>
        /// <remarks>
        /// When this happens, the user might try again later. You can't create or join rooms in OnPhotonMaxCcuReached(), cause the client will be disconnecting.
        /// You can raise the CCU limits with a new license (when you host yourself) or extended subscription (when using the Photon Cloud).
        /// The Photon Cloud will mail you when the CCU limit was reached. This is also visible in the Dashboard (webpage).
        /// </remarks>
        public virtual void OnPhotonMaxCccuReached()
        {
        }

        /// <summary>
        /// Called when a room's custom properties changed. The propertiesThatChanged contains all that was set via Room.SetCustomProperties.
        /// </summary>
        /// <remarks>
        /// Since v1.25 this method has one parameter: Hashtable propertiesThatChanged.<br/>
        /// Changing properties must be done by Room.SetCustomProperties, which causes this callback locally, too.
        /// </remarks>
        /// <param name="propertiesThatChanged"></param>
        public virtual void OnPhotonCustomRoomPropertiesChanged(Hashtable propertiesThatChanged)
        {
        }

        /// <summary>
        /// Called when custom player-properties are changed. Player and the changed properties are passed as object[].
        /// </summary>
        /// <remarks>
        /// Since v1.25 this method has one parameter: object[] playerAndUpdatedProps, which contains two entries.<br/>
        /// [0] is the affected PhotonPlayer.<br/>
        /// [1] is the Hashtable of properties that changed.<br/>
        ///
        /// We are using a object[] due to limitations of Unity's GameObject.SendMessage (which has only one optional parameter).
        ///
        /// Changing properties must be done by PhotonPlayer.SetCustomProperties, which causes this callback locally, too.
        ///
        /// Example:<pre>
        /// void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps) {
        ///     PhotonPlayer player = playerAndUpdatedProps[0] as PhotonPlayer;
        ///     Hashtable props = playerAndUpdatedProps[1] as Hashtable;
        ///     //...
        /// }</pre>
        /// </remarks>
        /// <param name="playerAndUpdatedProps">Contains PhotonPlayer and the properties that changed See remarks.</param>
        public virtual void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
        {
        }

        /// <summary>
        /// Called when the server sent the response to a FindFriends request and updated PhotonNetwork.Friends.
        /// </summary>
        /// <remarks>
        /// The friends list is available as PhotonNetwork.Friends, listing name, online state and
        /// the room a user is in (if any).
        /// </remarks>
        public virtual void OnUpdatedFriendList()
        {
        }

        /// <summary>
        /// Called when the custom authentication failed. Followed by disconnect!
        /// </summary>
        /// <remarks>
        /// Custom Authentication can fail due to user-input, bad tokens/secrets.
        /// If authentication is successful, this method is not called. Implement OnJoinedLobby() or OnConnectedToMaster() (as usual).
        ///
        /// During development of a game, it might also fail due to wrong configuration on the server side.
        /// In those cases, logging the debugMessage is very important.
        ///
        /// Unless you setup a custom authentication service for your app (in the [Dashboard](https://www.photonengine.com/dashboard)),
        /// this won't be called!
        /// </remarks>
        /// <param name="debugMessage">Contains a debug message why authentication failed. This has to be fixed during development time.</param>
        public virtual void OnCustomAuthenticationFailed(string debugMessage)
        {
        }

        /// <summary>
        /// Called when your Custom Authentication service responds with additional data.
        /// </summary>
        /// <remarks>
        /// Custom Authentication services can include some custom data in their response.
        /// When present, that data is made available in this callback as Dictionary.
        /// While the keys of your data have to be strings, the values can be either string or a number (in Json).
        /// You need to make extra sure, that the value type is the one you expect. Numbers become (currently) int64.
        ///
        /// Example: void OnCustomAuthenticationResponse(Dictionary&lt;string, object&gt; data) { ... }
        /// </remarks>
        /// <see cref="https://doc.photonengine.com/en/realtime/current/reference/custom-authentication"/>
        public virtual void OnCustomAuthenticationResponse(Dictionary<string, object> data)
        {
        }

        /// <summary>
        /// Called by PUN when the response to a WebRPC is available. See PhotonNetwork.WebRPC.
        /// </summary>
        /// <remarks>
        /// Important: The response.ReturnCode is 0 if Photon was able to reach your web-service.
        /// The content of the response is what your web-service sent. You can create a WebResponse instance from it.
        /// Example: WebRpcResponse webResponse = new WebRpcResponse(operationResponse);
        ///
        /// Please note: Class OperationResponse is in a namespace which needs to be "used":
        /// using ExitGames.Client.Photon;  // includes OperationResponse (and other classes)
        ///
        /// The OperationResponse.ReturnCode by Photon is:<pre>
        ///  0 for "OK"
        /// -3 for "Web-Service not configured" (see Dashboard / WebHooks)
        /// -5 for "Web-Service does now have RPC path/name" (at least for Azure)</pre>
        /// </remarks>
        public virtual void OnWebRpcResponse(OperationResponse response)
        {
        }

        /// <summary>
        /// Called when another player requests ownership of a PhotonView from you (the current owner).
        /// </summary>
        /// <remarks>
        /// The parameter viewAndPlayer contains:
        ///
        /// PhotonView view = viewAndPlayer[0] as PhotonView;
        ///
        /// PhotonPlayer requestingPlayer = viewAndPlayer[1] as PhotonPlayer;
        /// </remarks>
        /// <param name="viewAndPlayer">The PhotonView is viewAndPlayer[0] and the requesting player is viewAndPlayer[1].</param>
        public virtual void OnOwnershipRequest(object[] viewAndPlayer)
        {
        }

        /// <summary>
        /// Called when the Master Server sent an update for the Lobby Statistics, updating PhotonNetwork.LobbyStatistics.
        /// </summary>
        /// <remarks>
        /// This callback has two preconditions:
        /// EnableLobbyStatistics must be set to true, before this client connects.
        /// And the client has to be connected to the Master Server, which is providing the info about lobbies.
        /// </remarks>
        public virtual void OnLobbyStatisticsUpdate()
        {
        }
    }
}


/// <summary>
/// Container class for info about a particular message, RPC or update.
/// </summary>
/// \ingroup publicApi
public struct PhotonMessageInfo
{
    private readonly int timeInt;
    public readonly PhotonPlayer sender;
    public readonly PhotonView photonView;

    public PhotonMessageInfo(PhotonPlayer player, int timestamp, PhotonView view)
    {
        this.sender = player;
        this.timeInt = timestamp;
        this.photonView = view;
    }

    public double timestamp
    {
        get
        {
            uint u = (uint)this.timeInt;
            double t = u;
            return t / 1000;
        }
    }

    public override string ToString()
    {
        return string.Format("[PhotonMessageInfo: Sender='{1}' Senttime={0}]", this.timestamp, this.sender);
    }
}



/// <summary>Defines Photon event-codes as used by PUN.</summary>
internal class PunEvent
{
    public const byte RPC = 200;
    public const byte SendSerialize = 201;
    public const byte Instantiation = 202;
    public const byte CloseConnection = 203;
    public const byte Destroy = 204;
    public const byte RemoveCachedRPCs = 205;
    public const byte SendSerializeReliable = 206;  // TS: added this but it's not really needed anymore
    public const byte DestroyPlayer = 207;  // TS: added to make others remove all GOs of a player
    public const byte AssignMaster = 208;  // TS: added to assign someone master client (overriding the current)
    public const byte OwnershipRequest = 209;
    public const byte OwnershipTransfer = 210;
    public const byte VacantViewIds = 211;
}

/// <summary>
/// This container is used in OnPhotonSerializeView() to either provide incoming data of a PhotonView or for you to provide it.
/// </summary>
/// <remarks>
/// The isWriting property will be true if this client is the "owner" of the PhotonView (and thus the GameObject).
/// Add data to the stream and it's sent via the server to the other players in a room.
/// On the receiving side, isWriting is false and the data should be read.
///
/// Send as few data as possible to keep connection quality up. An empty PhotonStream will not be sent.
///
/// Use either Serialize() for reading and writing or SendNext() and ReceiveNext(). The latter two are just explicit read and
/// write methods but do about the same work as Serialize(). It's a matter of preference which methods you use.
/// </remarks>
/// <seealso cref="PhotonNetworkingMessage"/>
/// \ingroup publicApi
public class PhotonStream
{
    bool write = false;
    private Queue<object> writeData;
    private object[] readData;
    internal byte currentItem = 0; //Used to track the next item to receive.

    /// <summary>
    /// Creates a stream and initializes it. Used by PUN internally.
    /// </summary>
    public PhotonStream(bool write, object[] incomingData)
    {
        this.write = write;
        if (incomingData == null)
        {
            this.writeData = new Queue<object>(10);
        }
        else
        {
            this.readData = incomingData;
        }
    }

    public void SetReadStream(object[] incomingData, byte pos = 0)
    {
        this.readData = incomingData;
        this.currentItem = pos;
        this.write = false;
    }

    internal void ResetWriteStream()
    {
        writeData.Clear();
    }

    /// <summary>If true, this client should add data to the stream to send it.</summary>
    public bool isWriting
    {
        get { return this.write; }
    }

    /// <summary>If true, this client should read data send by another client.</summary>
    public bool isReading
    {
        get { return !this.write; }
    }

    /// <summary>Count of items in the stream.</summary>
    public int Count
    {
        get
        {
            return (this.isWriting) ? this.writeData.Count : this.readData.Length;
        }
    }

    /// <summary>Read next piece of data from the stream when isReading is true.</summary>
    public object ReceiveNext()
    {
        if (this.write)
        {
            Debug.LogError("Error: you cannot read this stream that you are writing!");
            return null;
        }

        object obj = this.readData[this.currentItem];
        this.currentItem++;
        return obj;
    }

    /// <summary>Read next piece of data from the stream without advancing the "current" item.</summary>
    public object PeekNext()
    {
        if (this.write)
        {
            Debug.LogError("Error: you cannot read this stream that you are writing!");
            return null;
        }

        object obj = this.readData[this.currentItem];
        //this.currentItem++;
        return obj;
    }

    /// <summary>Add another piece of data to send it when isWriting is true.</summary>
    public void SendNext(object obj)
    {
        if (!this.write)
        {
            Debug.LogError("Error: you cannot write/send to this stream that you are reading!");
            return;
        }

        this.writeData.Enqueue(obj);
    }

    /// <summary>Turns the stream into a new object[].</summary>
    public object[] ToArray()
    {
        return this.isWriting ? this.writeData.ToArray() : this.readData;
    }

    /// <summary>
    /// Will read or write the value, depending on the stream's isWriting value.
    /// </summary>
    public void Serialize(ref bool myBool)
    {
        if (this.write)
        {
            this.writeData.Enqueue(myBool);
        }
        else
        {
            if (this.readData.Length > currentItem)
            {
                myBool = (bool)this.readData[currentItem];
                this.currentItem++;
            }
        }
    }

    /// <summary>
    /// Will read or write the value, depending on the stream's isWriting value.
    /// </summary>
    public void Serialize(ref int myInt)
    {
        if (write)
        {
            this.writeData.Enqueue(myInt);
        }
        else
        {
            if (this.readData.Length > currentItem)
            {
                myInt = (int)this.readData[currentItem];
                currentItem++;
            }
        }
    }

    /// <summary>
    /// Will read or write the value, depending on the stream's isWriting value.
    /// </summary>
    public void Serialize(ref string value)
    {
        if (write)
        {
            this.writeData.Enqueue(value);
        }
        else
        {
            if (this.readData.Length > currentItem)
            {
                value = (string)this.readData[currentItem];
                currentItem++;
            }
        }
    }

    /// <summary>
    /// Will read or write the value, depending on the stream's isWriting value.
    /// </summary>
    public void Serialize(ref char value)
    {
        if (write)
        {
            this.writeData.Enqueue(value);
        }
        else
        {
            if (this.readData.Length > currentItem)
            {
                value = (char)this.readData[currentItem];
                currentItem++;
            }
        }
    }

    /// <summary>
    /// Will read or write the value, depending on the stream's isWriting value.
    /// </summary>
    public void Serialize(ref short value)
    {
        if (write)
        {
            this.writeData.Enqueue(value);
        }
        else
        {
            if (this.readData.Length > currentItem)
            {
                value = (short)this.readData[currentItem];
                currentItem++;
            }
        }
    }

    /// <summary>
    /// Will read or write the value, depending on the stream's isWriting value.
    /// </summary>
    public void Serialize(ref float obj)
    {
        if (write)
        {
            this.writeData.Enqueue(obj);
        }
        else
        {
            if (this.readData.Length > currentItem)
            {
                obj = (float)this.readData[currentItem];
                currentItem++;
            }
        }
    }

    /// <summary>
    /// Will read or write the value, depending on the stream's isWriting value.
    /// </summary>
    public void Serialize(ref PhotonPlayer obj)
    {
        if (write)
        {
            this.writeData.Enqueue(obj);
        }
        else
        {
            if (this.readData.Length > currentItem)
            {
                obj = (PhotonPlayer)this.readData[currentItem];
                currentItem++;
            }
        }
    }

    /// <summary>
    /// Will read or write the value, depending on the stream's isWriting value.
    /// </summary>
    public void Serialize(ref Vector3 obj)
    {
        if (write)
        {
            this.writeData.Enqueue(obj);
        }
        else
        {
            if (this.readData.Length > currentItem)
            {
                obj = (Vector3)this.readData[currentItem];
                currentItem++;
            }
        }
    }

    /// <summary>
    /// Will read or write the value, depending on the stream's isWriting value.
    /// </summary>
    public void Serialize(ref Vector2 obj)
    {
        if (write)
        {
            this.writeData.Enqueue(obj);
        }
        else
        {
            if (this.readData.Length > currentItem)
            {
                obj = (Vector2)this.readData[currentItem];
                currentItem++;
            }
        }
    }

    /// <summary>
    /// Will read or write the value, depending on the stream's isWriting value.
    /// </summary>
    public void Serialize(ref Quaternion obj)
    {
        if (write)
        {
            this.writeData.Enqueue(obj);
        }
        else
        {
            if (this.readData.Length > currentItem)
            {
                obj = (Quaternion)this.readData[currentItem];
                currentItem++;
            }
        }
    }
}


#if UNITY_5_0 || !UNITY_5
/// <summary>Empty implementation of the upcoming HelpURL of Unity 5.1. This one is only for compatibility of attributes.</summary>
/// <remarks>http://feedback.unity3d.com/suggestions/override-component-documentation-slash-help-link</remarks>
public class HelpURL : Attribute
{
    public HelpURL(string url)
    {
    }
}
#endif


#if !UNITY_MIN_5_3
// in Unity 5.3 and up, we have to use a SceneManager. This section re-implements it for older Unity versions

#if UNITY_EDITOR
namespace UnityEditor.SceneManagement
{
    /// <summary>Minimal implementation of the EditorSceneManager for older Unity, up to v5.2.</summary>
    public class EditorSceneManager
    {
        public static int loadedSceneCount
        {
            get { return string.IsNullOrEmpty(UnityEditor.EditorApplication.currentScene) ? -1 : 1; }
        }

        public static void OpenScene(string name)
        {
            UnityEditor.EditorApplication.OpenScene(name);
        }

        public static void SaveOpenScenes()
        {
            UnityEditor.EditorApplication.SaveScene();
        }

        public static void SaveCurrentModifiedScenesIfUserWantsTo()
        {
            UnityEditor.EditorApplication.SaveCurrentSceneIfUserWantsTo();
        }
    }
}
#endif

namespace UnityEngine.SceneManagement
{
    /// <summary>Minimal implementation of the SceneManager for older Unity, up to v5.2.</summary>
    public class SceneManager
    {
        public static void LoadScene(string name)
        {
            Application.LoadLevel(name);
        }

        public static void LoadScene(int buildIndex)
        {
            Application.LoadLevel(buildIndex);
        }
    }
}

#endif


public class SceneManagerHelper
{
    public static string ActiveSceneName
    {
        get
        {
            #if UNITY_MIN_5_3
            UnityEngine.SceneManagement.Scene s = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            return s.name;
            #else
            return Application.loadedLevelName;
            #endif
        }
    }

    public static int ActiveSceneBuildIndex
    {
        get
        {
            #if UNITY_MIN_5_3
            return UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
            #else
            return Application.loadedLevel;
            #endif
        }
    }


#if UNITY_EDITOR
    public static string EditorActiveSceneName
    {
        get
        {
            #if UNITY_MIN_5_3
            return UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().name;
            #else
            return System.IO.Path.GetFileNameWithoutExtension(UnityEditor.EditorApplication.currentScene);
            #endif
        }
    }
#endif
}


/// <summary>Reads an operation response of a WebRpc and provides convenient access to most common values.</summary>
/// <remarks>
/// See method PhotonNetwork.WebRpc.<br/>
/// Create a WebRpcResponse to access common result values.<br/>
/// The operationResponse.OperationCode should be: OperationCode.WebRpc.<br/>
/// </remarks>
public class WebRpcResponse
{
    /// <summary>Name of the WebRpc that was called.</summary>
    public string Name { get; private set; }
    /// <summary>ReturnCode of the WebService that answered the WebRpc.</summary>
    /// <remarks>
    /// 0 is commonly used to signal success.<br/>
    /// -1 tells you: Got no ReturnCode from WebRpc service.<br/>
    /// Other ReturnCodes are defined by the individual WebRpc and service.
    /// </remarks>
    public int ReturnCode { get; private set; }
    /// <summary>Might be empty or null.</summary>
    public string DebugMessage { get; private set; }
    /// <summary>Other key/values returned by the webservice that answered the WebRpc.</summary>
    public Dictionary<string, object> Parameters { get; private set; }

    /// <summary>An OperationResponse for a WebRpc is needed to read it's values.</summary>
    public WebRpcResponse(OperationResponse response)
    {
        object value;
        response.Parameters.TryGetValue(ParameterCode.UriPath, out value);
        this.Name = value as string;

        response.Parameters.TryGetValue(ParameterCode.WebRpcReturnCode, out value);
        this.ReturnCode = (value != null) ? (byte)value : -1;

        response.Parameters.TryGetValue(ParameterCode.WebRpcParameters, out value);
        this.Parameters = value as Dictionary<string, object>;

        response.Parameters.TryGetValue(ParameterCode.WebRpcReturnMessage, out value);
        this.DebugMessage = value as string;
    }

    /// <summary>Turns the response into an easier to read string.</summary>
    /// <returns>String resembling the result.</returns>
    public string ToStringFull()
    {
        return string.Format("{0}={2}: {1} \"{3}\"", Name, SupportClassPun.DictionaryToString(Parameters), ReturnCode, DebugMessage);
    }
}

/**
public class PBitStream
{
    List<byte> streamBytes;
    private int currentByte;
    private int totalBits = 0;

    public int ByteCount
    {
        get { return BytesForBits(this.totalBits); }
    }

    public int BitCount
    {
        get { return this.totalBits; }
        private set { this.totalBits = value; }
    }

    public PBitStream()
    {
        this.streamBytes = new List<byte>(1);
    }

    public PBitStream(int bitCount)
    {
        this.streamBytes = new List<byte>(BytesForBits(bitCount));
    }

    public PBitStream(IEnumerable<byte> bytes, int bitCount)
    {
        this.streamBytes = new List<byte>(bytes);
        this.BitCount = bitCount;
    }

    public static int BytesForBits(int bitCount)
    {
        if (bitCount <= 0)
        {
            return 0;
        }

        return ((bitCount - 1) / 8) + 1;
    }

    public void Add(bool val)
    {
        int bytePos = this.totalBits / 8;
        if (bytePos > this.streamBytes.Count-1 || this.totalBits == 0)
        {
            this.streamBytes.Add(0);
        }

        if (val)
        {
            int currentByteBit = 7 - (this.totalBits % 8);
            this.streamBytes[bytePos] |= (byte)(1 << currentByteBit);
        }

        this.totalBits++;
    }

    public byte[] ToBytes()
    {
        return this.streamBytes.ToArray();
    }

    public int Position { get; set; }

    public bool GetNext()
    {
        if (this.Position > this.totalBits)
        {
            throw new Exception("End of PBitStream reached. Can't read more.");
        }

        return Get(this.Position++);
    }

    public bool Get(int bitIndex)
    {
        int byteIndex = bitIndex / 8;
        int bitInByIndex = 7 - (bitIndex % 8);
        return ((this.streamBytes[byteIndex] & (byte)(1 << bitInByIndex)) > 0);
    }

    public void Set(int bitIndex, bool value)
    {
        int byteIndex = bitIndex / 8;
        int bitInByIndex = 7 - (bitIndex % 8);
        this.streamBytes[byteIndex] |= (byte)(1 << bitInByIndex);
    }
}
**/
