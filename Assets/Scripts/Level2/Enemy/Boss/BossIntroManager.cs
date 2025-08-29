using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using Unity.Cinemachine;

[System.Serializable]
public class DialogueLine
{
    public Sprite characterPortrait;
    [TextArea(3, 10)]
    public string line;
}

public class BossIntroManager : MonoBehaviour
{
    [Header("Character References")]
    public PlayerMovement playerMovement;
    public ArchimageAI archimageAI;

    [Header("Dialogue UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public Image portraitImageUI;

    [Header("Cinemachine Cameras")]
    public CinemachineCamera playerFollowCam;
    public CinemachineCamera bossArenaCam;
    public float cameraTransitionPause = 1.0f;

    [Header("Cinematic Bars")]
    public RectTransform leftBar;
    public RectTransform rightBar;
    public GameObject leftWall;
    public GameObject rightWall;
    public float barAnimationTime = 0.5f;

    [Header("Dialogue Content")]
    public DialogueLine[] dialogueLines;

    public float typingSpeed = 0.05f;

    public void StartIntroSequence()
    {
        StartCoroutine(IntroCoroutine());
    }

    private IEnumerator IntroCoroutine()
    {
        playerMovement.enabled = false;
        archimageAI.enabled = false;

        Rigidbody2D playerRb = playerMovement.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
        }

        Animator playerAnimator = playerMovement.GetComponent<Animator>();
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("isRunning", false);
            playerAnimator.SetBool("isJumping", false);
            playerAnimator.SetBool("isFalling", false);
        }
        
        dialoguePanel.SetActive(true);

        TextShake shaker = dialogueText.GetComponent<TextShake>();

        foreach (DialogueLine dialogueLine in dialogueLines)
        {
            if (portraitImageUI != null)
            {
                portraitImageUI.sprite = dialogueLine.characterPortrait;
            }
            
            yield return StartCoroutine(TypeLine(dialogueLine.line));
            
            if (shaker != null)
            {
                shaker.StartShake(dialogueLine.line);
            }
            
            yield return new WaitUntil(() => Input.anyKeyDown || Input.GetMouseButtonDown(0)); 

            if (shaker != null)
            {
                shaker.StopShake();
            }
        }

        dialoguePanel.SetActive(false);

        leftWall.SetActive(true);
        rightWall.SetActive(true);
        
        StartCoroutine(AnimateBars(true));

        if (bossArenaCam != null)
        {
            bossArenaCam.Priority = 11;
        }
        
        yield return new WaitForSeconds(cameraTransitionPause);
        
        // --- CORRECTED LOGIC TO START THE FIGHT ---
        playerMovement.enabled = true;
        archimageAI.enabled = true;         // This line is essential to "turn on" his brain.
        archimageAI.ActivateCombat();     // This line tells his brain it's time to fight.
        
        Debug.Log("Intro finished! Fight begins!");
    }

    private IEnumerator TypeLine(string line)
    {
        dialogueText.text = "";
        bool isTag = false;
        string cleanLine = "";

        foreach (char letter in line.ToCharArray())
        {
            if (letter == '<') { isTag = true; }
            if (!isTag) { cleanLine += letter; }
            if (letter == '>') { isTag = false; }
        }

        foreach (char letter in cleanLine.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    public IEnumerator AnimateBars(bool show)
    {
        leftBar.gameObject.SetActive(true);
        rightBar.gameObject.SetActive(true);
        
        float startXLeft = show ? -leftBar.rect.width : 0;
        float endXLeft = show ? 0 : -leftBar.rect.width;
        
        float startXRight = show ? rightBar.rect.width : 0;
        float endXRight = show ? 0 : rightBar.rect.width;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < barAnimationTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / barAnimationTime;

            Vector2 leftPos = leftBar.anchoredPosition;
            leftPos.x = Mathf.Lerp(startXLeft, endXLeft, t);
            leftBar.anchoredPosition = leftPos;

            Vector2 rightPos = rightBar.anchoredPosition;
            rightPos.x = Mathf.Lerp(startXRight, endXRight, t);
            rightBar.anchoredPosition = rightPos;

            yield return null;
        }
        
        if (!show)
        {
            leftBar.gameObject.SetActive(false);
            rightBar.gameObject.SetActive(false);
        }
    }
}