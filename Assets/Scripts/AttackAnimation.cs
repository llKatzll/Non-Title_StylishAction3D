using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAnimation : StateMachineBehaviour //MonoBehaviour
{

    private PlayerMove parent;
    public GameObject effect; //����Ʈ�� ����

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
        parent = GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<PlayerMove>(); //PlayerInpur for������ AttackAnimation�� �Ҵ��ϴ� ������� �ٲܿ���

        switch (attackType) // ��ų ��ġ�� ���Ͱ� ��ġ ���ʹϾ��� �����̼�
        {
            case AttackType.A:
                effect = Instantiate(effect, new Vector3(-0.0307480991f, 1.2700001f, 0.189999998f), Quaternion.Euler(30, -100, -200), parent.transform);
                break;
            case AttackType.B:
                effect = Instantiate(effect, new Vector3(0.19406724f, 1.10295975f, -0.0782125294f), Quaternion.Euler(86.5657806f, 87.8785095f, 66.9312592f), parent.transform);
                effect.transform.localScale = new Vector3(0.800000012f, 0.800000012f, 1);
                break;
            case AttackType.C1:
                effect = Instantiate(effect, new Vector3(0, 1, 1.09f), Quaternion.Euler(0, 0, 0), parent.transform);
                effect.transform.localScale = new Vector3(1.57f, 1.57f, 1);
                break;
            case AttackType.C2:
                effect = Instantiate(effect, new Vector3(0, 1, 0), Quaternion.Euler(0, 90, 90), parent.transform);
                break;
            case AttackType.D:
                effect = Instantiate(effect, new Vector3(0, 1, 0.369999886f), Quaternion.Euler(0.331470579f, 358.565155f, 5.0682826f), parent.transform);
                effect.transform.localScale = new Vector3(1.20000005f, 0.800000012f, 1);
                break;
            case AttackType.E:
                effect = Instantiate(effect, new Vector3(0.0774749517f, 0.974506676f, -0.0287958607f), Quaternion.Euler(351.56955f, 185.288879f, 101.211784f), parent.transform);
                effect.transform.localScale = new Vector3(0.100000001f, 0.100000001f, 1);
                break;
        }
        //                   ����  , ��ġ        , �����̼�ȸ����,            �θ�
        effect.SetActive(false);
    }


    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        frame = 0;
        base.OnStateEnter(animator, stateInfo, layerIndex);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (attackType == AttackType.C1)
        {
            animator.SetBool("isThreeCombo", false);
        }
        effect.SetActive(false);
        animator.SetBool("isAttacking", false);
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
                }
                break;
            case AttackType.C2:
                if (stateInfo.normalizedTime < .1f)
                {
                    animator.SetBool("isAttacking", false);
                }
                if (frame == 10)
                {
                    effect.SetActive(true);
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
                }
                break;
        }



        base.OnStateUpdate(animator, stateInfo, layerIndex);
    }
}