using Microsoft.AspNetCore.Mvc.Razor.Internal;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Veldrid.Sdl2;
using vmmsharp;
using Vortice.Direct3D11;
using static DMAW_DND.Enums;

namespace DMAW_DND
{
    public class Game
    {
        public Game()
        { }

        private static ulong _clientBase;
        private static ulong _matchmakingBase;
        private static ulong _engineBase;
        private static ulong _inputBase;

        private static ulong _replayInterface;
        private static ulong _world;
        public static ulong GWorld { get => _world; }
        private static ulong _owningGameInstance;
        private static ulong _localPlayer;
        private static ulong _localPlayerController; //FCameraCacheEntry

        public static string _currentMapName = "<empty>";
        public static bool _inGame = false;

        public static Stopwatch playerUpdateTimer = new();
        private static Semaphore playerLock = new(1, 1);
        private static Dictionary<int, string> FNameCache = new Dictionary<int, string>();

        private Dictionary<int, Player> _players = new Dictionary<int, Player>();
        public ulong PlayerController
        {
            get => _localPlayerController;
        }

        public Dictionary<int, Player> Players
        {
            get => _players;
        }
        public ulong World
        {
            get => _world;
        }
        public string CurrentMapName
        {
            get => _currentMapName;
        }
        public bool InGame
        {
            get
            {
                return _inGame;
            }
        }
        //local player location
        public Vector3 LocalPlayerLocation
        {
            get
            {
                return _players.FirstOrDefault(p => p.Value.Type == PlayerType.LocalPlayer).Value.Location;
            }
        }

        public void MapReadLoop()
        {
            var MapNameIndex = Memory.ReadValue<uint>(_world + 0x18);
            _currentMapName = Memory.ReadName(MapNameIndex);
            Program.Log($"Current Map: {_currentMapName}");
            if (_currentMapName == "<empty>" || _currentMapName == "None" || _currentMapName == null)
            {
                _inGame = false;
                Memory.GameStatus = Enums.GameStatus.Menu;
                _players.Clear();
                EntityManager.Items.Clear();
                return;
            }
            UpdatePlayersList();
        }

        public void GameLoop()
        {
            try
            {
                if (!EntityManager.Running)
                {
                    EntityManager.Start();
                }
                if (playerUpdateTimer.ElapsedMilliseconds > 5000)
                {
                    MapReadLoop();
                    playerUpdateTimer.Restart();
                }
                UpdatePlayerPositions();
            }
            catch (Exception ex)
            {
                Program.Log($"CRITICAL ERROR: {ex}");
                _players.Clear();
                //_isReady = false;
                throw;
            }
        }

