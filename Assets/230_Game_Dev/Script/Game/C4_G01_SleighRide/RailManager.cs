using System.Collections.Generic;
using UnityEngine;

namespace DoDoEng.Game.C4_G01
{
    public class RailManager : MonoBehaviour
    {
        GameObject[] stars;
        GameObject[] boosters;
        GameObject[] sprinklers;

        void Awake()
        {
            Transform items = transform.Find("items");
            List<GameObject> lA = new();
            List<GameObject> lB = new();
            List<GameObject> lC = new();
            foreach (Transform item in items)
            {
                if (!item.gameObject.activeSelf) continue;
                switch (item.tag)
                {
                    case "ItemStar":
                        lA.Add(item.gameObject);
                        break;
                    case "ItemBooster":
                        lB.Add(item.gameObject);
                        break;
                    case "ItemSprinkler":
                        lC.Add(item.gameObject);
                        break;
                }
            }
            stars = lA.ToArray();
            boosters = lB.ToArray();
            sprinklers = lC.ToArray();
            //Debug.Log(stars.Length);

            foreach (GameObject star in stars) star.SetActive(false);
            foreach (GameObject booster in boosters) booster.SetActive(false);
            foreach (GameObject sprinkler in sprinklers) sprinkler.SetActive(false);
        }

        public bool SetItemStar(int n = 0)
        {
            if (C4_G01_Main.Instance.IsRoundComplete(1)) return false;
            if (stars.Length < 10) return false;
            switch (n)
            {
                case 1:
                    stars[0].SetActive(true);
                    stars[1].SetActive(true);
                    stars[2].SetActive(false);
                    stars[3].SetActive(true);
                    stars[4].SetActive(true);
                    boosters[0].SetActive(true);
                    // -----------------------
                    stars[5].SetActive(false);
                    stars[6].SetActive(false);
                    stars[7].SetActive(true);
                    stars[8].SetActive(false);
                    stars[9].SetActive(false);
                    boosters[1].SetActive(false);
                    return true;
                case 2:
                    stars[0].SetActive(true);
                    stars[1].SetActive(false);
                    stars[2].SetActive(true);
                    stars[3].SetActive(true);
                    stars[4].SetActive(false);
                    boosters[0].SetActive(false);
                    // -----------------------
                    stars[5].SetActive(false);
                    stars[6].SetActive(true);
                    stars[7].SetActive(false);
                    stars[8].SetActive(false);
                    stars[9].SetActive(true);
                    boosters[1].SetActive(true);
                    return true;
                case 3:
                    stars[0].SetActive(true);
                    stars[1].SetActive(false);
                    stars[2].SetActive(true);
                    stars[3].SetActive(false);
                    stars[4].SetActive(true);
                    boosters[0].SetActive(true);
                    // -----------------------
                    stars[5].SetActive(false);
                    stars[6].SetActive(true);
                    stars[7].SetActive(false);
                    stars[8].SetActive(true);
                    stars[9].SetActive(false);
                    boosters[1].SetActive(false);
                    return true;
            }
            return false;
        }

        public static int COUNTER_SPRINKLER = 0;
        public void SetSprinkler()
        {
            if (C4_G01_Main.Instance.IsRoundComplete(1)) return;
            if (C4_G01_Main.Instance.currentRound == 2) return;
            if (sprinklers.Length == 0)
            {
                COUNTER_SPRINKLER++;
                return;
            }
            foreach (GameObject sprinkler in sprinklers)
            {
                if (++COUNTER_SPRINKLER > 6) break;
                if (COUNTER_SPRINKLER > 3)
                {
                    if (C4_G01_Main.Instance.currentRound == 3) sprinkler.SetActive(Random.Range(0, 5) != 0);
                    break;
                }
                sprinkler.SetActive(true);
            }
        }
    }
}