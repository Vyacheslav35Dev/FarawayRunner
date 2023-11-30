using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Scripts.Tracks;
using UnityEngine;
using UnityEngine.Serialization;
using YG;

namespace Game.Scripts.Character
{
	/// <summary>
	/// Handle everything related to controlling the character. Interact with both the Character (visual, animation) and CharacterCollider
	/// </summary>
	public class CharacterController : MonoBehaviour
	{
		static int s_DeadHash = Animator.StringToHash ("Dead");
		static int s_RunStartHash = Animator.StringToHash("runStart");
		static int s_MovingHash = Animator.StringToHash("Moving");
		static int s_JumpingHash = Animator.StringToHash("Jumping");
		static int s_JumpingSpeedHash = Animator.StringToHash("JumpSpeed");
		static int s_SlidingHash = Animator.StringToHash("Sliding");

		public TrackManager trackManager;
		
		public Character character;
		
		public CharacterCollider characterCollider;
		
		public float laneChangeSpeed = 1.0f;
	
		[Header("Controls")]
		public float jumpLength = 2.0f;     // Distance jumped
		public float jumpHeight = 1.2f;

		public float slideLength = 2.0f;

		[Header("Sounds")]
		public AudioClip slideSound;
		public AudioClip powerUpUseSound;
		public AudioSource powerupSource;

		public int Coins;

		protected bool m_IsRunning;
		protected bool m_Jumping;
		protected float m_JumpStart;
	
		protected AudioSource m_Audio;

		protected int m_CurrentLane = k_StartingLane;
		protected Vector3 m_TargetPosition = Vector3.zero;

		protected readonly Vector3 k_StartingPosition = Vector3.forward * 2f;

		protected const int k_StartingLane = 1;
		protected const float k_GroundingSpeed = 80f;
		protected const float k_ShadowRaycastDistance = 100f;
		protected const float k_ShadowGroundOffset = 0.01f;
		protected const float k_TrackSpeedToJumpAnimSpeedRatio = 0.6f;
		protected const float k_TrackSpeedToSlideAnimSpeedRatio = 0.9f;
		
		public int maxLife = 3;
		public int CurrentLife;
		
#if !UNITY_STANDALONE
		protected Vector2 m_StartingTouch;
		protected bool m_IsSwiping = false;
#endif

		public void Init()
		{
			transform.position = k_StartingPosition;
			m_TargetPosition = Vector3.zero;

			m_CurrentLane = k_StartingLane;
			characterCollider.transform.localPosition = Vector3.zero;
			
			CurrentLife = maxLife;

			m_Audio = GetComponent<AudioSource>();
		}
		
		public void Begin()
		{
			m_IsRunning = false;
			character.animator.SetBool(s_DeadHash, false);
			characterCollider.Init();
		}
		
		public void StartRunning()
		{   
			StartMoving();
			if (character.animator)
			{
				character.animator.Play(s_RunStartHash);
				character.animator.SetBool(s_MovingHash, true);
			}
		}
		
		public void End()
		{
			Debug.Log("CharacterController::End");
		}

		public void StartMoving()
		{
			m_IsRunning = true;
		}

		public void StopMoving(bool restart = false)
		{
			m_IsRunning = false;
			trackManager.StopMove();
			if (character.animator)
			{
				character.animator.SetBool(s_MovingHash, false);
			}

			if (!restart)
			{
				DelayAndRun().Forget();
			}
		}

		private async UniTask DelayAndRun()
		{
			await UniTask.Delay(1000);
			trackManager.StartMove();
		}
		
		protected void Update ()
		{
			var platform = YandexGame.EnvironmentData.deviceType;
			
			if (platform == "desktop")
			{
				if (Input.GetKeyDown(KeyCode.LeftArrow))
				{
					ChangeLane(-1);
				}
				else if(Input.GetKeyDown(KeyCode.RightArrow))
				{
					ChangeLane(1);
				}
				else if(Input.GetKeyDown(KeyCode.UpArrow))
				{
					Jump();
				}
			}
			else
			{
				if (Input.touchCount == 1)
				{
					if(m_IsSwiping)
					{
						Vector2 diff = Input.GetTouch(0).position - m_StartingTouch;
						diff = new Vector2(diff.x/Screen.width, diff.y/Screen.width);

						if(diff.magnitude > 0.01f) //we set the swip distance to trigger movement to 1% of the screen width
						{
							if(Mathf.Abs(diff.y) > Mathf.Abs(diff.x))
							{
								Jump();
							}
							else
							{
								if(diff.x < 0)
								{
									ChangeLane(-1);
								}
								else
								{
									ChangeLane(1);
								}
							}
						
							m_IsSwiping = false;
						}
					}

					if(Input.GetTouch(0).phase == TouchPhase.Began)
					{
						m_StartingTouch = Input.GetTouch(0).position;
						m_IsSwiping = true;
					}
					else if(Input.GetTouch(0).phase == TouchPhase.Ended)
					{
						m_IsSwiping = false;
					}
				}
			}

			Vector3 verticalTargetPosition = m_TargetPosition;

			if(m_Jumping)
			{
				if (trackManager.isMoving)
				{
					float correctJumpLength = jumpLength * (1.0f + trackManager.speedRatio);
					float ratio = (trackManager.worldDistance - m_JumpStart) / correctJumpLength;
					if (ratio >= 1.0f)
					{
						m_Jumping = false;
						character.animator.SetBool(s_JumpingHash, false);
					}
					else
					{
						verticalTargetPosition.y = Mathf.Sin(ratio * Mathf.PI) * jumpHeight;
					}
				}
				else if(!AudioListener.pause)
				{
					verticalTargetPosition.y = Mathf.MoveTowards (verticalTargetPosition.y, 0, k_GroundingSpeed * Time.deltaTime);
					if (Mathf.Approximately(verticalTargetPosition.y, 0f))
					{
						character.animator.SetBool(s_JumpingHash, false);
						m_Jumping = false;
					}
				}
			}

			characterCollider.transform.localPosition = Vector3.MoveTowards(characterCollider.transform.localPosition, verticalTargetPosition, laneChangeSpeed * Time.deltaTime);
		}

		private void Jump()
		{
			if (!m_IsRunning)
				return;
	    
			if (!m_Jumping)
			{
				float correctJumpLength = jumpLength * (1.0f + trackManager.speedRatio);
				m_JumpStart = trackManager.worldDistance;
				float animSpeed = k_TrackSpeedToJumpAnimSpeedRatio * (trackManager.speed / correctJumpLength);

				character.animator.SetFloat(s_JumpingSpeedHash, animSpeed);
				character.animator.SetBool(s_JumpingHash, true);
				m_Audio.PlayOneShot(character.jumpSound);
				m_Jumping = true;
			}
		}

		public void StopJumping()
		{
			if (m_Jumping)
			{
				character.animator.SetBool(s_JumpingHash, false);
				m_Jumping = false;
			}
		}

		private void ChangeLane(int direction)
		{
			if (!m_IsRunning)
				return;

			int targetLane = m_CurrentLane + direction;

			if (targetLane < 0 || targetLane > 2)
				return;

			m_CurrentLane = targetLane;
			m_TargetPosition = new Vector3((m_CurrentLane - 1) * trackManager.laneOffset, 0, 0);
		}
	}
}
