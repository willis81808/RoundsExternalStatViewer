using System;
using System.Collections.Generic;

namespace BepInExExternalViewTester.Schemas
{
    [Serializable]
    public class PlayerStat
    {
        public string player;
        public float health;
        public float max_health;
        public float damage;
        public float block;
        public float block_cooldown;
        public float reload;
        public float reload_cooldown;
        public float attack_speed;
        public float attack_cooldown;

        public PlayerStat(string playerName, float health, float maxHealth, float damage, float block, float blockCooldown, float reload, float reloadCooldown, float attackSpeed, float attackCooldown)
        {
            this.player = playerName;

            this.health = health;
            this.max_health = maxHealth;

            this.damage = damage;

            this.block = block;
            this.block_cooldown = blockCooldown;

            this.reload = reload;
            this.reload_cooldown = reloadCooldown;

            this.attack_speed = attackSpeed;
            this.attack_cooldown = attackCooldown;
        }
    }
}
