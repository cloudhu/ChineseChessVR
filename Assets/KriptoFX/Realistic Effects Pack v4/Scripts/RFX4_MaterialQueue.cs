/*
http://www.cgsoso.com/forum-211-1.html

CG搜搜 Unity3d 每日Unity3d插件免费更新 更有VIP资源！

CGSOSO 主打游戏开发，影视设计等CG资源素材。

插件如若商用，请务必官网购买！

daily assets update for try.

U should buy the asset from home store if u use it in your project!
*/

using UnityEngine;

[RequireComponent(typeof(Renderer))]
[ExecuteInEditMode]
public class RFX4_MaterialQueue : MonoBehaviour
{
  [Tooltip("Background=1000, Geometry=2000, AlphaTest=2450, Transparent=3000, Overlay=4000")]
  public int queue = 2000;

  public int[] queues;

  void Start()
  {
    Renderer renderer = GetComponent<Renderer>();
    if (!renderer || !renderer.sharedMaterial || queues == null)
      return;
    renderer.sharedMaterial.renderQueue = queue;
    for (int i = 0; i < queues.Length && i < renderer.sharedMaterials.Length; i++)
      renderer.sharedMaterials[i].renderQueue = queues[i];
  }

  void OnValidate()
  {
    Start();
  }

  void Update()
  {
    if (Application.isPlaying) return; 
    Start();
  }
}
