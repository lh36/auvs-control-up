using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 模拟登陆
{
    class PlayerStat
    {
        public string name = "";
        public int health;
        public int maxHealth;
        public int speed;
        public int attackPower;
        public float attackRate;
        public int attackRange;
        public int lastAttackTime;

        //友情提示，这个是JsonFx必须要的  
        public PlayerStat() { }  
    }
}
