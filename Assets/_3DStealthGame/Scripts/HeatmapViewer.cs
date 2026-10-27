using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class HeatmapViewer : MonoBehaviour
{
    void OnDrawGizmos()
    {
        AINavMesh ghostAI = FindObjectOfType<AINavMesh>();
        if (ghostAI == null || ghostAI.heatmap == null || ghostAI.heatmap.Count == 0) return;

        // Encontra o valor máximo de calor para normalizar as cores
        int maxHeat = ghostAI.heatmap.Values.Max();
        if (maxHeat == 0) maxHeat = 1;

        // Desenha um cubo para cada ponto no mapa de calor
        foreach (var spot in ghostAI.heatmap)
        {
            // A cor vai de azul (frio) para vermelho (quente)
            float heatRatio = (float)spot.Value / maxHeat;
            Gizmos.color = Color.Lerp(Color.blue, Color.red, heatRatio);
            Gizmos.DrawCube(spot.Key, Vector3.one * ghostAI.gridSize * 0.9f); // 0.9f para ver a separação
        }
    }
}