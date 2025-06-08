using UnityEngine;

namespace KingdomClash
{
    /// <summary>
    /// Class untuk menampilkan efek visual ketika resource dikumpulkan 
    /// </summary>
    public class GatheringEffect : MonoBehaviour
    {
        [SerializeField] private ParticleSystem gatheringParticle;
        [SerializeField] private float defaultDuration = 1.5f;
        
        private float activeTimer = 0f;
        private bool isTimerActive = false;
        
        private void Start()
        {
            if (gatheringParticle == null)
            {
                gatheringParticle = GetComponentInChildren<ParticleSystem>();
            }
            
            // Pastikan efek tidak aktif saat awal
            gameObject.SetActive(false);
        }
        
        private void Update()
        {
            if (isTimerActive)
            {
                activeTimer -= Time.deltaTime;
                
                if (activeTimer <= 0f)
                {
                    StopEffect();
                }
            }
        }
        
        /// <summary>
        /// Memulai efek pengumpulan resource
        /// </summary>
        /// <param name="duration">Durasi efek (atau default jika -1)</param>
        public void PlayEffect(float duration = -1f)
        {
            if (duration < 0) 
                duration = defaultDuration;
                
            gameObject.SetActive(true);
            
            if (gatheringParticle != null)
            {
                gatheringParticle.Play();
            }
            
            activeTimer = duration;
            isTimerActive = true;
        }
        
        /// <summary>
        /// Menghentikan efek pengumpulan resource
        /// </summary>
        public void StopEffect()
        {
            if (gatheringParticle != null)
            {
                gatheringParticle.Stop();
            }
            
            isTimerActive = false;
            gameObject.SetActive(false);
        }
        
        /// <summary>
        /// Memosisikan efek pada titik tertentu
        /// </summary>
        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }
    }
}
