using DMAW_DND;
using System.Numerics;

namespace DMAW_DND
{
    public class Item
    {
        public string Name { get; set; }
        public ulong spawnedAttributes { get; set; }
        public Enums.ActorType Type { get; set; }

        public uint? EnemyHealth { get; set; }
        public uint? EnemyMaxHealth { get; set; }
        public ulong ActorSkillComponent { get; set; }
        public Vector3 ActorLocation { get; set; }
        public Vector2 ZoomedPosition { get; set; } = new();
        public ulong ActorRootComponent { get; set; }
        public FTransform CompToWorld { get; set; }
    }



}