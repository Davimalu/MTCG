using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Models
{
    public class TradeOffer
    {
        public int? Id { get; set; }
        public required User User { get; set; }
        public required Card Card { get; set; }
        public bool RequestedMonster { get; set; }
        public float RequestedDamage { get; set; }
    }
}
