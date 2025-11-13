using Roulette;
using Roulette.Model;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SandBox
{
    public class TesteSave : MonoBehaviour
    {
        public TMP_InputField small;
        public TMP_InputField medium;
        public TMP_InputField big;
        
        public RouletteManager rouletteManager;
        
        public void Send()
        {
            var rouletteData = new RouletteData
            {
                numbers = new[] { int.Parse(small.text), int.Parse(medium.text), int.Parse(big.text) },
                time = 0
            };
            rouletteManager.StartWithJson(JsonUtility.ToJson(rouletteData));
        }

        public void SpinRandom()
        {
            RandomNumbers();
            Send();
        }
        public void RandomNumbers()
        {
            small.text = Random.Range(0, 9).ToString();
            medium.text = Random.Range(0, 9).ToString();
            big.text = Random.Range(0, 9).ToString();
        }
    }
}
