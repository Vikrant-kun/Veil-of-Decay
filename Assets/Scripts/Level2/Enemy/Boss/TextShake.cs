using UnityEngine;
using TMPro;
using System.Collections;

public class TextShake : MonoBehaviour
{
    public float shakeIntensity = 1.5f;
    private TMP_Text textComponent;

    void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
    }

    public void StartShake(string originalText)
    {
        // Stop any previous shaking before starting a new one
        StopAllCoroutines();
        StartCoroutine(ShakeText(originalText));
    }

    public void StopShake()
    {
        StopAllCoroutines();
        // You may want to add logic here to reset text to its original state if needed,
        // but since the text will be replaced by the next line, it's often not necessary.
    }

    private IEnumerator ShakeText(string originalText)
    {
        yield return null; // Wait one frame for the text to be fully typed out

        TMP_TextInfo textInfo = textComponent.textInfo;
        if (textInfo.characterCount == 0)
        {
            yield break; // No text to shake
        }

        Vector3[][] initialVertices = new Vector3[textInfo.characterCount][];
        
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            if (!textInfo.characterInfo[i].isVisible) continue;
            int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
            int vertexIndex = textInfo.characterInfo[i].vertexIndex;
            initialVertices[i] = new Vector3[4];
            for (int j = 0; j < 4; j++)
            {
                initialVertices[i][j] = textInfo.meshInfo[materialIndex].vertices[vertexIndex + j];
            }
        }

        while (true)
        {
            textInfo = textComponent.textInfo;
            bool inShakeZone = false;
            int tagOffset = 0;

            for (int i = 0; i < originalText.Length; i++)
            {
                if (i + 6 < originalText.Length && originalText.Substring(i, 7) == "<shake>")
                {
                    inShakeZone = true; i += 6; tagOffset += 7; continue;
                }
                if (i + 7 < originalText.Length && originalText.Substring(i, 8) == "</shake>")
                {
                    inShakeZone = false; i += 7; tagOffset += 8; continue;
                }

                int charIndex = i - tagOffset;
                if (charIndex >= textInfo.characterCount || charIndex < 0) continue;
                var charInfo = textInfo.characterInfo[charIndex];
                if (!charInfo.isVisible) continue;

                if (inShakeZone)
                {
                    int materialIndex = charInfo.materialReferenceIndex;
                    int vertexIndex = charInfo.vertexIndex;
                    Vector3[] sourceVertices = initialVertices[charIndex];
                    Vector3[] destinationVertices = textInfo.meshInfo[materialIndex].vertices;
                    Vector3 offset = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * shakeIntensity;

                    for (int j = 0; j < 4; j++)
                    {
                        destinationVertices[vertexIndex + j] = sourceVertices[j] + offset;
                    }
                }
            }
            textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
            yield return null;
        }
    }
}