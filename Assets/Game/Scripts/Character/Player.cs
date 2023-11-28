using System;
using Cysharp.Threading.Tasks;
using Game.Scripts.Data.PlayerData;
using Game.Scripts.Infra.Storage;
using UnityEngine;

namespace Game.Scripts.Character
{
    public class Player : IDisposable
    {
        private PlayerData _playerData;

        private IStorage _storage;

        public Player(IStorage storage)
        {
            _storage = storage;
        }
        
        public async UniTask Create()
        {
            _playerData = new PlayerData();
            
            _playerData.Id = await _storage.GetInt("id");
            _playerData.Coins = await _storage.GetInt("Coins");
            var type = await _storage.GetInt("charType");
            _playerData.Type = (CharacterType)type;
        }

        public async UniTask AddCoins(int value)
        {
            if (_playerData == null)
            {
                throw new Exception("Player::AddCoins _playerData == null! Player not initialized");
            }
            
            _playerData.Coins += value;
            await _storage.SetInt("Coins", _playerData.Coins);
        }
        
        public async UniTask<int> GetCoins()
        {
            if (_playerData == null)
            {
                throw new Exception("Player::GetCoins _playerData == null! Player not initialized");
            }
            var coins = await _storage.GetInt("Coins");
            if (_playerData != null && coins != _playerData.Coins)
            {
                _playerData.Coins = coins;
            }
            
            return _playerData.Coins;
        }

        public void Dispose()
        {
            _playerData = null;
            _storage = null;
        }
    }
}