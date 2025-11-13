using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace Roulette
{
    public class Roulette2D : MonoBehaviour
    {
        [Header("Configurações da Roleta")]
        [SerializeField] private Transform visualRoulette;
        public enum RouletteState { Idle, Preparing, Spinning, Stopping, Stopped }
        private RouletteState _currentState = RouletteState.Idle;
        private int _rouletteIndex = 0;
    
        [Header("Ângulo de Seleção")]
        [SerializeField] private float selectionAngle = 90f;
        [SerializeField] private float angleOffset = 0f;
    
        [Header("Animação de Preparação")]
        [SerializeField] private float preparationDuration = 0.8f;
        [SerializeField] private float preparationAngle = 30f;
        [SerializeField] private Ease preparationEase = Ease.OutBack;
    
        [Header("Configurações de Rotação")]
        [SerializeField] private float spinSpeed = 360f;
        [SerializeField] private float finalLerpDuration = 0.5f;
        [SerializeField] private Ease finalLerpEase = Ease.OutCubic;
    
        [Header("Eventos")]
        public UnityEvent<int> onRouletteStopped;
    
        private const int TotalNumbers = 10;
        private const float DegreesPerNumber = 360f / TotalNumbers;
    
    
        private float _currentTime;
        private float _stopTime = 10f;
        private int _finalNumber;
        private float _currentRotation;
        private float _targetFinalRotation;
    
        private Tweener _preparationTween;
        private Tweener _stopTween;
        public int GetFinalNumber() => _finalNumber;
    
        public bool IsStopped() => _currentState == RouletteState.Stopped;
        public void SetRouletteIndex(int index) => _rouletteIndex = index;
    
        public RouletteState GetState() => _currentState;
        private void Awake()
        {
            DOTween.SetTweensCapacity(200, 50);
            DOTween.defaultTimeScaleIndependent = true;
            _currentRotation = Random.Range(0f, 360f);
            UpdateVisualRotation();
        }
        private void Update()
        {
            if (_currentState != RouletteState.Spinning) return;
        
            _currentTime += Time.unscaledDeltaTime;
        
            if (_currentTime >= _stopTime)
            {
                StopWithAnimation();
                return;
            }
        
            var remainingRotation = _targetFinalRotation - _currentRotation;
            var remainingTime = _stopTime - _currentTime;
        
            if (remainingTime > 0)
            {
                var neededSpeed = remainingRotation / remainingTime;
                _currentRotation += neededSpeed * Time.unscaledDeltaTime;
            }
        
            UpdateVisualRotation();
        }
        private void OnDisable()
        {
            KillTween();
        }
        private void KillTween()
        {
            _preparationTween?.Kill();
            _stopTween?.Kill();
        }
        public void StartRoulette(int destinationNumber, float initialTime, float rouletteStopTime)
        {
            KillTween();
        
            _finalNumber = Mathf.Clamp(destinationNumber, 0, 9);
            _currentTime = initialTime;
            _stopTime = rouletteStopTime;
        
            CalculateTargetRotation();
        
            if (initialTime == 0f)
            {
                StartPreparationAnimation();
            }
            else
            {
                SynchronizeRotation(initialTime);
                _currentState = RouletteState.Spinning;
            }
        
            UpdateVisualRotation();
        }

        private void CalculateTargetRotation()
        {
            var currentNormalized = _currentRotation % 360f;
            if (currentNormalized < 0) currentNormalized += 360f;
        
            var numberPositionOnRoulette = _finalNumber * DegreesPerNumber;
            var effectiveSelectionAngle = selectionAngle + angleOffset;
            var targetAngleNormalized = (360f - numberPositionOnRoulette + effectiveSelectionAngle) % 360f;
        
            var rotationNeeded = targetAngleNormalized - currentNormalized;
        
            if (rotationNeeded <= 0)
            {
                rotationNeeded += 360f;
            }
        
            rotationNeeded += 1080f;
            _targetFinalRotation = _currentRotation + rotationNeeded;
        }

        private void StartPreparationAnimation()
        {
            _currentState = RouletteState.Preparing;
            var direction = (_rouletteIndex == 1) ? 1f : -1f;
            var prepTarget = _currentRotation + (preparationAngle * direction);
        
            _preparationTween = DOTween.To(() => _currentRotation, x => _currentRotation = x, prepTarget, preparationDuration)
                .SetEase(preparationEase)
                .SetUpdate(true)
                .OnUpdate(UpdateVisualRotation)
                .OnComplete(() => _currentState = RouletteState.Spinning);
        }
        private void SynchronizeRotation(float time)
        {
            if (time >= _stopTime)
            {
                _currentRotation = _targetFinalRotation;
                _currentState = RouletteState.Stopped;
                OnRouletteStopped();
            }
            else
            {
                var progress = time / _stopTime;
                var rotationAmount = _targetFinalRotation - _currentRotation;
                _currentRotation += rotationAmount * progress;
                _currentState = RouletteState.Spinning;
            }
        }
        private void StopWithAnimation()
        {
            _currentState = RouletteState.Stopping;
            _stopTween?.Kill();
        
            _stopTween = DOTween.To(() => _currentRotation, x => _currentRotation = x, _targetFinalRotation, finalLerpDuration)
                .SetEase(finalLerpEase)
                .SetUpdate(true)
                .OnUpdate(UpdateVisualRotation)
                .OnComplete(() => 
                {
                    _currentRotation = _targetFinalRotation;
                    UpdateVisualRotation();
                    _currentState = RouletteState.Stopped;
                    OnRouletteStopped();
                });
        }

        private void UpdateVisualRotation()
        {
            if (visualRoulette != null)
                visualRoulette.rotation = Quaternion.Euler(0f, 0f, -_currentRotation);
        }

        private void OnRouletteStopped()
        {
            onRouletteStopped?.Invoke(_finalNumber);
        }

        public int GetCurrentNumber()
        {
            var normalizedRotation = _currentRotation % 360f;
            if (normalizedRotation < 0) normalizedRotation += 360f;
        
            var effectiveSelectionAngle = selectionAngle + angleOffset;
            var numberPosition = (360f + effectiveSelectionAngle - normalizedRotation) % 360f;
            var number = Mathf.RoundToInt(numberPosition / DegreesPerNumber) % TotalNumbers;
        
            return number;
        }
    }
}