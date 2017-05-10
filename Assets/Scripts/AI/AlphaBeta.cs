// --------------------------------------------------------------------------------------------------------------------
// <copyright file=AlphaBeta.cs company=League of HTC Vive Dev>
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
//  ChineseChessVR
// </summary>
// <author>胡良云（CloudHu）</author>
//中文注释：胡良云（CloudHu） 5/1/2017

// --------------------------------------------------------------------------------------------------------------------
/// <summary>
/// FileName: AlphaBeta.cs
/// Author: 胡良云（CloudHu）
/// Corporation: 
/// Description: alpha-beta搜索算法
/// DateTime: 5/1/2017
/// </summary>
public class AlphaBeta : SearchEngine {

    #region Public Variables  //公共变量区域


    #endregion


    #region Private Variables   //私有变量区域

    protected int[,] m_HistoryTable = new int[90, 90];  //历史得分表
    //排序用的缓冲队列
    protected GlobalConst._chessMove[] m_TargetBuff = new GlobalConst._chessMove[100];
    #endregion


    #region MonoBehaviour CallBacks //回调函数区域
    #endregion

    #region Public Methods	//公共方法区域

    public override void SearchAGoodMove(byte[,] chessPosition)
    {
        CurPosition = chessPosition;    //把当前局面赋值给CurPosition
        m_nMaxDepth = m_nSearchDepth;   //设置搜索深度
        ResetHistoryTable();    //初始化历史记录表
        alphabetaSearch_Even(m_nMaxDepth, -20000, 20000);    //搜索算法
        //将棋盘修改为走过的
        int moveId = CurPosition[m_cmBestMove.From.x, m_cmBestMove.From.z];//取目标位置的棋子
        if (ChessmanManager.chessman[moveId].go!=null)
        {
            ChessmanManager.chessman[moveId].go.GetComponent<ChessmanController>().AISelectedChessman();
            NetworkTurn.Instance.MovingChessman(moveId, CurPosition[m_cmBestMove.To.x, m_cmBestMove.To.z], m_cmBestMove.To.x * 3, m_cmBestMove.To.z * 3);
        }
        
    }
    #endregion

    /// <summary>
    /// 偶数层alpha-beta搜索
    /// </summary>
    /// <param name="depth">深度</param>
    /// <param name="alpha">上边界</param>
    /// <param name="beta">下边界</param>
    /// <returns></returns>
    protected int alphabetaSearch_Even(int depth,int alpha,int beta)
    {
        int currentScore = -20000;  //当前初始为极小值
        int scoreAlphaBeta, possibleMoveCount;   //得分与计数
        byte chessType; //棋子类型
        int i = IsGameOver(CurPosition, depth); //检查是否游戏结束
        if (i!=0)
        {
            return i;   //游戏结束,返回估值
        }
        if (depth<=0)   //叶子节点取估值
        {
            return m_pEval.Evaluate(CurPosition, 0);
        }
        //列举出当前局面下一步所有可能的走法
        possibleMoveCount = ChessMoveGenerator.CreatePossibleMove(CurPosition, depth, 0);
        //取得所有走法的历史得分
        for (i = 0; i < possibleMoveCount; i++)
        {
            ChessMoveGenerator.m_MoveList[depth, i].Score = GetHistoryScore(ChessMoveGenerator.m_MoveList[depth, i]);
        }
        //对possibleMoveCount种走法按历史得分大小排序
        MergeSort(ChessMoveGenerator.GetChildMoveList(depth), possibleMoveCount, false);
        int bestMove = -1;  //记录最佳走法的变量
        for (i = 0; i < possibleMoveCount; i++) //遍历所有可能的走法
        {
            //根据走法产生新局面,生成子节点
            chessType = MakeMove(ChessMoveGenerator.m_MoveList[depth, i]);
            //递归调用搜索下一层的节点
            scoreAlphaBeta = -alphabetaSearch_Odd(depth - 1, -beta, -alpha);
            //撤销子节点
            UnMakeMove(ChessMoveGenerator.m_MoveList[depth, i], chessType);
            if (scoreAlphaBeta>currentScore)
            {
                currentScore = scoreAlphaBeta; //保留极大值
                if (depth==m_nMaxDepth)
                {   //在根部保存最佳方法
                    m_cmBestMove = ChessMoveGenerator.m_MoveList[depth, i];
                    bestMove = i;   //记录最佳走法的位置
                }
                if (scoreAlphaBeta>=alpha)
                {
                    alpha = scoreAlphaBeta; //修改alpha边界
                }
                if (alpha>=beta)
                {
                    bestMove = i;   //记录最佳走法的位置
                    break;  //剪枝,放弃搜索剩下的节点
                }
            }
        }
        //将最佳走法汇入历史记录表
        if (bestMove!=-1)
        {
            EnterHistoryScore(ChessMoveGenerator.m_MoveList[depth, bestMove], depth);
        }
        return currentScore;    //返回极大值或边界值
    }

