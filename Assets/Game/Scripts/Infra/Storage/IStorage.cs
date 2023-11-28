using Cysharp.Threading.Tasks;

namespace Game.Scripts.Infra.Storage
{
    public interface IStorage
    {
        /// <summary>
        /// Set int value in storage 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>Task.result</returns>
        public UniTask SetInt(string key, int value);
        
        /// <summary>
        /// Set float value in storage 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>Task.result</returns>
        public UniTask SetFloat(string key, float value);
        
        /// <summary>
        /// Set string value in storage 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>Task.result</returns>
        public UniTask SetString(string key, string value);
        
        /// <summary>
        /// Get int value in storage
        /// </summary>
        /// <param name="key"></param>
        /// <returns>Task.result</returns>
        public UniTask<int> GetInt(string key);
        
        /// <summary>
        /// Get float value in storage
        /// </summary>
        /// <param name="key"></param>
        /// <returns>Task.result</returns>
        public UniTask<float> GetFloat(string key);
        
        /// <summary>
        /// Get string value in storage
        /// </summary>
        /// <param name="key"></param>
        /// <returns>Task.result</returns>
        public UniTask<string> GetString(string key);
    }
}