        public void UpdatePlayersList()
        {
            try
            {
                //Console.Clear();
                //Program.Log($"Game is on map: {_currentMapName}");
                playerLock.WaitOne();
                Dictionary<int, Player> newPlayers = new();

                var gameStateBase = Memory.ReadPtr(_world + Offsets.GameStateBase); //AGameStateBase
                var playerArray = Memory.ReadValue<TArray>(gameStateBase + Offsets.PlayerArray); //TArray<APlayerState*>

                var playerScatterMap = new ScatterReadMap(playerArray.Count);
                var playerStateRound = playerScatterMap.AddRound(Memory.PID, false);
                var playerPawnPrivateRound = playerScatterMap.AddRound(Memory.PID, false);
                var playerNickNameCachedRound = playerScatterMap.AddRound(Memory.PID, false);
                var playerRootComponentRound = playerScatterMap.AddRound(Memory.PID, false);
                var playerControllerRound = playerScatterMap.AddRound(Memory.PID, false);
                var playerAbilitySystemComponentRound = playerScatterMap.AddRound(Memory.PID, false);
                var playerSpawnedAttributesComponentRound = playerScatterMap.AddRound(Memory.PID, false);
                var playerSkeletonMeshComponentRound = playerScatterMap.AddRound(Memory.PID, false);
                var playerCompToWorldRound = playerScatterMap.AddRound(Memory.PID, false);
                var playerAccountDataReplicationRound = playerScatterMap.AddRound(Memory.PID, false);
                var playerLocationRound = playerScatterMap.AddRound(Memory.PID, false);
                var playerRotationRound = playerScatterMap.AddRound(Memory.PID, false);
                var playerEquipmentRound = playerScatterMap.AddRound(Memory.PID, false);
                var playerEquipmentArrayRound = playerScatterMap.AddRound(Memory.PID, false);

                //traverse the player array
                for (int i = 0; i < playerArray.Count; i++)
                {
                    var playerStateRoundEntry = playerStateRound.AddEntry<ulong>(i, 0, playerArray.Data, typeof(ulong), (uint)(i * sizeof(ulong)));
                    var playerPawnPrivateRoundEntry = playerPawnPrivateRound.AddEntry<ulong>(i, 1, playerStateRoundEntry, typeof(ulong), Offsets.PlayerState.PlayerPawnPrivate);
                    playerNickNameCachedRound.AddEntry<FNickname>(i, 2, playerPawnPrivateRoundEntry, typeof(FNickname), Offsets.PawnPrivate.NickNameCached);
                    var playerRootComponentRoundEntry = playerRootComponentRound.AddEntry<ulong>(i, 3, playerPawnPrivateRoundEntry, typeof(ulong), Offsets.PawnPrivate.RootComponent);
                    playerControllerRound.AddEntry<ulong>(i, 4, playerPawnPrivateRoundEntry, typeof(ulong), Offsets.PawnPrivate.PlayerController);
                    var playerAbilitySystemComponentRoundEntry = playerAbilitySystemComponentRound.AddEntry<ulong>(i, 5, playerPawnPrivateRoundEntry, typeof(ulong), Offsets.PawnPrivate.AbilitySystemComponent);
                    playerSpawnedAttributesComponentRound.AddEntry<TArray>(i, 6, playerAbilitySystemComponentRoundEntry, typeof(TArray), Offsets.PawnPrivate.SpawnedAttributes);
                    var playerSkeletonMeshComponentRoundEntry = playerSkeletonMeshComponentRound.AddEntry<ulong>(i, 7, playerPawnPrivateRoundEntry, typeof(ulong), Offsets.PawnPrivate.SkeletalMeshComponent);
                    var playerCompToWorldRoundEntry = playerCompToWorldRound.AddEntry<FTransform>(i, 8, playerSkeletonMeshComponentRoundEntry, typeof(FTransform), Offsets.PawnPrivate.ComponentToWorld);
                    playerAccountDataReplicationRound.AddEntry<FAccountDataReplication>(i, 9, playerPawnPrivateRoundEntry, typeof(FAccountDataReplication), Offsets.PawnPrivate.AccountDataReplication);
                    playerLocationRound.AddEntry<FVector3>(i, 10, playerRootComponentRoundEntry, typeof(FVector3), Offsets.RootComponent.Location);
                    playerRotationRound.AddEntry<FRotator>(i, 11, playerRootComponentRoundEntry, typeof(FRotator), Offsets.RootComponent.Rotation);
                    var playerEquipmentRoundEntry = playerEquipmentRound.AddEntry<ulong>(i, 12, playerPawnPrivateRoundEntry, typeof(ulong), Offsets.PawnPrivate.EquipmentInventory);
                    playerEquipmentArrayRound.AddEntry<TArray>(i, 13, playerEquipmentRoundEntry, typeof(TArray), 0x110);
                }


                playerScatterMap.Execute(Memory.Mem);
                for (int i = 0; i < playerArray.Count; i++)
                {
                    if (playerScatterMap.Results[i][0].TryGetResult<ulong>(out var playerState) &&
                    playerScatterMap.Results[i][1].TryGetResult<ulong>(out var playerPawnPrivate) &&
                    playerPawnPrivate != 0)
                    {
                        playerScatterMap.Results[i][2].TryGetResult<FNickname>(out var playerNickNameCached);
                        playerScatterMap.Results[i][3].TryGetResult<ulong>(out var playerRootComponent);
                        playerScatterMap.Results[i][4].TryGetResult<ulong>(out var playerController);
                        playerScatterMap.Results[i][5].TryGetResult<ulong>(out var playerAbilitySystemComponent);
                        playerScatterMap.Results[i][6].TryGetResult<TArray>(out var playerSpawnedAttributes);
                        playerScatterMap.Results[i][7].TryGetResult<ulong>(out var playerSkeletonMeshComponent);
                        playerScatterMap.Results[i][8].TryGetResult<FTransform>(out var playerCompToWorld);
                        playerScatterMap.Results[i][9].TryGetResult<FAccountDataReplication>(out var playerAccountDataReplication);
                        playerScatterMap.Results[i][10].TryGetResult<FVector3>(out var playerLocation);
                        playerScatterMap.Results[i][11].TryGetResult<FRotator>(out var playerRotation);
                        playerScatterMap.Results[i][12].TryGetResult<ulong>(out var playerEquipmentInventory);
                        playerScatterMap.Results[i][13].TryGetResult<TArray>(out var playerEquipmentItemActorsArray);
                        //Program.Log($"Player Name: {Memory.ReadFString(playerNickNameCached.OriginalNickName)} SkeletonMesh : 0x{playerSkeletonMeshComponent:X}");
                        //// Bone test content
                        var playerBoneArray = Memory.ReadPtr(playerSkeletonMeshComponent + 0x5F8);
                        var playerBoneArrayCount = Memory.ReadValue<uint>(playerSkeletonMeshComponent + 0x5F8 + 0x8);
                        //Program.Log($"First Bone Array : 0x{playerBoneArray:X}, Bone Count: {playerBoneArrayCount}");
                        if (playerBoneArray == 0 || playerBoneArrayCount == 0 || playerBoneArrayCount == 1)
                        {
                            playerBoneArray = Memory.ReadPtr(playerSkeletonMeshComponent + 0x608);
                            playerBoneArrayCount = Memory.ReadValue<uint>(playerSkeletonMeshComponent + 0x608 + 0x8);
                            //Program.Log($"First Bone Array Empty: Second Bone Array : 0x{playerBoneArray:X}, Bone Count: {playerBoneArrayCount}");
                            while(playerBoneArray == 0)
                            {
                                playerBoneArray = Memory.ReadPtr(playerSkeletonMeshComponent + 0x608);
                            }
                        }
                        //Program.Log($" Final Bone Array : 0x{playerBoneArray:X}, Bone Count: {playerBoneArrayCount}");
                        HashSet<int> requiredBoneIndices = new HashSet<int>(BoneConnections.SelectMany(connection => new[] { (int)connection.Item1, (int)connection.Item2 }));
                        Dictionary<int, Bone> playerBones = new Dictionary<int, Bone>();
                        foreach (int index in requiredBoneIndices)
                        {
                            if (index < playerBoneArrayCount) // Ensure the index is within the array bounds
                            {
                                ulong boneAddress = playerBoneArray + (ulong)(index * 0x60); // Calculate address of the specific bone
                                var transform = Memory.ReadValue<FTransform>(boneAddress); // Read the transformation
                                playerBones[index] = new Bone
                                {
                                    Address = boneAddress,
                                    Transform = transform
                                };
                            }
                        }
                        //for (int j = 0; j < playerBoneArrayCount; j++)
                        //{
                        //    var transform = Memory.ReadValue<FTransform>(playerBoneArray + (ulong)(j * 0x60));                           
                        //    Bone bone = new Bone()
                        //    {
                        //        Address = playerBoneArray + (ulong)(j * 0x60),
                        //        Transform = transform,
                        //    };
                        //    playerBones.Add(j, bone);
                        //}
                        //// Bone end test content
                        var weaponLabel = "";
                        for (int j = 0; j < playerEquipmentItemActorsArray.Count; j++)
                        {
                            var playerEquipmentItemActor = Memory.ReadPtr(playerEquipmentItemActorsArray.Data + (ulong)(j * 0x8));
                            //Program.Log($"Player {Memory.ReadFString(playerNickNameCached.OriginalNickName)} Equipment Item {j} Actor: 0x{playerEquipmentItemActor:X}");
                            var ItemInfo = Memory.ReadValue<FDCItemInfo>(playerEquipmentItemActor + 0x338);
                            var DesignDataItem = Memory.ReadValue<FDesignDataItem>(playerEquipmentItemActor + 0x4B0);
                            weaponLabel += $"{Memory.ReadName((uint)ItemInfo.ItemData.ItemId.PrimaryAssetName.ComparisonIndex).Replace("Id_Item_", "")} - {Memory.ReadName((uint)DesignDataItem.RarityType.TagName.ComparisonIndex).Replace("Type.Item.Rarity.", "")} ";
                            //Program.Log($"Player: {Memory.ReadFString(playerNickNameCached.OriginalNickName)} - equipped item: {Memory.ReadName((uint)ItemInfo.ItemData.ItemId.PrimaryAssetName.ComparisonIndex).Replace("Id_Item_","")} - {Memory.ReadName((uint)DesignDataItem.RarityType.TagName.ComparisonIndex).Replace("Type.Item.Rarity.", "")}");
                        }
                        var playerSpawnedAttributesData = Memory.ReadPtr(playerSpawnedAttributes.Data);
                        var playerStats = Memory.ReadValue<FGamePlayAttributeDataSet>(playerSpawnedAttributesData);
                        _ = uint.TryParse(Memory.ReadFString(playerAccountDataReplication.PartyId), out uint partyIDInt) ? partyIDInt : 0;

                        newPlayers.Add(i, new Player
                        {
                            Type = playerController == _localPlayerController ? PlayerType.LocalPlayer : PlayerType.Default,
                            Name = Memory.ReadFString(playerNickNameCached.OriginalNickName),
                            Weapon = weaponLabel,
                            Class = Memory.ReadFString(playerNickNameCached.StreamingModeNickName).Split('#')[0],
                            Health = (int)playerStats.Health.CurrentValue, 
                            MaxHealth = (int)playerStats.MaxHealth.CurrentValue,
                            Strength = (int)playerStats.Strength.CurrentValue,
                            Vigor = (int)playerStats.Vigor.CurrentValue,
                            Agility = (int)playerStats.Agility.CurrentValue,
                            Dexterity = (int)playerStats.Dexterity.CurrentValue,
                            Will = (int)playerStats.Will.CurrentValue,
                            Knowledge = (int)playerStats.Knowledge.CurrentValue,
                            Resourcefulness = (int)playerStats.Resourcefulness.CurrentValue,
                            PhysicalDamageWeaponPrimary = (int)playerStats.PhysicalDamageWeaponPrimary.CurrentValue,
                            PhysicalDamageBase = (int)playerStats.PhysicalDamageBase.CurrentValue,
                            PhysicalPower = (int)playerStats.PhysicalPower.CurrentValue,
                            ArmorRating = (int)playerStats.ArmorRating.CurrentValue,
                            RootComponentPtr = playerRootComponent,
                            CompToWorld = playerCompToWorld,
                            SkeletonMeshPtr = playerSkeletonMeshComponent,
                            PawnPrivatePtr = playerPawnPrivate,
                            Location = new Vector3((float)playerCompToWorld.Translation.X * 0.08f, (float)playerCompToWorld.Translation.Y * 0.08f, (float)playerCompToWorld.Translation.Z * 0.08f),
                            Rotation = new FRotator(playerRotation.Yaw, playerRotation.Pitch, playerRotation.Roll),
                            Level = playerAccountDataReplication.Level,
                            PartyID = partyIDInt,
                            Bones = playerBones,
                        });

                    }
                }
                //Program.Log($"Adding {newPlayers.Count} players to the list");
                // Assuming playerSnapshots is now a Dictionary<string, Player.PlayerSnapshot>
               
                _players = newPlayers.ToDictionary(x => x.Key, x => x.Value);

            }
            catch (Exception ex)
            {
                Program.Log($"Error: {ex.Message}");
            }
            finally
            {
                playerLock.Release();
            }
        }

