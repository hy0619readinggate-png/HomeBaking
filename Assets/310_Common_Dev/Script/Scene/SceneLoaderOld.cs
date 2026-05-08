using beyondi.Behaviour;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DoDoEng.Activity.Framework;
using Spine;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DoDoEng.Common
{
    public class SceneLoaderOld : BYDSingleton<SceneLoaderOld>
    {
        // Methods
        public void LoadScene(string sceneName)
        {
            LOG.Info($"LoadScene() | {sceneName}", this);

            evaluateTimeline(introTL);
            StartCoroutine(coLoadScene(sceneName));
        }



        // Functions
        private void evaluateTimeline(PlayableDirector timeline, double time = 0)
        {
            timeline.time = time;
            timeline.Evaluate();
            timeline.Stop();
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Slider progressSLD = null;
        [SerializeField] private Animator anim = null;
        [SerializeField] private TMP_Text progressTMP = null;
        [SerializeField] private PlayableDirector introTL = null;
        [SerializeField] private PlayableDirector outroTL = null;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            progressSLD.gameObject.SetActive(false);
        }
        private void Start()
        {

        }
        private void Update()
        {
            if (progressSLD.gameObject.activeSelf)
            {
                progressTMP.text = $"{Math.Floor(progressSLD.value * 100)}%";
            }
        }

        // Unity Coroutine
        IEnumerator coLoadScene(string sceneName)
        {
            LOG.Info($"crLoadScene() | {sceneName}", this);

            // 화면 가리기
            evaluateTimeline(introTL);
            progressSLD.value = 0;
            progressSLD.gameObject.SetActive(true);
            anim.SetTrigger("Show");
            yield return new WaitForSecondsRealtime(1f);

            yield return coPlayTimeline(introTL);

            // 씬 로딩
            var op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false;

            var time = 0f;
            while (op.progress < 0.9f)
            {
                yield return null;
                time += Time.deltaTime;

                progressSLD.value = Mathf.Lerp(progressSLD.value, op.progress, time);
                if (progressSLD.value >= op.progress)
                    time = 0f;

                LOG.Info($"progressSLD.value | {progressSLD.value}", this);
            }

            // 씬 로딩 완료
            op.allowSceneActivation = true;
            yield return op;

            // Pause되었다면, Resume
            AudioMGR.One.StopAll();
            SystemManager.Resume();

            // 진행바 정리
            yield return progressSLD.DOValue(1, 0.3f).WaitForCompletion();
            yield return null;

            // 모듈 실행 준비 (데이터 로드등)
            var runner = FindObjectOfType<SceneRunnerBase>();
            if (runner == null)
            {
                LOG.Important($"No SceneRunnerBase instance. retry after 0.5s", this);
                yield return new WaitForSeconds(0.5f);

                runner = FindObjectOfType<SceneRunnerBase>();
                if (runner == null)
                    LOG.Important($"No SceneRunnerBase instance. SceneRunner is not running.", this);
            }

            if (runner != null)
            {
                Exception exception = null;
                yield return runner.Prepare().ToCoroutine((ex) => exception = ex);
                if (exception != null)
                {
                    LOG.Error($"{exception.Message}", this);
                    LOG.Error($"Cannot run any more because of an error!!!!", this);

                    yield return SystemUI.One.ErrorPU.ShowPopup(exception.Message).ToCoroutine();
                    yield return coErrorReturnScene(RunnerParam.ReturnScene);
                    yield break;
                }
            }


            // 화면 보이기
            yield return coPlayTimeline(outroTL);
            yield return null;

            anim.SetTrigger("Hide");
            yield return new WaitForSeconds(1f);
            outroTL.Stop();

            // 모듈 실행
            if (runner != null)
                runner.Run();
        }
        IEnumerator coErrorReturnScene(string sceneName)
        {
            // 화면 가리기
            progressSLD.value = 0;
            progressSLD.gameObject.SetActive(true);

            // 씬 로딩
            var op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false;

            var time = 0f;
            while (op.progress < 0.9f)
            {
                yield return null;
                time += Time.deltaTime;

                progressSLD.value = Mathf.Lerp(progressSLD.value, op.progress, time);
                if (progressSLD.value >= op.progress)
                    time = 0f;

                LOG.Info($"progressSLD.value | {progressSLD.value}", this);
            }

            // 씬 로딩 완료
            op.allowSceneActivation = true;
            yield return null;

            // 진행바 정리
            yield return progressSLD.DOValue(1, 0.3f).WaitForCompletion();
            progressSLD.gameObject.SetActive(false);
            yield return null;

            // 화면 보이기
            anim.SetTrigger("Hide");
            yield return new WaitForSeconds(1f);
        }
        IEnumerator coPlayTimeline(PlayableDirector timeline, float delay = 0.3f)
        {
            timeline.time = 0;
            timeline.Play();
            yield return new WaitForSecondsRealtime((float)timeline.duration);
            yield return new WaitForSecondsRealtime(delay);
        }
    }
}