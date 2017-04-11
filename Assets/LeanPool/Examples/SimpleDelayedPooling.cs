using UnityEngine;
using System.Collections.Generic;

// This script shows you how you can easily spawn and despawn a prefab using the delay functionality - same as Destroy(obj, __delay__)
public class SimpleDelayedPooling : MonoBehaviour
{
	public GameObject Prefab;
	
	public float DespawnDelay = 1.0f;
	
	public void SpawnPrefab()
	{
		var position = (Vector3)Random.insideUnitCircle * 6.0f;
		var clone    = Lean.LeanPool.Spawn(Prefab, position, Quaternion.identity, null);
		
		Lean.LeanPool.Despawn(clone, DespawnDelay);
	}
}