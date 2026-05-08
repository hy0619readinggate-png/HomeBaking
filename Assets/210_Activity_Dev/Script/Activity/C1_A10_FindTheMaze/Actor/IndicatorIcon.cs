using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C1_A10
{
    public class IndicatorIcon : MonoBehaviour
    {
        // Definitions
        private enum State { Empty, KeyOff, KeyOn, Blanc, Jack, Sheila }

        // Properties
        public Vector2 ScreenPosition => canvas.worldCamera.WorldToScreenPoint(transform.position);


        // Methods
        public void KeyOff()
        {
            LOG.Info($"KeyOff()", this);

            updateState(State.KeyOff);
        }
        public void KeyOn()
        {
            LOG.Info($"KeyOn()", this);

            updateState(State.KeyOn);
        }
        public void GetBlanc()
        {
            LOG.Info($"GetBlanc()", this);

            updateState(State.Blanc);
        }
        public void GetJack()
        {
            LOG.Info($"GetJack()", this);

            updateState(State.Jack);
        }
        public void GetSheila()
        {
            LOG.Info($"GetSheila()", this);

            updateState(State.Sheila);
        }



        // Fields : caching
        private Canvas canvas_ = null;
        private Canvas canvas => canvas_ ??= GetComponentInParent<Canvas>();

        // Functions
        private void updateState(State state)
        {
            emptyGO.SetActive(state == State.Empty);
            keyOffGO.SetActive(state == State.KeyOff);
            keyOnGO.SetActive(state == State.KeyOn);
            blancGO.SetActive(state == State.Blanc);
            jackGO.SetActive(state == State.Jack);
            sheilaGO.SetActive(state == State.Sheila);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject emptyGO = null;
        [SerializeField] private GameObject keyOffGO = null;
        [SerializeField] private GameObject keyOnGO = null;
        [SerializeField] private GameObject blancGO = null;
        [SerializeField] private GameObject jackGO = null;
        [SerializeField] private GameObject sheilaGO = null;

        // Unity Messages
        private void Awake()
        {
            updateState(State.Empty);
        }
        private void Start()
        {
        }
    }
}