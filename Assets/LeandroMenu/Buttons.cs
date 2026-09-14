using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Buttons : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string sceneName;

    [Header("Loading Objects")]
    [SerializeField] private GameObject activateOnLoadStart;
    [SerializeField] private GameObject deactivateOnLoadStart;

    [Header("Loading UI")]
    [SerializeField] private Slider loadingSlider;
    [SerializeField] private TMP_Text loadingText;

    [Header("Loading Text Settings")]
    [SerializeField] private float textChangeTime = 0.4f;

    private AsyncOperation preloadOperation;

    public bool IsPreloaded { get; private set; }

    public void PreloadScene()
    {
        if (preloadOperation != null)
        {
            Debug.LogWarning("A scene is already being preloaded.");
            return;
        }

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("No target scene has been assigned.");
            return;
        }

        // Activate/deactivate objects when loading begins.
        if (activateOnLoadStart != null)
            activateOnLoadStart.SetActive(true);

        if (deactivateOnLoadStart != null)
            deactivateOnLoadStart.SetActive(false);

        if (loadingSlider != null)
            loadingSlider.value = 0f;

        if (loadingText != null)
            loadingText.text = "Loading.";

        // Start the loading animation.
        StartCoroutine(LoadingTextAnimation());

        preloadOperation = SceneManager.LoadSceneAsync(sceneName);

        // Don't activate the scene yet.
        preloadOperation.allowSceneActivation = false;

        StartCoroutine(UpdateLoadingProgress());
    }

    private IEnumerator LoadingTextAnimation()
    {
        while (!IsPreloaded)
        {
            if (loadingText != null)
                loadingText.text = "Loading.";

            yield return new WaitForSeconds(textChangeTime);

            if (IsPreloaded)
                break;

            if (loadingText != null)
                loadingText.text = "Loading..";

            yield return new WaitForSeconds(textChangeTime);

            if (IsPreloaded)
                break;

            if (loadingText != null)
                loadingText.text = "Loading...";

            yield return new WaitForSeconds(textChangeTime);
        }
    }

    private IEnumerator UpdateLoadingProgress()
    {
        while (preloadOperation != null)
        {
            // Unity reports 0.9 as the maximum while activation is disabled.
            float progress = Mathf.Clamp01(preloadOperation.progress / 0.9f);

            if (loadingSlider != null)
                loadingSlider.value = progress;

            if (preloadOperation.progress >= 0.9f)
                break;

            yield return null;
        }

        IsPreloaded = true;

        if (loadingSlider != null)
            loadingSlider.value = 1f;

        if (loadingText != null)
            loadingText.text = "Loading...";

        Debug.Log("Scene fully preloaded. Activating scene...");

        // Automatically enter the scene.
        preloadOperation.allowSceneActivation = true;
    }
    public void EnterPreloadedScene()
    {
        if (preloadOperation == null)
        {
            Debug.LogWarning("No scene has been preloaded.");
            return;
        }

        if (!IsPreloaded)
        {
            Debug.LogWarning("Scene is still loading.");
            return;
        }

        preloadOperation.allowSceneActivation = true;
    }
}