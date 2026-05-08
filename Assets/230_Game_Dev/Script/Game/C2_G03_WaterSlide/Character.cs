//using System;
using System.Collections;
using System.Collections.Generic;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Game.C2_G03
{
	public class Character : MonoBehaviour
	{
		[SerializeField] CharacterAni dodo;
		[SerializeField] GameObject booster;
		[SerializeField] GameObject step;
		[SerializeField] GameObject bgWaterPark;
		[SerializeField] GameObject bgWater;
		[SerializeField] GameObject bgWaterWave;
		[SerializeField] GameObject wave;

		/// <summary>
		/// Singletone instance setup
		/// </summary>
		static Character _instance = null;
		static bool _bExit = false;
		public static Character Instance
		{
			get
			{
				return _bExit ? null : _instance;
			}
		}
		void Awake()
		{
			_instance = this;
		}

		void OnApplicationQuit()
		{
			_bExit = true;
			_instance = null;
		}

		// ////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Running code
		/// </summary>

		Rigidbody2D body;
		float VX;

		void Start()
		{
			body = GetComponent<Rigidbody2D>();
			VX = Camera.main.ViewportToWorldPoint(Camera.main.WorldToViewportPoint(transform.position)).x;
			Idle();
			LoadSound();
		}

		// ////////////////////////////////////////////////////////////////////////////////////////
		[SerializeField] AudioClip[] audioClips;
		Dictionary<string, AudioClip> audioDic = new Dictionary<string, AudioClip>();
		void LoadSound()
		{
			foreach (AudioClip clip in audioClips)
			{
				string key = clip.name;
				if (!audioDic.ContainsKey(key))
				{
					audioDic.Add(key, clip);
				}
			}
		}

		public AudioClip GetAudioClip(string key)
		{
			return audioDic[key];
		}
		public void PlayEffect(string key)
		{
			AudioMGR.One.PlayEffect(GetAudioClip(key));
		}
		public void PlayEffectLoop(string key)
		{
			AudioMGR.One.PlayEffectLL(GetAudioClip(key), true);
		}
		public void StopEffectLoop()
		{
			AudioMGR.One.StopEffectLL(false, 0);
		}

		// ////////////////////////////////////////////////////////////////////////////////////////
		CharacterAnimation curAni = 0;
		public CharacterAnimation CurAni => curAni;

		void PlayAnimation(CharacterAnimation ani)
		{
			dodo.PlayAnimation(ani);
			curAni = ani;
		}
		void PlayAnimation(CharacterAnimation ani1, CharacterAnimation ani2)
		{
			dodo.PlayAnimation(ani1, ani2);
			curAni = ani2;
		}
		void PlayAnimationLoop(CharacterAnimation ani)
		{
			dodo.PlayAnimationLoop(ani);
			curAni = ani;
		}


		// ////////////////////////////////////////////////////////////////////////////////////////
		Coroutine motionCoroutine = null;
		void Idle()
		{
			StopBoosterSound();
			if (motionCoroutine != null) StopCoroutine(motionCoroutine); motionCoroutine = null;
			Debug.Log("★ Idle");
			PlayAnimationLoop(CharacterAnimation.Idle);
		}

		[SerializeField] float boosterTime = 3;
		public bool isBooster => booster.activeSelf;
		float runBoosterTime = 0;
		public void Boost()
		{
			if (motionCoroutine != null) StopCoroutine(motionCoroutine); motionCoroutine = null;
			switch (Random.Range(1, 3))
			{
				default:
				case 1:
					PlayEffect("dodo_booster1");
					break;
				case 2:
					PlayEffect("dodo_booster2");
					break;
			}
			PlayBoosterSound();
		}
		void PlayBoosterSound()
		{
			if (isBooster) return;
			booster.SetActive(true);
			PlayAnimationLoop(CharacterAnimation.Boost);
			PlayEffectLoop("booster");
			runBoosterTime = 0;
			bOverSpeed = true;
		}
		void StopBoosterSound()
		{
			if (!isBooster) return;
			booster.SetActive(false);
			StopEffectLoop();
			runBoosterTime = 0;
			Move();
			StartCoroutine(IEStopBooster());
		}

		IEnumerator IEStopBooster()
		{
			yield return new WaitForSeconds(0.5f);
			bOverSpeed = false;
		}

		void Crash()
		{
			if (isBooster)
			{
				PlayEffect("flower");
				return;
			}
			Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("UI"), LayerMask.NameToLayer("IgnorePhysics2DCollision"), false);
			StopBoosterSound();
			if (motionCoroutine != null) StopCoroutine(motionCoroutine); motionCoroutine = null;
			motionCoroutine = StartCoroutine(IECrash());
			//PlayEffect("car_crush");
			PlayEffect("hit");
		}
		IEnumerator IECrash()
		{
			Debug.Log("★ Crash");
			PlayAnimation(CharacterAnimation.Crash);
			yield return new WaitWhile(() => dodo.IsAnimationPlaying(CharacterAnimation.Crash));
			C2_G03_Main.Instance.Crash();
			Move();
		}

		Vector3 fallPoint;
		void Fall()
		{
			Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("UI"), LayerMask.NameToLayer("IgnorePhysics2DCollision"), false);
			fallPoint = transform.position;
			StopBoosterSound();
			if (motionCoroutine != null) StopCoroutine(motionCoroutine); motionCoroutine = null;
			motionCoroutine = StartCoroutine(IEFall());
			//PlayEffect("fall_down");
			PlayEffect("fall_away");
		}
		IEnumerator IEFall()
		{
			Debug.Log("★ Fall");
			PlayAnimationLoop(CharacterAnimation.Fall);
			yield return new WaitForSeconds(1.5f);
			C2_G03_Main.Instance.Fall();
			RollBack();
		}

		bool isJumping = false;
		void JumpUp()
		{
			Debug.Log("★ JumpUp");
			PlayAnimation(CharacterAnimation.JumpStart, CharacterAnimation.JumpUp);
			isJumping = true;
		}
		void JumpDown()
		{
			Debug.Log("★ JumpDown");
			Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("UI"), LayerMask.NameToLayer("IgnorePhysics2DCollision"), false);
			PlayAnimationLoop(CharacterAnimation.JumpDown);
			isJumping = true;
		}

		bool bTouchDown = false;
		bool bGameStart = false;
		public bool IsGameStart => bGameStart;
		void Move()
		{
			Debug.Log("★ Move");
			if (swatch != null)
			{
				swatch.Stop();
				Debug.Log($"DROP TIME = {swatch.ElapsedMilliseconds}ms");
				swatch = null;
			}
			bGameStart = true;
			if (motionCoroutine != null) StopCoroutine(motionCoroutine); motionCoroutine = null;
			if (curAni == CharacterAnimation.JumpDown)
			{
				bTouchDown = true;
				PlayAnimation(CharacterAnimation.JumpEnd, CharacterAnimation.Move);
			}
			else
			{
				PlayAnimationLoop(CharacterAnimation.Move);
			}
		}

		Coroutine _coroutineAudioAmbientMove;
		IEnumerator IEAudioAmbientMove()
		{
			Move();
			bool bMoveAmb = false;
			while (gameObject && gameObject.activeSelf)
			{
				yield return null;
				if (wave.activeSelf && dodo.IsAnimationPlaying(CharacterAnimation.Move))
				{
					if (!bMoveAmb)
					{
						if (isJumping) PlayEffect("step"); isJumping = false;
						C2_G03_Main.Instance.PlayAmbient(GetAudioClip("amb_boat_moves"));
						bMoveAmb = true;
					}
				}
				else
				{
					if (bMoveAmb)
					{
						C2_G03_Main.Instance.StopAmbient(GetAudioClip("amb_boat_moves"));
						bMoveAmb = false;
					}
				}
			}
		}
		public void PlayAudioAmbientMove()
		{
			if (_coroutineAudioAmbientMove != null) return;
			_coroutineAudioAmbientMove = StartCoroutine(IEAudioAmbientMove());
		}


		[HideInInspector] public List<RailManager> Sprinklers = new List<RailManager>();
		void ShowSprinkler(int skip)
		{
			for (int n = 0; n < Sprinklers.Count; ++n)
			{
				if (n < skip) continue;
				Sprinklers[n].SetSprinkler();
			}
		}

		RailManager ItemRail = null;
		public void SetItemRail(RailManager rail)
		{
			RailManager.COUNTER_SPRINKLER = 0;
			Sprinklers.Clear();
			ItemRail = rail;
		}
		void Correct(AudioClip audioClip)
		{
			Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("UI"), LayerMask.NameToLayer("IgnorePhysics2DCollision"), false);
			StopBoosterSound();
			if (motionCoroutine != null) StopCoroutine(motionCoroutine); motionCoroutine = null;
			motionCoroutine = StartCoroutine(IECorrect(audioClip));
			PlayEffect("correct");
			AudioMGR.One.PlayNarration(audioClip);
			if (ItemRail != null)
			{
				ItemRail.SetItemStar(Random.Range(1, 4));
				ShowSprinkler(1);
			}
		}
		IEnumerator IECorrect(AudioClip audioClip)
		{
			Debug.Log("★ Correct");
			
			//yield return AudioMGR.One.PlayNarrationAndWait(audioClip);
			switch (Random.Range(1, 4))
			{
				case 1:
					PlayEffect("dodo_correct1");
					PlayAnimation(CharacterAnimation.Correct);
					yield return new WaitWhile(() => dodo.IsAnimationPlaying(CharacterAnimation.Correct));
					break;
				case 2:
					PlayEffect("dodo_correct2");
					PlayAnimation(CharacterAnimation.Correct2);
					yield return new WaitWhile(() => dodo.IsAnimationPlaying(CharacterAnimation.Correct2));
					break;
				case 3:
					PlayEffect("dodo_correct3");
					PlayAnimation(CharacterAnimation.Correct3);
					yield return new WaitWhile(() => dodo.IsAnimationPlaying(CharacterAnimation.Correct3));
					break;
			}
			C2_G03_Main.Instance.Correct();
			Move();
		}

		void Wrong()
		{
			Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("UI"), LayerMask.NameToLayer("IgnorePhysics2DCollision"), false);
			StopBoosterSound();
			if (motionCoroutine != null) StopCoroutine(motionCoroutine); motionCoroutine = null;
			motionCoroutine = StartCoroutine(IEWrong());
			//PlayEffect("fail_pang");
			PlayEffect("incorrect_v2");
			if (ItemRail != null)
			{
				ShowSprinkler(0);
			}
		}
		IEnumerator IEWrong()
		{
			Debug.Log("★ Wrong");
			switch (Random.Range(1, 3))
			{
				default:
				case 1:
					PlayEffect("dodo_incorrect1");
					PlayAnimation(CharacterAnimation.Wrong);
					yield return new WaitWhile(() => dodo.IsAnimationPlaying(CharacterAnimation.Wrong));
					break;
				case 2:
					PlayEffect("dodo_incorrect2");
					PlayAnimation(CharacterAnimation.Wrong2);
					yield return new WaitWhile(() => dodo.IsAnimationPlaying(CharacterAnimation.Wrong2));
					break;
			}
			C2_G03_Main.Instance.Wrong();
			Move();
		}

		/// <summary>
		/// ///////////////////////////////////////////////////////////////////////////////////////
		/// </summary>
		[SerializeField] float runSpeed = 5.0f;
		float xSpeedScale => body.gravityScale / 2.0f;
		float maxSpeed => runSpeed * xSpeedScale;
		float curSpeed = 0;
		public float Speed => curSpeed;
		int nTouchGrounds = 0;
		bool bJump => nTouchGrounds <= 0;
		bool bOverSpeed = false;
		public bool IsOverSpeed => bOverSpeed;

		void StandUp()
		{
			transform.rotation = Quaternion.Euler(0, 0, 0);
		}

		void AngleLimit()
		{
			if (bJump || curAni == CharacterAnimation.Fall)
			{
				StandUp();
			}
			else
			{
				float z = transform.rotation.eulerAngles.z;
				z = z > 180 ? z - 360 : z;
				if (Mathf.Abs(z) > 30.0f)
				{
					body.angularVelocity = 0;
					z = (z < 0) ? -30 : +30;
					transform.rotation = Quaternion.Euler(0, 0, z);
				}
			}
		}

		float endSpeed = 0;
		float endVTime = 0;
		float endSTime = 0;
		bool isSendFSM = false;
		bool isEndMotion = false;
		bool isEndMotionComplete = false;
		public bool IsEndMotionComplete => isEndMotionComplete;
		public void StartEndMotion()
		{
			isEndMotion = true;
		}
		public void GameJustStop()
		{
			StartEndMotion();
			isSendFSM = true;
			isEndMotionComplete = true;
		}
		[SerializeField] GameObject FXFinale;
		IEnumerator IEGameEndMotion()
		{
			Debug.Log("★ IEGameEndMotion");
			yield return null;
			PlayEffect("confetti");
			FXFinale.SetActive(true);
			yield return new WaitForSeconds(0.5f);
			PlayEffect("dodo_finish");
			PlayAnimation(CharacterAnimation.Correct);
			yield return new WaitWhile(() => dodo.IsAnimationPlaying(CharacterAnimation.Correct));
			yield return new WaitForSeconds(1.0f);
			FXFinale.SetActive(false);
			isEndMotionComplete = true;
		}

		[HideInInspector] public bool IsAliveQuiz = false;
		[HideInInspector] public bool IsShowQuiz = false;
		void FixedUpdate()
		{
			Time.timeScale = 1;
			AngleLimit();
			wave.SetActive(!bJump);
			if (!C2_G03_Main.Instance.IsPlay) return;

			if (isEndMotion)
			{
				if (isSendFSM) return;

				endVTime += Time.deltaTime;
				if (endVTime > 1) endVTime = 1;
				float V = Mathf.Lerp(VX, 0, endVTime);
				endSTime += Time.deltaTime * 0.25f;
				if (endSTime > 1) endSTime = 1;
				float S = Mathf.Lerp(endSpeed, 0, endSTime);
				transform.Translate(S * Time.deltaTime, 0, 0);
				if (S > 0)
				{
					float dx = Camera.main.ViewportToWorldPoint(Camera.main.WorldToViewportPoint(transform.position)).x - V;
					step.transform.Translate(-dx, 0, 0);
					bgWaterPark.transform.Translate(dx / 3f, 0, 0);
					bgWater.transform.Translate(dx * 2f / 3f, 0, 0);
					bgWaterWave.transform.Translate(-dx * 2f, 0, 0);
				}
				else
				if (!isSendFSM)
				{
					isSendFSM = true;
					StartCoroutine(IEGameEndMotion());
				}

				return;
			}

			if (isBooster)
			{
				runBoosterTime += Time.deltaTime;
				if (runBoosterTime >= boosterTime) StopBoosterSound();
			}
			if (bRollBack || curAni == CharacterAnimation.Fall)
			{
				curSpeed = 0;
				return;
			}
			if (curAni == CharacterAnimation.Wrong) return;
			if (curAni == CharacterAnimation.Wrong2) return;
			if (curAni == CharacterAnimation.Crash) return;
			if (!bGameStart) Move();

			float speedLimit = maxSpeed;
			if (isBooster) speedLimit += maxSpeed * 2;
			if (!bJump && body.velocity.y < 0) speedLimit += maxSpeed * 0.5f;
			if (IsShowQuiz) Time.timeScale = 0.5f;
			if (curSpeed < speedLimit)
			{
				curSpeed += speedLimit * Time.deltaTime;
				if (curSpeed > speedLimit) curSpeed = speedLimit;
			}
			else if (curSpeed > speedLimit)
			{
				curSpeed -= curSpeed * Time.deltaTime * 2;
				if (curSpeed < speedLimit) curSpeed = speedLimit;
			}

			{
				endSpeed = Speed;
				transform.Translate(Speed * Time.deltaTime, 0, 0);
				float dx = Camera.main.ViewportToWorldPoint(Camera.main.WorldToViewportPoint(transform.position)).x - VX;
				step.transform.Translate(-dx, 0, 0);
				bgWaterPark.transform.Translate(dx / 3f, 0, 0);
				bgWater.transform.Translate(dx * 2f / 3f, 0, 0);
				bgWaterWave.transform.Translate(-dx * 2f, 0, 0);
			}

			if (bJump)
			{
				Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("UI"), LayerMask.NameToLayer("IgnorePhysics2DCollision"), IsAliveQuiz && body.velocity.y > 0);
				if (curAni == CharacterAnimation.JumpUp && body.velocity.y < 0) JumpDown();
			}
			else
			{
				if (curAni == CharacterAnimation.JumpDown || curAni == CharacterAnimation.JumpUp)
				{
					body.angularVelocity = 0;
					Move();
				}
			}
		}


		// ////////////////////////////////////////////////////////////////////////////////////////
		bool bRollBack = false;
		System.Diagnostics.Stopwatch swatch = null;
		public void RollBack()
		{
			bRollBack = true;
			transform.position = fallPoint;
			transform.rotation = Quaternion.Euler(0, 0, 0);
			body.velocity = Vector2.zero;
			body.angularVelocity = 0;

			float dx = transform.position.x - fallPoint.x;
			step.transform.Translate(-dx, 0, 0);
			bgWaterPark.transform.Translate(dx / 3f, 0, 0);
			bgWater.transform.Translate(dx * 2f / 3f, 0, 0);
			bgWaterWave.transform.Translate(-dx * 2f, 0, 0);

			dx = 6;
			transform.Translate(dx, 0, 0);
			transform.position = new Vector3(transform.position.x, 7, 0);
			step.transform.Translate(-dx, 0, 0);
			bgWaterPark.transform.Translate(dx / 3f, 0, 0);
			bgWater.transform.Translate(dx * 2f / 3f, 0, 0);
			bgWaterWave.transform.Translate(-dx * 2f, 0, 0);

			dx = Camera.main.ViewportToWorldPoint(Camera.main.WorldToViewportPoint(transform.position)).x - VX;
			step.transform.Translate(-dx, 0, 0);
			bgWaterPark.transform.Translate(dx / 3f, 0, 0);
			bgWater.transform.Translate(dx * 2f / 3f, 0, 0);
			bgWaterWave.transform.Translate(-dx * 2f, 0, 0);

			//PlayEffect("enter");
			PlayEffect("restart");
			swatch = System.Diagnostics.Stopwatch.StartNew();
			swatch.Start();
		}

		[SerializeField][Range(0.0f, 500.0f)] float JumpForce = 0.0f;
		public void Jump()
		{
			Debug.Log($"★ Jump: {C2_G03_Main.Instance.IsPlay}, {bJump}, {curAni}");
			if (!C2_G03_Main.Instance.IsPlay) return;
			if (bJump) return;
			if (curAni == CharacterAnimation.Wrong) return;
			if (curAni == CharacterAnimation.Wrong2) return;
			if (curAni == CharacterAnimation.Correct) return;
			if (curAni == CharacterAnimation.Crash) return;
			body.velocity = Vector2.zero;
			body.AddForce(Vector2.up * 450 * xSpeedScale);
			if (!isBooster) body.AddForce(Vector2.right * JumpForce * xSpeedScale);
			JumpUp();
			PlayEffect("jump");
		}

		public void EndGame()
		{
			Debug.Log("★ EndGame");
			Time.timeScale = 1;
			Idle();
			body.velocity = Vector2.zero;
			body.angularVelocity = 0;
			transform.rotation = Quaternion.Euler(0, 0, 0);
			gameObject.GetComponent<Rigidbody2D>().isKinematic = true;
		}

		// ////////////////////////////////////////////////////////////////////////////////////////
		void OnCollisionEnter2D(Collision2D other)
		{
			if (other.gameObject.CompareTag("UpRail"))
			{
				ContactPoint2D contact = other.contacts[0];
				Vector2 normal = contact.normal;
				if (normal.y < 0)
				{
					body.angularVelocity = 0;
					transform.rotation = Quaternion.Euler(0, 0, 0);
					body.velocity = Vector2.zero;
				}
			}

			if (bTouchDown && (other.gameObject.CompareTag("UpRail") || other.gameObject.CompareTag("Rail")))
			{
				bTouchDown = false;
				body.velocity = Vector2.zero;
			}
		}

		void OnTriggerEnter2D(Collider2D other)
		{
			switch (other.gameObject.tag)
			{
				case "UpRail":
				case "Rail":
					++nTouchGrounds;
					if (bRollBack)
					{
						bRollBack = false;
						body.velocity = Vector2.zero;
						body.angularVelocity = 0;
						Move();
					}
					break;
				case "Fall":
					if (curAni != CharacterAnimation.Fall)
					{
						Fall();
					}
					break;

				case "Quiz":
					if (IsAliveQuiz)
					{
						ProblemBubble pb = other.gameObject.GetComponent<ProblemBubble>();
						if (!pb.IsHit)
						{
							pb.Hit();
							if (pb.IsCorrect) Correct(pb.wordClip); else Wrong();

							IsShowQuiz = false;
							foreach (Transform t in pb.gameObject.transform.parent)
							{
								ProblemBubble bb = t.GetComponent<ProblemBubble>();
								if (bb != null) bb.Skip();
							}
						}
					}
					break;

				case "ItemSprinkler":
					if (other.gameObject.GetComponent<Sprinkler>().Hit(this)) Crash();
					break;
			}
		}
		void OnTriggerExit2D(Collider2D other)
		{
			switch (other.gameObject.tag)
			{
				case "UpRail":
				case "Rail":
					--nTouchGrounds;
					break;
			}
		}
	}
}