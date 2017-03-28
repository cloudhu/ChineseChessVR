// --------------------------------------------------------------------------------------------------------------------
// <copyright file=ChessmanController.cs company=League of HTC Vive Developers>
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
//中文注释：胡良云（CloudHu） 3/24/2017

// --------------------------------------------------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using UnityEngine.AI;


/// <summary>
/// FileName: ChessmanController.cs
/// Author: 胡良云（CloudHu）
/// Corporation: 
/// Description: 负责控制棋子的行为
/// DateTime: 3/24/2017
/// </summary>
public class ChessmanController : VRTK_InteractableObject, IPunObservable {



    #region Public Variables  //公共变量区域
    [Tooltip("棋子唤醒时的音效")]
    public AudioSource awakeMusic,walkAS;

    [Tooltip("UI游戏对象预设")]
	public GameObject ChessmamUiPrefab;

    [Tooltip("战斗UI游戏对象预设")]
    public GameObject warUiPrefab;

    [Tooltip("玩家当前的体力值")]
	public float Health = 1f;

    [Tooltip("选中棋子的id")]
    public int selectedId;

    #endregion


    #region Private Variables   //私有变量区域

	private NavMeshAgent agent;    //寻路组件
	WarUI war; //战争UI
    #endregion


    #region MonoBehaviour CallBacks //回调函数区域
    // Use this for initialization
    void Start () {
        selectedId =int.Parse(this.gameObject.name);

		agent = this.GetComponent<NavMeshAgent>();//获取寻路组件，如果为空则添加之
		if (agent == null)
		{
			agent=gameObject.AddComponent<NavMeshAgent>();
			agent.radius = 0.3f;
		}
		agent.enabled = true;

        //创建棋子UI
		if (this.ChessmamUiPrefab != null)
		{
			GameObject _uiGo = Instantiate(this.ChessmamUiPrefab,Vector3.zero,Quaternion.identity,transform) as GameObject;
			_uiGo.transform.localPosition = new Vector3 (0, 1f, 0);
			//_uiGo.transform.SetParent (transform,false);
			_uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
		}
		else
		{
			Debug.LogWarning("<Color=Red><b>Missing</b></Color> PlayerUiPrefab reference on player Prefab.", this);
		}

        //创建战斗UI
        if (this.warUiPrefab != null)
        {
            GameObject _uiGo = Instantiate(this.warUiPrefab, Vector3.zero, Quaternion.identity, transform) as GameObject;
            _uiGo.transform.localPosition = new Vector3(0, 1.5f, 0);
            //_uiGo.transform.SetParent (transform,false);
            _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
			war = _uiGo.GetComponent<WarUI> ();
        }
        else
        {
            Debug.LogWarning("<Color=Red><b>Missing</b></Color> PlayerUiPrefab reference on player Prefab.", this);
        }

    }

	#endregion
	
	#region Public Methods	//公共方法区域

	public override void StartUsing(GameObject usingObject)
	{
		base.StartUsing(usingObject);
		PlaySound (awakeMusic);

		war.TrySelectChessman ();
	}

	public override void StopUsing(GameObject usingObject)
	{
		base.StopUsing(usingObject);

	}

	/// <summary>
	/// 设置目标位置.
	/// </summary>
	/// <param name="targetPosition">目标位置.</param>
	public void SetTarget(Vector3 targetPosition){
		
		agent.SetDestination (targetPosition);
		if (Vector3.Distance(this.transform.position, targetPosition) < 0f)
		{
			agent.Stop();
			ChessmanManager.chessman[selectedId]._x = targetPosition.x;
			ChessmanManager.chessman[selectedId]._z = targetPosition.z;
		}
		else
		{
			PlaySound(walkAS);
			agent.Resume();
		}
	}


	#endregion

	#region IPunObservable implementation

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
		{
			// 我们是本地玩家，则把数据发送给远程玩家
			// stream.SendNext(this.IsFiring);
			stream.SendNext(this.Health);
		}
		else
		{
			//网络玩家则接收数据
			//  this.IsFiring = (bool)stream.ReceiveNext();
			this.Health = (float)stream.ReceiveNext();
		}
	}

	#endregion

	#region Private Methods //私有方法

	/// <summary>
	/// 播放音效
	/// </summary>
	/// <param name="ac">声音</param>
	void PlaySound(AudioSource As)
	{
		if (As!=null && !As.isPlaying)
		{
			As.Play();
		}

	}
	/// <summary>
	/// 减血
	/// </summary>
	/// <param name="_damageAmount">伤害值</param>
	void ApplyDamage(float _damageAmount)
	{
		float t = Health - _damageAmount;
		if (t > 0)//如果健康值不低于0则造成伤害
			Health = t;
		else //反之则切换到死亡，并且震动下手柄提醒玩家已经消灭敌人
		{
			Health = 0;
			SwitchDead();
			HitHapticPulse(1000);
		}
	}
	/// <summary>
	/// 震动
	/// </summary>
	/// <param name="duration">震动时间.</param>
	void HitHapticPulse(ushort duration)
	{
		var deviceIndex = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Leftmost);
		var deviceIndex1 = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Rightmost);
		SteamVR_Controller.Input(deviceIndex).TriggerHapticPulse(duration);
		SteamVR_Controller.Input(deviceIndex1).TriggerHapticPulse(duration);
	}

	/// <summary>
	/// 切换死亡
	/// </summary>
	void SwitchDead()
	{
		//ani.SetBool("isDead", true);
		agent.enabled = false;//停止寻路

	}

	#endregion
	
}
