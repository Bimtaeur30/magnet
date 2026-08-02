using GameLib.EventChannelSystem;
using Magnet.Contracts.Save;
using PTY.Scripts.Events;
using Reflex.Attributes;
using TMPro;
using UnityEngine;

namespace PTY.Scripts.Save
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class BestScoreDisplay : MonoBehaviour
    {
        [SerializeField] private EventChannelSO magnetGameChannel;
        [Inject] private ISaveService _saveService;

        private TextMeshProUGUI _text;

        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
            magnetGameChannel.AddListener<BestStageUpdatedEvent>(OnBestStageUpdated);

            UpdateText(_saveService.BestStage);
        }

        private void OnDisable()
        {
            magnetGameChannel.RemoveListener<BestStageUpdatedEvent>(OnBestStageUpdated);
        }

        private void OnBestStageUpdated(BestStageUpdatedEvent evt)
        {
            UpdateText(evt.NewBestStage);
        }

        private void UpdateText(int bestStage)
        {
            _text.text = bestStage.ToString();
        }
    }
}
