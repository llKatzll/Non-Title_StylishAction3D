using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    #region Private Field
    CharacterController controller;
    float gravity;
    Animator animator;
    public float runspeed;
    public float threecomboendtiming;
    long lastForward; //���������� W�� ������ �ð�
    #endregion

    public float speed;
    // Start is called before the first frame update


    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    public void damaged()
    {
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
             (!animator.GetNextAnimatorStateInfo(0).IsTag("Skill") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Skill")))
        {
            Vector3 dir = (transform.right * speed * Time.deltaTime * Input.GetAxisRaw("Horizontal") * (animator.GetBool("isRunning") ? runspeed : 1)) + //����
            (transform.forward * speed * Time.deltaTime * Input.GetAxisRaw("Vertical") * (animator.GetBool("isRunning") ? runspeed : 1)) + //����
            transform.up * gravity; //���Ʒ�
            controller.Move(dir);
        }
       
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastForward <= 300)
            {
                animator.SetBool("isRunning", true);
            }
            lastForward = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }
        if (Input.GetAxisRaw("Vertical") == 0)
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

        if (Input.GetMouseButtonDown(0) && !animator.GetNextAnimatorStateInfo(0).IsTag("Guard") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Guard") &&
            (!animator.GetNextAnimatorStateInfo(0).IsTag("Skill") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Skill")) &&
            (!animator.GetNextAnimatorStateInfo(0).IsTag("Dodge") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Dodge"))) //��Ŭ���� ������ ���
        {
            if(animator.GetCurrentAnimatorStateInfo(0).shortNameHash == Animator.StringToHash("2") && !animator.GetBool("isAttacking"))
            {
                if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > threecomboendtiming)
                {
                    animator.SetBool("isThreeCombo", true);
                }
                else
                {
                    animator.SetBool("isThreeCombo", false);
                }
            }
            Debug.Log("Clicked");
            if (!animator.GetBool("isAttacking") && !animator.GetNextAnimatorStateInfo(0).IsTag("Attack") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
            {
                animator.CrossFade("AttackStart", .25f,0);
            }
            animator.SetBool("isAttacking", true);
        }


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
        if (Input.GetKeyUp(KeyCode.F)) //��� ��� (���µ� �ɸ��� �ð� �˻� ����)
        {
            animator.SetBool("isGuarding", false);
        }
    }
}
