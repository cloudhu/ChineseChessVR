// --------------------------------------------------------------------------------------------------------------------
// <copyright file=PlayerChat.cs company=League of HTC Vive Developers>
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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon.Chat;

/// <summary>
/// FileName: PlayerChat.cs
/// Author: 胡良云（CloudHu）
/// Corporation: 
/// Description: 这个脚本用来处理玩家的聊天
/// DateTime: 3/22/2017
/// </summary>
public class PlayerChat : MonoBehaviour,IChatClientListener {
	
	#region Public Variables  //公共变量区域
	
	[Tooltip("连接加入的频道")]
	public string[] ChannelsToJoinOnConnect; // 在Inspector窗口进行设置. 这里会自动加入一个频道

	[Tooltip("好友列表")]
	public string[] FriendsList;

	[Tooltip("聊天历史长度")]
	public int HistoryLengthToFetch; // 在Inspector窗口进行设置.


	public string UserName { get; set; }

	private string selectedChannelName; // 主要用于GUI/input

	[Tooltip("聊天客户端")]
	public ChatClient chatClient;

	[Tooltip("聊天面板")]
	public RectTransform ChatPanel;     //  在Inspector窗口进行设置(用来启用/禁用面板)

	[Tooltip("聊天输入框")]
	public InputField InputFieldChat;   // 在Inspector窗口进行设置

	[Tooltip("当前频道文本")]
	public Text CurrentChannelText;     // 在Inspector窗口进行设置

	[Tooltip("频道开关来实例化")]
	public Toggle ChannelToggleToInstantiate; // 在Inspector窗口进行设置

	[Tooltip("要实例化的好友UI项")]
	public GameObject FriendListUiItemtoInstantiate;

	[Tooltip("是否显示状态")]
	public bool ShowState = true;

	public int TestLength = 2048;

    #endregion


    #region Private Variables   //私有变量区域
    // 保存PlayerPref键来避免错别字
    static string playerNamePrefKey = "PlayerName";

    private byte[] testBytes = new byte[2048];

	private readonly Dictionary<string, Toggle> channelToggles = new Dictionary<string, Toggle>();

	private readonly Dictionary<string,FriendItem> friendListItemLUT =  new Dictionary<string, FriendItem>();

	private static string HelpText = "\n    -- 帮助 --\n" +
		"要订阅频道:\n" +
		"\t<color=#E07B00>\\subscribe</color> <color=green><list of channelnames></color>\n" +
		"\tor\n" +
		"\t<color=#E07B00>\\s</color> <color=green><list of channelnames></color>\n" +
		"\n" +
		"To leave channel(s):\n" +
		"\t<color=#E07B00>\\unsubscribe</color> <color=green><list of channelnames></color>\n" +
		"\tor\n" +
		"\t<color=#E07B00>\\u</color> <color=green><list of channelnames></color>\n" +
		"\n" +
		"To switch the active channel\n" +
		"\t<color=#E07B00>\\join</color> <color=green><channelname></color>\n" +
		"\tor\n" +
		"\t<color=#E07B00>\\j</color> <color=green><channelname></color>\n" +
		"\n" +
		"To send a private message:\n" +
		"\t\\<color=#E07B00>msg</color> <color=green><username></color> <color=green><message></color>\n" +
		"\n" +
		"To change status:\n" +
		"\t\\<color=#E07B00>state</color> <color=green><stateIndex></color> <color=green><message></color>\n" +
		"<color=green>0</color> = Offline " +
		"<color=green>1</color> = Invisible " +
		"<color=green>2</color> = Online " +
		"<color=green>3</color> = Away \n" +
		"<color=green>4</color> = Do not disturb " +
		"<color=green>5</color> = Looking For Group " +
		"<color=green>6</color> = Playing" +
		"\n\n" +
		"To clear the current chat tab (private chats get closed):\n" +
		"\t<color=#E07B00>\\clear</color>";
	
	#endregion
	
	
	#region MonoBehaviour CallBacks //回调函数区域
	public void Start()
	{
		DontDestroyOnLoad(gameObject);
		Application.runInBackground = true; // 必须在后台运行,否则如果没有聚焦到聊天窗口就会断连.

		ChatPanel.gameObject.SetActive(false);

        if (PlayerPrefs.HasKey(playerNamePrefKey))
        {
            
            UserName = PlayerPrefs.GetString(playerNamePrefKey);

        }

        if (string.IsNullOrEmpty(UserName))
		{
			UserName = "user" + Environment.TickCount%99; //随便编一个用户名,这里确保其唯一性
		}

		bool _AppIdPresent = string.IsNullOrEmpty(PhotonNetwork.PhotonServerSettings.ChatAppID);

		Connect ();
		if (string.IsNullOrEmpty(PhotonNetwork.PhotonServerSettings.ChatAppID))
		{
			Debug.LogError("你需要在PhotonServerSettings文件中设置聊天AppID.");
			return;
		}
	}
	
