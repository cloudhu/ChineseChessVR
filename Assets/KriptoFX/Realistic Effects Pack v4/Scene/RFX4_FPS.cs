/*
http://www.cgsoso.com/forum-211-1.html

CG搜搜 Unity3d 每日Unity3d插件免费更新 更有VIP资源！

CGSOSO 主打游戏开发，影视设计等CG资源素材。

插件如若商用，请务必官网购买！

daily assets update for try.

U should buy the asset from home store if u use it in your project!
*/

using UnityEngine;

public class RFX4_FPS : MonoBehaviour
{
  private readonly GUIStyle guiStyleHeader = new GUIStyle();
  float timeleft;

  private float fps;
  private int frames; // Frames drawn over the interval
 
  #region Non-public methods

  private void Awake()
  {
    guiStyleHeader.fontSize = 14;
    guiStyleHeader.normal.textColor = new Color(1, 1, 1);
  }

  private void OnGUI()
  {
     GUI.Label(new Rect(0, 0, 30, 30), "FPS: " + (int) fps, guiStyleHeader);
  }
	 
  private void Update()
  {
    timeleft -= Time.deltaTime;
    ++frames;

    if (timeleft <= 0.0) {
      fps = frames;
      timeleft = 1;
      frames = 0;
    }
  }
  #endregion
}
