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
    SwordThrow_Bounce // Bounce sword will bounce between emeies.




}
