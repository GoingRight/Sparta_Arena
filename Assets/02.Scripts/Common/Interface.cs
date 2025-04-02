using System.Collections;
using System.Collections.Generic;

public interface IMobHealthReadable
{
    float HealthRatio { get; }
}
public enum DanceType
{
    BuffAll,
    HealAll,
    DebuffPlayer
}
public enum StrategyType
{
    Idle,
    Attack,
    Hold,
    Retreat,
    Defend
}

public enum FormationRole
{
    Fighter,
    Supporter
}

public enum WeaponType
{
    Sword,
    Spear,
    Rifle
}
