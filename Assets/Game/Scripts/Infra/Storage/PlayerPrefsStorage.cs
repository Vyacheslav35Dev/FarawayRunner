using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Scripts.Infra.Storage
{
    public class PlayerPrefsStorage : IStorage
    {
        /// <summary>
        /// Set int value in storage 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>Task.result</returns>
        public async UniTask SetInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
        }
        
        /// <summary>
        /// Set float value in storage 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>Task.result</returns>
        public async UniTask SetFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
        }
        
        /// <summary>
        /// Set string value in storage 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>Task.result</returns>
        public async UniTask SetString(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
        }
        
        /// <summary>
        /// Get int value in storage
        /// </summary>
        /// <param name="key"></param>
        /// <returns>Task.result</returns>
        public async UniTask<int> GetInt(string key)
        {
            return PlayerPrefs.GetInt(key);
        }
        
        /// <summary>
        /// Get float value in storage
        /// </summary>
        /// <param name="key"></param>
        /// <returns>Task.result</returns>
        public async UniTask<float> GetFloat(string key)
        {
            return PlayerPrefs.GetFloat(key);
        }
        
        /// <summary>
        /// Get string value in storage
        /// </summary>
        /// <param name="key"></param>
        /// <returns>Task.result</returns>
        public async UniTask<string> GetString(string key)
        {
            return PlayerPrefs.GetString(key);
        }
    }
}