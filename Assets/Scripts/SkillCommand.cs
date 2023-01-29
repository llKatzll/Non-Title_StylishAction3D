using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SkillCommand : MonoBehaviour
{
    public enum Skill
    {
        A,
        B,
        C,
        D,
        E,
        F,
        G,
        H,
    }

    private Dictionary<string, Skill> whileDodgeCommand = new Dictionary<string, Skill>() {
        { "WWE", Skill.A },
        { "AAE", Skill.B },
        { "DDE", Skill.C },
    }; //������ ���߿� �νĵ� Ŀ�ǵ�

    private Dictionary<string, Skill> whileAttackCommnand = new Dictionary<string, Skill>() {
        { "ADSE", Skill.G },
    }; //��Ÿ ���߿� �νĵ� Ŀ�ǵ�

    private Dictionary<string, Skill> whileIdleCommand = new Dictionary<string, Skill>() {
        { "DASE", Skill.H },
    }; //�⺻ ���¿��� �νĵ� Ŀ�ǵ�

    long lastCommandTime = 0L;
    Animator animator;
    string _tmpCommand; //�츮�� �Է��� Ŀ�ǵ� (�ӽ÷� ����� ����)
    string tmpCommand
    {
        get
        {
            if (DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastCommandTime > 300) //Ű �Է� ���� �ð� ���̰� 0.45�ʸ� �ѱ�� ���
            {
                _tmpCommand = "";
            }
            return _tmpCommand;
        }
        set
        {
            if (DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastCommandTime > 300) //Ű �Է� ���� �ð� ���̰� 0.45�ʸ� �ѱ�� ���
            {
                _tmpCommand = "";
            }
            _tmpCommand = value;
            lastCommandTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            //Debug.Log(_tmpCommand);
            SkillCheck();
        }
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Update()
    {
        if (!transform.CompareTag("LocalPlayer"))
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            tmpCommand += "A";
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            tmpCommand += "S";
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            tmpCommand += "D";
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            tmpCommand += "W";
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            tmpCommand += "X";
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            tmpCommand += "Q";
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            tmpCommand += "E";
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            tmpCommand += "C";
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            tmpCommand += "Z";
        }
    }

    void SkillCheck()
    {
        if (animator.GetNextAnimatorStateInfo(0).IsTag("Skill") || animator.GetCurrentAnimatorStateInfo(0).IsTag("Skill")) //���� ��ų�� ������̸�
        {
            return;
        }
        if (animator.GetNextAnimatorStateInfo(0).IsTag("Attack") || animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack")) //���� ���°� �⺻ ���� �����ΰ��
        {
            if (whileAttackCommnand.ContainsKey(tmpCommand)) //whileAttack tmpCommand Ű�� �����ϴ� ���
            {
                //
                Debug.Log(whileAttackCommnand[tmpCommand]);
                switch (whileAttackCommnand[tmpCommand])
                {
                    case Skill.G:
                        animator.CrossFade("Skill_G", .25f);
                        break;
                }
            }
            return;
        }

        if (animator.GetNextAnimatorStateInfo(0).IsTag("Dodge") || animator.GetCurrentAnimatorStateInfo(0).IsTag("Dodge")) //���� ���°� ������ �����ΰ��
        {
            if (whileDodgeCommand.ContainsKey(tmpCommand)) //whileDodge tmpCommand Ű�� �����ϴ� ���
            {
                //
                Debug.Log(whileDodgeCommand[tmpCommand]);
                switch (whileDodgeCommand[tmpCommand])
                {
                    case Skill.A:
                        animator.CrossFade("Skill_A", .25f);
                        break;
                    case Skill.B:
                        animator.CrossFade("Skill_B", .25f);
                        break;
                    case Skill.C:
                        animator.CrossFade("Skill_C", .25f);
                        break;
                }
            }
            return;
        }

        if (!animator.GetNextAnimatorStateInfo(0).IsTag("Guard") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Guard")) //!�⺻ ���� && !������ && !����
        {
            //�ƹ��͵� ���ϴ� ����
            if (Input.GetKey(KeyCode.S) && Input.GetKeyDown(KeyCode.Q))
            {
                animator.CrossFade("Dodge_Back", .25f);
                _tmpCommand = "";
                return;
            }

            if (whileIdleCommand.ContainsKey(tmpCommand)) //whilIdle tmpCommand Ű�� �����ϴ� ���
            {
                //
                Debug.Log(whileIdleCommand[tmpCommand]);
                switch (whileIdleCommand[tmpCommand])
                {
                    case Skill.H:
                        animator.CrossFade("Skill_H", .25f);
                        break;
                }
            }
            return;
        }

    }
}