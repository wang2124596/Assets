using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FutureCode.Game.Command;

public class MonsterManager : MonoBehaviour {
    public int MaxMonsterCount = 15;
    public GameObject MonsterPrefab;
    public int ExistsMonsterCount { get { return Monsters.Count; } }
    public static MonsterManager Instance;
    public Dictionary<int, SkeletonBehaviour> Monsters;
    int nextId = 0;
    bool generating = false;
	// Use this for initialization
	void Start () {
        Instance = this;
        Monsters = new Dictionary<int, SkeletonBehaviour>();
	}

    void LateUpdate()
    {
        if (NetManager.Instance.Connected && GameSettings.Instance.IsPlayerClient)
        {
            if (!generating && ExistsMonsterCount < MaxMonsterCount)
            {
                var count = MaxMonsterCount - ExistsMonsterCount;
                StartCoroutine(GenerateMonster(count));
            }
        }
    }

    //void InitializeMonster()
    //{
    //    StartCoroutine(GenerateMonster());
    //}
    IEnumerator GenerateMonster(int count = 1)
    {
        generating = true;
        for (int i = 0; i < count; i++)
        {
            if (ExistsMonsterCount == MaxMonsterCount)
                break;
            var point = BirthPoint.GetEmptyPoint();
            while (!point.HasValue)
            {
                yield return new WaitForSeconds(1);
                point = BirthPoint.GetEmptyPoint();
            }
            var generatePos = point.Value;

            GenerateMonster(nextId, generatePos);
            var monster = Monsters[nextId];
            nextId++;
            var angle = Random.Range(0, 360);
            monster.transform.Rotate(monster.transform.up, angle);

            var cmd = monster.GetCurrentStateCmd(MonsterChangedStateType.Generated);
            NetManager.Instance.Send(cmd);

            yield return new WaitForSeconds(0.5f);
        }
        generating = false;
        //Monsters.Add(monster);
        //if (!initialized && nextId >= MaxMonsterCount)
        //    initialized = true;
    }
    public void MonsterDestroied(int id)
    {
        if (Monsters.ContainsKey(id))
        {
            GameObject.Destroy(Monsters[id].gameObject, 5);
            Monsters.Remove(id);
        }
        else
        {
            Debug.LogWarning("MonsterManager: Try to destroy the monster which id isn't exists.");
        }
    }
    public void MonsterStateChanged(MonsterStateChangedCommand cmd)
    {
        int id = cmd.MonsterID;
        Vector3 pos = new Vector3(cmd.PosX, cmd.PosY, cmd.PosZ);
        Quaternion rotation = new Quaternion(cmd.RotationX, cmd.RotationY, cmd.RotationZ, cmd.RotationW);

        if (!Monsters.ContainsKey(cmd.MonsterID))
        {
            GenerateMonster(id, pos, rotation, cmd.Life);
        }
        var monster = Monsters[id];
        monster.Life = cmd.Life;

        switch (cmd.ChangedStateType)
        {
            case MonsterChangedStateType.Attack:
                monster.AttackSync(pos, rotation, cmd.Time);
                break;
            case MonsterChangedStateType.Dead:
                monster.transform.position = pos;
                monster.transform.rotation = rotation;
                monster.Dead();
                break;
            case MonsterChangedStateType.StartWander:
                monster.StartLocomotionSync(pos, rotation, cmd.Time, 0);
                break;
            case MonsterChangedStateType.EndWander:
                monster.EndLocomotionSync(pos, rotation, cmd.Time);
                break;
            case MonsterChangedStateType.StartPursuit:
                monster.StartLocomotionSync(pos, rotation, cmd.Time, 1);
                break;
            case MonsterChangedStateType.EndPursuit:
                monster.EndLocomotionSync(pos, rotation, cmd.Time);
                break;
            case MonsterChangedStateType.Warn:
                monster.EndLocomotionSync(pos, rotation, cmd.Time);
                monster.Warn = true;
                break;
            case MonsterChangedStateType.Idle:
                monster.IdleSync(pos, rotation, cmd.Time);
                break;
        }
    }


    public void GenerateMonster(int id, Vector3 position, Quaternion rotation, int life = 100)
    {
        Debug.Log("generate monster");
        var monster = (GameObject)GameObject.Instantiate(MonsterPrefab, position, rotation);
        monster.transform.parent = transform;
        var sb = monster.GetComponent<SkeletonBehaviour>();
        sb.ID = id;
        sb.Life = life;
        Monsters.Add(sb.ID, sb);
    }
    public void GenerateMonster(int id, Vector3 position)
    {
        GenerateMonster(id, position, Quaternion.identity);
    }

    //public void DestoriedAll()
    //{
    //    foreach (var s in Monsters.Values)
    //    {
    //        if (!s.Death)
    //            GameObject.Destroy(s.transform.gameObject, 1f);
    //    }
    //    Monsters.Clear();

    //    ExistsMonsterCount = 0;
    //}
}
