using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Scripts.Coins;
using Game.Scripts.Infra.Music;
using Game.Scripts.Themes;
using UnityEngine;
using UnityEngine.AddressableAssets;
using CharacterController = Game.Scripts.Character.CharacterController;
using GameObject = UnityEngine.GameObject;

namespace Game.Scripts.Tracks
{
    public class TrackManager : MonoBehaviour
    {
        static int s_StartHash = Animator.StringToHash("Start");

        public delegate int MultiplierModifier(int current);
        public MultiplierModifier modifyMultiply;

        [Header("Character & Movements")]
        public CharacterController CharacterController;
        public float minSpeed = 5.0f;
        public float maxSpeed = 10.0f;
        public int speedStep = 4;
        public float laneOffset = 1.0f;

        public bool invincible = false;
        private bool m_IsTutorial = false;

        [Header("Objects")]
        
        public MeshFilter skyMeshFilter;

        [Header("Parallax")]
        public Transform parallaxRoot;
        public float parallaxRatio = 0.5f;

        
        public System.Action<TrackSegment> newSegmentCreated;
        public System.Action<TrackSegment> currentSegementChanged;

        public int trackSeed { get { return m_TrackSeed; } set { m_TrackSeed = value; } }

        public float timeToStart { get { return m_TimeToStart; } }  // Will return -1 if already started (allow to update UI)
        
        public float currentSegmentDistance { get { return m_CurrentSegmentDistance; } }
        public float worldDistance { get { return m_TotalWorldDistance; } }
        public float speed { get { return m_Speed; } }
        public float speedRatio { get { return (m_Speed - minSpeed) / (maxSpeed - minSpeed); } }
        public int currentZone { get { return m_CurrentZone; } }

        public TrackSegment currentSegment { get { return m_Segments[0]; } }
        public List<TrackSegment> segments { get { return m_Segments; } }
        
        public bool isMoving { get { return m_IsMoving; } }
        public bool isRerun { get { return m_Rerun; } set { m_Rerun = value; } }
        public bool isLoaded { get; set; }
        public bool firstObstacle { get; set; }

        protected float m_TimeToStart = -1.0f;
       
        protected int m_TrackSeed = -1;

        protected float m_CurrentSegmentDistance;
        protected float m_TotalWorldDistance;
        protected bool m_IsMoving;
        protected float m_Speed;

        protected float m_TimeSincePowerup;     // The higher it goes, the higher the chance of spawning one
        protected float m_TimeSinceLastPremium;

        protected int m_Multiplier;

        protected List<TrackSegment> m_Segments = new List<TrackSegment>();
        protected List<TrackSegment> m_PastSegments = new List<TrackSegment>();
        protected int m_SafeSegementLeft;

        public ThemeData currentTheme { get { return m_CurrentThemeData; } }
        
        [SerializeField]
        private ThemeData m_CurrentThemeData;
        
        protected int m_CurrentZone;
        protected float m_CurrentZoneDistance;
        protected int m_PreviousSegment = -1;

        protected int m_Score;
        protected float m_ScoreAccum;
        protected bool m_Rerun;
        
        Vector3 m_CameraOriginalPos = Vector3.zero;
    
        const float k_FloatingOriginThreshold = 10000f;

        protected const float k_CountdownToStartLength = 5f;
        protected const float k_CountdownSpeed = 1.5f;
        protected const float k_StartingSegmentDistance = 2f;
        protected const int k_StartingSafeSegments = 2;
        protected const int k_StartingCoinPoolSize = 256;
        protected const int k_DesiredSegmentCount = 10;
        protected const float k_SegmentRemovalDistance = -30f;
        protected const float k_Acceleration = 0.2f;

        [SerializeField] 
        private AssetReference _character;
        
        protected void Awake()
        {
            m_ScoreAccum = 0.0f;
        }

        private void StartMove(bool isRestart = true)
        {
            CharacterController.StartMoving();
            m_IsMoving = true;
            if (isRestart)
                m_Speed = minSpeed;
        }

        public void StopMove()
        {
            m_IsMoving = false;
        }

