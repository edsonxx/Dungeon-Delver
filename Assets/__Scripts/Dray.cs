﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dray : MonoBehaviour, IFacingMover, IKeyMaster
{
    public enum eMode { idle, move, attack, transition, knockback }

    [Header("Set in Inspector")]
    public float speed = 5;
    public float attackDuration = 0.25f;
    public float attackDelay = 0.5f;
    public float transitionDelay = 0.5f;// Room transition delay
    public int maxHealth = 10;
    public float knockbackSpeed = 10; // b
    public float knockbackDuration = 0.25f;
    public float invincibleDuration = 0.5f;

    [Header("Set Dynamically")]
    public int dirHeld = -1; // Direction of the held movement key
    public int facing = 1; // Direction Dray is facing
    public eMode mode = eMode.idle;
    public int numKeys = 0;
    public bool invincible = false;
    public bool hasGrappler = false;
    public Vector3 lastSafeLoc; // a
    public int lastSafeFacing;

    [SerializeField] // b
    private int _health;
    public int health
    { // c
        get { return _health; }
        set { _health = value; }
    }

    private float timeAtkDone = 0;
    private float timeAtkNext = 0;
    private float transitionDone = 0; // a
    private Vector2 transitionPos;    private float knockbackDone = 0; // d
    private float invincibleDone = 0;
    private Vector3 knockbackVel;
    private SpriteRenderer sRend;

    private Rigidbody rigid;
    private Animator anim;
    private InRoom inRm;

    private Vector3[] directions = new Vector3[] {
        Vector3.right, Vector3.up, Vector3.left, Vector3.down };

    private KeyCode[] keys = new KeyCode[] { KeyCode.RightArrow,
        KeyCode.UpArrow, KeyCode.LeftArrow, KeyCode.DownArrow };

    void Awake()
    {
        sRend = GetComponent<SpriteRenderer>();
        rigid = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        inRm = GetComponent<InRoom>();
        health = maxHealth;
        lastSafeLoc = transform.position; // The start position is safe.
        lastSafeFacing = facing;
    }    void Update()
    {
        if (invincible && Time.time > invincibleDone) invincible = false; // f
        sRend.color = invincible ? Color.red : Color.white;
        if (mode == eMode.knockback)
        {
            rigid.velocity = knockbackVel;
            if (Time.time < knockbackDone) return;
        }
        if (mode == eMode.transition)
        { // b
            rigid.velocity = Vector3.zero;
            anim.speed = 0;
            roomPos = transitionPos; // Keeps Dray in place
            if (Time.time < transitionDone) return;
            // The following line is only reached if Time.time >= transitionDone
            mode = eMode.idle;
        }
        dirHeld = -1;
        for (int i = 0; i < 4; i++)
        {
            if (Input.GetKey(keys[i])) dirHeld = i;
        }
        // Pressing the attack button(s)
        if (Input.GetKeyDown(KeyCode.Z) && Time.time >= timeAtkNext)
        {
            mode = eMode.attack;
            timeAtkDone = Time.time + attackDuration;
            timeAtkNext = Time.time + attackDelay;
        }
        //Finishing the attack when it's over
        if (Time.time >= timeAtkDone)
        {
            mode = eMode.idle;
        }
        //Choosing the proper mode if we're not attacking
        if (mode != eMode.attack)
        {
            if (dirHeld == -1)
            {
                mode = eMode.idle;
            }
            else
            {
                facing = dirHeld;
                mode = eMode.move;
            }
        }
        //————Act on the current mode————
        Vector3 vel = Vector3.zero;
        switch (mode)
        {
            case eMode.attack:
                anim.CrossFade("Dray_Attack_" + facing, 0);
                anim.speed = 0;
                break;
            case eMode.idle:
                anim.CrossFade("Dray_Walk_" + facing, 0);
                anim.speed = 0;
                break;
            case eMode.move:
                vel = directions[dirHeld];
                anim.CrossFade("Dray_Walk_" + facing, 0);
                anim.speed = 1;
                break;
        }
        rigid.velocity = vel * speed;

        if (dirHeld > -1) vel = directions[dirHeld];
        rigid.velocity = vel * speed;

        //animation
        if (dirHeld == -1)
        {
            anim.speed = 0;
        }
        else
        {
            anim.CrossFade("Dray_Walk_" + dirHeld, 0);
            anim.speed = 1;
        }
    }

    void LateUpdate()
    {
        // Get the half-grid location of this GameObject
        Vector2 rPos = GetRoomPosOnGrid(0.5f); // Forces halfgrid
                                               // c
                                               // Check to see whether we're in a Door tile
        int doorNum;
        for (doorNum = 0; doorNum < 4; doorNum++)
        {
            if (rPos == InRoom.DOORS[doorNum])
            {
                break; // d
            }
        }
        if (doorNum > 3 || doorNum != facing) return; // e
                                                      // Move to the next room
        Vector2 rm = roomNum;
        switch (doorNum)
        { // f
            case 0:
                rm.x += 1;
                break;
            case 1:
                rm.y += 1;
                break;
            case 2:
                rm.x -= 1;
                break;
            case 3:
                rm.y -= 1;
                break;
        }
        // Make sure that the rm we want to jump to is valid
        if (rm.x >= 0 && rm.x <= InRoom.MAX_RM_X)
        { // g
            if (rm.y >= 0 && rm.y <= InRoom.MAX_RM_Y)
            {
                roomNum = rm;
                transitionPos = InRoom.DOORS[(doorNum + 2) % 4]; // h
                roomPos = transitionPos;
                lastSafeLoc = transform.position; // b
                lastSafeFacing = facing;
                mode = eMode.transition; // i
                transitionDone = Time.time + transitionDelay;
            }
        }
    }
    void OnCollisionEnter(Collision coll)
    {
        if (invincible) return; // Return if Dray can't be damaged // g
        DamageEffect dEf = coll.gameObject.GetComponent<DamageEffect>();
        if (dEf == null) return; // If no DamageEffect, exit this method
        health -= dEf.damage;// Subtract the damage amount from health // h
        invincible = true; // Make Dray invincible
        invincibleDone = Time.time + invincibleDuration;
        if (dEf.knockback)
        { // Knockback Dray // i
          // Determine the direction of knockback
            Vector3 delta = transform.position - coll.transform.position;
            if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
            {
                // Knockback should be horizontal
                delta.x = (delta.x > 0) ? 1 : -1;
                delta.y = 0;
            }
            else
            {
                // Knockback should be vertical
                delta.x = 0;
                delta.y = (delta.y > 0) ? 1 : -1;
            }
            // Apply knockback speed to the Rigidbody
            knockbackVel = delta * knockbackSpeed;
            rigid.velocity = knockbackVel;
            // Set mode to knockback and set time to stop knockback
            mode = eMode.knockback;
            knockbackDone = Time.time + knockbackDuration;
        }
    }
    void OnTriggerEnter(Collider colld)
    {
        PickUp pup = colld.GetComponent<PickUp>
        (); // a
        if (pup == null) return;
        switch (pup.itemType)
        {
            case PickUp.eType.health:
                health = Mathf.Min(health + 2, maxHealth);
                break;
            case PickUp.eType.key:
                keyCount++;
                break;

            case PickUp.eType.grappler: // c
                hasGrappler = true;
                break;
        }
        Destroy(colld.gameObject);
    }    public void ResetInRoom(int healthLoss = 0)
    { // d
        transform.position = lastSafeLoc;
        facing = lastSafeFacing;
        health -= healthLoss;
        invincible = true; // Make Dray invincible
        invincibleDone = Time.time + invincibleDuration;
    }

        // Implementation of IFacingMover
        public int GetFacing()
    { // c
        return facing;
    }
    public bool moving
    { // d
        get
        {
            return (mode == eMode.move);
        }
    }
    public float GetSpeed()
    { // e
        return speed;
    }
    public float gridMult
    {
        get { return inRm.gridMult; }
    }
    public Vector2 roomPos
    { // f
        get { return inRm.roomPos; }
        set { inRm.roomPos = value; }
    }
    public Vector2 roomNum
    {
        get { return inRm.roomNum; }
        set { inRm.roomNum = value; }
    }
    public Vector2 GetRoomPosOnGrid(float mult = -1)
    {
        return inRm.GetRoomPosOnGrid(mult);
    }
    // Implementation of IKeyMaster
    public int keyCount
    { // d
        get { return numKeys; }
        set { numKeys = value; }
    }
}
