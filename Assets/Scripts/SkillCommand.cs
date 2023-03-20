using Nakama.TinyJson;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SkillCommand : MonoBehaviour
{
    public PlayerCondition condition;
    private Network network;

    private void Start()
    {
        condition = GetComponent<PlayerCondition>();
        network = FindObjectOfType<Network>();
    }

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
        DodgeBack,
    }

    private Dictionary<string, Skill> whileDodgeCommand = new Dictionary<string, Skill>() {
        { "WWE", Skill.A },
        { "AAE", Skill.B },
        { "DDE", Skill.C },
    }; //DodgeAttack

    private Dictionary<string, Skill> whileAttackCommnand = new Dictionary<string, Skill>() {
        { "ADSE", Skill.G },
    }; //WhileAttack

    private Dictionary<string, Skill> whileIdleCommand = new Dictionary<string, Skill>() {
        { "DASE", Skill.H },
    }; //IdleAttack

    long lastCommandTime = 0L;
    Animator animator;
    string _tmpCommand; //Key Stroke Class
    string tmpCommand
    {
        get
        {
            if (DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastCommandTime > 300) //Input
            {
                _tmpCommand = "";
            }
            return _tmpCommand;
        }
        set
        {
            if (DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastCommandTime > 300) //Input
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
                        condition.StaminaUse(300);
                        animator.CrossFade("Skill_G", .25f);
                        Dictionary<string, int> map = new Dictionary<string, int>()
                        {
                            {
                                "skill", (int) Skill.G
                            }
                        };
                        network.SendPacket(Network.NetworkOrder.PlayerSkill, map.ToJson());
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
                        condition.StaminaUse(150);
                        animator.CrossFade("Skill_A", .25f);
                        Dictionary<string, int> map = new Dictionary<string, int>()
                        {
                            {
                                "skill", (int) Skill.A
                            }
                        };
                        network.SendPacket(Network.NetworkOrder.PlayerSkill, map.ToJson());
                        break;
                    case Skill.B:
                        condition.StaminaUse(150);
                        animator.CrossFade("Skill_B", .25f);
                        map = new Dictionary<string, int>()
                        {
                            {
                                "skill", (int) Skill.B
                            }
                        };
                        network.SendPacket(Network.NetworkOrder.PlayerSkill, map.ToJson());
                        break;
                    case Skill.C:
                        condition.StaminaUse(150);
                        animator.CrossFade("Skill_C", .25f);
                        map = new Dictionary<string, int>()
                        {
                            {
                                "skill", (int) Skill.C
                            }
                        };
                        network.SendPacket(Network.NetworkOrder.PlayerSkill, map.ToJson());
                        break;
                }
            }
            return;
        }

        if (!animator.GetNextAnimatorStateInfo(0).IsTag("Guard") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Guard") //!�⺻ ���� && !������ && !����
            && !animator.GetNextAnimatorStateInfo(0).IsTag("isJumping") && !animator.GetCurrentAnimatorStateInfo(0).IsTag("isJumping"))

        {
            //�ƹ��͵� ���ϴ� ����
            if (Input.GetKey(KeyCode.S) && Input.GetKeyDown(KeyCode.Q))
            {
                condition.StaminaUse(100);
                animator.CrossFade("Dodge_Back", .25f);

                Dictionary<string, int> map = new Dictionary<string, int>()
                        {
                            {
                                "skill", (int) Skill.DodgeBack
                            }
                        };
                network.SendPacket(Network.NetworkOrder.PlayerSkill, map.ToJson());
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
                        condition.StaminaUse(200);
                        animator.CrossFade("Skill_H", .25f);
                        Dictionary<string, int> map = new Dictionary<string, int>()
                        {
                            {
                                "skill", (int) Skill.H
                            }
                        };
                        network.SendPacket(Network.NetworkOrder.PlayerSkill, map.ToJson());
                        break;
                }
            }
            return;
        }

    }
}