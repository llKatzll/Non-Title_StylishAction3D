using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    #region Private Field
    CharacterController controller;
    PlayerCondition condition;
    Coroutine healCoroutine;
    float gravity;
    Animator animator;
    public float runspeed;
    public float threecomboendtiming;
    long lastForward; //���������� W�� ������ �ð�
    #endregion

    public float speed;
    public int downStat = 0;
    public int maxDownStat = 0;
    // Start is called before the first frame update


    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        condition = GetComponent<PlayerCondition>();
    }

    public void damaged(Transform hitter, int downStat)
    {
        Debug.Log(hitter.name);
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Down")) //�ٿ���ϴ� ���϶����� �ǰݴ����� �ʴ´�.
        {
            return;
        }

        transform.LookAt(hitter);


        this.downStat += downStat;
        if (this.downStat >= maxDownStat)
        {
            this.downStat = 0;
            animator.CrossFade("KnockDown", .25f);
            return;
        }
        animator.SetTrigger("Damaged");
    }

    // Update is called once per frame
    void Update()
    {
        if (!transform.CompareTag("LocalPlayer"))
        {
            return;
        }
        animator.SetInteger("Horizontal", (int)Input.GetAxisRaw("Horizontal"));
        animator.SetInteger("Vertical", (int)Input.GetAxisRaw("Vertical"));

        if ((!animator.GetNextAnimatorStateInfo(0).IsTag("Attack") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack")) && //�������� �ƴ�"�鼭"
            (!animator.GetNextAnimatorStateInfo(0).IsTag("Guard") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Guard")) &&
            (!animator.GetNextAnimatorStateInfo(0).IsTag("Skill") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Skill")) && //��ų ��뵵
            (!animator.GetNextAnimatorStateInfo(0).IsTag("Dodge") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Dodge")))
        {
            Vector3 dir = (transform.right * speed * Time.deltaTime * Input.GetAxisRaw("Horizontal") * (animator.GetBool("isRunning") ? runspeed : 1)) + //����
            (transform.forward * speed * Time.deltaTime * Input.GetAxisRaw("Vertical") * (animator.GetBool("isRunning") ? runspeed : 1)) + //����
            transform.up * gravity; //���Ʒ�
            if (animator.GetBool("isRunning"))
            {
                condition.StaminaUse(1);
                if (healCoroutine != null)
                {
                    StopCoroutine(healCoroutine);
                    healCoroutine = null;
                }
            }
            else
            {
                if (healCoroutine == null)
                {
                    healCoroutine = StartCoroutine(StaminaHeal());
                }
            }
            controller.Move(dir);
        }
        else
        {
            if (healCoroutine != null)
            {
                StopCoroutine(healCoroutine);
                healCoroutine = null;
            }
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            if (DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastForward <= 300)
            {
                if (condition.CanUseStamina())
                {
                    animator.SetBool("isRunning", true); //
                }
            }
            lastForward = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        //�� �̻��� �Է��� �����ϱ� || 
        if (Input.GetAxisRaw("Vertical") == 0 || !condition.CanUseStamina())
        {
            animator.SetBool("isRunning", false);
        }

        if (controller.isGrounded)
        {
            gravity = -.1f; //�߷��� Ư���� ������ ����
        }
        else
        {
            gravity -= .05f; //�� �����Ӵ� �߷¿� 0.1�̶�� ���� �����ش�
        }

        if (Input.GetMouseButtonDown(0) && (!animator.GetNextAnimatorStateInfo(0).IsTag("Guard") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Guard"))
            && (!animator.GetNextAnimatorStateInfo(0).IsTag("Skill") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Skill"))
            && (!animator.GetNextAnimatorStateInfo(0).IsTag("Dodge") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Dodge"))) //��Ŭ���� ������ ���
        {
            if (!condition.CanUseStamina())
            {
                goto NEXT_ATTACK;
            }
            if (!animator.GetBool("isAttacking") && !animator.GetNextAnimatorStateInfo(0).IsTag("Attack") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
            {
                
                animator.CrossFade("AttackStart", .25f, 0);
                animator.SetBool("isThreeCombo", false);
                goto NEXT_ATTACK;
            }

            if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= .1f) //�ִϸ��̼� 10�ۼ�Ʈ�� �ȵǸ� ���Է� �Ǵ��ϰ� ����
            {
                goto NEXT_ATTACK;
            }

            if (animator.GetNextAnimatorStateInfo(0).IsTag("Attack") && animator.GetNextAnimatorStateInfo(0).normalizedTime <= .1f)
            {
                goto NEXT_ATTACK;
            }

            if (!animator.GetNextAnimatorStateInfo(0).IsTag("Attack") && animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= .5f)
            {
                goto NEXT_ATTACK;
            }

            if (animator.GetCurrentAnimatorStateInfo(0).shortNameHash == Animator.StringToHash("2") && !animator.GetBool("isAttacking"))
            {
                if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > .3f)
                {
                    animator.SetBool("isThreeCombo", true);
                }
            }
            if (!animator.GetBool("isAttacking") && (animator.GetNextAnimatorStateInfo(0).IsTag("Attack") || animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack")))
            {
                
                animator.SetBool("isAttacking", true);
            }

        }
        NEXT_ATTACK:


        if (Input.GetKeyDown(KeyCode.F) && (!animator.GetNextAnimatorStateInfo(0).IsTag("Attack") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack")) &&
            (!animator.GetNextAnimatorStateInfo(0).IsTag("Skill") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Skill")) &&
            (!animator.GetNextAnimatorStateInfo(0).IsTag("Dodge") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Dodge"))) //�����
        {
            if (!animator.GetBool("isGuarding") && !animator.GetNextAnimatorStateInfo(0).IsTag("Guard") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Guard"))
            {
                animator.CrossFade("GuardStart", .25f, 0);

            }
            animator.SetBool("isGuarding", true);
        }
        if (Input.GetKey(KeyCode.F))
        {
            condition.StaminaUse(2);
        }
        if (Input.GetKeyUp(KeyCode.F)) //��� ��� (���µ� �ɸ��� �ð� �˻� ����)
        {
            animator.SetBool("isGuarding", false);
        }
        if(Input.GetKeyDown(KeyCode.Space) && controller.isGrounded && (!animator.GetNextAnimatorStateInfo(0).IsTag("Attack") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack")) && //�������� �ƴ�"�鼭"
            (!animator.GetNextAnimatorStateInfo(0).IsTag("Guard") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Guard")) &&
            (!animator.GetNextAnimatorStateInfo(0).IsTag("Skill") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Skill")) && //��ų ��뵵
            (!animator.GetNextAnimatorStateInfo(0).IsTag("Dodge") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Dodge")) &&
            (!animator.GetNextAnimatorStateInfo(0).IsTag("isJumping") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("isJumping")))
        {
            animator.SetTrigger("isJumping");

        }
    }

    IEnumerator StaminaHeal()
    {
        yield return new WaitForSeconds(1f);
        while (true)
        {
            condition.StaminaUse(-10);
            yield return null;
        }
    }

}
