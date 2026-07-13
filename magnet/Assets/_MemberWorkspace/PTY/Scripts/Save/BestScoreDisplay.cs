using GameLib.EventChannelSystem;
using PTY.Scripts.Events;
using Reflex.Attributes;
using TMPro;
using UnityEngine;

namespace PTY.Scripts.Save
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class BestScoreDisplay : MonoBehaviour
    {
        [Inject] private ISaveService _saveService;

        [SerializeField] private EventChannelSO magnetGameChannel;

        private TextMeshProUGUI _text;

        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
            magnetGameChannel.AddListener<BestScoreUpdatedEvent>(OnBestScoreUpdated);

            UpdateText(_saveService.BestScore);
        }

        private void OnDisable()
        {
            magnetGameChannel.RemoveListener<BestScoreUpdatedEvent>(OnBestScoreUpdated);
        }

        private void OnBestScoreUpdated(BestScoreUpdatedEvent evt)
        {
            UpdateText(evt.NewBestScore);
        }

        private void UpdateText(int bestScore)
        {
            _text.text = bestScore.ToString();
        }
    }
}
