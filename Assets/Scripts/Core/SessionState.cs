using System;

namespace MazeOfEgos.Core
{
    public enum SessionPhase { Countdown, Playing, Caught, LevelClear, GameOver, Paused }
    public enum PursuerRole { Chaser, Ambusher, Flanker, Wanderer }
    public enum PursuerLife { InHouse, Exiting, Active, Returning, Recovering }
    public enum ItemKind { None, BananaPeel, OilSlick, SpeedBoost }
    public enum WallPhase { Closed, Warning, Opening, Open, Closing }
    public enum EventKind { Heart, PowerHeart, PursuerPaused, Caught, LevelClear, ExtraChance, ItemPicked, ItemUsed, Slip, WallWarning, WallOpened, WallClosed, NewLevel }

    [Serializable]
    public sealed class RuleProfile
    {
        public bool itemsEnabled = true, movingWallsEnabled = true, levelClearBonusEnabled = true;
        public RuleProfile Copy() => (RuleProfile)MemberwiseClone();
    }
    [Serializable]
    public sealed class PursuerState
    {
        public string id;
        public PursuerRole role;
        public PursuerLife life;
        public Cell from, to, previous;
        public double progress, releaseAt, recoverUntil, stunUntil, oilUntil, slipImmunityUntil;
        public int consumedPowerId;
        public PursuerState Copy() => (PursuerState)MemberwiseClone();
        public Point2 Position(MazeMap maze) => maze.Wrap(Point2.Lerp(from.Position, maze.Near(to.Position, from.Position), progress));
    }
    [Serializable]
    public sealed class TrapState
    {
        public int id;
        public ItemKind kind;
        public Point2 position;
        public double expiresAt;
        public TrapState Copy() => (TrapState)MemberwiseClone();
    }
    [Serializable]
    public sealed class WallState
    {
        public WallPhase phase;
        public double phaseUntil, cooldownUntil;
        public WallState Copy() => (WallState)MemberwiseClone();
    }
    public readonly struct GameEvent
    {
        public readonly EventKind Kind;
        public readonly int Actor;
        public readonly long Points;
        public GameEvent(EventKind kind, int actor = -1, long points = 0) { Kind = kind; Actor = actor; Points = points; }
    }
    [Serializable]
    public sealed class SessionSnapshot
    {
        public int version = 1;
        public string mazeId, mazeFingerprint;
        public RuleProfile rules;
        public SessionPhase phase, resumePhase;
        public double time, levelTime, phaseRemaining, savedPhaseRemaining, powerUntil, boostUntil, nextWallAttempt;
        public double yaw, pitch;
        public Point2 player;
        public int level, chances, remainingHearts, powerId, powerChain, nextTrapId;
        public long score;
        public bool extraChanceGiven, scatter;
        public uint itemRandom, aiRandom, wallRandom;
        public ItemKind inventory;
        public bool[] collected, itemAvailable;
        public PursuerState[] pursuers;
        public WallState[] walls;
        public TrapState[] traps;
        public Point2[] trail;
    }

    public static class LevelBalance
    {
        private static readonly int[] PowerDurations = { 6, 5, 4, 3, 2, 5, 2, 2, 1, 5, 2, 1, 1, 3, 1, 1, 0, 1, 0, 0, 0 };
        public static double PlayerSpeed(int level) => 2 * (level == 1 ? 0.80 : level <= 4 ? 0.90 : level <= 20 ? 1.0 : 0.90);
        public static double PursuerSpeed(int level) => 2 * (level == 1 ? 0.75 : level <= 4 ? 0.85 : 0.95);
        public static double FrightenedSpeed(int level) => 2 * (level == 1 ? 0.50 : level <= 4 ? 0.55 : 0.60);
        public static double PowerDuration(int level) => Math.Max(2, PowerDurations[Math.Min(Math.Max(level, 1), 21) - 1]);
    }
}
