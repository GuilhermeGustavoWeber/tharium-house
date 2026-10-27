# Tharium House — Motor de IA Espacial Preditiva & Stealth 3D

[![Premiação](https://img.shields.io/badge/Premiação-1º%20Lugar%20TechDay%202025%20(UniMAX)-d97706?style=for-the-badge&logo=target&logoColor=white)](https://guilhermegustavoweber.github.io)
[![Engine](https://img.shields.io/badge/Unity%203D-2022+-black?style=for-the-badge&logo=unity&logoColor=white)](https://unity.com)
[![Linguagem](https://img.shields.io/badge/C%23-.NET%20Standard-239120?style=for-the-badge&logo=csharp&logoColor=white)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![Simulador Online](https://img.shields.io/badge/Simulador%20Web-Canvas%202D%20Live-0f766e?style=for-the-badge&logo=googlechrome&logoColor=white)](https://guilhermegustavoweber.github.io)

> **Projeto vencedor do 1º Lugar Geral na Feira de Engenharia e Tecnologia TechDay 2025 (Centro Universitário Max Planck - UniMAX).**

---

## 1. Visão Geral da Arquitetura

O **Tharium House** é um jogo 3D de sobrevivência e infiltração (*stealth*) desenvolvido em Unity e C#. O principal diferencial técnico do projeto é o seu **agente autônomo adversário (Fantasma)**, cuja inteligência não se apoia em caminhos estáticos ou perseguição com trapaça de coordenadas (*omniscience*), mas em um **motor probabilístico de mapa de calor espacial tridimensional**.

O sistema aprende dinamicamente as rotas e esconderijos mais frequentados pelo jogador em partidas sucessivas, discretizando o espaço contínuo e criando patrulhas adaptativas.

```text
[Jogador se Movimenta] 
       │ (0.5s via PathRecorder.cs)
       ▼
[Discretização Espacial em Grid 3D] ──► Vector3(Round(x/g)*g, Round(y/g)*g, Round(z/g)*g)
       │
       ▼
[Heatmap Accumulator (Dictionary<Vector3, int>)]
       │
       ▼
[Top 5 Nós Mais Quentes] ──► [Embaralhamento Estocástico LINQ]
       │
       ▼
[Patrulha Preditiva em NavMesh] ──► [Sensor de Visão 120º + Raycast Oclusão] ──► [Perseguição Fatal]
```

---

## 2. Componentes Centrais de IA

Os scripts fundamentais estão localizados em `Assets/_3DStealthGame/Scripts/`:

### 1. `PathRecorder.cs` — Telemetria Espacial Contínua
Grava a posição tridimensional do jogador em intervalos discretos ($t = 0.5s$) sem sobrecarregar a memória, persistindo o histórico entre tentativas:
```csharp
public class PathRecorder : MonoBehaviour
{
    public float recordingInterval = 0.5f;
    public List<Vector3> pathPoints;

    void Start()
    {
        pathPoints = new List<Vector3>();
        InvokeRepeating("RecordPoint", 0f, recordingInterval);
    }

    void RecordPoint()
    {
        pathPoints.Add(transform.position);
    }
}
```

### 2. `AINavMesh.cs` — Discretização em Grade & Patrulha Probabilística
O agente processa o histórico de coordenadas contínuas e as mapeia em células cúbicas discretas de tamanho `gridSize`, acumulando frequência em um `Dictionary<Vector3, int>`:
```csharp
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
                heatmap[gridPosition]++;
            else
                heatmap.Add(gridPosition, 1);
        }
    }
}
```

#### Seleção Estocástica das Zonas Críticas:
Em vez de patrulhar os pontos em linha reta previsível, o agente filtra os 5 pontos de maior densidade térmica da história do jogador e os embaralha aleatoriamente para evitar padrões previsíveis:
```csharp
void GenerateNewHeatmapPatrolRoute()
{
    var hottestSpots = heatmap.OrderByDescending(kvp => kvp.Value)
                              .Take(5)
                              .Select(kvp => kvp.Key)
                              .ToList();

    // Embaralhamento estocástico para rota imprevisível
    currentHeatmapPatrolRoute = hottestSpots.OrderBy(x => Random.value).ToList();
    patrolRouteIndex = 0;
}
```

### 3. Sistema Sensorial de Visão Trigonométrica
* **Raio de Visão:** $10.0\text{ m}$
* **Abertura do Cone:** $120^\circ$
* **Oclusão Física:** Verificação de linha de visada (`Physics.Linecast` ou `Physics.Raycast`) contra máscara de camada de obstáculos do cenário (`obstructionMask`), impedindo que o inimigo enxergue através de paredes.

### 4. `HeatmapViewer.cs` — Telemetria Visual (Gizmos)
Para debug e auditoria técnica em tempo de desenvolvimento, o sistema desenha a matriz térmica 3D na aba *Scene* da Unity, interpolando as cores de **Azul (frio)** a **Vermelho (zona crítica)**:
```csharp
float heatRatio = (float)spot.Value / maxHeat;
Gizmos.color = Color.Lerp(Color.blue, Color.red, heatRatio);
Gizmos.DrawCube(spot.Key, Vector3.one * ghostAI.gridSize * 0.9f);
```

---

## 3. Como Executar o Projeto

1. Certifique-se de ter o **Unity Hub** e a versão **Unity 2022.3 LTS** (ou superior) instalada com suporte a módulos Desktop (Windows).
2. Clone o repositório:
   ```bash
   git clone https://github.com/GuilhermeGustavoWeber/tharium-house.git
   ```
3. Abra o **Unity Hub** > Clique em **Add** > Selecione a pasta clonada.
4. Abra a cena principal em:
   ```text
   Assets/_3DStealthGame/Scenes/Main.unity
   ```
5. Pressione **Play** no editor. Para visualizar o mapa de calor em tempo real, ative o botão **Gizmos** na janela da cena (*Scene View*).

---

## 4. Simulador Interativo 2D
Uma versão emulada em Canvas 2D da inteligência artificial deste jogo está disponível diretamente no meu portfólio oficial:  
👉 **[guilhermegustavoweber.github.io](https://guilhermegustavoweber.github.io)**

---

## Licença & Autoria
Desenvolvido por **Guilherme Gustavo Weber**  
Contato: `guilherme7weber@gmail.com` • [LinkedIn](https://linkedin.com/in/guilherme-gustavo-weber/)
