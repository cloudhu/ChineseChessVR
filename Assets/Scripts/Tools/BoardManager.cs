// --------------------------------------------------------------------------------------------------------------------
// <copyright file=BoardManager.cs company=League of HTC Vive Developers>
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
//中文注释：胡良云（CloudHu） 3/26/2017

// --------------------------------------------------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// FileName: BoardManager.cs
/// Author: 胡良云（CloudHu）
/// Corporation: 
/// Description: 
/// DateTime: 3/26/2017
/// </summary>
public class BoardManager : MonoBehaviour {

    #region Public Variables  //公共变量区域

    #endregion


    #region Private Variables   //私有变量区域

    private BoardPoint[] points = new BoardPoint[90]; //棋盘上的90个坐标点
    private List<BoardPoint> hidePoints = new List<BoardPoint>(); //展示后需要隐藏的坐标点
    private List<int> occupiedId = new List<int>(); //被占用的棋子ID     占用位
    private List<int> freedId = new List<int>();    //没有被占用的棋子ID 即空位

	private float unitStep = 3f;
    #endregion


    #region MonoBehaviour CallBacks //回调函数区域
    // Use this for initialization
    void Start () {
        points = transform.GetComponentsInChildren<BoardPoint>();
        for (int i = 0; i < points.Length; i++)	//把90个点位分为 占用位和空位
        {
            if (points[i].isOccupied)
            {
                occupiedId.Add(i);
            }
            else
                freedId.Add(i);
        }
	}
	
    #endregion

    #region Public Methods	//公共方法区域

    /// <summary>
    /// 棋子离开该位置，取消该位置的占用
    /// </summary>
    public void leavePoint(float fromX,float fromZ)
    {
        for (int i = 0; i < occupiedId.Count; i++)
        {
            int leaveId = occupiedId[i];
            if (points[leaveId].transform.localPosition.x==fromX && points[leaveId].transform.localPosition.z==fromZ)
            {
                points[leaveId].isOccupied = false;
                occupiedId.RemoveAt(i);
                freedId.Add(leaveId);
                //Debug.Log("leavePoint:"+ points[leaveId]);
                return;
            }
            
        }
    }

    /// <summary>
    /// 占用对应ID的位置
    /// </summary>
    /// <param name="occupyId"></param>
    public void occupyPoint(int occupyId)
    {
        if (occupyId==-1)
        {
            return;
        }
        points[occupyId].isOccupied = true;
        occupiedId.Add(occupyId);
        freedId.Remove(occupyId);
    }

    /// <summary>
    /// 分别展示各个棋子可行的路线
    /// </summary>
    /// <param name="id">棋子id</param>
    public void showPossibleWay(int id)
    {
		float x = ChessmanManager.chessman[id]._x;	//棋子位置x
		float z= ChessmanManager.chessman[id]._z;
        switch (ChessmanManager.chessman[id]._type)
        {
            case ChessmanManager.Chessman.TYPE.KING:
                ShowKingWay(id);
                break;
            case ChessmanManager.Chessman.TYPE.GUARD:
                ShowGuardWay(id);
                break;
            case ChessmanManager.Chessman.TYPE.ELEPHANT:
                ShowElephantWay(id);
                break;
            case ChessmanManager.Chessman.TYPE.HORSE:
			ShowHorseWay(id,x,z);
                break;
            case ChessmanManager.Chessman.TYPE.ROOK:
                ShowRookWay(id);
                break;
            case ChessmanManager.Chessman.TYPE.CANNON:
                ShowCannonWay(id);
                break;
            case ChessmanManager.Chessman.TYPE.PAWN:
			ShowPawnWay(id,x,z);
                break;
        }
    }

	/// <summary>
	/// 在选择好目标后调用,将可能的位置隐藏.
	/// </summary>
	public void hidePossibleWay(){
        if (hidePoints.Count==0)
        {
            return;
        }
        for (int i = 0; i < hidePoints.Count; i++)
        {
            hidePoints[i].HidePointer();
        }
		hidePoints.Clear ();
	}


    #endregion

    #region Private Methods  //辅助方法


