using beyondi.Util;
using DoDoEng.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DoDoEng.Activity.C1_A06
{
    public class Ride : MonoBehaviour
    {
        // Properties
        public int CurrentCharacterID => character.CharacterID;
        public Seat AnswerSeat => problemSeats.Where(s => !s.IsOccupied).Single(s => s.IsAnswer);

        // Methods
        public void Initialize(bool isRoller)
        {
            LOG.Info($"Initialize() | {isRoller}", this);

            var ids = UtilArray.Random(4, 6);
            foreach (var (seat, i) in extraSeats.Select((s, i) => (s, i)))
                seat.Occupy(ids[i]);
            problemSeats.ForEach(s => s.Initialize(isRoller));

            characterIDQueue = new Queue<int>(UtilArray.Random(1, 3));
            var id = characterIDQueue.Dequeue();
            character.Setup(id);
        }
        public void Setup(ProblemData problemData)
        {
            LOG.Info($"Setup() | {problemData}", this);

            var emptySeats = problemSeats.Where(s => !s.IsOccupied);
            foreach (var (seat, i) in emptySeats.Select((s, i) => (s, i)))
            {
                seat.Setup(problemData.Examples[i]);
                seat.Balloon.Setup(problemData.Examples[i]);
            }

            LOG.Important($"--------------, {problemSeats.Length}", this);
            var answerIDX = Array.FindIndex(problemSeats, e => e.IsAnswer && !e.IsOccupied);
            aff.Setup(answerIDX);
        }
        public void NextCharacter()
        {
            LOG.Info($"NextCharacter()", this);

            var id = characterIDQueue.Dequeue();
            character.Setup(id);
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            character.EnableInteraction(enable);
            balloons.ForEach(b => b.EnableInteraction(enable));
        }
        public void ShowBalloons()
        {
            LOG.Info($"ShowBalloons()", this);

            problemSeats
                .Where(s => !s.IsOccupied)
                .ForEach(s => s.Balloon.Show());
        }
        public void HideAllBalloon()
        {
            LOG.Info($"HideAllBalloon()", this);

            problemSeats
                .Where(s => !s.IsOccupied)
                .ForEach(s => s.Balloon.Hide());
        }
        public void HideWrongBalloon()
        {
            LOG.Info($"HideWrongBalloon()", this);

            problemSeats
                .Where(s => !s.IsOccupied)
                .Where(s => !s.IsAnswer)
                .ForEach(s => s.Balloon.Hide());
        }

        // Methods
        public void SitDownCharacter(Seat seat)
        {
            LOG.Info($"SitDownCharacter()", this);

            character.gameObject.SetActive(false);

            seat.Occupy(character.CharacterID);
            correctVfxGO.transform.position = seat.CenterPosition;
            correctVfxGO.SetActive(true);
        }
        public void CheerUp(Seat seat)
        {
            LOG.Info($"CheerUp()", this);

            seats.ForEach(s => s.CheerUp(seat == s));
        }
        public void Idle()
        {
            LOG.Info($"Idle()", this);

            seats.ForEach(s => s.Idle());
        }
        public void Sad()
        {
            LOG.Info($"Sad()", this);

            seats.ForEach(s => s.Sad());
        }

        // Methods
        public Seat[] GetExamSeats()
        {
            LOG.Info($"GetExamSeats()", this);

            return seats.Where(s => !s.IsOccupied).ToArray();
        }
        public int[] GetCharacterIDs()
        {
            return seats.Select(seat => seat.CharacterID).ToArray();
        }




        // Fields : cachinge
        private Seat[] seats_ = null;
        private Seat[] seats => seats_ ??= GetComponentsInChildren<Seat>(true);

        // Fields
        private Queue<int> characterIDQueue = null;
        private Seat[] problemSeats = null;
        private Seat[] extraSeats = null;

        // Event Handlers
        private void character_OnBeginDrag()
        {
            LOG.Function(this);

            balloons.ForEach(b => b.EnableInteraction(false));
        }
        private void character_OnEndDrag()
        {
            balloons.ForEach(b => b.EnableInteraction(true));
        }




        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Balloon[] balloons = null;
        [SerializeField] private Character character = null;
        [SerializeField] private GameObject correctVfxGO = null;
        [SerializeField] private Affordance aff = null;

        // Unity Messages
        private void Awake()
        {
            problemSeats = seats.Where(s => s.IsProblemSeat).ToArray();
            extraSeats = seats.Where(s => !s.IsProblemSeat).ToArray();

            foreach (var (seat, i) in problemSeats.Select((s, i) => (s, i)))
                seat.Balloon = balloons[i];

            correctVfxGO.SetActive(false);
            aff.gameObject.SetActive(true);
        }
        private void Start()
        {
        }
        private void OnEnable()
        {
            character.OnBeginDrag += character_OnBeginDrag;
            character.OnEndDrag += character_OnEndDrag;
        }
        private void OnDisable()
        {
            character.OnBeginDrag -= character_OnBeginDrag;
            character.OnEndDrag -= character_OnEndDrag;
        }
    }
}