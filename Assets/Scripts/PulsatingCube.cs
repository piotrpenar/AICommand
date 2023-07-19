using UnityEngine;
using System.Collections;

public class PulsatingCube : MonoBehaviour
{
    private bool isPulsating = false;
    private float scaleChange = 0.1f;
    private float originalScale;

    private void Start()
    {
        originalScale = transform.localScale.x;
    }

    private void OnMouseDown()
    {
        isPulsating = !isPulsating;
    }

    private void Update()
    {
        if (isPulsating)
        {
            float newScale = originalScale + Mathf.Sin(Time.time) * scaleChange;
            transform.localScale = new Vector3(newScale, newScale, newScale);
        }
        else
        {
            transform.localScale = new Vector3(originalScale, originalScale, originalScale);
        }
    }
}