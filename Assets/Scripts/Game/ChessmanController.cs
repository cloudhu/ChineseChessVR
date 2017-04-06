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


/// <summary>
/// FileName: ChessmanController.cs
/// Author: 胡良云（CloudHu）
/// Corporation: 
/// Description: 负责控制棋子的行为
/// DateTime: 3/24/2017
/// </summary>
public class ChessmanController : VRTK_InteractableObject {



    #region Public Variables  //公共变量区域
    [Tooltip("棋子的音效")]
    public AudioClip awakeMusic,ArrivalAC,RunAC,DieAC;

    [Tooltip("UI游戏对象预设")]
	public GameObject ChessmamUiPrefab;

    [Tooltip("战斗UI游戏对象预设")]
    public GameObject warUiPrefab;

    [Tooltip("选中棋子的id")]
    public int selectedId;

    #endregion


    #region Private Variables   //私有变量区域
	AudioSource As;
	Animator ani;
	//private NavMeshAgent agent;    //寻路组件
	WarUI war; //战争UI
    #endregion


    #region MonoBehaviour CallBacks //回调函数区域
    // Use this for initialization
    void Start () {
        selectedId =int.Parse(this.gameObject.name);
		ani = transform.GetComponent<Animator> ();
		if (this.As == null) this.As = FindObjectOfType<AudioSource>();
		/*agent = this.GetComponent<NavMeshAgent>();//获取寻路组件，如果为空则添加之  ！！用iTween代替寻路
		if (agent == null)	
		{
			agent=gameObject.AddComponent<NavMeshAgent>();
			agent.radius = 0.3f;
		}
		agent.enabled = true;*/

        //创建棋子UI
		if (this.ChessmamUiPrefab != null)
		{
			GameObject _uiGo = Instantiate(this.ChessmamUiPrefab,Vector3.zero,Quaternion.identity,transform) as GameObject;
			_uiGo.transform.localPosition = new Vector3 (0, transform.position.y+1f, 0);
		}
		else
		{
			Debug.LogWarning("<Color=Red><b>Missing</b></Color> PlayerUiPrefab reference on player Prefab.", this);
		}

        //创建战斗UI
        if (this.warUiPrefab != null)
        {
            GameObject _uiGo = Instantiate(this.warUiPrefab, Vector3.zero, Quaternion.identity, transform) as GameObject;
			_uiGo.transform.localPosition = new Vector3(0, transform.position.y+2.5f, 0);
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
		war.TrySelectChessman ();
		PlaySound (awakeMusic);
		ani.SetTrigger ("TH Sword Jump");
        //Debug.Log("StartUsing :Called war.TrySelectChessman ();");
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
		Hashtable ht = new Hashtable ();
		ht.Add ("position", targetPosition);
		ht.Add ("orienttopath", true);
		ht.Add ("onstart", "Move");
		ht.Add ("oncomplete","Stop");
		ht.Add ("islocal",true);
		ht.Add ("time", 5.0f);
		iTween.MoveTo (gameObject, ht);


		ChessmanManager.chessman[selectedId]._x = targetPosition.x;
		ChessmanManager.chessman[selectedId]._z = targetPosition.z;
	}

	/// <summary>
	/// 切换死亡
	/// </summary>
	public void SwitchDead()
	{
		HitHapticPulse (2);
		ani.SetTrigger ("TH Sword Die");
	}
	#endregion


	#region Private Methods //私有方法

	void Move(){
		ani.SetBool ("TH Sword Run",true);
		PlaySound (RunAC);
	}

	void Stop(){
		ani.SetBool ("TH Sword Run",false);
		PlaySound (ArrivalAC);
	}
		
	void PureDead(){
		gameObject.SetActive (false);
	}

	/// <summary>
	/// 播放音效
	/// </summary>
	/// <param name="ac">声音</param>
	void PlaySound(AudioClip Ac)
	{

		if (Ac!=null && !As.isPlaying)
		{
			As.PlayOneShot (Ac);
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

	#endregion
	
}
