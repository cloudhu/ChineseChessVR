using UnityEngine;
using System.Collections;

public struct BlendInfo {
	public int weight;
	public float time;
	public int index;
}

public interface AnimationInfo  {
	BlendInfo GetBlendInfo(string control_param);
}
