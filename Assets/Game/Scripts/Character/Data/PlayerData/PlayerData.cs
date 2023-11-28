using System;
using Game.Scripts.Infra.Storage;

namespace Game.Scripts.Data.PlayerData
{
    [Serializable]
    public class PlayerData
    {
        public int Id;
        public int Coins;
        public CharacterType Type;

        public float MasterVolume = float.MinValue;
        public float MusicVolume = float.MinValue; 
        public float MasterSFXVolume = float.MinValue;
    }
}