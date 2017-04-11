// --------------------------------------------------------------------------------------------------------------------
// <copyright file=PositionManager.cs company=League of HTC Vive Developers>
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
/// <summary>
/// FileName: PositionManager.cs
/// Author: 胡良云（CloudHu）
/// Corporation: 
/// Description: 控制棋子的位移
/// DateTime: 3/24/2017
/// </summary>
public class PositionManager : MonoBehaviour {

    #region Public Variables  //公共变量区域
    [Tooltip("默认步长")]
    public static float step = 3f;
	
	#endregion


	#region Private Variables   //私有变量区域
	

	#endregion
	
	
	#region MonoBehaviour CallBacks //回调函数区域
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    #endregion

    #region Public Methods	//公共方法区域

    /// <summary>
    /// 能否移动将/帅
    /// </summary>
    /// <param name="selectedId">选中的棋子id</param>
    /// <param name="x">想要移动的位置坐标x</param>
    /// <param name="z">想要移动的位置坐标z</param>
    /// <param name="destoryId">摧毁棋子的ID</param>
    /// <returns>可以移动或摧毁返回真，否则为假</returns>
    public static bool canMoveKing(int selectedId,float x,float z,int destoryId){
        /*
		1.将帅被限制在九宫格内移动
		2.移动的步长为一格，一格的距离为3
		3.将帅不能在一条直线上面对面（中间无棋子遮挡）,如一方占据中路三线中的一线,在无遮挡的情况下,另一方必须回避该线,否则会被对方秒杀
		*/

       //九宫限制已经在BoardManager中做了，这里不再重复

        float _x = ChessmanManager.chessman[selectedId]._x;
        float _z = ChessmanManager.chessman[selectedId]._z;
        if (_x!=x && _z!=z) //九宫内直线运动
        {
            return false;
        }

		if (Mathf.Abs(_x-x)!=step && Mathf.Abs(_z - z)!= step) //判断移动步长
        {
            return false;
        }

        int enemyId=0;  //敌方的将帅
        if (selectedId==0)
        {
            enemyId = 16;
        }

        if (ChessmanManager.chessman[enemyId]._z== z)   //如果将帅在Z轴上相遇，则判断双方中间是否有其他棋子遮挡
        {
            for (int i = 1; i < ChessmanManager.chessman.Length; i++)
            {

                if ( !ChessmanManager.chessman[i]._dead && ChessmanManager.chessman[i]._z == z)  //如果z轴上有棋子，说明有遮挡
                {
                    if (i != 16)  //排除将帅
                    {
                        return true;
                    }
                }

            }
            return false;
        }

		return true;
	}


    /// <summary>
    /// 能否移动士/仕
    /// </summary>
    /// <param name="selectedId">选中的棋子id</param>
    /// <param name="x">想要移动的位置坐标x</param>
    /// <param name="z">想要移动的位置坐标z</param>
    /// <param name="destoryId">摧毁棋子的ID</param>
    /// <returns>可以移动或摧毁返回真，否则为假</returns>
    public static bool canMoveGuard(int selectedId, float x, float z, int destoryId)
    {
        /* 
         * 1.目标位置在九宫格内 
         * 2.只许沿着九宫中的斜线行走一步（方格的对角线） 
        */

        //九宫限制已经在BoardManager中做了，这里不再重复

        float _x = Mathf.Abs(ChessmanManager.chessman[selectedId]._x - x);  //判断x轴移动步长
        float _z = Mathf.Abs(ChessmanManager.chessman[selectedId]._z - z);  //判断z轴移动步长
        if (_x != step || _z != step) return false;  //x轴和z轴的步长都等于3则是延九宫格斜线运动

        return true;
    }

