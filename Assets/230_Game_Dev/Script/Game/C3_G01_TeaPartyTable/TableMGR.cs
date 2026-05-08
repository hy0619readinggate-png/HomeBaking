using UnityEngine;
using beyondi.Util;
using System.Collections;
using DoDoEng.Common;
using Random = UnityEngine.Random;
using UnityEngine.UI;

namespace DoDoEng.Game.C3_G01
{

    public enum ObstacleType
    {
        AppleRed_Normal, AppleRed_Chocolate, AppleRed_Caramel,
        AppleGreen_Normal, AppleGreen_Chocolate, AppleGreen_Caramel,

        Macaron_Pink, Macaron_Yellow, Macaron_Mint, Macaron_Brown, Macaron_Orange, Macaron_Green,

        None = 999
    }


    public class TableMGR : MonoBehaviour
    {

        // Definition
        public enum TableObjectType { Alphabet, Objects, Broomstick }


        // Properties
        public bool Completed => runCoroutine == null;


        // Methods
        public void Setup(ProblemData pData)
        {
            wordDatas = pData.WordDatas;

            spawnObjectCount = pData.ObjectCountMAX;
            speed = pData.Speed;
            interval = pData.Interval;

            broomMinCount = pData.BroomMinCount;

            alphabetPF = pData.Alphabet;
            broomPF = pData.Broom;
            obstaclesPF = pData.Obstacles;
        }
        public void Ready(int wNO)
        {
            completed = false;

            wordData = wordDatas[wNO - 1];

            letterOrders = C3_G01_Main.Instance.GetLetterOrderOfWord(wNO);
            letters = C3_G01_Main.Instance.GetLetters(wNO);

            Debug.Log(letterOrders);
            broomAvailableCount = C3_G01_Main.Instance.GetBroomAvailableCount(wNO);
            broomUseCount = 0;



            var wordLetters = wordData.Word.ToCharArray();
            for (int i = 0; i < wordLetters.Length; i++)
                _LetterUI[i].Setup(letterOrders.Contain(i), wordLetters[i].ToString());

            if (textAutoArray != null)
                textAutoArray.CheckText(wordLetters);
        }
        public void Run()
        {
            Debug.Log("COIN : TableMGR | Run()");
            runCoroutine = StartCoroutine(coRun());
            spawnCoroutine = StartCoroutine(coSpawn());
        }
        public Coroutine Clear()
        {
            completed = true;

            if (runCoroutine != null) StopCoroutine(runCoroutine);
            if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);

