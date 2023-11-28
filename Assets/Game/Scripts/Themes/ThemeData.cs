using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game.Scripts.Themes
{
    [System.Serializable]
    public struct ThemeZone
    {
        public int length;
        public AssetReference[] prefabList;
    }

    /// <summary>
    /// This is an asset which contains all the data for a theme.
    /// As an asset it live in the project folder, and get built into an asset bundle.
    /// </summary>
    [CreateAssetMenu(fileName ="themeData", menuName ="FarawayRunner/Theme Data")]
    public class ThemeData : ScriptableObject
    {
        [Header("Theme Data")]
        public string themeName;
        
        [Header("Objects")]
        public ThemeZone[] zones;
        public GameObject collectiblePrefab;
        public GameObject premiumCollectible;

        [Header("Decoration")]
        public GameObject[] cloudPrefabs;
        public Vector3 cloudMinimumDistance = new Vector3(0, 20.0f, 15.0f);
        public Vector3 cloudSpread = new Vector3(5.0f, 0.0f, 1.0f);
        public int cloudNumber = 10;
        public Mesh skyMesh;
        public Mesh UIGroundMesh;
        public Color fogColor;
    }
}