    void ShowKingWay(int id)
    {
        for (int i = 12; i < 30; i++) //九宫限制
        {
            if (!points[i].isOccupied)
            {
                if (PositionManager.canMoveKing(id, points[i].transform.localPosition.x, points[i].transform.localPosition.z, -1))
                {
                    points[i].ShowBeams();
					hidePoints.Add (points[i]);
                }
            }
        }
    }

    void ShowGuardWay(int id)
    {
        for (int i = 12; i < 30; i++)   //九宫限制
        {
            if (!points[i].isOccupied)
            {
                if (PositionManager.canMoveGuard(id, points[i].transform.localPosition.x, points[i].transform.localPosition.z, -1))
                {
                    points[i].ShowBeams();
					hidePoints.Add (points[i]);
                }

            }
        }
    }

    void ShowElephantWay(int id)
    {

        for (int i = 0; i < 14; i++)    //相的位置只有14种可能
        {
            if (!points[i].isOccupied)
            {
                if (PositionManager.canMoveElephant(id, points[i].transform.localPosition.x, points[i].transform.localPosition.z, -1))
                {
                    points[i].ShowBeams();
					hidePoints.Add (points[i]);
                }

            }
        }
    }



	void ShowHorseWay(int id,float x,float z)
    {
        for (int i = 0; i < freedId.Count; i++)
        {
            int aId = freedId[i];
            float _x = points[aId].transform.localPosition.x;	//节点位置x	
			float _z = points[aId].transform.localPosition.z;
			float _Step = Mathf.Abs(_x - x)+Mathf.Abs(_z - z);  //判断移动步长
			if (_Step==9f) {
				if (PositionManager.canMoveHorse(id, _x, _z, -1))
				{
					points[aId].ShowBeams();
					hidePoints.Add (points[aId]);
				}
			}
        }
    }

    void ShowRookWay(int id)
    {
        for (int i = 0; i < freedId.Count; i++)
        {
            int aId = freedId[i];
            float _x = points[aId].transform.localPosition.x;
            float _z = points[aId].transform.localPosition.z;
            if (_x== ChessmanManager.chessman[id]._x || _z== ChessmanManager.chessman[id]._z)
            {
                if (PositionManager.canMoveRook(id, _x, _z, -1))
                {
                    points[aId].ShowBeams();
					hidePoints.Add (points[aId]);
                }
            }
            
        }
    }

    void ShowCannonWay(int id)
    {
        for (int i = 0; i < freedId.Count; i++)
        {
            int aId = freedId[i];
            float _x = points[aId].transform.localPosition.x;
            float _z = points[aId].transform.localPosition.z;
            if (_x == ChessmanManager.chessman[id]._x || _z == ChessmanManager.chessman[id]._z)
            {
                if (PositionManager.canMoveCannon(id, _x, _z, -1))
                {
                    points[aId].ShowBeams();
					hidePoints.Add (points[aId]);
                }
            }
            
        }
    }

	void ShowPawnWay(int id,float x,float z)
    {
		/* 
         * 0.不能后退，且每次直走一步
         * 1.在没有过河界前，只能向前，不能横着走
         * 2.过了河界之后，每行一步棋可以向前直走，或者横走（左、右）一步
         */
        for (int i = 0; i < freedId.Count; i++)
        {
            int aId = freedId[i];
            float _x = points[aId].transform.localPosition.x;
			float _z = points[aId].transform.localPosition.z;

			if (_x==x || _z==z) {
				float Step = Mathf.Abs(_x - x)+Mathf.Abs(_z - z);  //移动步长
				if (Step==unitStep) //步长为一个单位步长
				{
					if (id < 16) { //红方阵营 Red
						if (x > 0) {
							if (z == _z) {
								ShowPointer (aId);
							}
						} else {
							if (_x <= x) {
								ShowPointer (aId);
							}
						}
					} else {	//黑方阵营 Black
						if (x < 0) {
							if (z == _z) {
								ShowPointer (aId);
							}
						} else {
							if (_x >= x) {
								ShowPointer (aId);
							}
						}
					}
				}
			}       
        }
    }

	/// <summary>
	/// Shows the pointer 把可行位置的光标显示出来.
	/// </summary>
	void ShowPointer(int aId){
		points [aId].ShowBeams ();
		hidePoints.Add (points [aId]);
	}
    #endregion


}
