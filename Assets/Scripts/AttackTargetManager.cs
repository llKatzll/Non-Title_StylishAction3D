using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackTargetManager : MonoBehaviour
{

    public AttackTarget[] attackTargets;

    public void Attack(int type, Transform hitter, int downStat)
    {
        attackTargets[type].Attack(hitter, downStat);
    }
}