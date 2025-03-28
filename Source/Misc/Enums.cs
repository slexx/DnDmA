﻿using System;
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
            thigh_l = 56,
            calf_l = 57,
            foot_l = 58,
            ball_l = 59,
            calf_twist_01_l = 60,
            thigh_twist_01_l = 61,
            thigh_r = 62,
            calf_r = 63,
            foot_r = 64,
            ball_r = 65,
            calf_twist_01_r = 66,
            thigh_twist_01_r = 67,
            ik_foot_root = 68,
            ik_foot_l = 69,
            ik_foot_r = 70,
            ik_hand_root = 71,
            ik_hand_gun = 72,
            ik_hand_l = 73,
            weapon_l = 74,
            ik_hand_l_socket = 75,
            ik_hand_r = 76,
            weapon_r = 77,
            ik_hand_r_socket = 78,
            camera_root = 79,
            camera_rot_root = 80,
            Camera = 81,
            VB_MorphTarget = 82,
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
