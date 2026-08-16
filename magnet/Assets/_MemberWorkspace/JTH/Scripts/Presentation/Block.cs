using GameLib.ObjectPool.Runtime;
using JTH.Scripts.Data;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace JTH.Scripts.Presentation
{
    public sealed class Block : AbstractMonoPoolable
    {
        [SerializeField] private SpriteRenderer skinRenderer;
        [Tooltip("칸 스킨 클리핑용. SetSortingOrder에서 Custom Range로 인접 마스크와 격리")]
        [SerializeField] private SpriteMask spriteMask;
        [SerializeField] private BoardConfigSO boardConfigSO;
        [SerializeField] private Animator hintAnimator;

        private Vector2Int _offset;

        public Vector2Int Offset
        {
            get => _offset;
            set
            {
                _offset = value;
                transform.localPosition = new Vector3(value.x, value.y, 1) * boardConfigSO.CellSize;
            }
        }

        public Texture Skin => skinRenderer.sprite.texture;

        public const int SortingBandPreview = 1000;
        public const int SortingBandPlaced = 10000;
        public const int SortingBandStaging = 20000;

        private static int _nextMaskSlot;
        private bool _dimmed;
        private float _dimMultiply = 1f;
        private float _alpha = 1f;

        private Sprite _placedSprite;
        private Sprite _hintSprite;
        private bool _hintActive;
        private AnimationClip _playingClip;
        private PlayableGraph _hintGraph;
        private AnimationClipPlayable _hintPlayable;

        private void Awake()
        {
            float cellSize = boardConfigSO.CellSize;
            float fillAmount = boardConfigSO.CellFill;
            spriteMask.transform.localScale = new Vector3(cellSize * fillAmount, cellSize * fillAmount, 1);

            Vector3 visualOffset = Vector2.one * cellSize / 2;
            spriteMask.transform.localPosition = visualOffset;

            if (hintAnimator == null && skinRenderer != null)
            {
                hintAnimator = skinRenderer.GetComponent<Animator>();
            }

            if (hintAnimator != null)
            {
                hintAnimator.enabled = false;
            }
        }

        private void Update()
        {
            LoopHintClipIfNeeded();
        }

        public override void ResetItem()
        {
            base.ResetItem();

            Debug.Assert(skinRenderer != null, "[Block] spriteRenderer is not assigned.", this);
            Debug.Assert(spriteMask != null, "[Block] spriteMask is not assigned.", this);

            SetClearHint(false);
            ApplySortingBand(SortingBandPlaced);
            SetDimmed(false, 0);
            SetAlpha(Color.white.a);
            _placedSprite = null;
        }

        private void OnDisable()
        {
            StopHintClip();
        }

        private void OnDestroy()
        {
            StopHintClip();
        }

        public void ApplySkin(Sprite skinSprite)
        {
            _placedSprite = skinSprite;
            if (_hintActive)
            {
                ApplySprite(_hintSprite != null ? _hintSprite : skinSprite);
                return;
            }

            ApplySprite(skinSprite);
        }

        /// <summary>
        /// 비활성 링 UX용. RGB만 <paramref name="multiply"/> 배로 어둡게 하고, false면 기본색 복원.
        /// </summary>
        public void SetDimmed(bool isDimmed, float multiply)
        {
            _dimmed = isDimmed;
            _dimMultiply = multiply;
            RefreshColor();
        }

        /// <summary>
        /// 스프라이트 알파 배율. 프리뷰 고스트 등에서 사용.
        /// </summary>
        public void SetAlpha(float value)
        {
            _alpha = Mathf.Clamp01(value);
            RefreshColor();
        }

        public void SetClearHint(bool enabled, Sprite unifiedSprite = null, AnimationClip clip = null)
        {
            if (!enabled)
            {
                ClearHint();
                return;
            }

            if (_hintActive && _hintSprite == unifiedSprite && _playingClip == clip)
            {
                return;
            }

            _hintActive = true;
            _hintSprite = unifiedSprite;
            ApplySprite(unifiedSprite != null ? unifiedSprite : _placedSprite);
            PlayHintClip(clip);
        }

        private void ClearHint()
        {
            _hintActive = false;
            _hintSprite = null;
            StopHintClip();
            ApplySprite(_placedSprite);
        }

        private void PlayHintClip(AnimationClip clip)
        {
            StopHintClip();
            _playingClip = clip;
            if (hintAnimator == null || clip == null)
            {
                return;
            }

            hintAnimator.enabled = true;
            _hintGraph = PlayableGraph.Create("BlockClearHint");
            _hintGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            AnimationPlayableOutput output = AnimationPlayableOutput.Create(_hintGraph, "Hint", hintAnimator);
            _hintPlayable = AnimationClipPlayable.Create(_hintGraph, clip);
            _hintPlayable.SetApplyFootIK(false);
            output.SetSourcePlayable(_hintPlayable);
            _hintGraph.Play();
        }

        private void StopHintClip()
        {
            if (_hintGraph.IsValid())
            {
                _hintGraph.Destroy();
            }

            _hintPlayable = default;
            _playingClip = null;

            if (hintAnimator != null)
            {
                hintAnimator.enabled = false;
            }
        }

        private void LoopHintClipIfNeeded()
        {
            if (!_hintActive || _playingClip == null || !_hintPlayable.IsValid())
            {
                return;
            }

            if (_playingClip.isLooping)
            {
                return;
            }

            float duration = _playingClip.length;
            if (duration <= 0f)
            {
                return;
            }

            double time = _hintPlayable.GetTime();
            if (time >= duration)
            {
                _hintPlayable.SetTime(time % duration);
            }
        }

        private void ApplySprite(Sprite sprite)
        {
            if (skinRenderer == null)
            {
                return;
            }

            if (sprite != null)
            {
                skinRenderer.sprite = sprite;
            }

            RefreshColor();
        }

        private void RefreshColor()
        {
            if (skinRenderer == null)
            {
                return;
            }

            Color color = Color.white;
            if (_dimmed)
            {
                color.r *= _dimMultiply;
                color.g *= _dimMultiply;
                color.b *= _dimMultiply;
            }

            color.a = _alpha;
            skinRenderer.color = color;
        }

        public void ApplySortingBand(int bandBase)
        {
            SetSortingOrder(bandBase);
        }

        private void SetSortingOrder(int sortingOrder)
        {
            int order = sortingOrder + _nextMaskSlot++;
            skinRenderer.sortingOrder = order;
            ApplyMaskIsolation(order);
        }

        private void ApplyMaskIsolation(int order)
        {
            if (spriteMask == null)
            {
                return;
            }

            spriteMask.isCustomRangeActive = true;
            spriteMask.backSortingLayerID = spriteMask.frontSortingLayerID = skinRenderer.sortingLayerID;
            spriteMask.frontSortingOrder = order;
            spriteMask.backSortingOrder = order - 1;
        }
    }
}
