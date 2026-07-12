using Solo.MOST_IN_ONE;
using UnityEngine;

namespace World
{
    public class TokenSpawner : MonoBehaviour
    {
        public GameObject[] tokenPrefabs;
        public int currentTokens;
        public int maxTokens;

        public double timePassed;
        public double timeTillSpawn;
        public float minTime;
        public float maxTime;

        public float spawnRadius;
        public float spawnHeight;

        public GameObject player;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            RandomTime();
        }

        // Update is called once per frame
        private void Update()
        {
            timePassed += Time.deltaTime;

            if (timePassed < timeTillSpawn) return;

            RandomTime();

            if (currentTokens >= maxTokens) return;

            // It would be nice if we could add some collision detection
            // here (unistops, player, existing tokens...)

            var randomIndex = Random.Range(0, tokenPrefabs.Length);

            var offset = new Vector3(Random.Range(-spawnRadius, spawnRadius), spawnHeight,
                Random.Range(-spawnRadius, spawnRadius));

            var worldPos = player.transform.position + offset;
            var rayOrigin = new Vector3(worldPos.x, worldPos.y + 1000f, worldPos.z);

            if (!Physics.Raycast(rayOrigin, Vector3.down, out var hit)) return;

            currentTokens++;
            var token = Instantiate(tokenPrefabs[randomIndex], player.transform.parent);

            var tokenRenderer = token.GetComponentInChildren<Renderer>();
            var halfHeight = tokenRenderer != null ? tokenRenderer.bounds.extents.y : 0f;
            token.transform.position = hit.point + Vector3.up * (halfHeight + spawnHeight);

            MOST_HapticFeedback.GenerateWithCooldown(MOST_HapticFeedback.HapticTypes.MediumImpact, 1.5f);
            
            var destroyer = token.GetComponent<DestroyAfterTime>();

            if (destroyer == null) return;

            destroyer.spawner = this;
        }

        private void RandomTime()
        {
            timeTillSpawn = Random.Range(minTime, maxTime);
            timePassed = 0.0;
        }
    }
}