    /// <summary>
    /// 能否移动相/象
    /// </summary>
    /// <param name="selectedId">选中的棋子id</param>
    /// <param name="x">想要移动的位置坐标x</param>
    /// <param name="z">想要移动的位置坐标z</param>
    /// <param name="destoryId">摧毁棋子的ID</param>
    /// <returns>可以移动或摧毁返回真，否则为假</returns>
    public static bool canMoveElephant(int selectedId, float x, float z, int destoryId)
    {
        /* 
         * 1.目标位置不能越过河界走入对方的领地 
         * 2.只能斜走（两步），可以使用汉字中的田字形象地表述：田字格的对角线，即俗称象（相）走田字 
         * 3.当象（相）行走的路线中，及田字中心有棋子时（无论己方或者是对方的棋子），则不允许走过去，俗称：塞象（相）眼。 
        */

        if (selectedId<16 && x<0)   //红相不能过河
        {
            return false;
        }

        if (selectedId>16 && x>0)   //黑象不得越界
        {
            return false;
        }

        float _x = Mathf.Abs(ChessmanManager.chessman[selectedId]._x - x);  //判断x轴移动步长
        float _z = Mathf.Abs(ChessmanManager.chessman[selectedId]._z - z);  //判断z轴移动步长
        if (_x != 2*step || _z != 2*step) return false;  //x轴和z轴的步长都等于3则是延九宫格斜线运动

        _x = (ChessmanManager.chessman[selectedId]._x + x) *0.5f; //得出象眼的位置
        _z = (ChessmanManager.chessman[selectedId]._z + z) *0.5f;

        for (int i = 5; i < ChessmanManager.chessman.Length; i++)
        {
            if ( !ChessmanManager.chessman[i]._dead && ChessmanManager.chessman[i]._z == _z && ChessmanManager.chessman[i]._x==_x )  //如果象眼有棋子，则被塞
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 能否移动车/車
    /// </summary>
    /// <param name="selectedId">选中的棋子id</param>
    /// <param name="x">想要移动的位置坐标x</param>
    /// <param name="z">想要移动的位置坐标z</param>
    /// <param name="destoryId">摧毁棋子的ID</param>
    /// <returns>可以移动或摧毁返回真，否则为假</returns>
    public static bool canMoveRook(int selectedId, float x, float z, int destoryId)
    {
        /* 
         * 1.每行一步棋可以上、下直线行走（进、退）；左、右横走 
         * 2.中间不能隔棋子 
         * 3.行棋步数不限 
         */
        float _x = ChessmanManager.chessman[selectedId]._x;
        float _z=  ChessmanManager.chessman[selectedId]._z;

        if (_x!=x && _z!=z)
        {
            return false;
        }

        //设置車到目标位置之间的区间，如果区间内有棋子，则无法到达目标位置
        float min = _z,max=z;  

        if(_x == x)
        {
            if (_z > z)
            {
                min = z;
                max = _z;
            }
            for (int i = 0; i < ChessmanManager.chessman.Length; i++)
            {
                if (!ChessmanManager.chessman[i]._dead && ChessmanManager.chessman[i]._x==x)
                {
                    if (ChessmanManager.chessman[i]._z>min && ChessmanManager.chessman[i]._z<max)
                    {
                        return false;
                    }
                }
            }

        }
        else
        {
            if (_x > x)
            {
                min = x;
                max = _x;
            }
            else
            {
                min = _x;
                max = x;
            }
            for (int i = 0; i < ChessmanManager.chessman.Length; i++)
            {
                if (!ChessmanManager.chessman[i]._dead && ChessmanManager.chessman[i]._z == z)
                {
                    if (ChessmanManager.chessman[i]._x > min && ChessmanManager.chessman[i]._x < max)
                    {
                        return false;
                    }
                }
            }

        }

        return true;
    }

    /// <summary>
    /// 能否移动马/馬
    /// </summary>
    /// <param name="selectedId">选中的棋子id</param>
    /// <param name="x">想要移动的位置坐标x</param>
    /// <param name="z">想要移动的位置坐标z</param>
    /// <param name="destoryId">摧毁棋子的ID</param>
    /// <returns>可以移动或摧毁返回真，否则为假</returns>
    public static bool canMoveHorse(int selectedId, float x, float z, int destoryId)
    {
        /* 
        * 1.马走日字（斜对角线） 
        * 2.可以将马走日分解为：先一步直走（或一横）再一步斜走 
        * 3.如果在要去的方向，第一步直行处（或者横行）有别的棋子挡住，则不许走过去（俗称：蹩马腿） 
        */

        float _x = ChessmanManager.chessman[selectedId]._x;
        float _z = ChessmanManager.chessman[selectedId]._z;

        if (_x==x || _z==z) //马走日，所以排除直线
        {
            return false;
        }

        float _xStep = Mathf.Abs(_x - x);  //判断x轴移动步长
        float _zStep = Mathf.Abs(_z - z);  //判断z轴移动步长

        if (_xStep ==2*step && _zStep == step)   //利用步长判断是否走日字
        {
            if (_x < x)
            {
                for (int i = 0; i < 32; i++)
                {   
                    if (!ChessmanManager.chessman[i]._dead && ChessmanManager.chessman[i]._z==_z && ChessmanManager.chessman[i]._x==(_x+step)) //判断是否绊马腿
                    {
                        return false;
                    }
                }
            }
            else
            {
                for (int i = 0; i < 32; i++)
                {
                    if (!ChessmanManager.chessman[i]._dead && ChessmanManager.chessman[i]._z == _z && ChessmanManager.chessman[i]._x == (_x - step))   //判断是否绊马腿
                    {
                        return false;
                    }
                }
            }
      
        }
        else
        {
            if (_z < z)
            {
                for (int i = 0; i < 32; i++)
                {
                    if (!ChessmanManager.chessman[i]._dead && ChessmanManager.chessman[i]._x == _x && ChessmanManager.chessman[i]._z == (_z + step)) //判断是否绊马腿
                    {
                        return false;
                    }
                }
            }
            else
            {
                for (int i = 0; i < 32; i++)
                {
                    if (!ChessmanManager.chessman[i]._dead && ChessmanManager.chessman[i]._x == _x && ChessmanManager.chessman[i]._z == (_z - step)) //判断是否绊马腿
                    {
                        return false;
                    }
                }
            }
        }


        return true;
    }

    /// <summary>
    /// 能否移动炮
    /// </summary>
    /// <param name="selectedId">选中的棋子id</param>
    /// <param name="x">想要移动的位置坐标x</param>
    /// <param name="z">想要移动的位置坐标z</param>
    /// <param name="destoryId">摧毁棋子的ID</param>
    /// <returns>可以移动或摧毁返回真，否则为假</returns>
    public static bool canMoveCannon(int selectedId, float x, float z, int destoryId)
    {
        /* 
         * 1.此棋的行棋规则和车（車）类似，横平、竖直，只要前方没有棋子的地方都能行走 
         * 2.但是，它的吃棋规则很特别，必须跳过一个棋子（无论是己方的还是对方的）去吃掉对方的一个棋子。俗称：隔山打炮 
         */

        float _x = ChessmanManager.chessman[selectedId]._x;
        float _z = ChessmanManager.chessman[selectedId]._z;

        if (_x != x && _z != z) //直线移动
        {
            return false;
        }

        //设置炮到目标位置之间的区间，如果区间内有棋子，则无法到达目标位置
        float min = _z, max = z;

        int obstructs=0;    //炮与目标之间的棋子数量
        if (_x == x)
        {
            if (_z > z)
            {
                min = z;
                max = _z;
            }
            for (int i = 0; i < 32; i++)
            {
                if (!ChessmanManager.chessman[i]._dead && ChessmanManager.chessman[i]._x == x)
                {
                    if (ChessmanManager.chessman[i]._z > min && ChessmanManager.chessman[i]._z < max)
                    {
                        ++obstructs;
                    }
                }
            }

        }
        else
        {
            if (_x > x)
            {
                min = x;
                max = _x;
            }
            else
            {
                min = _x;
                max = x;
            }
            for (int i = 0; i < 32; i++)
            {
                if (!ChessmanManager.chessman[i]._dead && ChessmanManager.chessman[i]._z == z)
                {
                    if (ChessmanManager.chessman[i]._x > min && ChessmanManager.chessman[i]._x < max)
                    {
                        ++obstructs;
                    }
                }
            }

        }

        if (destoryId != -1 && obstructs==1)    //如果炮与目标之间只有一个棋子，则可以摧毁目标
        {
            return true;
        }

        if (obstructs != 0) return false; //如果没有摧毁对象，则不能有障碍物

        return true;
    }

    /// <summary>
    /// 能否移动兵/卒
    /// </summary>
    /// <param name="selectedId">选中的棋子id</param>
    /// <param name="x">想要移动的位置坐标x</param>
    /// <param name="z">想要移动的位置坐标z</param>
    /// <param name="destoryId">摧毁棋子的ID</param>
    /// <returns>可以移动或摧毁返回真，否则为假</returns>
    public static bool canMovePawn(int selectedId, float x, float z, int destoryId)
    {
        /* 
         * 0.不能后退，且每次直走一步
         * 1.在没有过河界前，只能向前，不能横着走
         * 2.过了河界之后，每行一步棋可以向前直走，或者横走（左、右）一步
         */

        float _x = ChessmanManager.chessman[selectedId]._x;
        float _z = ChessmanManager.chessman[selectedId]._z;

        if (selectedId<16)  //红色阵营
        {
            if (x > _x) return false;   //不能后退
            if (_x>0)
            {
                if (_z != z) return false;  //过河前不能横着走
            }
        }
        else
        {
            if (x < _x) return false;   //不能后退
            if (_x<0)
            {
                if (_z != z) return false;  //过河前不能横着走
                
            }
        }


        return true;
    }

        #endregion

}
