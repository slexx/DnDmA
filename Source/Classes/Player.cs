using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace DMAW_DND
{
    public class Player
    {
        public string Name { get; set; }
        public string Weapon { get; set; }
        public string Class { get; set; }
        public float Health { get; set; }
        public float MaxHealth { get; set; }
        public float Strength { get; set; }
        public float Vigor { get; set; }
        public float Agility { get; set; }
        public float Dexterity { get; set; }
        public float Will { get; set; }
        public float Knowledge { get; set; }
        public float Resourcefulness { get; set; }
        public float PhysicalDamageWeaponPrimary { get; set; }
        public float PhysicalDamageBase { get; set; }
        public float PhysicalPower { get; set; }
        public float ArmorRating { get; set; }
        public int Level { get; set; }
        public ulong RootComponentPtr { get; set; }
        public ulong PawnPrivatePtr { get; set; }
        public ulong SkeletonMeshPtr { get; set; }
        public Vector3 Location { get; set; }
        public FRotator Rotation { get; set; }
        public FTransform CompToWorld { get; set; }
        public Dictionary<int, Bone> Bones { get; set; } = new Dictionary<int, Bone>();
        public uint PartyID { get; set; }
        
        public PlayerType Type { get; set; }

        public bool IsLocalPlayer()
        {
            return this.Type == PlayerType.LocalPlayer;
        }
    };

    public enum PlayerType
    {
        Default,
        LocalPlayer,
        Friendly,
        Enemy
    }
}
