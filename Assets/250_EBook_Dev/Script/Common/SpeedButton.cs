using DoDoEng.Common;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.EBook
{
    [RequireComponent(typeof(Button))]
    public class SpeedButton : MonoBehaviour
    {
        // Definitions
        private static float[] SPEEDS = new float[] { 1.0f, 1.25f, 1.5f, 0.75f };

        // Properties
        public float Speed => SPEEDS[currentIdx];

        // Methods
        public void Reset()
        {
            LOG.Function(this);

            changeSpeed(0);
        }

        // Events
        public event Action<float> OnSpeedChanged;



        // Fields : caching
        private Button btn_ = null;
        private Button btn => btn_ ??= GetComponent<Button>();

        // Fields
        private int currentIdx = 0;

        // Functions
        private void changeSpeed(int idx)
        {
            currentIdx = idx;
            speedTXT.text = $"{Speed}x";

            OnSpeedChanged?.Invoke(Speed);
        }
        private void toggleSpeed()
        {
            var idx = (currentIdx + 1) % SPEEDS.Length;
            changeSpeed(idx);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TextMeshProUGUI speedTXT = null;

        // Unity Messages
        private void Awake()
        {
            btn.onClick.AddListener(() => toggleSpeed());

            changeSpeed(0);
        }
        private void Start()
        {
        }

        
    }
}