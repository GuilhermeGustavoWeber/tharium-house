using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq; // Essencial para a lógica de ordenação e aleatoriedade

public class AINavMesh : MonoBehaviour
{
    public Transform player;
    private NavMeshAgent agent;

    // Variáveis de Visão
    public float viewRadius = 10f;
    [Range(0, 360)] public float viewAngle = 120f;
    public LayerMask playerMask;
    public LayerMask obstructionMask;
    private bool playerVisible = false;
    private float timeSinceLastSeen = 0f;
    public float loseSightTime = 3f;

    // Variáveis de Patrulha Aleatória (usadas como fallback)
    public float patrolRadius = 15f;
    
    // --- LÓGICA DE PATRULHA DO MAPA DE CALOR ---
    public Dictionary<Vector3, int> heatmap; 
    public float gridSize = 1.0f;
    private bool probabilisticMode = false;
    private List<Vector3> currentHeatmapPatrolRoute; // O "plano de patrulha" atual
    private int patrolRouteIndex = 0; // Qual ponto do plano estamos seguindo

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        if (GameManager.allPlayerPaths != null && GameManager.allPlayerPaths.Count > 0)
        {
            probabilisticMode = true;
            GenerateHeatmap();
            GenerateNewHeatmapPatrolRoute(); // Cria o primeiro plano de patrulha
            FollowHeatmapPatrol(); // Começa a seguir o plano
            Debug.Log("Fantasma em MODO PROBABILÍSTICO. Iniciando patrulha de pontos quentes.");
        }
        else
        {
            probabilisticMode = false;
            Debug.Log("Fantasma em MODO PATRULHA PADRÃO.");
            GoToRandomPatrolPoint();
        }
    }

    void Update()
    {
        DetectPlayer();

        // Prioridade 1: Perseguir o jogador se ele estiver visível
        if (playerVisible)
        {
            agent.SetDestination(player.position);
            timeSinceLastSeen = 0f;
            return;
        }
        
        // Se o jogador não está visível, continuamos a patrulha
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            if (probabilisticMode)
            {
                // Chegamos ao nosso destino do plano, então avançamos para o próximo
                patrolRouteIndex++; 
                FollowHeatmapPatrol();
            }
            else
            {
                // Comportamento padrão se não houver dados
                GoToRandomPatrolPoint(); 
            }
        }
    }

    // Gera o Dicionário do Mapa de Calor
    void GenerateHeatmap()
    {
        heatmap = new Dictionary<Vector3, int>();
        foreach (var path in GameManager.allPlayerPaths)
        {
            foreach (var point in path)
            {
                Vector3 gridPosition = new Vector3(
                    Mathf.Round(point.x / gridSize) * gridSize,
                    Mathf.Round(point.y / gridSize) * gridSize,
                    Mathf.Round(point.z / gridSize) * gridSize
                );

                if (heatmap.ContainsKey(gridPosition))
                {
                    heatmap[gridPosition]++;
                }
                else
                {
                    heatmap.Add(gridPosition, 1);
                }
            }
        }
    }

    // --- NOVA FUNÇÃO PARA CRIAR O PLANO DE PATRULHA ---
    void GenerateNewHeatmapPatrolRoute()
    {
        if (heatmap == null || heatmap.Count == 0)
        {
            currentHeatmapPatrolRoute = new List<Vector3>(); // Cria uma lista vazia para evitar erros
            return;
        }

        // Pega os 5 pontos mais quentes e os embaralha para criar uma rota imprevisível
        currentHeatmapPatrolRoute = heatmap.OrderByDescending(spot => spot.Value)
                                           .Take(5)
                                           .Select(spot => spot.Key)
                                           .OrderBy(x => Random.value) // Embaralha a ordem!
                                           .ToList();
        
        patrolRouteIndex = 0; // Reseta o índice para começar do início do novo plano
        Debug.Log("Novo plano de patrulha gerado com " + currentHeatmapPatrolRoute.Count + " pontos.");
    }

    // --- NOVA FUNÇÃO PARA SEGUIR O PLANO ---
    void FollowHeatmapPatrol()
    {
        // Se o nosso plano de patrulha acabou (ou não existe), criamos um novo.
        if (currentHeatmapPatrolRoute == null || patrolRouteIndex >= currentHeatmapPatrolRoute.Count)
        {
            GenerateNewHeatmapPatrolRoute();
            // Se mesmo depois de gerar um novo plano, ele estiver vazio, recorremos à patrulha aleatória
            if (currentHeatmapPatrolRoute.Count == 0)
            {
                GoToRandomPatrolPoint();
                return;
            }
        }

        // Define o destino para o próximo ponto do nosso plano de patrulha
        agent.SetDestination(currentHeatmapPatrolRoute[patrolRouteIndex]);
    }

    // Função de fallback para patrulha aleatória
    void GoToRandomPatrolPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += transform.position;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    // Função de detecção de jogador (Agora com cálculo de ângulo)
    void DetectPlayer()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        // Verifica se o jogador está dentro do raio de visão
        if (distance <= viewRadius)
        {
            Vector3 dirToPlayer = (player.position - transform.position).normalized;

            // Verifica o ângulo entre a frente do fantasma e a direção do jogador
            float angle = Vector3.Angle(transform.forward, dirToPlayer);

            // Se o jogador estiver dentro do campo de visão e sem obstruções, o fantasma o vê
            if (angle <= viewAngle / 2f)
            {
                if (!Physics.Linecast(transform.position, player.position, obstructionMask))
                {
                    playerVisible = true;
                    return;
                }
            }
        }
        playerVisible = false;
    }

    // Função de Gizmos (sem alterações)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        if (playerVisible)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, player.position);
        }

        // Desenha o campo de visão
        Vector3 left = DirFromAngle(-viewAngle / 2);
        Vector3 right = DirFromAngle(viewAngle / 2);
        
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + left * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + right * viewRadius);
    }

    // Função auxiliar para calcular a direção do ângulo
    Vector3 DirFromAngle(float angle)
    {
        angle += transform.eulerAngles.y;
        return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad));
    }
}
