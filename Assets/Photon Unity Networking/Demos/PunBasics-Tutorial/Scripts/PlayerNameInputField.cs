// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlayerNameInputField.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Networking Demos
// </copyright>
// <summary>
//  Let the player input his name to be saved as the network player Name, viewed by alls players above each  when in the same room. 
// </summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;

using System.Collections;

namespace ExitGames.Demos.DemoAnimator
{
    /// <summary>
    /// 玩家姓名输入字段。让用户输入自己的名字，就会出现在游戏中的玩家头上。
    /// </summary>
    [RequireComponent(typeof(InputField))]
	public class PlayerNameInputField : MonoBehaviour
	{
        #region Private Variables

        // 保存PlayerPref键来避免错别字
        static string playerNamePrefKey = "PlayerName";

        #endregion

        #region MonoBehaviour CallBacks

        /// <summary>
        /// 初始化阶段被Unity在游戏对象上调用的MonoBehaviour方法
        /// </summary>
        void Start () {
		
			string defaultName = "";
			InputField _inputField = this.GetComponent<InputField>();

			if (_inputField!=null)
			{
				if (PlayerPrefs.HasKey(playerNamePrefKey))
				{
					defaultName = PlayerPrefs.GetString(playerNamePrefKey);
					_inputField.text = defaultName;
				}
			}

			PhotonNetwork.playerName =	defaultName;
		}

        #endregion

        #region Public Methods

        /// <summary>
        /// 设置该玩家的名字，并把它保存在PlayerPrefs待用。
        /// </summary>
        /// <param name="value">The name of the Player</param>
        public void SetPlayerName(string value)
		{
            // #Important  | 重要
            PhotonNetwork.playerName = value + " "; //强制加一个拖尾空格字符串，以免该值是一个空字符串，否则playerName不会被更新。

            PlayerPrefs.SetString(playerNamePrefKey,value);
		}
		
		#endregion
	}
}
