using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMAW_DND
{
    public class Enums
    {
        public enum GameStatus
        {
            // Token: 0x0400044A RID: 1098
            NotFound,
            // Token: 0x0400044B RID: 1099
            Found,
            // Token: 0x0400044C RID: 1100
            Menu,
            // Token: 0x0400044D RID: 1101
            LoadingLoot,
            // Token: 0x0400044E RID: 1102
            Matching,
            // Token: 0x0400044F RID: 1103
            InGame,
            // Token: 0x04000450 RID: 1104
            Error
        }

        public enum ActorType
        {
            Unknown,
            Item,
            Key,
            Potion,
            Misc,
            Statue,
            Chest,
            Trap,
            Portal,
            Goblin,
            Undead,
            Demon,
            Special,
            Bug,
            Mimic,
            Boss,
            Lever,
            Ore,
            NPC // Represents enemy actors
        }
        public enum PlayerBone
        {
            Root = 0,
            Pelvis = 1,
            spine_01 = 2,
            spine_02 = 3,
            spine_03 = 4,
            clavicle_l = 5,
            upperarm_l = 6,
            lowerarm_l = 7,
            hand_l = 8,
            thumb_01_l = 9,
            thumb_02_l = 10,
            thumb_03_l = 11,
            index_01_l = 12,
            index_02_l = 13,
            index_03_l = 14,
            middle_01_l = 15,
            middle_02_l = 16,
            middle_03_l = 17,
            ring_01_l = 18,
            ring_02_l = 19,
            ring_03_l = 20,
            pinky_01_l = 21,
            pinky_02_l = 22,
            pinky_03_l = 23,
            fk_weapon_l = 24,
            shield_l = 25,
            lowerarm_twist_01_l = 26,
            upperarm_twist_01_l = 27,
            neck_01 = 28,
            Head = 29,
            eyelid_l = 30,
            eyelid_r = 31,
            jaw = 32,
            clavicle_r = 33,
            upperarm_r = 34,
            lowerarm_r = 35,
            lowerarm_twist_01_r = 36,
            shield_r = 37,
            hand_r = 38,
            pinky_01_r = 39,
            pinky_02_r = 40,
            pinky_03_r = 41,
            ring_01_r = 42,
            ring_02_r = 43,
            ring_03_r = 44,
            middle_01_r = 45,
            middle_02_r = 46,
            middle_03_r = 47,
            index_01_r = 48,
            index_02_r = 49,
            index_03_r = 50,
            thumb_01_r = 51,
            thumb_02_r = 52,
            thumb_03_r = 53,
            fk_weapon_r = 54,
            upperarm_twist_01_r = 55,
            sheath_spine_03 = 56,
            sash_front_spine_02 = 57,
            sash_front_spine_02_tip = 58,
            sash_back_spine_02 = 59,
            sash_back_spine_02_tip = 60,
            sheath_spine_01 = 61,
            sash_front_spine_01 = 62,
            sash_front_spine_01_tip = 63,
            sash_back_spine_01 = 64,
            sash_back_spine_01_tip = 65,
            thigh_l = 66,
            calf_l = 67,
            foot_l = 68,
            ball_l = 69,
            calf_twist_01_l = 70,
            thigh_twist_01_l = 71,
            sheath_thigh_l = 72,
            thigh_r = 73,
            calf_r = 74,
            foot_r = 75,
            ball_r = 76,
            calf_twist_01_r = 77,
            thigh_twist_01_r = 78,
            sheath_thigh_r = 79,
            sheath_pelivs = 80,
            ik_foot_root = 81,
            ik_foot_l = 82,
            ik_foot_r = 83,
            ik_hand_root = 84,
            ik_hand_gun = 85,
            ik_hand_l = 86,
            weapon_l = 87,
            ik_hand_l_socket = 88,
            SpellSocket_L = 89,
            weapon_l_emote = 90,
            ik_hand_r = 91,
            weapon_r = 92,
            ik_hand_r_socket = 93,
            SpellSocket_M = 94,
            SpellSocket = 95,
            weapon_r_emote = 96,
            camera_root = 97,
            camera_rot_root = 98,
            Camera = 99,
            emote_root = 100,
            VB_MorphTarget = 101,
        }

        public static readonly List<(PlayerBone, PlayerBone)> BoneConnections = new()
        {
            (PlayerBone.Head, PlayerBone.neck_01),
            (PlayerBone.neck_01, PlayerBone.spine_03),
            (PlayerBone.spine_03, PlayerBone.spine_02),
            (PlayerBone.spine_02, PlayerBone.spine_01),

            (PlayerBone.neck_01, PlayerBone.upperarm_l),
            (PlayerBone.upperarm_l, PlayerBone.lowerarm_l),
            (PlayerBone.lowerarm_l, PlayerBone.ik_hand_l),

            (PlayerBone.neck_01, PlayerBone.upperarm_r),
            (PlayerBone.upperarm_r, PlayerBone.lowerarm_r),
            (PlayerBone.lowerarm_r, PlayerBone.ik_hand_r),

            (PlayerBone.spine_01, PlayerBone.thigh_l),
            (PlayerBone.thigh_l, PlayerBone.calf_l),
            (PlayerBone.calf_l, PlayerBone.foot_l),

            (PlayerBone.spine_01, PlayerBone.thigh_r),
            (PlayerBone.thigh_r, PlayerBone.calf_r),
            (PlayerBone.calf_r, PlayerBone.foot_r),
        };
    }
}
