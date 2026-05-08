using UnityEngine;

namespace DoDoEng.EBook.Quiz
{
    [DefaultExecutionOrder(-100)]
    public class T3FloatMGR : MonoBehaviour
    {
        // Methods
        public T3ExampleFloat Get()
        {
            var sequenceFloat = Instantiate(sequenceFloatTemplate, transform);
            sequenceFloat.gameObject.SetActive(true);

            return sequenceFloat;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private T3ExampleFloat sequenceFloatTemplate = null;

        // Unity Messages
        private void Awake()
        {
            sequenceFloatTemplate.gameObject.SetActive(false);
        }
    }
}