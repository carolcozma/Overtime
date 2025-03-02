using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMelee : MonoBehaviour
{

    [Header("Important variables: ")]
    public EnemyMaster enemy;
    //public Transform DamageSphereOrigin;
    public LayerMask PlayerLayer;
    //public float DamageSphereRange;
    public float DistanceToStartPunch;
    public Transform KnifePosition;
    public List<MeleeDamagePoint> meleeDamagePoints = new List<MeleeDamagePoint>();


    [Header("Statistics: ")]
    public float meleeAttackSpeed;

    [Header("Animations durations")]
    public float enemyPunchDuration;

    //States:
    public bool isMeleeAttacking = false;
    private bool canAttack = true;
    private bool thisAttackHasHitThePlayer;

    [Serializable]
    public class MeleeDamagePoint
    {
        public Transform DamageSphereOrigin;
        public float DamageSphereRange;
    }

    private void Start()
    {
        enemy.animator.SetLayerWeight(1, 1);
        UpdateAnimClipTimes();
    }

    void Update()
    {
        // start attacking
        if (enemy.enemyMovement.canSeePlayer && Vector3.Distance(enemy.EnemyCenter.position, Player.m.transform.position) <= DistanceToStartPunch && canAttack)
        {
            enemy.animator.SetTrigger("StartPunch");
            enemy.soundManager.Play("slash");

            canAttack = false;

            thisAttackHasHitThePlayer = false;

            Invoke(nameof(stopAttacking), enemyPunchDuration);
        }

        // check for targets in melee range
        if (isMeleeAttacking)
            DealDamageFromDamagePoint();
    }

    public void DealDamageFromDamagePoint()
    {
        //detect player
        List<Collider> hitObjects = new List<Collider>();
        foreach (MeleeDamagePoint damagePoint in meleeDamagePoints)
        {
            hitObjects.AddRange(Physics.OverlapSphere(damagePoint.DamageSphereOrigin.position, damagePoint.DamageSphereRange, PlayerLayer));
        }

        foreach (Collider obj in hitObjects)
        {
            switch (LayerMask.LayerToName(obj.gameObject.layer))
            {
                case "Player":
                    if (!thisAttackHasHitThePlayer)
                    {
                        thisAttackHasHitThePlayer = true;
                        Player.m.TakeDamage(enemy.myWeaponClass.meleeDamage);
                    }
                break;
            }
        }
    }

    private void stopAttacking() { isMeleeAttacking = false; Invoke(nameof(resetCanAttack), meleeAttackSpeed + 0.1f); }// +0.1f because of the transition duration
    private void resetCanAttack() { canAttack = true; }

    public void UpdateAnimClipTimes()
    {
        AnimationClip[] clips = enemy.animator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            switch (clip.name)
            {
                case "Enemy Stab":
                    enemyPunchDuration = clip.length;
                    break;
            }
        }
    }

    
    void OnDrawGizmosSelected()
    {
        for (int i = 0; i < meleeDamagePoints.Count;i++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(meleeDamagePoints[i].DamageSphereOrigin.position, meleeDamagePoints[i].DamageSphereRange);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(enemy.EnemyCenter.position, DistanceToStartPunch);
    }
    
}
