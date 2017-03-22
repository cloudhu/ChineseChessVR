// --------------------------------------------------------------------------------------------------------------------
// <copyright file=ChessmanManager.cs company=League of HTC Vive Developers>
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// FileName: ChessmanManager.cs
/// Author: 胡良云（CloudHu）
/// Corporation: 
/// Description: 这个脚本用于管理棋子
/// DateTime: 3/22/2017
/// </summary>
public class ChessmanManager : Photon.MonoBehaviour {
	
	#region Public Variables  //公共变量区域

	//红方阵营
	public GameObject Red_King;  	//帅
	public GameObject Red_Guard; 	//仕 	
	public GameObject Red_Elephant; //象 
	public GameObject Red_Horse;  	//馬
	public GameObject Red_Rook;  	//車
	public GameObject Red_Cannon;   //炮
	public GameObject Red_Pawn; 	//卒

	//黑方阵营
	public GameObject Black_King;  	  //将
	public GameObject Black_Guard;    //士
	public GameObject Black_Elephant; //相 
	public GameObject Black_Horse;    //马
	public GameObject Black_Rook;  	  //车
	public GameObject Black_Cannon;   //炮
	public GameObject Black_Pawn; 	  //兵 

	//初始化32个棋子
	public static Chessman[] chessman = new Chessman[32];  

	public struct Chessman  
	{  
		public enum TYPE { KING, GUARD, ELEPHANT, HORSE, ROOK, CANNON, PAWN };  

		//棋子的ID  
		public int _id;  

		//棋子是红子还是黑子,ID小于16的是红子，大于16是黑子  
		public bool _red;  

		//棋子是否死亡  
		public bool _dead;  

		//棋子在棋盘中的位置,  
		public float _x;  
		public float _y;  

		//棋子的类型  
		public TYPE _type;  

		//棋子初始化，赋予32个棋子对应的属性参数  
		public void init(int id)  
		{  
			_id = id;  
			_red = id < 16;  
			_dead = false;  

			//每个点上的棋子的类型  
			ChessmanPos[] pos = {  
				new ChessmanPos(-2.04f,2.25f,Chessman.TYPE.KING),  
				new ChessmanPos(-1.53f,2.25f,Chessman.TYPE.GUARD),  
				new ChessmanPos(-1.02f,2.25f,Chessman.TYPE.GUARD),  
				new ChessmanPos(-0.51f,2.25f,Chessman.TYPE.ELEPHANT),  
				new ChessmanPos(0,2.25f,Chessman.TYPE.ELEPHANT),  
				new ChessmanPos(0.51f,2.25f,Chessman.TYPE.HORSE),  
				new ChessmanPos(1.02f,2.25f,Chessman.TYPE.HORSE),  
				new ChessmanPos(1.53f,2.25f,Chessman.TYPE.ROOK),  
				new ChessmanPos(2.04f,2.25f,Chessman.TYPE.ROOK),  

				new ChessmanPos(-1.53f,1.23f,Chessman.TYPE.CANNON),  
				new ChessmanPos(1.53f,1.23f,Chessman.TYPE.CANNON),  

				new ChessmanPos(-2.04f,0.72f,Chessman.TYPE.PAWN),  
				new ChessmanPos(-1.02f,0.72f,Chessman.TYPE.PAWN),  
				new ChessmanPos(0,0.72f,Chessman.TYPE.PAWN),  
				new ChessmanPos(1.02f,0.72f,Chessman.TYPE.PAWN),  
				new ChessmanPos(2.04f,0.72f,Chessman.TYPE.PAWN),  
			};  
			if (id < 16)  
			{  
				_x = pos[id].x;  
				_y = pos[id].y;  
				_type = pos[id].type;  
			}  
			else  
			{  
				_x = -pos[id - 16].x;  
				_y = -pos[id - 16].y - 0.1f;  
				_type = pos[id - 16].type;  
			}  
		}  
	}  

	/// <summary>  
	/// 通过棋子的坐标，获得对应的类型，将三个值关联在一起  
	/// </summary>  
	public struct ChessmanPos  
	{  
		public float x, y;  
		public Chessman.TYPE type;  
		public ChessmanPos(float _x, float _y, Chessman.TYPE _type)  
		{  
			x = _x;  
			y = _y;  
			type = _type;  
		}  
	}  
 
	#endregion


	#region Private Variables   //私有变量区域
	

	#endregion
	
	
	#region MonoBehaviour CallBacks //回调函数区域

	void Awake()  
	{  
		ChessmanInit();  
	}  
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	#endregion
	
	#region Public Methods	//公共方法区域

	/// <summary>  
	/// 通过棋子的ID和类型赋予棋子对应的预制体  
	/// </summary>  
	/// <param name="id">棋子id</param>  
	/// <param name="type">棋子类型</param>  
	/// <returns></returns>  
	public GameObject GetPrefab(int id, ChessmanManager.Chessman.TYPE type)  
	{  
		if (id < 16)  //id小于16表示是红方阵营
		{  
			switch (type)  
			{  
				case ChessmanManager.Chessman.TYPE.KING:  
					return Red_King;  
				case ChessmanManager.Chessman.TYPE.GUARD:  
					return Red_Guard;  
				case ChessmanManager.Chessman.TYPE.ELEPHANT:  
					return Red_Elephant;  
				case ChessmanManager.Chessman.TYPE.HORSE:  
					return Red_Horse;  
				case ChessmanManager.Chessman.TYPE.ROOK:  
					return Red_Rook;  
				case ChessmanManager.Chessman.TYPE.CANNON:  
					return Red_Cannon;  
				case ChessmanManager.Chessman.TYPE.PAWN:  
					return Red_Pawn;  
			}  
		}  
		else  //否则为黑方阵营
		{  
			switch (type)  
			{  
				case ChessmanManager.Chessman.TYPE.KING:  
					return Black_King;  
				case ChessmanManager.Chessman.TYPE.GUARD:  
					return Black_Guard;  
				case ChessmanManager.Chessman.TYPE.ELEPHANT:  
					return Black_Elephant;  
				case ChessmanManager.Chessman.TYPE.HORSE:  
					return Black_Horse;  
				case ChessmanManager.Chessman.TYPE.ROOK:  
					return Black_Rook;  
				case ChessmanManager.Chessman.TYPE.CANNON:  
					return Black_Cannon;  
				case ChessmanManager.Chessman.TYPE.PAWN:  
					return Black_Pawn;   
			}  
		}  

		return Black_Pawn;  
	}  


	/// <summary>  
	/// 初始化棋子  
	/// </summary>  
	public void ChessmanInit()  
	{  
		for (int i = 0; i < 32; ++i)  
		{  
			chessman[i].init(i);  
		}  

		//实例化32个棋子  
		for (int i = 0; i < 32; ++i)  
		{  
			GameObject prefabs = GetPrefab(i, chessman[i]._type);  
			GameObject ChessMan = PhotonNetwork.Instantiate(prefabs.name, new Vector3(chessman[i]._x, chessman[i]._y, 0), Quaternion.identity,0) as GameObject; 
			ChessMan.transform.SetParent (transform);
			ChessMan.AddComponent<BoxCollider>();  
		}  
	}  
	
	#endregion
	
}
