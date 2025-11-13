using System.Collections;
using Roulette.Model;
using UnityEngine;

namespace Roulette
{
    public class RouletteManager : MonoBehaviour
    {
        [Header("Roletas (Esquerda → Meio → Direita)")]
        [SerializeField] private Roulette2D[] roulettes;
    
        [Header("Configuração de Tempos")]
        [SerializeField] private float[] stopTimes = { 10f, 8.5f, 7f };
        
        [Header("Resultado Final")]
        [SerializeField] private float resultDisplayDuration = 5f;
        
        private int _stoppedCount;
        private bool _showingResult;
        private int[] _finalNumbers;
        
        public bool AllRoulettesStopped() => _stoppedCount >= roulettes.Length;
        public bool IsShowingResult() => _showingResult;
        private void Awake()
        {
            ValidateSetup();
            ConfigureRouletteIndices();
            SubscribeToEvents();
        }

        private void ValidateSetup()
        {
            if (roulettes == null || roulettes.Length == 0)
                return;

            for (var i = 0; i < roulettes.Length; i++)
            {
                if (roulettes[i] == null)
                    return;
            }
        }

        private void ConfigureRouletteIndices()
        {
            for (var i = 0; i < roulettes.Length; i++)
            {
                if (roulettes[i] != null)
                {
                    roulettes[i].SetRouletteIndex(i);
                }
            }
        }

        private void SubscribeToEvents()
        {
            for (var i = 0; i < roulettes.Length; i++)
            {
                if (roulettes[i] != null)
                {
                    var index = i;
                    roulettes[i].onRouletteStopped.AddListener((number) => OnRouletteStopped(index, number));
                }
            }
        }
        
        private void OnRouletteStopped(int rouletteIndex, int number)
        {
            _stoppedCount++;
            
            if (_stoppedCount >= roulettes.Length)
            {
                OnAllRoulettesStopped();
            }
        }
        
        private void OnAllRoulettesStopped()
        {
            _finalNumbers = GetFinalNumbers();
            _showingResult = true;
            StartCoroutine(DisplayResultRoutine());
        }

        private IEnumerator DisplayResultRoutine()
        {
            yield return new WaitForSecondsRealtime(resultDisplayDuration);
            _showingResult = false;
        }
        
        public void StartWithJson(string json)
        {
            if (string.IsNullOrEmpty(json))
                return;

            RouletteData data = JsonUtility.FromJson<RouletteData>(json);
            
            if (data?.numbers == null || data.numbers.Length == 0)
                return;

            StartRoulettes(data.numbers, data.time);
        }

        private void StartRoulettes(int[] numbers, float initialTime = 0f)
        {
            _stoppedCount = 0;
            _showingResult = false;

            if (numbers.Length < roulettes.Length)
                return;

            for (var i = 0; i < roulettes.Length; i++)
            {
                if (roulettes[i] == null) continue;

                var number = i < numbers.Length ? numbers[i] : 0;
                var stopTime = i < stopTimes.Length ? stopTimes[i] : 10f;
                
                roulettes[i].StartRoulette(number, initialTime, stopTime);
            }
        }
        
        public int[] GetFinalNumbers()
        {
            var numbers = new int[roulettes.Length];
            for (var i = 0; i < roulettes.Length; i++)
            {
                numbers[i] = roulettes[i].GetFinalNumber();
            }
            return numbers;
        }
        
        public int[] GetCurrentNumbers()
        {
            var numbers = new int[roulettes.Length];
            for (var i = 0; i < roulettes.Length; i++)
            {
                numbers[i] = roulettes[i].GetCurrentNumber();
            }
            return numbers;
        }
        
    }
}