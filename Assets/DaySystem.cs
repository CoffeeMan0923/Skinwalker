using UnityEngine;

public class DaySystem : MonoBehaviour
{
    [Header("Light")]
    [SerializeField] private Light directionalLight;

    [SerializeField] private Color dayLightColor = Color.white;
    [SerializeField] private Color nightLightColor = new Color(0.15f, 0.2f, 0.4f);

    [SerializeField] private float dayIntensity = 1f;
    [SerializeField] private float nightIntensity = 0.1f;

    [Header("Shadow Strength")]
    [SerializeField] private float dayShadowStrength = 0.8f;
    [SerializeField] private float nightShadowStrength = 1f;

    [Header("Day / Night Timing")]
    [SerializeField] private float pureDayLength = 60f;
    [SerializeField] private float transitionToNightLength = 30f;
    [SerializeField] private float pureNightLength = 60f;
    [SerializeField] private float transitionToDayLength = 30f;

    [Header("Settings")]
    [SerializeField] private bool lockOnNight = false;

    private float time;
    private Phase currentPhase;
    private bool wasLockedOnNight;

    private enum Phase
    {
        PureDay,
        TransitionToNight,
        PureNight,
        TransitionToDay
    }

    private void Start()
    {

        if (directionalLight == null)
            directionalLight = GetComponent<Light>();
        directionalLight.shadowStrength = dayShadowStrength;
        ResetToBeginningOfDay();
    }

    private void Update()
    {
        if (lockOnNight)
        {
            if (!wasLockedOnNight)
            {
                currentPhase = Phase.PureNight;
                time = 0f;
                wasLockedOnNight = true;
            }

            directionalLight.color = nightLightColor;
            directionalLight.intensity = nightIntensity;
            directionalLight.shadowStrength = nightShadowStrength;

            return;
        }

        if (wasLockedOnNight)
        {
            wasLockedOnNight = false;
        }

        time += Time.deltaTime;

        switch (currentPhase)
        {
            case Phase.PureDay:
                PureDay();
                break;

            case Phase.TransitionToNight:
                TransitionToNight();
                break;

            case Phase.PureNight:
                PureNight();
                break;

            case Phase.TransitionToDay:
                TransitionToDay();
                break;
        }
    }

    private void PureDay()
    {
        directionalLight.color = dayLightColor;
        directionalLight.intensity = dayIntensity;
        directionalLight.shadowStrength = dayShadowStrength;

        if (time >= pureDayLength)
        {
            time = 0f;
            currentPhase = Phase.TransitionToNight;
        }
    }

    private void TransitionToNight()
    {
        float t = Mathf.Clamp01(time / transitionToNightLength);

        directionalLight.color = Color.Lerp(
            dayLightColor,
            nightLightColor,
            t
        );

        directionalLight.intensity = Mathf.Lerp(
            dayIntensity,
            nightIntensity,
            t
        );

        directionalLight.shadowStrength = Mathf.Lerp(
            dayShadowStrength,
            nightShadowStrength,
            t
        );

        if (time >= transitionToNightLength)
        {
            time = 0f;
            currentPhase = Phase.PureNight;
        }
    }

    private void PureNight()
    {
        directionalLight.color = nightLightColor;
        directionalLight.intensity = nightIntensity;
        directionalLight.shadowStrength = nightShadowStrength;

        if (time >= pureNightLength)
        {
            time = 0f;
            currentPhase = Phase.TransitionToDay;
        }
    }

    private void TransitionToDay()
    {
        float t = Mathf.Clamp01(time / transitionToDayLength);

        directionalLight.color = Color.Lerp(
            nightLightColor,
            dayLightColor,
            t
        );

        directionalLight.intensity = Mathf.Lerp(
            nightIntensity,
            dayIntensity,
            t
        );

        directionalLight.shadowStrength = Mathf.Lerp(
            nightShadowStrength,
            dayShadowStrength,
            t
        );

        if (time >= transitionToDayLength)
        {
            time = 0f;
            currentPhase = Phase.PureDay;
        }
    }

    public void ResetToBeginningOfDay()
    {
        time = 0f;
        currentPhase = Phase.PureDay;
        wasLockedOnNight = false;

        directionalLight.color = dayLightColor;
        directionalLight.intensity = dayIntensity;
        directionalLight.shadowStrength = dayShadowStrength;
    }
}
