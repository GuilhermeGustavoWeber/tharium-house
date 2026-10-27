using UnityEngine;
using System.Collections.Generic; // Necessário para usar Listas

public class PathRecorder : MonoBehaviour
{
    public float recordingInterval = 0.5f; // A cada 0.5 segundos, grava um ponto.
    public List<Vector3> pathPoints; // A lista que vai guardar os pontos do caminho.

    void Start()
    {
        // Inicializa a lista de pontos.
        pathPoints = new List<Vector3>();

        // Começa a chamar a função "RecordPoint" repetidamente.
        // O primeiro chamado é após 0 segundos, e depois se repete a cada 'recordingInterval' segundos.
        InvokeRepeating("RecordPoint", 0f, recordingInterval);
    }

    void RecordPoint()
    {
        // Adiciona a posição atual do jogador à lista de pontos.
        pathPoints.Add(transform.position);
    }
}