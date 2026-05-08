using TMPro;
using UnityEngine;


namespace DoDoEng.Common
{
    [ExecuteInEditMode]
    public class TextMeshWrapText : MonoBehaviour
    {
        // Fields : caching
        private TMP_Text tmpText_ = null;
        private TMP_Text tmpText => tmpText_ ??= GetComponent<TMP_Text>();

        // Fields
        private AnimationCurve old_Curve;
        private float old_CurveScale;
        private string old_Text;

        // Functions
        private AnimationCurve copyAnimationCurve(AnimationCurve curve)
        {
            var newCurve = new AnimationCurve();
            newCurve.keys = curve.keys;

            return newCurve;
        }
        private void applyCurve()
        {
            tmpText.ForceMeshUpdate(); // Generate the mesh and populate the textInfo with data we can use and manipulate.

            var textInfo = tmpText.textInfo;
            if (textInfo == null) return;

            var characterCount = textInfo.characterCount;
            if (characterCount == 0) return;

            var boundsMinX = tmpText.bounds.min.x;  //textInfo.meshInfo[0].mesh.bounds.min.x;
            var boundsMaxX = tmpText.bounds.max.x;  //textInfo.meshInfo[0].mesh.bounds.max.x;

            for (var i = 0; i < characterCount; i++)
            {
                if (!textInfo.characterInfo[i].isVisible)
                    continue;

                var vertexIndex = textInfo.characterInfo[i].vertexIndex;

                // Get the index of the mesh used by this character.
                var materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
                var vertices = textInfo.meshInfo[materialIndex].vertices;

                // Compute the baseline mid point for each character
                var offsetToMidBaseline =
                    new Vector3(
                        (vertices[vertexIndex + 0].x + vertices[vertexIndex + 2].x) / 2,
                        textInfo.characterInfo[i].baseLine);
                //float offsetY = VertexCurve.Evaluate((float)i / characterCount + loopCount / 50f); // Random.Range(-0.25f, 0.25f);

                // Apply offset to adjust our pivot point.
                vertices[vertexIndex + 0] += -offsetToMidBaseline;
                vertices[vertexIndex + 1] += -offsetToMidBaseline;
                vertices[vertexIndex + 2] += -offsetToMidBaseline;
                vertices[vertexIndex + 3] += -offsetToMidBaseline;

                // Compute the angle of rotation for each character based on the animation curve
                var x0 = (offsetToMidBaseline.x - boundsMinX) / (boundsMaxX - boundsMinX);  // Character's position relative to the bounds of the mesh.
                var x1 = x0 + 0.0001f;
                var y0 = VertexCurve.Evaluate(x0) * CurveScale;
                var y1 = VertexCurve.Evaluate(x1) * CurveScale;

                var horizontal = new Vector3(1, 0, 0);
                //Vector3 normal = new Vector3(-(y1 - y0), (x1 * (boundsMaxX - boundsMinX) + boundsMinX) - offsetToMidBaseline.x, 0);
                var tangent = new Vector3(x1 * (boundsMaxX - boundsMinX) + boundsMinX, y1) - new Vector3(offsetToMidBaseline.x, y0);

                var dot = Mathf.Acos(Vector3.Dot(horizontal, tangent.normalized)) * 57.2957795f;
                var cross = Vector3.Cross(horizontal, tangent);
                var angle = cross.z > 0 ? dot : 360 - dot;
                var matrix = Matrix4x4.TRS(new Vector3(0, y0, 0), Quaternion.Euler(0, 0, angle), Vector3.one);

                vertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 0]);
                vertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 1]);
                vertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 2]);
                vertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 3]);

                vertices[vertexIndex + 0] += offsetToMidBaseline;
                vertices[vertexIndex + 1] += offsetToMidBaseline;
                vertices[vertexIndex + 2] += offsetToMidBaseline;
                vertices[vertexIndex + 3] += offsetToMidBaseline;
            }

            // Upload the mesh with the revised information
            tmpText.UpdateVertexData();
        }


        // Unity Inspectors
        [Header("★ Config")]
        [SerializeField]
        private AnimationCurve VertexCurve =
            new AnimationCurve(
                new Keyframe(0, 0),
                new Keyframe(0.25f, 2.0f),
                new Keyframe(0.5f, 0),
                new Keyframe(0.75f, 2.0f),
                new Keyframe(1, 0f));
        [SerializeField] private float CurveScale = 1.0f;

        // Unity Messages
        private void Awake()
        {
            VertexCurve.preWrapMode = WrapMode.Clamp;
            VertexCurve.postWrapMode = WrapMode.Clamp;

            applyCurve();
        }
        private void Start()
        {
        }
        private void OnEnable()
        {
            applyCurve();
        }
        private void OnDisable()
        {
        }
        private void LateUpdate()
        {
            if (tmpText.havePropertiesChanged ||
                old_CurveScale != CurveScale ||
                old_Text != tmpText.text ||
                old_Curve == null ||
                old_Curve.keys.Length != VertexCurve.keys.Length ||
                old_Curve.keys[1].value != VertexCurve.keys[1].value)
            {
                old_CurveScale = CurveScale;
                old_Text = tmpText.text;
                old_Curve = copyAnimationCurve(VertexCurve);

                applyCurve();
            }
        }
    }
}