        public async UniTask Begin()
        {
            firstObstacle = true;
            m_CameraOriginalPos = Camera.main.transform.position;
            
            if (m_TrackSeed != -1)
                Random.InitState(m_TrackSeed);
            else
                Random.InitState((int)System.DateTime.Now.Ticks);

            m_CurrentSegmentDistance = k_StartingSegmentDistance;
            m_TotalWorldDistance = 0.0f;
            
            var charObj = await Addressables.InstantiateAsync(_character);
            var player = charObj.GetComponent<Character.Character>();
                
            CharacterController.character = player;
            CharacterController.trackManager = this;

            CharacterController.Init();
            
            player.transform.SetParent(CharacterController.characterCollider.transform, false);
            Camera.main.transform.SetParent(CharacterController.transform, true);
            
            m_CurrentZone = 0;
            m_CurrentZoneDistance = 0;

            skyMeshFilter.sharedMesh = m_CurrentThemeData.skyMesh;
            RenderSettings.fogColor = m_CurrentThemeData.fogColor;
            RenderSettings.fog = true;

            gameObject.SetActive(true);
            CharacterController.gameObject.SetActive(true);
            CharacterController.Coins = 0;
                
            m_Score = 0;
            m_ScoreAccum = 0;

            m_SafeSegementLeft = k_StartingSafeSegments;

            Coin.coinPool = new Pooler(currentTheme.collectiblePrefab, k_StartingCoinPoolSize);

            CharacterController.Begin();
            await WaitToStart();
            isLoaded = true;
        }
        
        async UniTask WaitToStart()
        {
            CharacterController.character.animator.Play(s_StartHash);
            CharacterController.StartRunning();
            await UniTask.Delay(1000);
            StartMove();
        }

        public void End()
        {
            foreach (TrackSegment seg in m_Segments)
            {
                Addressables.ReleaseInstance(seg.gameObject);
                _spawnedSegments--;
            }

            for (int i = 0; i < m_PastSegments.Count; ++i)
            {
                Addressables.ReleaseInstance(m_PastSegments[i].gameObject);
            }

            m_Segments.Clear();
            m_PastSegments.Clear();

            CharacterController.End();

            gameObject.SetActive(false);
            Addressables.ReleaseInstance(CharacterController.character.gameObject);
            CharacterController.character = null;

            Camera.main.transform.SetParent(null);
            Camera.main.transform.position = m_CameraOriginalPos;

            CharacterController.gameObject.SetActive(false);

            for (int i = 0; i < parallaxRoot.childCount; ++i)
            {
                _parallaxRootChildren--;
                Destroy(parallaxRoot.GetChild(i).gameObject);
            }
        }


