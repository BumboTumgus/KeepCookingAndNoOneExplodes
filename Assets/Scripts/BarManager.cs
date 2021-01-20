using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BarManager : MonoBehaviour
{
    [SerializeField] private float currentBarValue;
    [SerializeField] private float targetValue;
    [SerializeField] private float primaryLerpSpeed = 5;
    [SerializeField] private Image primaryContent = null;
    private float maximumValue = 100;
    private float minimumValue = 0;

    // reset the bar with a new max Value.
    public void Initialize(float maxValue, float minValue, float startingValue)
    {
        maximumValue = maxValue;
        minimumValue = minValue;
        currentBarValue = startingValue;
        targetValue = startingValue;

        primaryContent.fillAmount = currentBarValue / maximumValue;
    }

    // Checks to see if our values are close to the target or not and slowly moves towards them if the lerp option is ticked.
    private void Update()
    {
        // Primary bar logic.
        if (currentBarValue != targetValue)
            currentBarValue = Mathf.Lerp(currentBarValue, targetValue, primaryLerpSpeed / 100);

        SetBarValue();
    }

    // Used to set the bars values to their targets
    private void SetBarValue()
    {
        primaryContent.fillAmount = currentBarValue / maximumValue;
    }

    // USed by other classes to set what value this bar will display.
    public void SetValue(float target)
    {
        targetValue = target;
        if (targetValue > maximumValue)
            targetValue = maximumValue;
        else if (targetValue < minimumValue)
            targetValue = minimumValue;
    }
}
