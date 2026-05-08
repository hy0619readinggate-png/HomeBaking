using UnityEngine;

namespace DoDoEng.Common
{
    [CreateAssetMenu(fileName = "DataDefinitionSO", menuName = "DoDoEng/DataDefinitionSO", order = 2)]
    public class DataDefinitionSO : ScriptableObject
    {
        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private string comment;
    }
}