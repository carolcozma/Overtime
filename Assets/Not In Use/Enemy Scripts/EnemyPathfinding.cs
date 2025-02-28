using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPathfinding : MonoBehaviour
{   
    public NavMeshAgent agent;
    public VisionCone visionCone;
    public LayerMask groundLayer, playerLayer, enemyLayer;
    public EnemyType enemyType;

    [Header("Patrolling")]
    public bool randomPatrolling = true;
    [Tooltip("For random patrolling")]
    public float walkPointRange;
    private Vector3 walkPoint;
    private bool walkPointSet;
    [Tooltip("For specific patrolling")]
    public Vector3[] walkPoints;
    private int walkPointIndex = 0;

    [Header("Attacking")]
    public float damage;
    public float attackCooldown;
    private bool alreadyAttacked;

    [Header("States")]
    public float attackRange;
    public float announceRange, proximityRange;
    public bool canSeePlayer, canAttackPlayer, gotAnnounced;
    public Vector3 announcedPosition;

    public bool rememberPlayer = false;
    public float forgetTimer;
    private float remainingTime;

    private EnemyShooting enemyShooting;

    [Header("Animations")]
    public Animator animator;
    //public Animation idle;
    //public Animation run;
    [Tooltip("Name of attack animation IN ANIMATOR")]
    public string attack;
    [HideInInspector]
    public bool stopAnimation = false;

    private void Awake()
    {
        agent = gameObject.GetComponent<NavMeshAgent>();
        visionCone = gameObject.GetComponentInChildren<VisionCone>();
    }

    private void Start()
    {
        if (enemyType == EnemyType.Ranged) {
            attackRange = visionCone.visionDistance;
            enemyShooting = gameObject.GetComponent<EnemyShooting>();
        }
    }

    private void Update()
    {
        if (agent.velocity.magnitude <= 0.1f)
            animator.Play("Idle", 0);
        else
            animator.Play("Run", 0);
        // Check if the enemy can see or attack the player.
        canSeePlayer = visionCone.IsInView(Player.m.playerObject) || Physics.CheckSphere(gameObject.transform.position, proximityRange, playerLayer);
        canAttackPlayer = Physics.CheckSphere(gameObject.transform.position, attackRange, playerLayer);

        if (!canSeePlayer) {
            // If the enemy can't see the player and he forgot the player, he patrols. If another enemy announced the player to this enemy, start chasing.
            if (gotAnnounced) {
                Chase(announcedPosition, false);
            } else {
                if (rememberPlayer)
                    Chase(Player.m.gameObject.transform.position, true);
                else
                    Patrol();
            }
        } else {
            // If the enemy saw the player, announce it to others and start chasing or attacking. Also discard previous announcements.
            gotAnnounced = false;
            rememberPlayer = true;
            remainingTime = forgetTimer;
            Announce();
            if (!canAttackPlayer)
                Chase(Player.m.gameObject.transform.position, true);
            else
                Attack();
        }

        if (remainingTime > 0f) {
            remainingTime -= Time.deltaTime;
        } else {
            rememberPlayer = false;
        }

        if (stopAnimation)
        {
            stopAnimation = false;
            //animator.SetLayerWeight(1, 0);
           // animator.Play("Default", 1);
        }
    }

    private void Announce()
    {
        // Announce to all other enemies in range.
        Collider[] enemies = Physics.OverlapSphere(gameObject.transform.position, announceRange, enemyLayer);

        foreach (Collider enemy in enemies)
        {
            // Condition to prevent announcing to himself.
            if (enemy.transform.parent.gameObject.Equals(gameObject))
                continue;

            EnemyPathfinding pathfinding = enemy.transform.parent.GetComponent<EnemyPathfinding>();
            if (pathfinding == null)
                continue;

            pathfinding.announcedPosition = Player.m.gameObject.transform.position - new Vector3(0, Player.m.gameObject.transform.position.y, 0);
            pathfinding.gotAnnounced = true;
        }
    }

    private void Patrol()
    {
        // Get next position to patrol.
        if (!walkPointSet)
        {
            if (randomPatrolling)
                SearchWalkPoint();
            else
            {
                walkPoint = walkPoints[walkPointIndex];
                walkPointIndex = (walkPointIndex + 1) % walkPoints.Length;
            }

            walkPointSet = true;
        }

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        // Check if the enemy reached the destination.
        if (Vector3.Distance(gameObject.transform.position, walkPoint) <= 1f)
            walkPointSet = false;
    }

    // Generates random point for patrolling.
    private void SearchWalkPoint()
    {
        do
        {
            float randomX = Random.Range(-walkPointRange, walkPointRange);
            float randomZ = Random.Range(-walkPointRange, walkPointRange);

            walkPoint = gameObject.transform.position + new Vector3(randomX, 0, randomZ);
        } while (!Physics.Raycast(walkPoint, -gameObject.transform.up, 2f, groundLayer));
    }

    private void Chase(Vector3 position, bool sawPlayer)
    {
        agent.SetDestination(position);

        // If the enemy didn't see the player himself, discards announced information when arriving.
        if (!sawPlayer && Vector3.Distance(gameObject.transform.position, position) < 1f)
        {
            gotAnnounced = false;
            rememberPlayer = false;
        }
    }

    private void Attack()
    {
        // Stop the enemy.
        agent.SetDestination(gameObject.transform.position);

        // Attack.
        if (!alreadyAttacked)
        {
            //animator.SetLayerWeight(1, 1);
            //animator.Play(attack, 1);
            alreadyAttacked = true;
            if (enemyType == EnemyType.Ranged) {
                enemyShooting.Shoot(Player.m.transform, damage);
            } else {
                Player.m.TakeDamage(damage);
            }

            Invoke(nameof(ResetAttack), attackCooldown);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    // Used for gizmos.
    private Vector3 rotateVector(Vector3 orig, float angle)
    {
        float x2 = Mathf.Cos(angle) * orig.x - Mathf.Sin(angle) * orig.z;
        float z2 = Mathf.Sin(angle) * orig.x + Mathf.Cos(angle) * orig.z;

        return new Vector3(x2, orig.y, z2);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(gameObject.transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(gameObject.transform.position, announceRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(gameObject.transform.position, proximityRange);
        Vector3 fwd = gameObject.transform.forward * visionCone.visionDistance;
        Gizmos.color = Color.white;
        Gizmos.DrawRay(visionCone.gameObject.transform.position, fwd);
        Gizmos.DrawRay(visionCone.gameObject.transform.position, rotateVector(fwd, visionCone.visionAngle));
        Gizmos.DrawRay(visionCone.gameObject.transform.position, rotateVector(fwd, -visionCone.visionAngle));
    }
}