        private int _parallaxRootChildren = 0;
        private int _spawnedSegments = 0;
        void Update()
        {
            while (_spawnedSegments < (k_DesiredSegmentCount))
            {
                SpawnNewSegment().Forget();
                _spawnedSegments++;
            }

            if (parallaxRoot != null && currentTheme.cloudPrefabs.Length > 0)
            {
                while (_parallaxRootChildren < currentTheme.cloudNumber)
                {
                    float lastZ = parallaxRoot.childCount == 0 ? 0 : parallaxRoot.GetChild(parallaxRoot.childCount - 1).position.z + currentTheme.cloudMinimumDistance.z;

                    GameObject cloud = currentTheme.cloudPrefabs[Random.Range(0, currentTheme.cloudPrefabs.Length)];
                    if (cloud != null)
                    {
                        GameObject obj = Instantiate(cloud);
                        obj.transform.SetParent(parallaxRoot, false);

                        obj.transform.localPosition =
                            Vector3.up * (currentTheme.cloudMinimumDistance.y +
                                          (Random.value - 0.5f) * currentTheme.cloudSpread.y)
                            + Vector3.forward * (lastZ + (Random.value - 0.5f) * currentTheme.cloudSpread.z)
                            + Vector3.right * (currentTheme.cloudMinimumDistance.x +
                                               (Random.value - 0.5f) * currentTheme.cloudSpread.x);

                        obj.transform.localScale = obj.transform.localScale * (1.0f + (Random.value - 0.5f) * 0.5f);
                        obj.transform.localRotation = Quaternion.AngleAxis(Random.value * 360.0f, Vector3.up);
                        _parallaxRootChildren++;
                    }
                }
            }

            if (!m_IsMoving)
                return;

            float scaledSpeed = m_Speed * Time.deltaTime;
            m_ScoreAccum += scaledSpeed;
            m_CurrentZoneDistance += scaledSpeed;

            int intScore = Mathf.FloorToInt(m_ScoreAccum);
            if (intScore != 0) AddScore(intScore);
            m_ScoreAccum -= intScore;

            m_TotalWorldDistance += scaledSpeed;
            m_CurrentSegmentDistance += scaledSpeed;

            if (m_CurrentSegmentDistance > m_Segments[0].worldLength)
            {
                m_CurrentSegmentDistance -= m_Segments[0].worldLength;

                m_PastSegments.Add(m_Segments[0]);
                m_Segments.RemoveAt(0);
                _spawnedSegments--;

                if (currentSegementChanged != null) currentSegementChanged.Invoke(m_Segments[0]);
            }

            Vector3 currentPos;
            Quaternion currentRot;
            Transform characterTransform = CharacterController.transform;

            m_Segments[0].GetPointAtInWorldUnit(m_CurrentSegmentDistance, out currentPos, out currentRot);
            
            bool needRecenter = currentPos.sqrMagnitude > k_FloatingOriginThreshold;

            // Parallax Handling
            if (parallaxRoot != null)
            {
                Vector3 difference = (currentPos - characterTransform.position) * parallaxRatio; ;
                int count = parallaxRoot.childCount;
                for (int i = 0; i < count; i++)
                {
                    Transform cloud = parallaxRoot.GetChild(i);
                    cloud.position += difference - (needRecenter ? currentPos : Vector3.zero);
                }
            }

            if (needRecenter)
            {
                int count = m_Segments.Count;
                for (int i = 0; i < count; i++)
                {
                    m_Segments[i].transform.position -= currentPos;
                }

                count = m_PastSegments.Count;
                for (int i = 0; i < count; i++)
                {
                    m_PastSegments[i].transform.position -= currentPos;
                }

                // Recalculate current world position based on the moved world
                m_Segments[0].GetPointAtInWorldUnit(m_CurrentSegmentDistance, out currentPos, out currentRot);
            }

            characterTransform.rotation = currentRot;
            characterTransform.position = currentPos;

            if (parallaxRoot != null && currentTheme.cloudPrefabs.Length > 0)
            {
                for (int i = 0; i < parallaxRoot.childCount; ++i)
                {
                    Transform child = parallaxRoot.GetChild(i);

                    // Destroy unneeded clouds
                    if ((child.localPosition - currentPos).z < -50)
                    {
                        _parallaxRootChildren--;
                        Destroy(child.gameObject);
                    }
                }
            }

            // Still move past segment until they aren't visible anymore.
            for (int i = 0; i < m_PastSegments.Count; ++i)
            {
                if ((m_PastSegments[i].transform.position - currentPos).z < k_SegmentRemovalDistance)
                {
                    m_PastSegments[i].Cleanup();
                    m_PastSegments.RemoveAt(i);
                    i--;
                }
            }

            PowerupSpawnUpdate();

            if (!m_IsTutorial)
            {
                if (m_Speed < maxSpeed)
                    m_Speed += k_Acceleration * Time.deltaTime;
                else
                    m_Speed = maxSpeed;
            }

            m_Multiplier = 1 + Mathf.FloorToInt((m_Speed - minSpeed) / (maxSpeed - minSpeed) * speedStep);

            if (modifyMultiply != null)
            {
                foreach (MultiplierModifier part in modifyMultiply.GetInvocationList())
                {
                    m_Multiplier = part(m_Multiplier);
                }
            }

            MusicPlayer.instance.UpdateVolumes(speedRatio);
        }

        public void PowerupSpawnUpdate()
        {
            m_TimeSincePowerup += Time.deltaTime;
            m_TimeSinceLastPremium += Time.deltaTime;
        }

        public void ChangeZone()
        {
            m_CurrentZone += 1;
            if (m_CurrentZone >= m_CurrentThemeData.zones.Length)
                m_CurrentZone = 0;

            m_CurrentZoneDistance = 0;
        }