            return clearCoroutine = StartCoroutine(coClear());
        }
        public void Stop()
        {
            completed = true;

            if (runCoroutine != null) StopCoroutine(runCoroutine);
            if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
            if (clearCoroutine != null) StopCoroutine(clearCoroutine);

            runCoroutine = null;
            spawnCoroutine = null;
            clearCoroutine = null;


            foreach (var spawnTR in _SpawnsTR)
            {
                var objects = spawnTR.GetComponentsInChildren<TableObject>(true);
                foreach (var obj in objects)
                    obj.Clear();
            }

            var wordLetters = wordData.Word.ToCharArray();
            for (int i = 0; i < wordLetters.Length; i++)
                _LetterUI[i].gameObject.SetActive(false);

            _TableMoveANI.speed = 0f;
        }
        public void Halt(bool halt)
        {
            if (halt)
            {
                if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);

                foreach (var spawnTR in _SpawnsTR)
                {
                    var objects = spawnTR.GetComponentsInChildren<TableObject>(true);
                    foreach (var obj in objects)
                        obj.Halt(true);
                }

                _TableMoveANI.speed = 0f;
            }
            else
            {
                if (!completed)
                    spawnCoroutine = StartCoroutine(coSpawn());
            }
        }
        public void Correct()
        {
            _Feedback.Correct();
            completed = true;

            playLetterSound();
        }
        public void Bonus()
        {
            main.UpdateBonusHP();
            _Feedback.Correct();
        }
        public void Wrong()
        {
            AudioMGR.One.PlayNarration(wordData.SoundClip);

            main.UpdateHP();
            _Feedback.Wrong();
        }
        public void Broom()
        {
            broomUseCount++;
            _BroomEffectGO.SetActive(true);
            AudioMGR.One.PlayEffect(_BroomClip);
            //broom = null;

            foreach (var spawnTR in _SpawnsTR)
            {
                var objects = spawnTR.GetComponentsInChildren<TableObject>(true);
                foreach (var obj in objects)
                {
                    // 정답만 남겨둠..
                    if (CheckAnswer(obj.Letter) == false)
                        obj.Clear();
                }
            }

        }
        public void HaltForBroom(bool halt)
        {
            if (halt)
            {
                foreach (var spawnTR in _SpawnsTR)
                {
                    var objects = spawnTR.GetComponentsInChildren<TableObject>(true);
                    foreach (var obj in objects)
                        obj.Halt(true);
                }

                _TableMoveANI.speed = 0f;
            }
            else
            {
                foreach (var spawnTR in _SpawnsTR)
                {
                    var objects = spawnTR.GetComponentsInChildren<TableObject>(true);
                    foreach (var obj in objects)
                        obj.Halt(false, speed);
                }
                _TableMoveANI.speed = speed;
            }
        }
        public bool CheckAnswer(char? value)
        {
            var answer = getAnswerText();

            return answer.Equals(value);
        }



        // Fields : cachings
        private TextAutoArray textAutoArray_ = null;
        private TextAutoArray textAutoArray => textAutoArray_ ??= GetComponent<TextAutoArray>();


        // Fields
        private int spawnObjectCount = 0;
        private float speed = 0f;
        private float interval = 0f;
        private int currentLetterOrder = 0;
        private int letterIndex = 0;
        private int broomMinCount = 0;

        private GameObject alphabetPF = null;
        private GameObject broomPF = null;
        private GameObject[] obstaclesPF = null;

        private WordData[] wordDatas = null;
        private WordData wordData = null;

        private int[] letterOrders = null;
        private char[] letters = null;

        private bool completed = false;
        private Coroutine runCoroutine = null;
        private Coroutine spawnCoroutine = null;
        private Coroutine clearCoroutine = null;

        private int broomAvailableCount = 0;
        private int broomUseCount = 0;
        private TableObject broom = null;

        // Fields : caching
        private C3_G01_Main main_ = null;
        private C3_G01_Main main => main_ ??= FindAnyObjectByType<C3_G01_Main>();
        private C3_G01_ProblemMGR pMGR_ = null;
        private C3_G01_ProblemMGR pMGR => pMGR_ ??= FindObjectOfType<C3_G01_ProblemMGR>();

        private readonly int hashkey_R = Animator.StringToHash("R");
        private readonly int hashkey_O = Animator.StringToHash("O");
        private readonly int hashkey_Y = Animator.StringToHash("Y");
        private readonly int hashkey_BY = Animator.StringToHash("BY");
        private readonly int hashkey_P = Animator.StringToHash("P");
        private readonly int hashkey_N = Animator.StringToHash("N");
        private readonly int hashkey_G = Animator.StringToHash("G");



        // Functions
        private TableObject createObject(GameObject target, Transform parent)
        {
            var go = Instantiate(target, parent);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;

            return go.GetComponent<TableObject>();
        }
        private int getTableObjectCreatedCount()
        {
            int count = 0;

            foreach (var spawnTR in _SpawnsTR)
            {
                var objects = spawnTR.GetComponentsInChildren<TableObject>(true);
                foreach (var obj in objects)
                {
                    if (obj.IsBroom == false)
                        count++;
                }
            }

            return count;
        }
        private char getAnswerText()
        {
            return wordData.Word[currentLetterOrder];
        }
        private void playLetterSound()
        {
            switch (getAnswerText())
            {
                case 'a': AudioMGR.One.PlayNarration(_AlphabetClip[0]); break;
                case 'b': AudioMGR.One.PlayNarration(_AlphabetClip[1]); break;
                case 'c': AudioMGR.One.PlayNarration(_AlphabetClip[2]); break;
                case 'd': AudioMGR.One.PlayNarration(_AlphabetClip[3]); break;
                case 'e': AudioMGR.One.PlayNarration(_AlphabetClip[4]); break;
                case 'f': AudioMGR.One.PlayNarration(_AlphabetClip[5]); break;
                case 'g': AudioMGR.One.PlayNarration(_AlphabetClip[6]); break;
                case 'h': AudioMGR.One.PlayNarration(_AlphabetClip[7]); break;
                case 'i': AudioMGR.One.PlayNarration(_AlphabetClip[8]); break;
                case 'j': AudioMGR.One.PlayNarration(_AlphabetClip[9]); break;
                case 'k': AudioMGR.One.PlayNarration(_AlphabetClip[10]); break;
                case 'l': AudioMGR.One.PlayNarration(_AlphabetClip[11]); break;
                case 'm': AudioMGR.One.PlayNarration(_AlphabetClip[12]); break;
                case 'n': AudioMGR.One.PlayNarration(_AlphabetClip[13]); break;
                case 'o': AudioMGR.One.PlayNarration(_AlphabetClip[14]); break;
                case 'p': AudioMGR.One.PlayNarration(_AlphabetClip[15]); break;
                case 'q': AudioMGR.One.PlayNarration(_AlphabetClip[16]); break;
                case 'r': AudioMGR.One.PlayNarration(_AlphabetClip[17]); break;
                case 's': AudioMGR.One.PlayNarration(_AlphabetClip[18]); break;
                case 't': AudioMGR.One.PlayNarration(_AlphabetClip[19]); break;
                case 'u': AudioMGR.One.PlayNarration(_AlphabetClip[20]); break;
                case 'v': AudioMGR.One.PlayNarration(_AlphabetClip[21]); break;
                case 'w': AudioMGR.One.PlayNarration(_AlphabetClip[22]); break;
                case 'x': AudioMGR.One.PlayNarration(_AlphabetClip[23]); break;
                case 'y': AudioMGR.One.PlayNarration(_AlphabetClip[24]); break;
                case 'z': AudioMGR.One.PlayNarration(_AlphabetClip[25]); break;
            }
        }
        private void stopTable()
        {
            if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);

            spawnCoroutine = null;

            foreach (var spawnTR in _SpawnsTR)
            {
                var objects = spawnTR.GetComponentsInChildren<TableObject>(true);
                foreach (var obj in objects)
                    obj.Clear();
            }

            _TableMoveANI.speed = 0f;
        }

        // Event Handlers
        private void onClick_BoardBTN()
        {
            AudioMGR.One.PlayNarration(wordData.SoundClip);
        }


        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TeaCupFeedback _Feedback = null;
        [SerializeField] private Animator _TableMoveANI = null;
        [Space()]
        [SerializeField] private Transform[] _SpawnsTR = null;
        [SerializeField] private LetterUI[] _LetterUI = null;
        [SerializeField] private GameObject _BroomEffectGO = null;
        [SerializeField] private Animator _ProblemBoardANI = null;
        [SerializeField] private Button _ProblemBoardBTN = null;
        [Space()]
        [SerializeField] private AudioClip _BroomClip = null;
        [SerializeField] private AudioClip _QuestClip = null;
        [SerializeField] private AudioClip[] _AlphabetClip = null;

        // Unity Messages
        private void Awake()
        {
            _ProblemBoardBTN.onClick.AddListener(onClick_BoardBTN);
        }
        private void Start()
        {
            _TableMoveANI.speed = 0f;
            _ProblemBoardBTN.interactable = false;
            _BroomEffectGO.SetActive(false);
        }

        // Unity Coroutine
        IEnumerator coRun()
        {
            AudioMGR.One.PlayEffect(_QuestClip);
            _ProblemBoardBTN.interactable = true;
            _TableMoveANI.speed = speed;

            var wordLetters = wordData.Word.ToCharArray();

            for (int i = 0; i < wordLetters.Length; i++)
                _LetterUI[i].gameObject.SetActive(true);

            AudioMGR.One.PlayNarration(wordData.SoundClip);
            yield return null;

            for (int i = 0; i < letterOrders.Length; i++)
            {
                currentLetterOrder = letterOrders[i];
                _LetterUI[currentLetterOrder].Glow();
                yield return null;

                completed = false;
                yield return new WaitUntil(() => completed);

                _LetterUI[currentLetterOrder].Correct();
                yield return null;
            }

            stopTable();
            yield return new WaitForSeconds(1.5f);

            AudioMGR.One.PlayNarration(wordData.SoundClip);
            yield return new WaitForSeconds(wordData.SoundClip.length);

            _ProblemBoardANI.SetTrigger("Success");
            yield return null;

            _ProblemBoardBTN.interactable = false;
            runCoroutine = null;
            yield return null;
        }
        IEnumerator coSpawn()
        {
            // Table Rail, Object 점점 빨라지게
            float curSpeed = 0;
            while (curSpeed <= speed)
            {
                curSpeed += Time.deltaTime;
                _TableMoveANI.speed = curSpeed;

                foreach (var spawnTR in _SpawnsTR)
                {
                    var objects = spawnTR.GetComponentsInChildren<TableObject>(true);
                    foreach (var obj in objects)
                        obj.Halt(false, curSpeed);
                }
                yield return null;
            }

            while (true)
            {
                // Ready
                if (letterIndex >= letters.Length)
                    letterIndex = 0;

                var spawnMaxCount = Random.Range(1, spawnObjectCount + 1);
                var spawnPoints = UtilArray.Extract(_SpawnsTR, spawnMaxCount);
                yield return null;


                for (int i = 0; i < spawnMaxCount; i++)
                {
                    var spawnPoint = spawnPoints[i];
                    TableObject obstacle = null;
                    char? letter = null;

                    if (i == 0)
                    {
                        // 알파벳은 오브젝트 1개는무조건 생성
                        obstacle = createObject(alphabetPF, spawnPoint);

                        var ani = obstacle.GetComponent<TableAlphabet>();
                        var rnd = Random.Range(0, 3);


                        if (pMGR.PNO == 1)
                        {
                            switch (rnd)
                            {
                                case 0: ani.ChangeColor(hashkey_R); break;
                                case 1: ani.ChangeColor(hashkey_P); break;
                                case 2: ani.ChangeColor(hashkey_Y); break;
                            }
                        }
                        else if (pMGR.PNO == 2)
                        {
                            switch (rnd)
                            {
                                case 0: ani.ChangeColor(hashkey_G); break;
                                case 1: ani.ChangeColor(hashkey_N); break;
                                case 2: ani.ChangeColor(hashkey_O); break;
                            }
                        }
                        else if (pMGR.PNO == 3)
                        {
                            switch (rnd)
                            {
                                case 0: ani.ChangeColor(hashkey_BY); break;
                                case 1: ani.ChangeColor(hashkey_N); break;
                                case 2: ani.ChangeColor(hashkey_G); break;
                            }
                        }

                        letter = letters[letterIndex];
                    }
                    else
                    {
                        // 나머지 장애물 오브젝트 생성

                        var obstacleIndex = Random.Range(0, obstaclesPF.Length);
                        obstacle = createObject(obstaclesPF[obstacleIndex], spawnPoint);
                    }


                    if (obstacle != null)
                    {
                        obstacle.Setup(speed, letter);
                        obstacle.Move();
                    }

                    yield return null;
                    //letterIndex++;
                    //
                }



                // Broom
                float broomCreatedTime = 0f;
                if (broom == null && broomUseCount < broomAvailableCount)
                {
                    var countCreated = getTableObjectCreatedCount();
                    if (countCreated >= broomMinCount)
                    {
                        broomCreatedTime = 1f;
                        _BroomEffectGO.SetActive(false);

                        var spawnIndex = Random.Range(0, _SpawnsTR.Length);

                        yield return new WaitForSeconds(1f);

                        broom = createObject(broomPF, _SpawnsTR[spawnIndex]);
                        broom.Setup(speed, null);
                        broom.Move();
                    }
                }


                // Wait
                yield return new WaitForSeconds(interval - broomCreatedTime);

                letterIndex++;
                yield return null;
            }
        }
        IEnumerator coClear()
        {
            yield return new WaitForSeconds(1f);

            foreach (var spawnTR in _SpawnsTR)
            {
                var objects = spawnTR.GetComponentsInChildren<TableObject>(true);
                foreach (var obj in objects)
                {
                    obj.Clear();
                    yield return new WaitForSeconds(0.1f);
                }
            }

            // UI
            var wordLetters = wordData.Word.ToCharArray();
            for (int i = 0; i < wordLetters.Length; i++)
                _LetterUI[i].gameObject.SetActive(false);
            yield return null;
        }
    }
}
