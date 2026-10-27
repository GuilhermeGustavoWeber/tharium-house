using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class GameEnding : MonoBehaviour
{
    public float fadeDuration = 1f;
    public float displayImageDuration = 1f;
    public GameObject player;
    public UIDocument uiDocument;
    public AudioSource caughtAudio;

    bool m_IsPlayerCaught;
    float m_Timer;
    bool m_HasAudioPlayed;

    VisualElement m_CaughtScreen;

    void Start()
    {
        if (uiDocument != null)
        {
            m_CaughtScreen = uiDocument.rootVisualElement.Q<VisualElement>("CaughtScreen");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {
            SavePathAndRestart();
        }
    }

    public void CaughtPlayer()
    {
        if (!m_IsPlayerCaught)
        {
            m_IsPlayerCaught = true;
            m_Timer = 0f;
            m_HasAudioPlayed = false;
        }
    }

    void Update()
    {
        if (m_IsPlayerCaught)
        {
            EndLevel(m_CaughtScreen, true, caughtAudio);
        }
    }

    void EndLevel(VisualElement element, bool doRestart, AudioSource audioSource)
    {
        if (element == null && uiDocument != null)
            element = uiDocument.rootVisualElement.Q<VisualElement>("CaughtScreen");

        if (!m_HasAudioPlayed && audioSource != null)
        {
            audioSource.Play();
            m_HasAudioPlayed = true;
        }

        m_Timer += Time.deltaTime;

        if (element != null)
        {
            float t = Mathf.Clamp01(m_Timer / fadeDuration);
            element.style.opacity = Mathf.SmoothStep(0f, 1f, t);
        }

        if (m_Timer > fadeDuration + displayImageDuration)
        {
            if (doRestart)
            {
                SavePathAndRestart();
            }
        }
    }

    void SavePathAndRestart()
    {
        PathRecorder recorder = player.GetComponent<PathRecorder>();
        if (recorder != null && recorder.pathPoints.Count > 1)
        {
            // MUDANÇA PRINCIPAL: Usamos .Add() para adicionar à coleção.
            GameManager.allPlayerPaths.Add(new List<Vector3>(recorder.pathPoints));
            
            GameManager.lastPlayerPath = new List<Vector3>(recorder.pathPoints);

            Debug.Log($"Caminho salvo! Total de caminhos na memória: {GameManager.allPlayerPaths.Count}");
        }
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}