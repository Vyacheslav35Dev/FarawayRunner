using System;
using UnityEngine;

public class LerpBetweenValuesData
{
    public LerpBetweenValuesData(float startValue, float endValue, float duration, AnimationCurve animationCurve,
        Action<float> onLerp)
    {
        StartValue = startValue;
        EndValue = endValue;
        Duration = duration;
        AnimationCurve = animationCurve;
        OnLerp = onLerp;
    }

    public float StartValue { get; }
    public float EndValue { get; }
    public float Duration { get; }
    public AnimationCurve AnimationCurve { get; }
    public Action<float> OnLerp { get; }
}