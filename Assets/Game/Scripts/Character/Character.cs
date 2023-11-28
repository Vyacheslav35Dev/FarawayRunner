using UnityEngine;

namespace Game.Scripts.Character
{
    public class Character : MonoBehaviour
    {
        public Animator animator;
        
        [Header("Sound")]
        public AudioClip jumpSound;
        public AudioClip hitSound;
        public AudioClip deathSound;
    }
}