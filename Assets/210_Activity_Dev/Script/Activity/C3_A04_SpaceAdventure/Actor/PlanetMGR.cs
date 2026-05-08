using beyondi.Util;
using DoDoEng.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DoDoEng.Activity.C3_A04
{
    public class PlanetMGR : MonoBehaviour
    {
        // Methods
        public void Setup(ProblemData problem)
        {
            LOG.Info($"Setup() | {problem}", this);

            if (planets != null)
                planets.ForEach(p => p.Clear());

            var list = new List<Planet>();
            foreach (var ed in problem.Examples)
            {
                var planet = spawnPlanet(ed.Shape, ed.Text);
                list.Add(planet);
            }
            planets = list.ToArray();
        }
        public Planet[] SetAnswerPlanet(char alphabet)
        {
            LOG.Function(this, $"{alphabet}");

            var answers = planets
                .Where(p => !p.IsCollected)
                .Where(p => p.Value == alphabet.ToString());
            answers.ForEach(p => p.SetAsAnswer(true));

            return answers.ToArray();
        }
        public void ClearAnswerPlanet()
        {
            LOG.Function(this);

            planets.ForEach(p => p.SetAsAnswer(false));
        }
        public void DeactivateAll()
        {
            LOG.Info($"DeactivateAll()", this);

            planets.ForEach(p => p.Deactive());
        }



        // Fields
        private Planet[] planets = null;
        private Dictionary<string, AudioClip> alphabetClipDict = new Dictionary<string, AudioClip>();

        // Functions
        private Planet spawnPlanet(int shape, string alphabet)
        {
            for (var i = 0; i < overlapMaxAttempt; i++)
            {
                var areaCOL = UtilArray.ExtractOne(spawnCOL);
                var pos = getSpawnPosition(areaCOL);
                var collider = Physics2D.OverlapCircle(pos, overlapRadius, layerMask);
                if (collider == null)
                {
                    LOG.Assert(shape <= planetPB.Length, $"Shape must be in range Planet Prefabs.", this);
                    var planet = Instantiate(planetPB[shape - 1], pos, Quaternion.identity, planetParetTR);
                    planet.transform.SetLocalZ(0);
                    planet.OverlabRadius = overlapRadius;
                    planet.Init(alphabet, alphabetClipOf(alphabet));
                    planet.gameObject.name = $"Planet_{alphabet}";
                    planet.gameObject.SetActive(true);
                    return planet;
                }
            }

            LOG.Warning($"No avail position for planet", this);

            // 강제 생성
            {
                var areaCOL = UtilArray.ExtractOne(spawnCOL);
                var pos = getSpawnPosition(areaCOL);
                var planet = Instantiate(planetPB[shape - 1], pos, Quaternion.identity, planetParetTR);
                planet.transform.SetLocalZ(0);
                planet.Init(alphabet, alphabetClipOf(alphabet));
                planet.OverlabRadius = overlapRadius;
                planet.gameObject.name = $"Planet_{alphabet}";
                planet.gameObject.SetActive(true);
                return planet;
            }
        }
        private Vector3 getSpawnPosition(BoxCollider2D areaCOL)
        {
            var x = Random.Range(0f, 1f);
            var y = Random.Range(0f, 1f);
            var pos = new Vector3(
                Mathf.Lerp(areaCOL.bounds.min.x, areaCOL.bounds.max.x, x),
                Mathf.Lerp(areaCOL.bounds.min.y, areaCOL.bounds.max.y, y));

            return pos;
        }
        private AudioClip alphabetClipOf(string alphabet)
        {
            if (alphabetClipDict.TryGetValue(alphabet.ToLower(), out var clip))
                return clip;
            else return null;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Planet[] planetPB = null;
        [SerializeField] private Transform planetParetTR = null;
        [SerializeField] private BoxCollider2D[] spawnCOL = null;
        [Header("★ Config")]
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private float overlapRadius = 1;
        [SerializeField] private int overlapMaxAttempt = 5;
        [Header("★ Sound")]
        [SerializeField] private AudioClip[] alphabetCLIP = null;

        // Unity Messages
        private void Awake()
        {
            Util.RemoveAllChildren(planetParetTR);

            for (var i = 0; i < alphabetCLIP.Length; i++)
            {
                var alphabet = ((char)('a' + i)).ToString();
                alphabetClipDict[alphabet] = alphabetCLIP[i];
            }
        }
        private void Start()
        {

        }
    }
}