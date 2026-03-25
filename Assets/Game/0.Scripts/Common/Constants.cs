using UnityEngine;

public static class Constants
{
    public const int SKILLSLOT_COUNT = 4;
    public const float PLAYAREA_Y_MIN = 5f;
    public const float PLAYAREA_Y_MAX = 20f;
    public const float PLAYAREA_RADIUS = 25f;

    public const string MENUSCENE_NAME = "Main Menu 2";
    public const string GAMESCENE_NAME = "1105build";

    public enum EPlayerState // 플레이어 상태
    {
        None, Walk, Attack, Damaged, Dead
    }

    public enum EEnemyState // 공통 적 상태
    {
        None, Search, Attack, Damaged, Dead
    }

    public enum EGameLevel // 게임 난이도(임시)
    {
        Easy = 1, Normal, Hard, Boss
    }

    public enum EGameState // 게임 상태
    {
        None, Play, Pause, Clear, GameOver, FocusMode, Menu
    }

    [System.Serializable]
    public class EnemyStatus // 적 스탯
    {
        public int maxHp;
        public int hp;
        public int attackPower;
    }
}
