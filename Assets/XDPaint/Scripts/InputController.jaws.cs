using System.Collections.Generic;
using UnityEngine;


// devBOX(swon) - 코드 리뷰하셔요.
// InputController의 ignoreForRaycasts를 동적으로 제어하기 위한 코드 추가
// 원래 클래스에 partial 키워드 추가 필요
namespace XDPaint.Controllers
{
    public partial class InputController
    {
        public void AddIgnoreForRaycasts(GameObject go)
        {
            var list = new List<GameObject>(ignoreForRaycasts);
            list.Add(go);

            ignoreForRaycasts = list.ToArray();
        }
        public void RemoveIgnoreForRaycasts(GameObject go)
        {
            var list = new List<GameObject>(ignoreForRaycasts);
            list.Remove(go);

            ignoreForRaycasts = list.ToArray();
        }
        public void EnableInteraction(bool enable)
        {
            interactable = enable;
        }
    }
}