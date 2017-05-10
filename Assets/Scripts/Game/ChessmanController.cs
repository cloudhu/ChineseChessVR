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
using Lean;
using System.Collections;
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
	public AudioClip spawnMusic,awakeMusic,ArrivalAC,RunAC,DieAC,attack;

    [Tooltip("战斗UI游戏对象预设")]
    public GameObject warUiPrefab;

    [Tooltip("选中棋子的id")]
    public int ChessmanId;

    [Tooltip("攻击特效预设")]
    public GameObject attackEffectPrefab;

    [Tooltip("选择特效预设")]
    public GameObject selectedEffectPrefab;
    #endregion


    #region Private Variables   //私有变量区域
    private AudioSource As;  //音源组件
    private Animator ani;   //动画组件
    private ChessmanManager chessmanManager;    //棋子总管
    private bool isRed; //是否是红方,true是红色
    private GameObject attackEffect;    //攻击特效
    private GameObject SelectEffect;    //选择特效
    private bool isKilling = false;     //是否击杀
    private Vector3 targetPosition;     //目标位置
    Transform enemyKingTransform;       //敌方将帅
    #endregion


    #region MonoBehaviour CallBacks //回调函数区域

    // Use this for initialization
    void Start () {   
        ChessmanId =int.Parse(this.gameObject.name);
		isRed = ChessmanId<16;
		ani = transform.GetComponent<Animator> ();
        if (this.As == null) this.As = gameObject.AddComponent<AudioSource>();
        PlaySound(spawnMusic);

        chessmanManager= transform.parent.GetComponent<ChessmanManager>();
        //创建战斗UI
        if (this.warUiPrefab != null)
        {
            GameObject _uiGo = Instantiate(this.warUiPrefab, Vector3.zero, Quaternion.identity, transform);
			_uiGo.transform.localPosition = new Vector3(0, 4f, 0);
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
        //Debug.Log(usingObject + "Using");
		trySelectChessman ();
	}

	public override void StopUsing(GameObject usingObject)
	{
		base.StopUsing(usingObject);

	}

    /// <summary>
    /// 尝试选择棋子
    /// </summary>
	public void trySelectChessman(){
		if (NetworkTurn.Instance._selectedId != ChessmanId) {
			NetworkTurn.Instance.OnSelectChessman(ChessmanId, transform.localPosition.x, transform.localPosition.z);
            //Debug.Log(ChessmanId + "trying");
		}
	}

    /// <summary>
    /// 确认选择棋子
    /// </summary>
	public void SelectedChessman(){
        
		Search ();
        AISelectedChessman();
    }

    /// <summary>
    /// AI选择棋子
    /// </summary>
    public void AISelectedChessman()
    {
        PlaySound(awakeMusic);
        if (SelectEffect == null)
        {
            SelectEffect = Instantiate(selectedEffectPrefab, transform.parent, false) as GameObject;
        }
        SelectEffect.transform.localPosition = transform.localPosition;
        if (!attackEffect.activeSelf) attackEffect.SetActive(true);
        ani.SetTrigger("TH Sword Jump");
        if (enemyKingTransform == null)
        {
            if (isRed)
            {
                enemyKingTransform = ChessmanManager.chessman[GlobalConst.B_KING].go.transform;
            }
            else
                enemyKingTransform = ChessmanManager.chessman[GlobalConst.R_KING].go.transform;
        }
    }

    /// <summary>
    /// 设置目标位置.
    /// </summary>
    /// <param name="targetPosition">目标位置.</param>
    public void SetTarget(float x,float z,int killId){
		targetPosition = new Vector3 (x, 0.57f, z);
        if (killId!=GlobalConst.NOCHESS)
        {
            Attack();
            isKilling = true;
        }
        if (!isKilling)
        {
            MovingTo();
        }

	}

	/// <summary>
	/// 切换死亡
	/// </summary>
	public void SwitchDead()
	{
		HitHapticPulse (500);
        
        ani.SetTrigger("TH Sword Take Damage");
        Invoke("DelayDead", 3f);
	}

    void DelayDead()
    {
        HitHapticPulse(500);
        ani.SetTrigger("TH Sword Die");
        PlaySound(DieAC);
        Invoke("PureDead", 1.5f);
    }
    #endregion


    #region Private Methods //私有方法

    void Attack()
    {
        switch (ChessmanManager.chessman[ChessmanId]._type)
        {
            case ChessmanManager.Chessman.TYPE.KING:
                LightningCall();
                break;
            case ChessmanManager.Chessman.TYPE.GUARD:
                DragonCall();
                break;
            case ChessmanManager.Chessman.TYPE.ELEPHANT:
                DragonCall();
                break;
            case ChessmanManager.Chessman.TYPE.HORSE:
                DragonCall();
                break;
            case ChessmanManager.Chessman.TYPE.ROOK:
                DragonCall();
                break;
            case ChessmanManager.Chessman.TYPE.CANNON:
                DragonCall();
                break;
            case ChessmanManager.Chessman.TYPE.PAWN:
                DragonCall();
                break;
            default:
                break;
        }
    }

    void Search(){
		chessmanManager.hidePointer ();
		int x = (int)ChessmanManager.chessman[ChessmanId]._x;	//棋子位置x
        x = x == 0 ? 0 : x / 3; //换算成棋盘坐标
		int z = (int)ChessmanManager.chessman[ChessmanId]._z;
        z = z == 0 ? 0 : z / 3;        
		switch (ChessmanManager.chessman[ChessmanId]._type)
		{
		case ChessmanManager.Chessman.TYPE.KING:
                ChessMoveGenerator.Gen_KingMove(GlobalConst.Instance.ChessBoard, x, z, 100);
                break;
		case ChessmanManager.Chessman.TYPE.GUARD:
                ChessMoveGenerator.Gen_GuardMove(GlobalConst.Instance.ChessBoard, x, z, 100);
                break;
		case ChessmanManager.Chessman.TYPE.ELEPHANT:
                ChessMoveGenerator.Gen_ElephantMove(GlobalConst.Instance.ChessBoard, x, z, 100);
                break;
		case ChessmanManager.Chessman.TYPE.HORSE:
                ChessMoveGenerator.Gen_HorseMove(GlobalConst.Instance.ChessBoard, x, z, 100);
                break;
		case ChessmanManager.Chessman.TYPE.ROOK:
                ChessMoveGenerator.Gen_RookMove(GlobalConst.Instance.ChessBoard, x, z, 100);
                break;
		case ChessmanManager.Chessman.TYPE.CANNON:
                ChessMoveGenerator.Gen_CannonMove(GlobalConst.Instance.ChessBoard, x, z, 100);
                break;
		case ChessmanManager.Chessman.TYPE.PAWN:
                if (isRed)
                {
                    ChessMoveGenerator.Gen_RPawnMove(GlobalConst.Instance.ChessBoard, x, z, 100);
                }
                else
                {
                    ChessMoveGenerator.Gen_BPawnMove(GlobalConst.Instance.ChessBoard, x, z, 100);
                }
			break;
		}
	}

    void MovingTo()
    {
        chessmanManager.hidePointer();
        transform.LookAt(targetPosition);
        Hashtable ht = new Hashtable();
        ht.Add("position", targetPosition);
        //ht.Add ("orienttopath", true);
        ht.Add("onstart", "Move");
        ht.Add("oncomplete", "Stop");
        ht.Add("islocal", true);
        ht.Add("speed", 2.0f);
        iTween.MoveTo(gameObject, ht);
        ht.Clear();
        //换算成UI棋盘地图上的坐标
        float _x = targetPosition.z == 0 ? 0-234f : (targetPosition.z / 3f) * 58f-234f;
        float _y = targetPosition.x == 0 ? 0-248f : (targetPosition.x / 3f) * 56f-248f;
        Vector3 targetPos = new Vector3(_x, _y, 0);
        ht.Add("position", targetPos);
        ht.Add("islocal", true);
        ht.Add("time", 0.5f);
        iTween.MoveTo(ChessMap.chessman[ChessmanId].go, ht);
        ChessmanManager.chessman[ChessmanId]._x = targetPosition.x;
        ChessmanManager.chessman[ChessmanId]._z = targetPosition.z;
    }

	void Move(){
		ani.SetBool ("TH Sword Run",true);
		PlaySound (RunAC);
	}

	void Stop(){
		ani.SetBool ("TH Sword Run",false);
        transform.LookAt(enemyKingTransform);
		PlaySound (ArrivalAC);
	}
		
    /// <summary>
    /// 死亡动画结束后调用
    /// </summary>
	void PureDead(){
		ChessMap.chessman [ChessmanId].go.SetActive (false);
		gameObject.SetActive (false);
	}

    void LightningCall()
    {
        PlaySound(attack);
        if (attackEffect == null)
        {
            attackEffect = Instantiate(attackEffectPrefab, transform, false) as GameObject;
        }
        if (!attackEffect.activeSelf) attackEffect.SetActive(true);
        ani.SetBool("TH Sword Idle", false);
        ani.SetBool("Wave Hand", true);
        Invoke("WaveHandEnd", 5f);
    }

    /// <summary>
    /// 召唤神龙特效
    /// </summary>
    void DragonCall()
    {
        
        if (attackEffect == null)
        {
            attackEffect = Instantiate(attackEffectPrefab, transform.parent,false) as GameObject;
        }
        attackEffect.transform.localPosition = targetPosition;
        if (!attackEffect.activeSelf) attackEffect.SetActive(true);
        ani.SetTrigger("TH Sword Cast Spell");
        PlaySound(attack);
        Invoke("SwordCastSpellEnd", 5f);
    }

    void WaveHandEnd()
    {
        ani.SetBool("Wave Hand", false);
        ani.SetBool("TH Sword Idle", true);
        attackEffect.SetActive(false);
        isKilling = false;
        MovingTo();
    }

    /// <summary>
    /// TH Sword Cast Spell动画结束后调用
    /// </summary>
    void SwordCastSpellEnd()
    {
        //Debug.Log("SwordCastSpellEnd");
        MovingTo();
        isKilling = false;
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
