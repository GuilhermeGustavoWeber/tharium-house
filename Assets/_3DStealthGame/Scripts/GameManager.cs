using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Agora é uma lista DE LISTAS. Uma coleção de todos os caminhos.
    // Inicializamos a lista aqui para que ela nunca seja nula.
    public static List<List<Vector3>> allPlayerPaths = new List<List<Vector3>>();

    // Mantemos a variável antiga para o script de visualização do último caminho.
    public static List<Vector3> lastPlayerPath; 

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}