        private readonly Vector3 _offScreenSpawnPos = new Vector3(-100f, -100f, -100f);
        public async UniTask SpawnNewSegment()
        {
            if (!m_IsTutorial)
            {
                if (m_CurrentThemeData.zones[m_CurrentZone].length < m_CurrentZoneDistance)
                    ChangeZone();
            }

            int segmentUse = Random.Range(0, m_CurrentThemeData.zones[m_CurrentZone].prefabList.Length);
            if (segmentUse == m_PreviousSegment) segmentUse = (segmentUse + 1) % m_CurrentThemeData.zones[m_CurrentZone].prefabList.Length;

            var segmentToUseOp = await m_CurrentThemeData.zones[m_CurrentZone].prefabList[segmentUse].InstantiateAsync(_offScreenSpawnPos, Quaternion.identity);
            
            TrackSegment newSegment = (segmentToUseOp).GetComponent<TrackSegment>();

            Vector3 currentExitPoint;
            Quaternion currentExitRotation;
            if (m_Segments.Count > 0)
            {
                m_Segments[m_Segments.Count - 1].GetPointAt(1.0f, out currentExitPoint, out currentExitRotation);
            }
            else
            {
                currentExitPoint = transform.position;
                currentExitRotation = transform.rotation;
            }

            newSegment.transform.rotation = currentExitRotation;

            Vector3 entryPoint;
            Quaternion entryRotation;
            newSegment.GetPointAt(0.0f, out entryPoint, out entryRotation);


            Vector3 pos = currentExitPoint + (newSegment.transform.position - entryPoint);
            newSegment.transform.position = pos;
            newSegment.manager = this;

            newSegment.transform.localScale = new Vector3((Random.value > 0.5f ? -1 : 1), 1, 1);
            newSegment.objectRoot.localScale = new Vector3(1.0f / newSegment.transform.localScale.x, 1, 1);

            if (m_SafeSegementLeft <= 0)
            {
                SpawnObstacle(newSegment);
            }
            else
                m_SafeSegementLeft -= 1;

            m_Segments.Add(newSegment);

            if (newSegmentCreated != null) newSegmentCreated.Invoke(newSegment);
        }
        
        public void SpawnObstacle(TrackSegment segment)
        {
            if (segment.possibleObstacles.Length != 0)
            {
                for (int i = 0; i < segment.obstaclePositions.Length; ++i)
                {
                    AssetReference assetRef = segment.possibleObstacles[Random.Range(0, segment.possibleObstacles.Length)];
                    SpawnFromAssetReference(assetRef, segment, i).Forget();
                }
            }

            SpawnCoinAndPowerup(segment).Forget();
        }

        private async UniTask SpawnFromAssetReference(AssetReference reference, TrackSegment segment, int posIndex)
        {
            var obj = await Addressables.LoadAssetAsync<GameObject>(reference);
            Obstacle obstacle = obj.GetComponent<Obstacle>();
            obstacle.Spawn(segment, segment.obstaclePositions[posIndex]);
        }

        private async UniTask SpawnCoinAndPowerup(TrackSegment segment)
        {
            const float increment = 3f;
            float currentWorldPos = 0.0f;
            int currentLane = Random.Range(0, 3);
            
            while (currentWorldPos < segment.worldLength)
            {
                Vector3 pos;
                Quaternion rot;
                segment.GetPointAtInWorldUnit(currentWorldPos, out pos, out rot);


                bool laneValid = true;
                int testedLane = currentLane;
                while (Physics.CheckSphere(pos + ((testedLane - 1) * laneOffset * (rot * Vector3.right)), 0.4f, 1 << 9))
                {
                    testedLane = (testedLane + 1) % 3;
                    if (currentLane == testedLane)
                    {
                        // Couldn't find a valid lane.
                        laneValid = false;
                        break;
                    }
                }

                currentLane = testedLane;

                if (laneValid)
                {
                    pos = pos + ((currentLane - 1) * laneOffset * (rot * Vector3.right));
                    
                    GameObject toUse = null;
                    toUse = Coin.coinPool.Get(pos, rot);
                    toUse.transform.SetParent(segment.collectibleTransform, true);

                    if (toUse != null)
                    {
                        Vector3 oldPos = toUse.transform.position;
                        toUse.transform.position += Vector3.back;
                        toUse.transform.position = oldPos;
                    }
                }

                currentWorldPos += increment;
            }
        }

        public void AddScore(int amount)
        {
            int finalAmount = amount;
            m_Score += finalAmount * m_Multiplier;
        }
    }
}