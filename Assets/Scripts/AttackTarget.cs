using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackTarget : MonoBehaviour
{
    List<GameObject> targets = new List<GameObject>();

    public void Attack(Transform hitter, int downStat)
    {
        for (int i = 0; i < targets.Count; i++)
        {
            Debug.Log(targets[i]);
            targets[i].GetComponent<PlayerMove>().damaged(hitter, downStat);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        targets.Add(other.gameObject);
    }

    public void OnTriggerExit(Collider other)
    {
        targets.Remove(other.gameObject);
    }
}