        public void UpdatePlayerPositions()
        {
            try
            {
                //Console.Clear();
                //update player positions
                playerLock.WaitOne();

                //get players dictionary
                foreach (var player in _players)
                {
                    var playerScatterMap = new ScatterReadMap(1);

                    var playerCompToWorldRound = playerScatterMap.AddRound(Memory.PID, false);
                    var playerLocationRound = playerScatterMap.AddRound(Memory.PID, false);
                    var playerRootComponentRound = playerScatterMap.AddRound(Memory.PID, false);
                    var playerRotationRound = playerScatterMap.AddRound(Memory.PID, false);

                    for (int i = 0; i < 1; i++)
                    {
                        var playerCompToWorldRoundEntry = playerCompToWorldRound.AddEntry<FTransform>(i, 0, player.Value.SkeletonMeshPtr, typeof(FTransform), Offsets.ComponentToWorld);
                        var playerRootComponentRoundEntry = playerRootComponentRound.AddEntry<ulong>(i, 1, player.Value.PawnPrivatePtr, typeof(ulong), Offsets.PawnPrivate.RootComponent);
                        var playerRotationRoundEntry = playerRotationRound.AddEntry<FRotator>(i, 2, playerRootComponentRoundEntry, typeof(FRotator), Offsets.RootComponent.Rotation);
                    }
                    playerScatterMap.Execute(Memory.Mem);
                    for(int i = 0;i < 1; i++)
                    {
                        if(playerScatterMap.Results[i][0].TryGetResult<FTransform>(out var playerCompToWorld) &&
                        playerScatterMap.Results[i][1].TryGetResult<ulong>(out var playerRootComponent) &&
                        playerScatterMap.Results[i][2].TryGetResult<FRotator>(out var playerRotation))
                        {
                            //update bones
                            var newBones = new Dictionary<int, Bone>(player.Value.Bones);
                            for (int j = 0; j < player.Value.Bones.Count; j++)
                            {
                                if (player.Value.Bones.TryGetValue(j, out var bone))
                                {
                                    var transform = Memory.ReadValue<FTransform>(bone.Address);
                                    bone.Transform = transform;
                                    newBones[j] = bone;
                                }
                            }
                            _players[player.Key].CompToWorld = playerCompToWorld;
                            _players[player.Key].Location = new Vector3((float)playerCompToWorld.Translation.X * 0.08f, (float)playerCompToWorld.Translation.Y * 0.08f, (float)playerCompToWorld.Translation.Z * 0.08f);
                            _players[player.Key].Rotation = new FRotator(playerRotation.Yaw, playerRotation.Pitch, playerRotation.Roll);
                            _players[player.Key].Bones = newBones;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Program.Log($"Error: {ex.Message}");
            }
            finally
            {
                playerLock.Release();
            }
        }
        public void WaitForNewGame()
        {
            while (true)
            {
                //Console.Clear();
                _world = Memory.ReadPtr(Memory.ModuleBase + Offsets.GWorld);
                //Program.Log($"World: 0x{_world:X}");
                _owningGameInstance = Memory.ReadPtr(_world + Offsets.OwningGameInstance);
                //Program.Log($"OwningGameInstance: 0x{_owningGameInstance:X}");

                if (_owningGameInstance != 0)
                {
                    var localPlayersPtr = Memory.ReadPtr(_owningGameInstance + Offsets.LocalPlayers);
                    var localPlayer = Memory.ReadPtr(localPlayersPtr);
                    //Program.Log($"LocalPlayer: 0x{localPlayer:X}");
                    var localPlayerController = Memory.ReadPtr(localPlayer + Offsets.PlayerController);
                    //Program.Log($"(waitfornewgame) LocalPlayerController: 0x{localPlayerController:X}");
                    var HUD = Memory.ReadPtr(localPlayerController + Offsets.PlayerHUD);
                    var HUDnameIndex = Memory.ReadValue<uint>(HUD + Offsets.NameIndex);
                    //Program.Log($"HUD: {Memory.ReadName(HUDnameIndex)}");
                    if ((Memory.ReadName(HUDnameIndex) == "BP_HUDDungeon_C"))
                    {
                        _inGame = true;
                        _localPlayer = localPlayer;
                        _localPlayerController = localPlayerController;
                        playerUpdateTimer.Start();
                        EntityManager.Start();
                        Memory.GameStatus = Enums.GameStatus.InGame;
                        break;
                    }
                }
            }
        }
       
    }
}
