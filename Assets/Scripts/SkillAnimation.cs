using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillAnimation : StateMachineBehaviour //MonoBehaviour
{

    private PlayerMove parent;
    private PlayerCondition condition;
    private AttackTargetManager attackTarget;
    public GameObject effect; //이펙트의 원본

    public bool isLocalPlayer;
    public enum SkillType
    {
        A,
        B,
        C,
        D,
        E,
        F,
        G,
        H,
        I
    }

    public SkillType skillType;
    private int frame = 0;


    private void Awake()
    {
        if (isLocalPlayer)
        {
            attackTarget = GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<AttackTargetManager>();
            parent = GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<PlayerMove>(); //PlayerInpur for문으로 AttackAnimation에 할당하는 방법으로 바꿀예정
            condition = parent.GetComponent<PlayerCondition>();
        }
        else
        {
            attackTarget = GameObject.FindGameObjectWithTag("Player").GetComponent<AttackTargetManager>();
            parent = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMove>(); //PlayerInpur for문으로 AttackAnimation에 할당하는 방법으로 바꿀예정
            condition = parent.GetComponent<PlayerCondition>();
        }

        switch (skillType)
        {
            case SkillType.A:
                effect = Instantiate(effect, new Vector3(0, 1, 0) + parent.transform.position, Quaternion.Euler(0, 0, 0), parent.transform);
                break;
            case SkillType.B:
                effect = Instantiate(effect, new Vector3(0, 1, 0) + parent.transform.position, Quaternion.Euler(0, 90, 0), parent.transform);
                break;
            case SkillType.C:
                effect = Instantiate(effect, new Vector3(0, 1, 0) + parent.transform.position, Quaternion.Euler(0, 90, 0), parent.transform);
                break;
            case SkillType.D:
                effect = Instantiate(effect, new Vector3(0, 1, 0) + parent.transform.position, Quaternion.Euler(0, 180, 0), parent.transform);
                break;
            case SkillType.E:
                effect = Instantiate(effect, new Vector3(0, 1, 0) + parent.transform.position, Quaternion.Euler(0, 180, -50), parent.transform);
                break;
            case SkillType.F:
                effect = Instantiate(effect, new Vector3(0, 1, 0) + parent.transform.position, Quaternion.Euler(0, 180, -50), parent.transform);
                break;
            case SkillType.G:
                effect = Instantiate(effect, new Vector3(0, 1, 0) + parent.transform.position, Quaternion.Euler(0, 180, -50), parent.transform);
                break;
            case SkillType.H:
                effect = Instantiate(effect, new Vector3(0, 1, 0) + parent.transform.position, Quaternion.Euler(0, 180, -50), parent.transform);
                break;
            case SkillType.I:
                effect = Instantiate(effect, new Vector3(0, 1, 0) + parent.transform.position, Quaternion.Euler(0, 180, -50), parent.transform);
                break;
        }
        effect.SetActive(false);
    }


    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        frame = 0;
        base.OnStateEnter(animator, stateInfo, layerIndex);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        effect.SetActive(false);
        base.OnStateExit(animator, stateInfo, layerIndex);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) //애니메이션 한 프레임마다 업데이트
    {
        frame++;
        if (frame == 10)
        {
            condition.StaminaUse(1);
            effect.SetActive(true);
            attackTarget.Attack((int)skillType + 6, parent.transform, 100);
        }
        base.OnStateUpdate(animator, stateInfo, layerIndex);
    }
}