    /// <summary>
    /// 奇数层alpha-beta搜索
    /// </summary>
    /// <param name="depth">深度</param>
    /// <param name="alpha">上边界</param>
    /// <param name="beta">下边界</param>
    /// <returns></returns>
    protected int alphabetaSearch_Odd(int depth, int alpha, int beta)
    {
        int curScore, possibleMoveCount, bestScore,i;
        byte chessType;
        //检查是否游戏结束
        int max = IsGameOver(CurPosition, depth);   //获取极值
        if (max != 0)
        {
            return max; //胜负已分则返回极值
        }
        if (depth <= 0)   //叶子节点取估值
        {
            return m_pEval.Evaluate(CurPosition, 1);
        }
        //列举出当前局面下一步所有可能的走法
        possibleMoveCount = ChessMoveGenerator.CreatePossibleMove(CurPosition, depth, 1);
        //取得所有走法的历史得分
        for (i = 0; i < possibleMoveCount; i++)
        {
            ChessMoveGenerator.m_MoveList[depth, i].Score = GetHistoryScore(ChessMoveGenerator.m_MoveList[depth, i]);
        }
        //对possibleMoveCount种走法按历史得分大小排序
        MergeSort(ChessMoveGenerator.GetChildMoveList(depth), possibleMoveCount, false);
        int bestMove = -1;  //记录最佳走法的变量
        //根据走法产生第一个节点
        chessType = MakeMove(ChessMoveGenerator.m_MoveList[depth, 0]);
        //使用全窗口搜索第一个节点
        bestScore = -alphabetaSearch_Even(depth - 1, -beta, -alpha);
        //撤销第一个节点
        UnMakeMove(ChessMoveGenerator.m_MoveList[depth, 0], chessType);
        if (depth == m_nMaxDepth)
        {   //在根部保存最佳方法
            m_cmBestMove = ChessMoveGenerator.m_MoveList[depth, 0];
            bestMove = 0;
        }
        for (max = 1; max < possibleMoveCount; max++) //从第二个节点开始遍历所有可能的走法
        {
            if (bestScore < beta) //如果不能beta剪枝
            {
                if (bestScore > alpha)
                {
                    alpha = bestScore;
                }
                //根据走法产生新局面
                chessType = MakeMove(ChessMoveGenerator.m_MoveList[depth, max]);
                //递归调用搜索下一层的节点
                curScore = -alphabetaSearch_Even(depth - 1, -alpha - 1, -alpha);
                if (curScore > alpha && curScore < beta)
                {
                    //fail high,高于期望值,重新搜索
                    bestScore = -alphabetaSearch_Odd(depth - 1, -alpha - 1, -alpha);
                    if (depth == m_nMaxDepth)
                    {   //在根部保存最佳方法
                        m_cmBestMove = ChessMoveGenerator.m_MoveList[depth, max];
                        bestMove = max;   //记录最佳走法的位置
                    }
                }
                else if (curScore > bestScore)
                {
                    bestScore = curScore;   //窄窗口搜索命中
                    if (depth == m_nMaxDepth)
                    {   //在根部保存最佳方法
                        m_cmBestMove = ChessMoveGenerator.m_MoveList[depth, max];
                        bestMove = max;   //记录最佳走法的位置
                    }
                }
                //撤销子节点
                UnMakeMove(ChessMoveGenerator.m_MoveList[depth, max], chessType);
            }
        }
        //将最佳走法汇入历史记录表
        if (bestMove != -1)
        {
            EnterHistoryScore(ChessMoveGenerator.m_MoveList[depth, bestMove], depth);
        }
        return bestScore;    //返回极大值或边界值
    }