	public void Update()
	{
		if (this.chatClient != null)
		{
			this.chatClient.Service(); // 确保定时调用这个方法!该方法在内部有消耗限制 ,所以可以经常调用!
		}

	}

	#endregion
	
	#region Public Methods	//公共方法区域
	public void Connect()
	{

		this.chatClient = new ChatClient(this);	//创建聊天客户端
		this.chatClient.Connect(PhotonNetwork.PhotonServerSettings.ChatAppID, "1.0", new ExitGames.Client.Photon.Chat.AuthenticationValues(UserName));	//设置AppID并验证用户

		this.ChannelToggleToInstantiate.gameObject.SetActive(false);
		Debug.Log("Connecting as: " + UserName);

	}

	/// <summary>为了避免编辑器没有反应, 在OnApplicationQuit方法中断开所有Photon连接.</summary>
	public void OnApplicationQuit()
	{
		if (this.chatClient != null)
		{
			this.chatClient.Disconnect();
		}
	}

	public void OnEnterSend()
	{
		if (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter))
		{
			SendChatMessage(this.InputFieldChat.text);
			this.InputFieldChat.text = "";
		}
	}

	public void OnClickSend()
	{
		if (this.InputFieldChat != null)
		{
			SendChatMessage(this.InputFieldChat.text);
			this.InputFieldChat.text = "";
		}
	}

	/// <summary>
	/// 发送帮助信息到当前频道.
	/// </summary>
	public void PostHelpToCurrentChannel()
	{
		this.CurrentChannelText.text += HelpText;
	}

	public void DebugReturn(ExitGames.Client.Photon.DebugLevel level, string message)
	{
		if (level == ExitGames.Client.Photon.DebugLevel.ERROR)
		{
			UnityEngine.Debug.LogError(message);
		}
		else if (level == ExitGames.Client.Photon.DebugLevel.WARNING)
		{
			UnityEngine.Debug.LogWarning(message);
		}
		else
		{
			UnityEngine.Debug.Log(message);
		}
	}

	public void OnConnected()
	{
		if (this.ChannelsToJoinOnConnect != null && this.ChannelsToJoinOnConnect.Length > 0)
		{
			this.chatClient.Subscribe(this.ChannelsToJoinOnConnect, this.HistoryLengthToFetch);
		}
			

		this.ChatPanel.gameObject.SetActive(true);

		if (FriendsList!=null  && FriendsList.Length>0)
		{
			this.chatClient.AddFriends(FriendsList); // Add some users to the server-list to get their status updates

			// add to the UI as well
			foreach(string _friend in FriendsList)
			{
				if (this.FriendListUiItemtoInstantiate != null && _friend!= this.UserName)
				{
					this.InstantiateFriendButton(_friend);
				}

			}

		}

		if (this.FriendListUiItemtoInstantiate != null)
		{
			this.FriendListUiItemtoInstantiate.SetActive(false);
		}


		this.chatClient.SetOnlineStatus(ChatUserStatus.Online); // You can set your online state (without a mesage).
	}

	public void OnDisconnected()
	{

	}

	public void OnChatStateChange(ChatState state)
	{

	}

	public void OnSubscribed(string[] channels, bool[] results)
	{
		foreach (string channel in channels)
		{
			this.chatClient.PublishMessage(channel, "says 'hi'."); 

			if (this.ChannelToggleToInstantiate != null)
			{
				this.InstantiateChannelButton(channel);

			}
		}

		Debug.Log("OnSubscribed: " + string.Join(", ", channels));

		/*
        // select first subscribed channel in alphabetical order
		if (this.chatClient.PublicChannels.Count > 0)
		{
			var l = new List<string>(this.chatClient.PublicChannels.Keys);
			l.Sort();
			string selected = l[0];
			if (this.channelToggles.ContainsKey(selected))
			{
				ShowChannel(selected);
				foreach (var c in this.channelToggles)
				{
					c.Value.isOn = false;
				}
				this.channelToggles[selected].isOn = true;
				AddMessageToSelectedChannel(WelcomeText);
			}
		}
		*/

		// 切换到最新创建的频道
		ShowChannel(channels[0]);
	}

	public void OnUnsubscribed(string[] channels)
	{
		foreach (string channelName in channels)
		{
			if (this.channelToggles.ContainsKey(channelName))
			{
				Toggle t = this.channelToggles[channelName];
				Destroy(t.gameObject);

				this.channelToggles.Remove(channelName);

				Debug.Log("Unsubscribed from channel '" + channelName + "'.");

				// Showing another channel if the active channel is the one we unsubscribed from before
				if (channelName == selectedChannelName && channelToggles.Count > 0)
				{
					IEnumerator<KeyValuePair<string, Toggle>> firstEntry = channelToggles.GetEnumerator();
					firstEntry.MoveNext();

					ShowChannel(firstEntry.Current.Key);

					firstEntry.Current.Value.isOn = true;
				}
			}
			else
			{
				Debug.Log("Can't unsubscribe from channel '" + channelName + "' because you are currently not subscribed to it.");
			}
		}
	}

	public void OnGetMessages(string channelName, string[] senders, object[] messages)
	{
		if (channelName.Equals(this.selectedChannelName))
		{
			// update text
			ShowChannel(this.selectedChannelName);
		}
	}

	public void OnPrivateMessage(string sender, object message, string channelName)
	{
		// as the ChatClient is buffering the messages for you, this GUI doesn't need to do anything here
		// you also get messages that you sent yourself. in that case, the channelName is determinded by the target of your msg
		this.InstantiateChannelButton(channelName);

		byte[] msgBytes = message as byte[];
		if (msgBytes != null)
		{
			Debug.Log("Message with byte[].Length: "+ msgBytes.Length);
		}
		if (this.selectedChannelName.Equals(channelName))
		{
			ShowChannel(channelName);
		}
	}

	/// <summary>
	/// New status of another user (you get updates for users set in your friends list).
	/// </summary>
	/// <param name="user">Name of the user.</param>
	/// <param name="status">New status of that user.</param>
	/// <param name="gotMessage">True if the status contains a message you should cache locally. False: This status update does not include a
	/// message (keep any you have).</param>
	/// <param name="message">Message that user set.</param>
	public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
	{

		Debug.LogWarning("status: " + string.Format("{0} is {1}. Msg:{2}", user, status, message));

		if (friendListItemLUT.ContainsKey(user))
		{
			FriendItem _friendItem = friendListItemLUT[user];
			if ( _friendItem!=null) _friendItem.OnFriendStatusUpdate(status,gotMessage,message);
		}
	}

	public void AddMessageToSelectedChannel(string msg)
	{
		ChatChannel channel = null;
		bool found = this.chatClient.TryGetChannel(this.selectedChannelName, out channel);
		if (!found)
		{
			Debug.Log("AddMessageToSelectedChannel failed to find channel: " + this.selectedChannelName);
			return;
		}

		if (channel != null)
		{
			channel.Add("Bot", msg);
		}
	}



	public void ShowChannel(string channelName)
	{
		if (string.IsNullOrEmpty(channelName))
		{
			return;
		}

		ChatChannel channel = null;
		bool found = this.chatClient.TryGetChannel(channelName, out channel);
		if (!found)
		{
			Debug.Log("ShowChannel failed to find channel: " + channelName);
			return;
		}

		this.selectedChannelName = channelName;
		this.CurrentChannelText.text = channel.ToStringMessages();
		Debug.Log("ShowChannel: " + this.selectedChannelName);

		foreach (KeyValuePair<string, Toggle> pair in channelToggles)
		{
			pair.Value.isOn = pair.Key == channelName ? true : false;
		}
	}

	public void OpenDashboard()
	{
		Application.OpenURL("https://www.photonengine.com/en/Dashboard/Chat");
	}

	#endregion

	#region Private Methods	//私有方法区域



	private void SendChatMessage(string inputLine)
	{
		if (string.IsNullOrEmpty(inputLine))
		{
			return;
		}
		if ("test".Equals(inputLine))
		{
			if (this.TestLength != this.testBytes.Length)
			{
				this.testBytes = new byte[this.TestLength];
			}

			this.chatClient.SendPrivateMessage(this.chatClient.AuthValues.UserId, testBytes, true);
		}


		bool doingPrivateChat = this.chatClient.PrivateChannels.ContainsKey(this.selectedChannelName);
		string privateChatTarget = string.Empty;
		if (doingPrivateChat)
		{
			// the channel name for a private conversation is (on the client!!) always composed of both user's IDs: "this:remote"
			// so the remote ID is simple to figure out

			string[] splitNames = this.selectedChannelName.Split(new char[] { ':' });
			privateChatTarget = splitNames[1];
		}
		//UnityEngine.Debug.Log("selectedChannelName: " + selectedChannelName + " doingPrivateChat: " + doingPrivateChat + " privateChatTarget: " + privateChatTarget);


		if (inputLine[0].Equals('\\'))
		{
			string[] tokens = inputLine.Split(new char[] {' '}, 2);
			if (tokens[0].Equals("\\help"))
			{
				PostHelpToCurrentChannel();
			}
			if (tokens[0].Equals("\\state"))
			{
				int newState = 0;


				List<string> messages = new List<string>();
				messages.Add ("i am state " + newState);
				string[] subtokens = tokens[1].Split(new char[] {' ', ','});

				if (subtokens.Length > 0)
				{
					newState = int.Parse(subtokens[0]);
				}

				if (subtokens.Length > 1)
				{
					messages.Add(subtokens[1]);
				}

				this.chatClient.SetOnlineStatus(newState,messages.ToArray()); // this is how you set your own state and (any) message
			}
			else if ((tokens[0].Equals("\\subscribe") || tokens[0].Equals("\\s")) && !string.IsNullOrEmpty(tokens[1]))
			{
				this.chatClient.Subscribe(tokens[1].Split(new char[] {' ', ','}));
			}
			else if ((tokens[0].Equals("\\unsubscribe") || tokens[0].Equals("\\u")) && !string.IsNullOrEmpty(tokens[1]))
			{
				this.chatClient.Unsubscribe(tokens[1].Split(new char[] {' ', ','}));
			}
			else if (tokens[0].Equals("\\clear"))
			{
				if (doingPrivateChat)
				{
					this.chatClient.PrivateChannels.Remove(this.selectedChannelName);
				}
				else
				{
					ChatChannel channel;
					if (this.chatClient.TryGetChannel(this.selectedChannelName, doingPrivateChat, out channel))
					{
						channel.ClearMessages();
					}
				}
			}
			else if (tokens[0].Equals("\\msg") && !string.IsNullOrEmpty(tokens[1]))
			{
				string[] subtokens = tokens[1].Split(new char[] {' ', ','}, 2);
				if (subtokens.Length < 2) return;

				string targetUser = subtokens[0];
				string message = subtokens[1];
				this.chatClient.SendPrivateMessage(targetUser, message);
			}
			else if ((tokens[0].Equals("\\join") || tokens[0].Equals("\\j")) && !string.IsNullOrEmpty(tokens[1]))
			{
				string[] subtokens = tokens[1].Split(new char[] { ' ', ',' }, 2);

				// If we are already subscribed to the channel we directly switch to it, otherwise we subscribe to it first and then switch to it implicitly
				if (channelToggles.ContainsKey(subtokens[0]))
				{
					ShowChannel(subtokens[0]);
				}
				else
				{
					this.chatClient.Subscribe(new string[] { subtokens[0] });
				}
			}
			else
			{
				Debug.Log("The command '" + tokens[0] + "' is invalid.");
			}
		}
		else
		{
			if (doingPrivateChat)
			{
				this.chatClient.SendPrivateMessage(privateChatTarget, inputLine);
			}
			else
			{
				this.chatClient.PublishMessage(this.selectedChannelName, inputLine);
			}
		}
	}

	private void InstantiateChannelButton(string channelName)
	{
		if (this.channelToggles.ContainsKey(channelName))
		{
			Debug.Log("Skipping creation for an existing channel toggle.");
			return;
		}

		Toggle cbtn = (Toggle)GameObject.Instantiate(this.ChannelToggleToInstantiate);
		cbtn.gameObject.SetActive(true);
		cbtn.GetComponentInChildren<ChannelSelector>().SetChannel(channelName);
		cbtn.transform.SetParent(this.ChannelToggleToInstantiate.transform.parent, false);

		this.channelToggles.Add(channelName, cbtn);
	}

	private void InstantiateFriendButton(string friendId)
	{
		GameObject fbtn = (GameObject)GameObject.Instantiate(this.FriendListUiItemtoInstantiate);
		fbtn.gameObject.SetActive(true);
		FriendItem  _friendItem =	fbtn.GetComponent<FriendItem>();

		_friendItem.FriendId = friendId;

		fbtn.transform.SetParent(this.FriendListUiItemtoInstantiate.transform.parent, false);

		this.friendListItemLUT[friendId] = _friendItem;
	}
	#endregion
}
