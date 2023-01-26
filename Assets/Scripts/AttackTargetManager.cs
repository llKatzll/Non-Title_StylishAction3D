using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackTargetManager : MonoBehaviour
{

    public AttackTarget[] attackTargets;

    public void Attack(int type)
    {
        attackTargets[type].Attack();
    }
}