using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;
using System.Collections.Generic;

// This script shows you how to benchmark the speed of spawning and despawning prefabs vs instantiate and destroy
public class SimplePoolBenchmark : MonoBehaviour
{
	public int Step = 100;
	
	public GameObject Prefab;
	
	public Lean.LeanPool Pool;
	
	public Text BenchmarkText;
	
	private List<GameObject> spawnedClones = new List<GameObject>();
	
	private List<GameObject> instantiatedClones = new List<GameObject>();
	
	private Stopwatch benchmark = new Stopwatch();
	
	public void SpawnClones()
	{
		BeginBenchmark();
		{
			for (var i = 0; i < Step; i++)
			{
				var position = (Vector3)Random.insideUnitCircle * 6.0f;
				var clone    = Lean.LeanPool.Spawn(Prefab, position, Quaternion.identity, null);
				
				spawnedClones.Add(clone);
			}
		}
		EndBenchmark("SpawnClones");
	}
	
	public void DespawnClones()
	{
		BeginBenchmark();
		{
			for (var i = 0; i < Step; i++)
			{
				var index = spawnedClones.Count - 1;
				
				if (index >= 0)
				{
					var clone = spawnedClones[index];
					
					spawnedClones.RemoveAt(index);
					
					Lean.LeanPool.Despawn(clone);
				}
			}
		}
		EndBenchmark("DespawnClones");
	}
	
	public void FastSpawnClones()
	{
		BeginBenchmark();
		{
			for (var i = 0; i < Step; i++)
			{
				var position = (Vector3)Random.insideUnitCircle * 6.0f;
				var clone    = Pool.FastSpawn(position, Quaternion.identity, null);
				
				spawnedClones.Add(clone);
			}
		}
		EndBenchmark("FastSpawnClones");
	}
	
	public void FastDespawnClones()
	{
		BeginBenchmark();
		{
			for (var i = 0; i < Step; i++)
			{
				var index = spawnedClones.Count - 1;
				
				if (index >= 0)
				{
					var clone = spawnedClones[index];
					
					spawnedClones.RemoveAt(index);
					
					Pool.FastDespawn(clone);
				}
			}
		}
		EndBenchmark("FastDespawnClones");
	}
	
	public void InstantiateClones()
	{
		BeginBenchmark();
		{
			for (var i = 0; i < Step; i++)
			{
				var position = (Vector3)Random.insideUnitCircle * 6.0f;
				var clone    = (GameObject)Instantiate(Prefab, position, Quaternion.identity);
				
				instantiatedClones.Add(clone);
			}
		}
		EndBenchmark("SpawnClones");
	}
	
	public void DestroyClones()
	{
		BeginBenchmark();
		{
			for (var i = 0; i < Step; i++)
			{
				var index = instantiatedClones.Count - 1;
				
				if (index >= 0)
				{
					var clone = instantiatedClones[index];
					
					instantiatedClones.RemoveAt(index);
					
					DestroyImmediate(clone);
				}
			}
		}
		EndBenchmark("DestroyClones");
	}
	
	private void BeginBenchmark()
	{
		benchmark.Reset();
		benchmark.Start();
	}
	
	private void EndBenchmark(string title)
	{
		benchmark.Stop();
		
		if (BenchmarkText != null)
		{
			BenchmarkText.text = title + " took " + benchmark.ElapsedMilliseconds + "ms";
		}
		
		//UnityEngine.Debug.Log(BenchmarkText.text);
		//UnityEngine.Debug.Break();
	}
}