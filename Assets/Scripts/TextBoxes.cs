using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextBoxes : MonoBehaviour
{
    [Header("Image Settings")]
    public RawImage talkingImage;
    public GameObject textMover;

    [Header("Text Box Movement")]
    public float textMoveDistance = 500f;
    public float textMoveSpeed = 0.5f;

    [Header("Text Settings")]
    public TMP_Text text;
    public string currentText = "none";

    [Header("Current Dio")]
    public CharacterTypes charType;
    public DioScriptableObj dio;

    private Coroutine dioCoroutine;

    private bool isTalking = true;
    private bool isDioRunning = false;

    private Quaternion originalRotation;

    void Awake()
    {
        if (talkingImage != null)
            originalRotation = talkingImage.transform.localRotation;

        StartDio(charType, dio, true);
    }

    void Update()
    {
    }

    public void StartDio(CharacterTypes chty, DioScriptableObj discob, bool firstDio = false)
    {
        if (dioCoroutine != null)
            StopCoroutine(dioCoroutine);

        charType = chty;
        dio = discob;

        if (charType == null || dio == null)
        {
            Debug.LogWarning("StartDio was called with a null CharacterTypes or DioScriptableObj.");
            return;
        }

        isDioRunning = true;

        if (talkingImage != null)
        {
            talkingImage.texture = charType.noTalkingImage;
            talkingImage.transform.localRotation = originalRotation;
        }

        if (firstDio)
            dioCoroutine = StartCoroutine(StartDioCoroutine());
        else
            dioCoroutine = StartCoroutine(DioCoroutine());
    }

    private IEnumerator StartDioCoroutine()
    {
        yield return MoveTextMover(true);
        dioCoroutine = StartCoroutine(DioCoroutine());
    }

    public void Dio()
    {
        if (!isDioRunning || charType == null || dio == null)
            return;

        if (dioCoroutine != null)
            StopCoroutine(dioCoroutine);

        dioCoroutine = StartCoroutine(DioCoroutine());
    }

    private IEnumerator DioCoroutine()
    {
        currentText = dio.theText;
        text.text = "";

        float characterDelay = 1f / charType.textSpeed;
        float talkingTimer = 0f;
        float rotationTimer = 0f;
        float rotationDirection = 1f;

        isTalking = true;

        while (text.text.Length < currentText.Length)
        {
            text.text += currentText[text.text.Length];

            if (talkingImage != null)
            {
                rotationTimer += characterDelay;

                float rotationProgress =
                    Mathf.Clamp01(rotationTimer / charType.imageRotaSpeed);

                float currentRotation =
                    Mathf.Lerp(0f, charType.rotationMax, rotationProgress)
                    * rotationDirection;

                talkingImage.transform.localRotation =
                    originalRotation *
                    Quaternion.Euler(0f, 0f, currentRotation);

                if (rotationTimer >= charType.imageRotaSpeed)
                {
                    rotationTimer = 0f;
                    rotationDirection *= -1f;
                }
            }

            talkingTimer += characterDelay;

            if (talkingTimer >= charType.talkingImageSpeed)
            {
                talkingTimer = 0f;

                if (talkingImage != null)
                {
                    if (talkingImage.texture == charType.talkingImage)
                        talkingImage.texture = charType.noTalkingImage;
                    else
                        talkingImage.texture = charType.talkingImage;
                }
            }

            yield return new WaitForSeconds(characterDelay);
        }

        isTalking = false;

        if (talkingImage != null)
        {
            talkingImage.transform.localRotation = originalRotation;
            talkingImage.texture = charType.noTalkingImage;
        }

        yield return new WaitForSeconds(charType.postTextWait);

        if (dio.nextText != null)
        {
            DioScriptableObj nextDio = dio.nextText;
            StartDio(charType, nextDio, false);
        }
        else
        {
            yield return MoveTextMover(false);
            ResetDio();
        }
    }

    private IEnumerator MoveTextMover(bool moveDown)
    {
        if (textMover == null)
            yield break;

        Vector3 startPosition = textMover.transform.localPosition;

        float direction = moveDown ? -1f : 1f;

        Vector3 targetPosition =
            startPosition +
            Vector3.up * direction * textMoveDistance;

        float timer = 0f;

        while (timer < textMoveSpeed)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / textMoveSpeed);
            t = Mathf.SmoothStep(0f, 1f, t);

            textMover.transform.localPosition =
                Vector3.Lerp(startPosition, targetPosition, t);

            yield return null;
        }

        textMover.transform.localPosition = targetPosition;
    }

    public void ResetDio()
    {
        if (dioCoroutine != null)
        {
            StopCoroutine(dioCoroutine);
            dioCoroutine = null;
        }

        isDioRunning = false;
        isTalking = false;

        charType = null;
        dio = null;

        currentText = "none";

        if (text != null)
            text.text = "";

        if (talkingImage != null)
        {
            talkingImage.texture = null;
            talkingImage.transform.localRotation = originalRotation;
        }
    }
}
