using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterManager : MonoBehaviour
{
	public enum MonsterEngageType
	{
		Immediate,
		OnSight,
		None,
	}

	public GameObject meleeMonsterPrefab;
	public GameObject rangedMonsterPrefab;

	private List<MonsterController> monsters;

	void Start ()
	{
		monsters = new List<MonsterController> ();
	}

	public void SpawnMonsterMelee (MonsterEngageType engageType, bool spawnInSight, Transform t)
	{
		SpawnMonster (meleeMonsterPrefab, engageType, spawnInSight, t);
	}

	public void SpawnMonsterRanged (MonsterEngageType engageType, bool spawnInSight, Transform t)
	{
		SpawnMonster (rangedMonsterPrefab, engageType, spawnInSight, t);
	}

	private void SpawnMonster (GameObject prefab, MonsterEngageType engageType, bool spawnInSight, Transform t)
	{
		GameObject monster = Instantiate (prefab, t.position, t.rotation);
		MonsterController monsterController = monster.GetComponent<MonsterController> ();

		monsterController.OnMonsterDied += OnManagedMonsterDied;
		monsterController.spawnInSight = spawnInSight;
		if (engageType == MonsterEngageType.Immediate) {
			monsterController.engageOnSpawn = true;
		} else if (engageType == MonsterEngageType.OnSight) {
			monsterController.engageOnSight = true;
		}

		monsters.Add (monsterController);
	}

	public bool ExistLivingMonsters ()
	{
		return monsters.Count > 0;
	}

	public void OnManagedMonsterDied (MonsterController monsterController)
	{
		monsters.Remove (monsterController);
	}
}
