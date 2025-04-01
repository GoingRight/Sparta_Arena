using System.Collections;
using System.Collections.Generic;

public interface IMobHealthReadable
{
    float HealthRatio { get; }
}
public enum DanceType
{
    Buff,
    Heal,
    Debuff
}
public enum StrategyType
{
    Idle,
    Attack,
    Defend
}

public enum FormationRole
{
    Fighter,
    Supporter
}
public enum MobState
{
    Idle,
    Walk,
    Run,
    Act1,
    Act2,
    Act3,
    Hit,
    Dead
}