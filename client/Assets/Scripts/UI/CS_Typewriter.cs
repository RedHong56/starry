using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class TypewriterEffect : MonoBehaviour
{
    [SerializeField] private TMP_Text target;
    [SerializeField] private float    speed     = 0.05f;
    [SerializeField] private float    postPause = 1.2f;

    private Coroutine _routine;

    public void Play(string text, Action onComplete = null)
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(Routine(text, onComplete));
    }

    public void Stop()
    {
        if (_routine != null) StopCoroutine(_routine);
    }

    public void Append(string text, Action onComplete = null)
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(AppendRoutine(text, onComplete));
    }

    private IEnumerator AppendRoutine(string text, Action onComplete)
    {
        foreach (char c in text)
        {
            target.text += c;
            yield return new WaitForSeconds(speed);
        }
        if (onComplete != null)
        {
            yield return new WaitForSeconds(postPause);
            onComplete.Invoke();
        }
    }

    private IEnumerator Routine(string text, Action onComplete)
    {
        target.text = string.Empty;
        foreach (char c in text)
        {
            target.text += c;
            yield return new WaitForSeconds(speed);
        }

        if (onComplete != null)
        {
            yield return new WaitForSeconds(postPause);
            onComplete.Invoke();
        }
    }
}
