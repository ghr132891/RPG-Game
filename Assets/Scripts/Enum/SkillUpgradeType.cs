 public enum SkillUpgradeType
{
    None,


    // ---------Dash Tree----------
    Dash,//Dash to avoid damage
    Dash_CloneOnStart,//Creat a clone when dash starts
    Dash_CloneOnStartAndArrival,//Creat a clone when dash ends and starts
    Dash_ShardOnStart,//Creat a shard when dash starts
    Dash_ShardOnStartAndArrival,//Creat a shard when dash ends and starts

    // --------Shard Tree----------
    Shard,//The shard explodes when touched by enemies or when time goes up.
    Shard_MoveToEnemy,//Shard will move towards nearest enemy.
    Shard_MultiCast,//Shard ability can have up to N charges. You can cast them all in a raw
    Shard_Teleport,//You can swap places with the last shard you created
    Shard_TeleportHpRewind,//When you swap places with shard, you HP % is same as it was when you created shard

    // --------Sword Throw Tree-----------
    SwordThrow,// You can throw sword to damage enemies from range.
    SwordThrow_Spin,// Your sword will spin at one point and damage emeies. Like a chainraw.
    SwordThrow_Pierce,// Pierce sword will pierce N targets.
    SwordThrow_Bounce,// Bounce sword will bounce between emeies.

    // --------Time Echo Tree-------------
    TimeEcho,// Creat a clone of a player,it can take damage from enemies.
    TimeEcho_SingleAttack,// Time Echo can perform a single attack.
    TimeEcho_MultiAttack,// Time Echo can perform N attacks.
    TimeEcho_ChanceToMultiply,// Time Echo has a chance to creat another time echo when attacks.

    TimeEcho_HealWisp,// When time echo dies it creats a wisps that files toward the player to heal it
                      // Heal is to percantage of damage taken when died
    TimeEcho_CleanseWisp,// Wisp will now remove negative effects from players.
    TimeEcho_CooldownWisp// Wisp will reduce cooldown of all skills by N seconds.



}
