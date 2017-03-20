using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PunPlayerScores : MonoBehaviour
{
    public const string PlayerScoreProp = "score";
}

public static class ScoreExtensions
{
	/// <summary>
	/// 设置分数
	/// </summary>
	/// <param name="player">玩家.</param>
	/// <param name="newScore">分数.</param>
    public static void SetScore(this PhotonPlayer player, int newScore)
    {
        Hashtable score = new Hashtable();  // using PUN's implementation of Hashtable
        score[PunPlayerScores.PlayerScoreProp] = newScore;

        player.SetCustomProperties(score);  // this locally sets the score and will sync it in-game asap.
    }

	/// <summary>
	/// 加分
	/// </summary>
	/// <param name="player">玩家.</param>
	/// <param name="scoreToAddToCurrent">加的分数.</param>
    public static void AddScore(this PhotonPlayer player, int scoreToAddToCurrent)
    {
        int current = player.GetScore();
        current = current + scoreToAddToCurrent;

        Hashtable score = new Hashtable();  // 使用PUN的Hashtable实现
        score[PunPlayerScores.PlayerScoreProp] = current;

        player.SetCustomProperties(score);  // 这会在本地设置分数并尽快地在游戏内同步.
    }

	/// <summary>
	/// 获取得分.
	/// </summary>
	/// <returns>返回分数.</returns>
	/// <param name="player">玩家.</param>
    public static int GetScore(this PhotonPlayer player)
    {
        object score;
        if (player.CustomProperties.TryGetValue(PunPlayerScores.PlayerScoreProp, out score))
        {
            return (int) score;
        }

        return 0;
    }
}