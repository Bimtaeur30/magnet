using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

public sealed class Opinion_UI : MonoBehaviour
{
    private const int DiscordMessageLimit = 2000;

    [Header("Discord")]
    [SerializeField]
    private string webhookUrl;

    [Header("UI")]
    [SerializeField]
    private TMP_InputField messageInput;

    [SerializeField]
    private Button submitButton;

    [SerializeField]
    private TMP_Text resultText;

    private bool isSending;

    private void Awake()
    {
        if (submitButton != null)
        {
            submitButton.onClick.AddListener(HandleSubmit);
        }
    }

    private void OnDestroy()
    {
        if (submitButton != null)
        {
            submitButton.onClick.RemoveListener(HandleSubmit);
        }
    }

    private void HandleSubmit()
    {
        if (isSending)
        {
            return;
        }

        if (messageInput == null)
        {
            Debug.LogError("Message Input이 연결되지 않았습니다.");
            return;
        }

        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            SetResult("Webhook URL이 설정되지 않았습니다.");
            return;
        }

        string message = messageInput.text.Trim();

        if (string.IsNullOrWhiteSpace(message))
        {
            SetResult("내용을 입력해주세요.");
            return;
        }

        if (message.Length > DiscordMessageLimit)
        {
            SetResult($"메시지는 {DiscordMessageLimit}자까지 입력할 수 있습니다.");
            return;
        }

        StartCoroutine(SendMessageRoutine(message));
    }

    private IEnumerator SendMessageRoutine(string message)
    {
        SetSendingState(true);
        SetResult("전송 중...");

        var payload = new DiscordWebhookPayload
        {
            content = message,

            // 모든 멘션 비활성화
            allowed_mentions = new AllowedMentions
            {
                parse = Array.Empty<string>()
            }
        };

        string json = JsonUtility.ToJson(payload);
        byte[] requestBody = Encoding.UTF8.GetBytes(json);

        using var request = new UnityWebRequest(
            webhookUrl,
            UnityWebRequest.kHttpVerbPOST
        );

        request.uploadHandler = new UploadHandlerRaw(requestBody);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        bool isSuccess =
            request.result == UnityWebRequest.Result.Success &&
            request.responseCode >= 200 &&
            request.responseCode < 300;

        if (isSuccess)
        {
            ClearInputFieldSafely();
            SetResult("전송되었습니다.");
        }
        else
        {
            string responseText = request.downloadHandler?.text ?? string.Empty;

            Debug.LogError(
                $"Discord 전송 실패\n" +
                $"응답 코드: {request.responseCode}\n" +
                $"오류: {request.error}\n" +
                $"응답: {responseText}"
            );

            SetResult($"전송 실패: HTTP {request.responseCode}");
        }

        SetSendingState(false);
    }

    private void ClearInputFieldSafely()
    {
        if (messageInput == null)
        {
            return;
        }

        // 입력창이 선택된 상태에서 문자열만 제거하면
        // TMP 내부 선택 범위가 꼬일 수 있으므로 먼저 비활성화한다.
        messageInput.DeactivateInputField();

        // onValueChanged를 호출하지 않고 텍스트 제거
        messageInput.SetTextWithoutNotify(string.Empty);

        // 커서와 선택 범위를 빈 문자열의 유효한 위치인 0으로 초기화
        messageInput.caretPosition = 0;
        messageInput.stringPosition = 0;

        messageInput.selectionStringAnchorPosition = 0;
        messageInput.selectionStringFocusPosition = 0;

        messageInput.selectionAnchorPosition = 0;
        messageInput.selectionFocusPosition = 0;

        messageInput.ForceLabelUpdate();

        // 현재 선택된 UI 오브젝트가 입력창이면 선택 해제
        if (EventSystem.current != null &&
            EventSystem.current.currentSelectedGameObject == messageInput.gameObject)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    private void SetSendingState(bool sending)
    {
        isSending = sending;

        if (submitButton != null)
        {
            submitButton.interactable = !sending;
        }

        if (messageInput != null)
        {
            messageInput.interactable = !sending;
        }
    }

    private void SetResult(string message)
    {
        if (resultText != null)
        {
            resultText.text = message;
        }
    }

    [Serializable]
    private sealed class DiscordWebhookPayload
    {
        public string content;
        public AllowedMentions allowed_mentions;
    }

    [Serializable]
    private sealed class AllowedMentions
    {
        public string[] parse;
    }
}