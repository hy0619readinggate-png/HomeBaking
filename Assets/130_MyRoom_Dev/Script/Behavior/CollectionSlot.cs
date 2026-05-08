using beyondi.Util;
using System.Linq;
using System.Collections;
using DoDoEng.Common;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

// devBOX - 삭제해야 함
#pragma warning disable 0414

namespace DoDoEng.MyRoom.Behavior
{
	public class CollectionSlot : MonoBehaviour
	{
		// Definitions
		// Properties
		public int Index => index;
		public MyRoomPet Pet => pet;
		public bool Checked => gameObject.activeSelf && checkGO.activeSelf;

        // Methods
        public void Activate(bool active)
        {
            LOG.Function(this, $"{active}");

            gameObject.SetActive(active);
        }
        public void Init(int idxPet, int level, bool isPetBook = false, bool isSilhouette = false)
		{
            LOG.Info($"Init({idxPet}, {level})", this);

            checkGO.SetActive(false);
            newGO.SetActive(false);

			for (int i = 0; i < pedestals.Length; i++)
			{
				pedestals[i].SetActive(Mathf.Floor(idxPet / 4) == i);
            }

			levelGO.SetActive(!isPetBook);
			nameTMP.gameObject.SetActive(!isPetBook);

            for (int i = 0; i < silhouettes.Length; i++)
            {
				silhouettes[i].SetActive(false);
            }

			if (isSilhouette)
			{
				pet.Activate(false);
                silhouettes[idxPet * 3 + 3 - level].SetActive(true);
            }
			else
			{
                pet.Activate(true);
                pet.Init(idxPet, level, false, isPetBook);
			}
        }
		public void Init(int index = -1, bool idle = false, bool idleHalf = false)
		{
			LOG.Info($"Init({index})", this);

            checkGO.SetActive(false);
            newGO.SetActive(false);

            this.index = index;

			if (0 <= index && index < UserData.One.Pets.Count)
			{
				var data = UserData.One.Pets[index];
                nameTMP.text = data.Name;

                if (data.Level < 4)
                {
                    levelTMP.text = data.Level.ToString();
                    levelSlider.value = data.Affection - (float)data.Level + 1.0f;
                    maxGO.SetActive(false);
                }
                else
                {
                    levelTMP.text = "3";
                    levelSlider.value = 1.0f;
                    maxGO.SetActive(true);
                }
                levelSlider.gameObject.SetActive(true);
                pet.Init(data, idle, idleHalf);
                newGO.SetActive(data.New);
            }
			else
			{
                nameTMP.text = string.Empty;
                levelTMP.text = "1";
                levelSlider.value = 0;
                levelSlider.gameObject.SetActive(false);
                pet.gameObject.SetActive(false);
                pet.Init(null);
            }	
        }
		public void Check(bool check)
		{
			checkGO.SetActive(check);
		}

        // Events
        public event System.Action<CollectionSlot> OnClick;



        // Fields : caching

        // Fields
        private int index;

		// Functions
		// Event Handlers
		// Overrides



		// Unity Inspectors
		[Header("★ Bindings")]
		[SerializeField] private TMP_Text nameTMP = null;
		[SerializeField] private GameObject checkGO = null;
		[SerializeField] private GameObject newGO = null;
		[SerializeField] private TMP_Text levelTMP = null;
		[SerializeField] private Slider levelSlider = null;
		[SerializeField] private MyRoomPet pet = null;
        [SerializeField] private GameObject levelGO = null;
        [SerializeField] private GameObject[] silhouettes = null;
        [SerializeField] private GameObject[] pedestals = null;
        [SerializeField] private GameObject silhouetteGO = null;
        [SerializeField] private GameObject maxGO = null;

        // Unity Messages
        private void Awake()
		{
            silhouetteGO.SetActive(true);
            GetComponent<Button>().onClick.AddListener(() => OnClick?.Invoke(this));
            silhouettes.ForEach(silhouette => silhouette.SetActive(false));
        }
		private void Start()
		{
		   
		}

		// Unity Coroutine
	}
}