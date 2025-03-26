using Newtonsoft.Json;
using Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DMAW_DND
{
    internal static class Offsets
    {
        public const int GWorld = 0x9A91A98;
        public const int GNames = 0x987A880;
        public const int GameStateBase = 0x158;
        public const int PlayerArray = 0x2B0;
        public const int LevelsArray = 0x170;
        public const int ActorsArray = 0x98;
        public const int OwningGameInstance = 0x1B8;
        public const int LocalPlayers = 0x38;
        public const int PlayerController = 0x30;
        public const int PlayerCameraManager = 0x350;
        //public const int PlayerMinimalViewInfo = 0x22B0;
        public const int PlayerMinimalViewInfo = 0x1320;
        public const int PlayerHUD = 0x348;
        public const int NameIndex = 0x18; //uObject
        public const int ComponentToWorld = 0x240;
        public const int Instigator = 0x188;
        //public const int isOpen = 0x3F0;
        //public const int isHidden = 0x40C;
        [StructLayout(LayoutKind.Explicit)]
        public struct ActorStatus
        {
            [FieldOffset(0x3F8)]
            public bool IsOpen; // This will start at the base address + 0x3F0
        }

        public class PlayerState
        {
            public const int PlayerPawnPrivate = 0x310;
            public const int PlayerNamePrivate = 0x388;
        }
        public class PawnPrivate
        {
            public const int SkeletalMeshComponent = 0x320;
            public const int ComponentToWorld = 0x240;
            public const int AccountID = 0x798; //fString
            public const int NickNameCached = 0x858; //fNickname
            public const int MeshDeformerInstance = 0x5B8;
            public const int RootComponent = 0x1A0;
            public const int PlayerController = 0x2D0;
            public const int AbilitySystemComponent = 0x710;
            public const int AccountDataReplication = 0x7B8;
            public const int EquipmentInventory = 0xBD8;
            public const int SpawnedAttributes = 0x10A0;
        }
        public class RootComponent
        {
            public const int Location = 0x260; 
            public const int Rotation = 0x140;
        }
    }
}