    #region HistoryHeuristic    //历史启发搜索排序法
    /// <summary>
    /// 将历史记录表中所有项目清零
    /// </summary>
    public void ResetHistoryTable()
    {
        m_HistoryTable.Initialize();
    }

    /// <summary>
    /// 取给定走法的历史得分
    /// </summary>
    /// <param name="move">走法</param>
    /// <returns>历史得分</returns>
    public int GetHistoryScore(GlobalConst._chessMove move)
    {
        int nFrom, nTo;
        nFrom = move.From.z * 9 + move.From.x;  //原始位置
        nTo = move.From.z * 9 + move.To.x;  //目标位置
        return m_HistoryTable[nFrom, nTo];  //返回历史得分
    }

    /// <summary>
    /// 将最佳走法汇入历史记录
    /// </summary>
    /// <param name="move">走法</param>
    /// <param name="depth">深度</param>
    public void EnterHistoryScore(GlobalConst._chessMove move, int depth)
    {
        int nFrom, nTo;
        nFrom = move.From.z * 9 + move.From.x;  //原始位置
        nTo = move.From.z * 9 + move.To.x;  //目标位置
        m_HistoryTable[nFrom, nTo] += 2 << depth;   //增量为2的depth次方
    }

    /// <summary>
    /// 对走法队列从小到大排序
    /// </summary>
    /// <param name="source">原始队列</param>
    /// <param name="target">目标队列</param>
    /// <param name="l"></param>
    /// <param name="m"></param>
    /// <param name="r"></param>
    public void Merge(GlobalConst._chessMove[] source, GlobalConst._chessMove[] target, int l, int m, int r)
    {
        int i = l;
        int j = m + 1;
        int k = l;
        while ((i <= m) && (j <= r))
        {
            if (source[i].Score <= source[j].Score)
            {
                target[k++] = source[i++];
            }
            else
            {
                target[k++] = source[j++];
            }
        }
        if (i > m)
        {
            for (int q = j; q <= r; q++)
            {
                target[k++] = source[q];
            }
        }
        else
        {
            for (int q = i; q <= m; q++)
            {
                target[k++] = source[q];
            }
        }
    }

    /// <summary>
    /// 对走法队列从大到小排序
    /// </summary>
    /// <param name="source">原始队列</param>
    /// <param name="target">目标队列</param>
    public void Merge_A(GlobalConst._chessMove[] source, GlobalConst._chessMove[] target, int l, int m, int r)
    {
        int i = l;
        int j = m + 1;
        int k = l;
        while ((i <= m) && (j <= r))
        {
            if (source[i].Score >= source[j].Score)
            {
                target[k++] = source[i++];
            }
            else
            {
                target[k++] = source[j++];
            }

        }
        if (i > m)
        {
            for (int q = j; q <= r; q++)
            {
                target[k++] = source[q];
            }
        }
        else
        {
            for (int q = i; q <= m; q++)
            {
                target[k++] = source[q];
            }
        }
    }

    /// <summary>
    /// 合并大小为S的相邻子数组
    /// </summary>
    /// <param name="source">原始队列</param>
    /// <param name="target">目标队列</param>
    public void MergePass(GlobalConst._chessMove[] source, GlobalConst._chessMove[] target, int s, int n, bool direction)
    {
        int i = 0;
        while (i <= n - 2 * s)
        {
            //合并大小为s的相邻二段子数组
            if (direction)
            {
                Merge(source, target, i, i + s - 1, i + 2 * s - 1);
            }
            else
            {
                Merge_A(source, target, i, i + s - 1, i + 2 * s - 1);
            }
            i = i + 2 * s;
        }
        if (i + s < n)  //剩余的元素个数小于2s
        {
            if (direction)
            {
                Merge(source, target, i, i + s - 1, n - 1);
            }
            else
            {
                Merge_A(source, target, i, i + s - 1, n - 1);
            }
        }
        else
        {
            for (int j = i; j <= n - 1; j++)
            {
                target[j] = source[j];
            }
        }
    }


    public void MergeSort(GlobalConst._chessMove[] source, int n, bool direction)
    {
        int s = 1;
        while (s < n)
        {
            MergePass(source, m_TargetBuff, s, n, direction);
            s += s;
            MergePass(m_TargetBuff, source, s, n, direction);
            s += s;
        }
    }
    #endregion  //

}
