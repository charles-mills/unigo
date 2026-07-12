using System;

namespace Gameplay
{
    [Serializable]
    public class PlayerDataRecord
    {
        public string playerName;
        public string playerTeam;
        public int brandsCaptured;
        public int catchFail;
        public int catchSuccess;
        public int enemyDefeated;

        public PlayerDataRecord(string newPlayerName)
        {
            playerName = newPlayerName;
            playerTeam = "";
            catchSuccess = 0;
            catchFail = 0;
            brandsCaptured = 0;
            enemyDefeated = 0;
        }

        public void IncrementBrandsCaptured()
        {
            brandsCaptured++;
        }

        public int GetBrandsCaptured()
        {
            return brandsCaptured;
        }

        public void IncrementCatchSuccess()
        {
            catchSuccess++;
        }

        public int GetCatchSuccess()
        {
            return catchSuccess;
        }

        public void IncrementCatchFail()
        {
            catchFail++;
        }

        public int GetCatchFail()
        {
            return catchFail;
        }

        public string GetPlayerTeam()
        {
            return playerTeam;
        }

        public void IncrementEnemyDefeated()
        {
            enemyDefeated++;
        }

        public int GetEnemyDefeated()
        {
            return enemyDefeated;
        }
    }
}