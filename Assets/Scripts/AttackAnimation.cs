using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAnimation : StateMachineBehaviour //MonoBehaviour
{
    private AttackTargetManager attackTarget;
    private PlayerCondition condition;
    private PlayerMove parent;
    public GameObject effect; //����Ʈ�� ����

    public bool isLocalPlayer;

    public enum AttackType
    {
        A,
        B,
        C1,
        C2,
        D,
        E,
    }

    public AttackType attackType;
    private int frame = 0;


    private void Awake()
    {
        if (isLocalPlayer)
        {
            attackTarget = GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<AttackTargetManager>();
            parent = GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<PlayerMove>(); //PlayerInpur for������ AttackAnimation�� �Ҵ��ϴ� ������� �ٲܿ���
            condition = parent.GetComponent<PlayerCondition>();
        }
        else
        {
            attackTarget = GameObject.FindGameObjectWithTag("Player").GetComponent<AttackTargetManager>();
            parent = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMove>(); //PlayerInpur for������ AttackAnimation�� �Ҵ��ϴ� ������� �ٲܿ���
            condition = parent.GetComponent<PlayerCondition>();
        }


        switch (attackType) //skill effect settings
        {
            case AttackType.A:
                effect = Instantiate(effect, new Vector3(-0.0307480991f, 1.2700001f, 0.189999998f) + parent.transform.position, Quaternion.Euler(30, -100, -200), parent.transform);
                break;
            case AttackType.B:
                effect = Instantiate(effect, new Vector3(-0.0500000007f, 1.10295975f, -0.0782125294f) + parent.transform.position, Quaternion.Euler(86.5657806f, 87.8785095f, 66.9312592f), parent.transform);
                effect.transform.localScale = new Vector3(0.800000012f, 0.800000012f, 1);
                break;
            case AttackType.C1:
                effect = Instantiate(effect, new Vector3(0, 1, 1.09f) + parent.transform.position, Quaternion.Euler(0, 0, 0), parent.transform);
                effect.transform.localScale = new Vector3(1.57f, 1.57f, 1);
                break;
            case AttackType.C2:
                effect = Instantiate(effect, new Vector3(0, 1, 0) + parent.transform.position, Quaternion.Euler(0, 90, 90), parent.transform);
                break;
            case AttackType.D:
                effect = Instantiate(effect, new Vector3(0, 1, 0.369999886f) + parent.transform.position, Quaternion.Euler(0.331470579f, 358.565155f, 5.0682826f), parent.transform);
                effect.transform.localScale = new Vector3(1.20000005f, 0.800000012f, 1);
                break;
            case AttackType.E:
                effect = Instantiate(effect, new Vector3(0.0774749517f, 0.974506676f, -0.0287958607f) + parent.transform.position, Quaternion.Euler(351.56955f, 185.288879f, 101.211784f), parent.transform);
                effect.transform.localScale = new Vector3(0.100000001f, 0.100000001f, 1);
                break;
        }
        effect.SetActive(false);
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetFloat("AnimationSpeed", 1f);
        animator.SetBool("isAttacking", false);
        frame = 0;
        base.OnStateEnter(animator, stateInfo, layerIndex);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        effect.SetActive(false);
        if (attackType == AttackType.E)
        {
            animator.SetBool("isThreeCombo", false);
        }
        base.OnStateExit(animator, stateInfo, layerIndex);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) //�ִϸ��̼� �� �����Ӹ��� ������Ʈ
    {
        frame++;
        switch (attackType)
        {
            case AttackType.A:
                if (stateInfo.normalizedTime < .2f)
                {
                    animator.SetBool("isAttacking", false);
                }
                if (frame == 10)
                {
                    effect.SetActive(true);
                    attackTarget.Attack(0, parent.transform, 10);
                    if (isLocalPlayer) condition.StaminaUse(100);
                }
                break;
            case AttackType.B:
                if (stateInfo.normalizedTime < .2f)
                {
                    animator.SetBool("isAttacking", false);
                }
                if (frame == 10)
                {
                    effect.SetActive(true);
                    attackTarget.Attack(1, parent.transform, 10);
                    if (isLocalPlayer) condition.StaminaUse(100);
                }
                break;
            case AttackType.C1:
                if (stateInfo.normalizedTime < .2f)
                {
                    animator.SetBool("isAttacking", false);
                }
                if (frame == 10)
                {
                    effect.SetActive(true);
                    attackTarget.Attack(2, parent.transform, 10);
                    if (isLocalPlayer) condition.StaminaUse(100);
                }
                break;
            case AttackType.D:
                if (stateInfo.normalizedTime < .1f)
                {
                    animator.SetBool("isAttacking", false);
                }
                if (frame == 10)
                {
                    effect.SetActive(true);
                    attackTarget.Attack(4, parent.transform, 100);
                    if (isLocalPlayer) condition.StaminaUse(100);
                }
                break;
            case AttackType.E:
                if (stateInfo.normalizedTime < .2f)
                {
                    animator.SetBool("isAttacking", false);
                }
                if (frame == 10)
                {
                    effect.SetActive(true);
                    attackTarget.Attack(5, parent.transform, 100);
                    if (isLocalPlayer) condition.StaminaUse(200);
                }
                break;
        }



        base.OnStateUpdate(animator, stateInfo, layerIndex);
    }
}