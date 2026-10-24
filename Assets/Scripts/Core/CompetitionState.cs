using System;
using System.Collections.Generic;

namespace MazeOfEgos.Core
{
    public enum CompetitionPhase { Countdown, Playing, RoundResults, MatchResults, Paused }
    // Separate from ItemKind: no legacy save value changes meaning.
    public enum CompetitionItem { None, Banana, Oil, Boost, WaterBalloon, CreamPie }
    public enum CompetitionRoundEnd { None, TimeExpired, HeartsCollected }
    public enum CompetitionLife { Active, KnockedOut }
    public enum CompetitionEventKind { Heart, ItemPicked, ItemUsed, Slip, Knockout, Respawn, RoundEnded, MatchEnded }
    public readonly struct CompetitionEvent
    {
        public readonly CompetitionEventKind Kind;
        public readonly string Actor, Other;
        public CompetitionEvent(CompetitionEventKind kind, string actor, string other = null) { Kind = kind; Actor = actor; Other = other; }
    }

    [Serializable]
    public sealed class CompetitionRules
    {
        public string id = "competition-v4";
        public int roundDurationTicks = 7200, trophiesToWin = 3;
        public double speed = 1.6;
        public int heartWaveTicks = 600, groupsPerWave = 2, heartsPerGroup = 3, maximumHearts = 240;
        public int crateCooldownTicks = 600;
        public int knockoutTicks = 300, protectionTicks = 180, slipTicks = 48, oilTicks = 150, boostTicks = 240;
        public int windupTicks = 21, balloonFlightTicks = 36, bananaFlightTicks = 22, reactionTicks = 18;
        public double boostMultiplier = 1.6, oilMultiplier = .55, cakeRange = 4, cakeSpeed = 4, balloonRange = 3, splashRadius = 1;
        public bool movingWalls = true;
        public bool randomRoundStarts = true, replenishHearts = false, sharedMinimap = true, endWhenHeartsCollected = true;
        public CompetitionRules Copy() => (CompetitionRules)MemberwiseClone();
        public void Validate()
        {
            if (id != "competition-v4" || roundDurationTicks != 7200 || trophiesToWin != 3 ||
                double.IsNaN(speed) || double.IsInfinity(speed) || speed <= 0 || speed > 4 ||
                heartWaveTicks < 60 || heartWaveTicks > 7200 || groupsPerWave < 1 || groupsPerWave > 2 ||
                heartsPerGroup < 1 || heartsPerGroup > 3 || maximumHearts < 1 || maximumHearts > 240 ||
                crateCooldownTicks < 60 || crateCooldownTicks > 7200 || knockoutTicks != 300 || protectionTicks != 180 ||
                slipTicks < 1 || slipTicks > 120 || oilTicks < 1 || oilTicks > 600 || boostTicks < 1 || boostTicks > 600 ||
                windupTicks < 12 || windupTicks > 90 || balloonFlightTicks < 12 || balloonFlightTicks > 120 || bananaFlightTicks < 1 || bananaFlightTicks > 120 ||
                reactionTicks < 6 || reactionTicks > 60 || !InRange(boostMultiplier, 1, 2) || !InRange(oilMultiplier, .2, .9) ||
                !InRange(cakeRange, 1, 5) || !InRange(cakeSpeed, 1, 6) || !InRange(balloonRange, 1, 4) || !InRange(splashRadius, .3, 1))
                throw new ArgumentException("Invalid or unsupported competition rule profile.");
        }
        private static bool InRange(double value, double min, double max) => !double.IsNaN(value) && value >= min && value <= max;
    }
    [Serializable]
    public sealed class CompetitionParticipant
    {
        public string id;
        public Point2 position;
        public int heading, queuedHeading = -1;
        public bool moving = true;
        public long roundScore, bankedScore;
        public int trophies;
        public CompetitionItem inventory;
        public bool[] insideCrates;
        public CompetitionLife life;
        public long knockoutUntil, knockoutAt, protectedUntil, slipUntil, oilUntil, boostUntil, throwUntil, lastUseTick, trapImmunityUntil;
        public CompetitionItem lastUsedItem;
        public List<Point2> trail = new List<Point2>();
        public CompetitionParticipant Copy()
        {
            var copy = (CompetitionParticipant)MemberwiseClone();
            copy.insideCrates = insideCrates == null ? null : (bool[])insideCrates.Clone();
            copy.trail = trail == null ? null : new List<Point2>(trail); return copy;
        }
    }
    [Serializable]
    public struct CompetitionAttack
    {
        public int id;
        public string owner;
        public CompetitionItem kind;
        public Point2 origin, target;
        public long createdTick, launchTick, landTick, expiresTick;
        public Point2 Position(MazeMap maze, double tick)
        {
            double fraction = landTick > launchTick ? Geometry.Clamp((tick - launchTick) / (landTick - launchTick), 0, 1) : 1;
            return maze.Wrap(Point2.Lerp(origin, maze.Near(target, origin), fraction));
        }
    }
    public readonly struct CompetitionActorView
    {
        public readonly string Id;
        public readonly Point2 Position;
        public readonly int Heading, Trophies;
        public readonly long RoundScore, TotalScore, KnockoutUntil, ProtectedUntil, SlipUntil, OilUntil, BoostUntil, ThrowUntil, LastUseTick, KnockoutAt;
        public readonly CompetitionLife Life;
        public readonly CompetitionItem Inventory, LastUsedItem;
        public CompetitionActorView(CompetitionParticipant p, bool settled)
        {
            Id = p.id; Position = p.position; Heading = p.heading; Trophies = p.trophies;
            RoundScore = p.roundScore; TotalScore = p.bankedScore + (settled ? 0 : p.roundScore);
            Life = p.life; Inventory = p.inventory; KnockoutUntil = p.knockoutUntil; ProtectedUntil = p.protectedUntil;
            SlipUntil = p.slipUntil; OilUntil = p.oilUntil; BoostUntil = p.boostUntil; ThrowUntil = p.throwUntil;
            LastUseTick = p.lastUseTick; LastUsedItem = p.lastUsedItem; KnockoutAt = p.knockoutAt;
        }
    }
    [Serializable]
    public sealed class CompetitionHeart
    {
        public int id;
        public Cell cell;
        public CompetitionHeart Copy() => (CompetitionHeart)MemberwiseClone();
    }
    [Serializable]
    public sealed class CompetitionCrate
    {
        public Cell cell;
        public long availableAtTick;
        public CompetitionCrate Copy() => (CompetitionCrate)MemberwiseClone();
    }
    [Serializable]
    public sealed class CompetitionWall
    {
        public WallPhase phase;
        public long untilTick, cooldownUntilTick;
        public CompetitionWall Copy() => (CompetitionWall)MemberwiseClone();
    }
    public readonly struct CompetitionCommand
    {
        public readonly string ParticipantId;
        public readonly long Tick;
        public readonly int Heading;
        public CompetitionCommand(string participantId, long tick, int heading)
        { ParticipantId = participantId; Tick = tick; Heading = heading; }
    }
    [Serializable]
    public sealed class CompetitionSnapshot
    {
        internal CompetitionSnapshot CopyEnvelope() => (CompetitionSnapshot)MemberwiseClone();
        public const string FileName = "competition-v1.json";
        public string format;
        public int version;
        public string mazeId, mazeFingerprint, humanId;
        public CompetitionRules rules;
        public CompetitionPhase phase, resumePhase;
        public long tick, nextHeartWaveTick, nextWallAttemptTick, roundStartedTick;
        public CompetitionRoundEnd roundEnd;
        public int roundNumber, settledRound, roundElapsedTicks, countdownRemaining, nextHeartId;
        public uint resourceRandom, itemRandom, tieRandom, wallRandom, spawnRandom;
        public CompetitionParticipant[] participants;
        public CompetitionHeart[] hearts;
        public CompetitionCrate[] crates;
        public CompetitionWall[] walls;
        public string[] roundWinners, matchWinners;
        public CompetitionAttack[] attacks;
        public int nextAttackId;
        public bool computerPlayers;
        public double viewAspect = 16.0 / 9;
        public CompetitionBrainState[] brains;
        public CompetitionStatistics statistics;
    }
    [Serializable]
    public sealed class CompetitionStatistics
    {
        public long hearts, itemUses, knockouts, respawns;
        public int[] usesByItem = new int[6], usesByActor = new int[5], knockoutsByActor = new int[5];
        public CompetitionStatistics Copy() => new CompetitionStatistics { hearts = hearts, itemUses = itemUses, knockouts = knockouts, respawns = respawns,
            usesByItem = (int[])usesByItem.Clone(), usesByActor = (int[])usesByActor.Clone(), knockoutsByActor = (int[])knockoutsByActor.Clone() };
    }
}
