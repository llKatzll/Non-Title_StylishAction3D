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

    Network network;
     
    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        condition = GetComponent<PlayerCondition>();
        network = FindObjectOfType<Network>();

        //for(int i = 0; i < 5; i++)
        //{
        //    yield return new WaitForSeconds(1); //1�������� ����ϴ� �۾� (10�������� ����Ѵ�)
        //}

        if (!transform.CompareTag("LocalPlayer"))
        {
            network.otherPlayer = this;
            network.otherPlayerTransform = transform;
        }

        if (network.UserIndex == 0)
        {
            if(transform.CompareTag("LocalPlayer"))
            {
                controller.Move(Vector3.zero);
                Debug.Log("LocalPlayer" + "0" + transform.position);
            } else
            {
                controller.Move(transform.forward * 10);
                Debug.Log("Player" + "0" + transform.position);
            }
        } else
        {
            if (transform.CompareTag("LocalPlayer"))
            {
                controller.Move(transform.forward * 10);
                Debug.Log("LocalPlayer" + "1" + transform.position);
            }
            else
            {
                controller.Move(Vector3.zero);
                Debug.Log("Player" + "1" + transform.position);
            }
        }

        Application.targetFrameRate = 60;
        network.Ping();
    }

    public void damaged(Transform hitter, int downStat)
    {
        //hitter : Ÿ����
        //transform : �ǰ���
        //Ÿ���ڿ� �ǰ����� Tag�� ����
        if (hitter.CompareTag(transform.tag)) //�±װ� ���� ���ٸ� 
        {
            return;
        }

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

    void FixedUpdate()
    {
        if(network.otherPlayer == null)
        {
            return;
        }
        if (!transform.CompareTag("LocalPlayer"))
        {
            if (controller.isGrounded)
            {
                gravity = -.1f; //�߷��� Ư���� ������ ����
            } 
            else
            {
                gravity -= .05f; //�� �����Ӵ� �߷¿� 0.1�̶�� ���� �����ش�
            }

            if (animator.GetBool("isGuarding"))
            {
                if (condition.CanUseStamina())
                {
                    condition.StaminaUse(2);
                }
                else
                {
                    animator.SetBool("isGuarding", false);
                }
            }

            if ((!animator.GetNextAnimatorStateInfo(0).IsTag("Attack") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack")) && //�������� �ƴ�"�鼭"
            (!animator.GetNextAnimatorStateInfo(0).IsTag("Guard") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Guard")) &&
            (!animator.GetNextAnimatorStateInfo(0).IsTag("Skill") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Skill")) && //��ų ��뵵
            (!animator.GetNextAnimatorStateInfo(0).IsTag("Dodge") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Dodge")))
            {
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

            }
            else
            {
                if (healCoroutine != null)
                {
                    StopCoroutine(healCoroutine);
                    healCoroutine = null;
                }
            }
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

            //���Ⱑ �߰�
            var destination = transform.position + dir;
            //End

            network.Move(new float[] { Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), destination.x, destination.y, destination.z });

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
    }

    void Update()
    {
        if (network.otherPlayer == null)
        {
            return;
        }
        if (!transform.CompareTag("LocalPlayer"))
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            if (DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastForward <= 300)
            {
                if (condition.CanUseStamina())
                {
                    animator.SetBool("isRunning", true); //
                    network.SendPacket(Network.NetworkOrder.PlayerRunStart, null);
                }
            }
            lastForward = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        //�� �̻��� �Է��� �����ϱ� || 
        if (Input.GetAxisRaw("Vertical") == 0 || !condition.CanUseStamina())
        {
            animator.SetBool("isRunning", false);
            network.SendPacket(Network.NetworkOrder.PlayerRunEnd, null);
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
                network.SendPacket(Network.NetworkOrder.PlayerAttackStart, null); //���� ����
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
                network.SendPacket(Network.NetworkOrder.PlayerAttack, animator.GetBool("isThreeCombo").ToString());
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

            network.SendPacket(Network.NetworkOrder.PlayerGuard, null);
            animator.SetBool("isGuarding", true);
        }

        if (Input.GetKey(KeyCode.F))
        {
            if(condition.CanUseStamina()) //FŰ�� �� �����������鼭, ���¹̳��� ����� �� �ִ� ��Ȳ�̸�
            {
                condition.StaminaUse(2); //���¹̳��� ���
            } else //���¹̳��� ����� �� ���� ��Ȳ�̶��
            {
                network.SendPacket(Network.NetworkOrder.PlayerGuardEnd, null); //���� �ߴ��� �����Ѵ�.
                animator.SetBool("isGuarding", false); //���� �ߴ�
            }
        }
        
        if (Input.GetKeyUp(KeyCode.F) || (!Input.GetKey(KeyCode.F) && animator.GetBool("isGuarding"))) //��� ��� (���µ� �ɸ��� �ð� �˻� ����)
        {
            network.SendPacket(Network.NetworkOrder.PlayerGuardEnd, null);
            animator.SetBool("isGuarding", false);
        }

        if(Input.GetKeyDown(KeyCode.Space) && controller.isGrounded && (!animator.GetNextAnimatorStateInfo(0).IsTag("Attack") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack")) && //�������� �ƴ�"�鼭"
            (!animator.GetNextAnimatorStateInfo(0).IsTag("Guard") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Guard")) &&
            (!animator.GetNextAnimatorStateInfo(0).IsTag("Skill") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Skill")) && //��ų ��뵵
            (!animator.GetNextAnimatorStateInfo(0).IsTag("Dodge") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Dodge")) &&
            (!animator.GetNextAnimatorStateInfo(0).IsTag("isJumping") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("isJumping")))
        {
            animator.SetTrigger("isJumping");
            network.SendPacket(Network.NetworkOrder.PlayerJump, null);
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

    public void Jump()
    {
        if ((!animator.GetNextAnimatorStateInfo(0).IsTag("Attack") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack")) && //�������� �ƴ�"�鼭"
            (!animator.GetNextAnimatorStateInfo(0).IsTag("Guard") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Guard")) &&
            (!animator.GetNextAnimatorStateInfo(0).IsTag("Skill") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Skill")) && //��ų ��뵵
            (!animator.GetNextAnimatorStateInfo(0).IsTag("Dodge") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Dodge")) &&
            (!animator.GetNextAnimatorStateInfo(0).IsTag("isJumping") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("isJumping")))
        {
            animator.SetTrigger("isJumping");
        }
    }

    public void RunStart()
    {
        animator.SetBool("isRunning", true);
    }

    public void RunEnd()
    {
        animator.SetBool("isRunning", false);
    }

    public void GuardStart()
    {
        if (!animator.GetBool("isGuarding") && !animator.GetNextAnimatorStateInfo(0).IsTag("Guard") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Guard"))
        {
            animator.CrossFade("GuardStart", .25f, 0);

        }
        animator.SetBool("isGuarding", true);
    }

    public void GuardEnd()
    {
        animator.SetBool("isGuarding", false);
    }

    public void AttackStart()
    {
        animator.CrossFade("AttackStart", .25f, 0);
        animator.SetBool("isThreeCombo", false);
    }
    public void Attack(bool b)
    {
        if(b)
        {
            animator.SetBool("isThreeCombo", true);
        }
        animator.SetBool("isAttacking", true);
    }
    
    //��Ƽ�÷��̾� - �ٸ� ������ �����̴� �Լ�
    public void Move(float[] dirF)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(delegate
        {
            animator.SetInteger("Horizontal", (int)dirF[0]);
            animator.SetInteger("Vertical", (int)dirF[1]);

            if ((!animator.GetNextAnimatorStateInfo(0).IsTag("Attack") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack")) && //�������� �ƴ�"�鼭"
            (!animator.GetNextAnimatorStateInfo(0).IsTag("Guard") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Guard")) &&
            (!animator.GetNextAnimatorStateInfo(0).IsTag("Skill") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Skill")) && //��ų ��뵵
            (!animator.GetNextAnimatorStateInfo(0).IsTag("Dodge") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Dodge")))
            {
                Vector3 dir = (transform.right * speed * Time.deltaTime * dirF[0] * (animator.GetBool("isRunning") ? runspeed : 1)) + //����
                (transform.forward * speed * Time.deltaTime * dirF[1] * (animator.GetBool("isRunning") ? runspeed : 1)) + //����
                transform.up * gravity; //���Ʒ�

                //�߰�
                dir = new Vector3(dirF[2], dirF[3], dirF[4]) - transform.position;
                //End

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
        });
    }

    public void SkillUse(SkillCommand.Skill skillType)
    {
        switch(skillType)
        {
            case SkillCommand.Skill.G:
                condition.StaminaUse(300);
                animator.CrossFade("Skill_G", .25f);
                break;
            case SkillCommand.Skill.A:
                condition.StaminaUse(150);
                animator.CrossFade("Skill_A", .25f);
                break;
            case SkillCommand.Skill.B:
                condition.StaminaUse(150);
                animator.CrossFade("Skill_B", .25f);
                break;
            case SkillCommand.Skill.C:
                condition.StaminaUse(150);
                animator.CrossFade("Skill_C", .25f);
                break;
            case SkillCommand.Skill.DodgeBack:
                condition.StaminaUse(100);
                animator.CrossFade("Dodge_Back", .25f);
                break;
            case SkillCommand.Skill.H:
                condition.StaminaUse(200);
                animator.CrossFade("Skill_H", .25f);
                break;
        }
    }

}
