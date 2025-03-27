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
        public const int GWorld = 0x9AE0B58; //updated
        public const int GNames = 0x98C9940; //updated
        public const int GameStateBase = 0x158; //Engine.World
        public const int PlayerArray = 0x2B0; //Engine.GameStateBase
        public const int LevelsArray = 0x170;  //Engine.World
        public const int ActorsArray = 0x98; //Engine.Level
        public const int OwningGameInstance = 0x1B8; //Engine.World
        public const int LocalPlayers = 0x38; //Engine.GameInstance
        public const int PlayerController = 0x30; //Engine.Player
        public const int PlayerCameraManager = 0x350; //Engine.PlayerController
                                                      //public const int PlayerMinimalViewInfo = 0x22B0;
        public const int PlayerMinimalViewInfo = 0x1320; //Engine.PlayerCameraManager CameraCachePrivate
        public const int PlayerHUD = 0x348; //Engine.PlayerController
        public const int NameIndex = 0x18; //uObject
        public const int ComponentToWorld = 0x240;
        public const int Instigator = 0x188; //Engine.Actor
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
            public const int PlayerPawnPrivate = 0x310; //Engine.PlayerState
            public const int PlayerNamePrivate = 0x330; //Engine.PlayerState
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