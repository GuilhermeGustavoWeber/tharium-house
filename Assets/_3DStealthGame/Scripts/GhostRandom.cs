using UnityEngine;
using UnityEngine.AI;

public class GhostRandom : MonoBehaviour
{
    public Transform player;
    private NavMeshAgent agent;

    // Visão
    public float viewRadius = 10f;
    [Range(0, 360)] public float viewAngle = 120f;
    public LayerMask playerMask;
    public LayerMask obstructionMask;

    private bool playerVisible = false;
    private float timeSinceLastSeen = 0f;
    public float loseSightTime = 3f;

    // Patrulha aleatória
    public float patrolRadius = 15f;
    public float waitTimeAtPoint = 2f;
    private float waitTimer = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        GoToRandomPatrolPoint();
    }

    void Update()
    {
        DetectPlayer();

        if (playerVisible)
        {
            agent.SetDestination(player.position);
            timeSinceLastSeen = 0f;
        }
        else
        {
            timeSinceLastSeen += Time.deltaTime;

            if (timeSinceLastSeen >= loseSightTime)
            {
                Patrol();
            }
        }
    }

    void Patrol()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTimeAtPoint)
            {
                GoToRandomPatrolPoint();
                waitTimer = 0f;
            }
        }
    }

    void GoToRandomPatrolPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += transform.position;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    void DetectPlayer()
    {
        if (player == null) return;

        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= viewRadius)
        {
            float angle = Vector3.Angle(transform.forward, dirToPlayer);

            if (angle <= viewAngle / 2f)
            {
                if (!Physics.Raycast(transform.position + Vector3.up, dirToPlayer, distance, obstructionMask))
                {
                    playerVisible = true;
                    return;
                }
            }
        }

        playerVisible = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Vector3 left = DirFromAngle(-viewAngle / 2);
        Vector3 right = DirFromAngle(viewAngle / 2);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + left * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + right * viewRadius);
    }

    Vector3 DirFromAngle(float angle)
    {
        angle += transform.eulerAngles.y;
        return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad));
    }
}
