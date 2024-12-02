using System.Collections.Generic;
using UnityEngine;

namespace Gameplay
{
    public class VisualEffectsManager : MonoBehaviour
    {
        public ParticleSystem[] prefabs;

        public static VisualEffectsManager Instance { get; private set; }

        private List<ParticleSystem>[] effectPool;

        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else if (Instance != this)
                Destroy(gameObject);

            effectPool = new List<ParticleSystem>[prefabs.Length];
        }

        public void InstantiateEffect(int index, Vector3 position)
        {
            if (prefabs == null || index < 0 || index >= prefabs.Length) return;

            // search for existing effect in pool
            if (effectPool[index] != null && effectPool.Length > 0)
            {
                foreach (ParticleSystem vfx in effectPool[index])
                {
                    if (vfx.isPlaying) continue;
                    vfx.gameObject.SetActive(true);
                    vfx.transform.position = position;
                    vfx.Play();
                    return;
                }
            }

            // create new particle system and play it
            GameObject obj = Instantiate(prefabs[index].gameObject);
            obj.transform.position = position;
            ParticleSystem _vfx = obj.GetComponent<ParticleSystem>();
            if (_vfx == null) return;
            _vfx.Play();
            // add to pool
            if (effectPool[index] == null) 
                effectPool[index] = new List<ParticleSystem>();
            effectPool[index].Add(_vfx);
        }
    }
}
