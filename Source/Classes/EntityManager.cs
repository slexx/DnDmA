using DMAW_DND;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using vmmsharp;
using static DMAW_DND.Enums;

namespace DMAW_DND
{

    internal class EntityManager
    {
        public static bool bLootEnabled = true;
        //public static ulong _gWorld;
        //_gWorld = Memory.GWorld get/set
        //public static ulong _gWorld = Memory.game.World;
        static Dictionary<int, Item> _items = new Dictionary<int, Item>();

        public class ActorInfo
        {
            public string Name { get; set; }
            public Enums.ActorType Type { get; set; }
            public bool? IsOpen { get; set; } = null;
            public bool? IsHidden { get; set; } = null;
            public int? Health { get; set; } = null;
            public int? MaxHealth { get; set; } = null;

            public ActorInfo(string name, Enums.ActorType type)
            {
                Name = name;
                Type = type;
            }
        }

        public static Dictionary<string, ActorInfo> ActorsToDefine = new Dictionary<string, ActorInfo>
        {
            // Examples of adding actor info
            //Items
            {"BP_StaticMeshItemHolder_C", new ActorInfo("Item", ActorType.Item)},
            //Shrines
            {"BP_AltarOfSacrifice_C", new ActorInfo("Altar of Sacrifice", ActorType.Statue)},
            {"BP_Statue01_C", new ActorInfo("Health Statue", ActorType.Statue)},
            {"BP_Statue02_C", new ActorInfo("Shield Statue", ActorType.Statue)},
            {"BP_Statue03_C", new ActorInfo("Power Statue", ActorType.Statue)},
            {"BP_Statue04_C", new ActorInfo("Speed Statue", ActorType.Statue)},
            //Portals
            {"BP_FloorPortalScrollEscape_C", new ActorInfo("Escape Portal", ActorType.Portal)},
            {"BP_FloorPortalScrollDown_C", new ActorInfo("Down Portal", ActorType.Portal)},
            //Chests
            {"BP_Chest_Marvelous_C", new ActorInfo("Marvelous Chest", ActorType.Chest)},
            {"BP_MarvelousChest_HR2_C", new ActorInfo("Marvelous Chest", ActorType.Chest)},
            {"BP_MarvelousChest_HR_C", new ActorInfo("Marvelous Chest", ActorType.Chest)},
            {"BP_OrnateChestLarge_G1_C", new ActorInfo("Ornate Chest Large", ActorType.Chest)},
            {"BP_OrnateChestMedium_G1_C", new ActorInfo("Ornate Chest Medium", ActorType.Chest)},
            {"BP_OrnateChestSmall_G1_C", new ActorInfo("Ornate Chest Small", ActorType.Chest)},
            {"BP_OrnateChestLarge_N1_C", new ActorInfo("Ornate Chest Large", ActorType.Chest)},
            {"BP_OrnateChestMedium_N1_C", new ActorInfo("Ornate Chest Medium", ActorType.Chest)},
            {"BP_OrnateChestSmall_N1_C", new ActorInfo("Ornate Chest Small", ActorType.Chest)},
            {"BP_OrnateChestLarge_N2_C", new ActorInfo("Ornate Chest Large", ActorType.Chest)},
            {"BP_OrnateChestLarge_N3_C", new ActorInfo("Ornate Chest Large", ActorType.Chest)},
            {"BP_GoldChest_HR2_C", new ActorInfo("Gold Chest", ActorType.Chest)},
            {"BP_GoldChest_HR_C", new ActorInfo("Gold Chest", ActorType.Chest)},
            {"BP_OrnateChestLarge_HR2_C", new ActorInfo("Ornate Chest Large", ActorType.Chest)},
            {"BP_OrnateChestMedium_HR2_C", new ActorInfo("Ornate Chest Medium", ActorType.Chest)},
            {"BP_OrnateChestSmall_HR2_C", new ActorInfo("Ornate Chest Small", ActorType.Chest)},
            {"BP_OrnateChestLarge_HR_C", new ActorInfo("Ornate Chest Large", ActorType.Chest)},
            {"BP_OrnateChestMedium_HR_C", new ActorInfo("Ornate Chest Medium", ActorType.Chest)},
            {"BP_OrnateChestSmall_HR_C", new ActorInfo("Ornate Chest Small", ActorType.Chest)},
            {"BP_OrnateChestLarge_N0_C", new ActorInfo("Ornate Chest Large", ActorType.Chest)},
            {"BP_MarvelousChest_N0_C", new ActorInfo("Marvelous Chest", ActorType.Chest)},
            {"BP_GoldChest_N0_C", new ActorInfo("Gold Chest", ActorType.Chest)},
            {"BP_GoldChest_N1_C", new ActorInfo("Gold Chest", ActorType.Chest)},

            //Special
            {"BP_CaveTroll_Common_C", new ActorInfo("Cave Troll", ActorType.Boss)},
            {"BP_CaveTroll_Elite_C", new ActorInfo("Cave Troll", ActorType.Boss)},
            {"BP_GhostKing_Common_C", new ActorInfo("Ghost King", ActorType.Boss)},
            {"BP_GhostKing_Elite_C", new ActorInfo("Ghost King", ActorType.Boss)},
            {"BP_Lich_Common_C", new ActorInfo("Lich", ActorType.Boss)},
            {"BP_Lich_Elite_C", new ActorInfo("Lich", ActorType.Boss)},
            {"BP_Cyclops_Common_C", new ActorInfo("Cyclops", ActorType.Boss)},
            {"BP_Cyclops_Elite_C", new ActorInfo("Cyclops", ActorType.Boss)},
            {"BP_SkeletonWarlord_Common_C", new ActorInfo("Skeleton Warlord", ActorType.Boss)},
            {"BP_SkeletonWarlord_Elite_C", new ActorInfo("Skeleton Warlord", ActorType.Boss)},
            //Mimics
            {"BP_Mimic_Large_Flat_Common_C", new ActorInfo("Mimic", ActorType.Mimic)},
            {"BP_Mimic_Large_Flat_Elite_C", new ActorInfo("Mimic", ActorType.Mimic)},
            {"BP_Mimic_Large_Flat_Nightmare_C", new ActorInfo("Mimic", ActorType.Mimic)},
            {"BP_Mimic_Large_MidLevel_Common_C", new ActorInfo("Mimic", ActorType.Mimic)},
            {"BP_Mimic_Large_MidLevel_Elite_C", new ActorInfo("Mimic", ActorType.Mimic)},
            {"BP_Mimic_Large_MidLevel_Nightmare_C", new ActorInfo("Mimic", ActorType.Mimic)},
            {"BP_Mimic_Large_Ornate_Common_C", new ActorInfo("Mimic", ActorType.Mimic)},
            {"BP_Mimic_Large_Ornate_Elite_C", new ActorInfo("Mimic", ActorType.Mimic)},
            {"BP_Mimic_Large_Ornate_Nightmare_C", new ActorInfo("Mimic", ActorType.Mimic)},
            {"BP_Mimic_Large_Simple_Common_C", new ActorInfo("Mimic", ActorType.Mimic)},
            {"BP_Mimic_Large_Simple_Elite_C", new ActorInfo("Mimic", ActorType.Mimic)},
            {"BP_Mimic_Large_Simple_Nightmare_C", new ActorInfo("Mimic", ActorType.Mimic)},
            {"BP_Mimic_Medium_MidLevel_Common_C", new ActorInfo("Mimic", ActorType.Mimic)},
            {"BP_Mimic_Medium_MidLevel_Elite_C", new ActorInfo("Mimic", ActorType.Mimic)},
            {"BP_Mimic_Medium_MidLevel_Nightmare_C", new ActorInfo("Mimic", ActorType.Mimic)},
            {"BP_Mimic_Medium_Ornate_Common_C", new ActorInfo("Mimic", ActorType.Mimic)},
            {"BP_Mimic_Medium_Simple_Common_C", new ActorInfo("Mimic", ActorType.Mimic)},
            {"BP_Mimic_Small_Simple_Common_C", new ActorInfo("Mimic", ActorType.Mimic)},
            //Levers
            {"BP_Bookshelf_Book_A01_GEN_VARIABLE_BP_Bookshelf_Book_A01_C_CAT", new ActorInfo("Book Lever", ActorType.Lever)}, //BP_Bookshelf_Book_A01_GEN_VARIABLE_BP_Bookshelf_Book_A01_C_CAT
            {"BP_StatueLever_C", new ActorInfo("Statue Lever", ActorType.Lever)},
            {"BP_ChaliceLever_C", new ActorInfo("Chalice Lever", ActorType.Lever)},
            {"BP_MetalGobletLever_C", new ActorInfo("Goblet Lever", ActorType.Lever)},
            {"BP_PressurePlate_OnlyActivate_C", new ActorInfo("Pressure Plate", ActorType.Lever)},
            {"BP_PressurePlate_C", new ActorInfo("Pressure Plate", ActorType.Lever)},
            //Keys
            {"BP_GoldenKey_C", new ActorInfo("Golden Key", ActorType.Special)},
            {"BP_SkullKey_C", new ActorInfo("Skull Key", ActorType.Special)},
            //Ore
            {"BP_CopperOre_N_C", new ActorInfo("Copper Ore", ActorType.Ore)},
            {"BP_IronOre_N_C", new ActorInfo("Iron Ore", ActorType.Ore)},
            {"BP_CobaltOre_N_C", new ActorInfo("Cobalt Ore", ActorType.Ore)},
            {"BP_RubysilverOre_N_C", new ActorInfo("RubySilver Ore", ActorType.Ore)},
            {"BP_GoldOre_N_C", new ActorInfo("Gold Ore", ActorType.Ore) },
            {"BP_CopperOre_HR_C", new ActorInfo("Copper Ore", ActorType.Ore)},
            {"BP_IronOre_HR_C", new ActorInfo("Iron Ore", ActorType.Ore)},
            {"BP_CobaltOre_HR_C", new ActorInfo("Cobalt Ore", ActorType.Ore)},
            {"BP_RubysilverOre_HR_C", new ActorInfo("RubySilver Ore", ActorType.Ore)},
            {"BP_GoldOre_HR_C", new ActorInfo("Gold Ore", ActorType.Ore) },
        };


        /*public static List<string> ActorsToDraw = new List<string>()
        //public static HashSet<string> ActorsToDraw = new HashSet<string>()
        {
            //Items
            //"BP_StaticMeshItemHolder_C",
            ////Keys?
            //"BP_GoldenKey_C",
            ////Potions
            //"BP_InvisibilityPotion_C",
            //"BP_ProtectionPotion_C",
            //"BP_HealingPotion_C",
            ////Misc
            //"BP_CampfireKit_C",
            //"BP_SurgicalKit_C",
            //"BP_PressurePlate_OnlyActivate_C",
            //Shrines
            "BP_AltarOfSacrifice_C",
            "BP_Statue01_C", //Health Statue
            "BP_Statue02_C", //Shield Statue
            "BP_Statue03_C", //Power Statue
            "BP_Statue04_C", //Speed Statue
            //Chests
            "BP_Chest_Marvelous_C",
            "BP_OrnateChestLarge_G1_C",
            "BP_OrnateChestMedium_G1_C",
            "BP_OrnateChestSmall_G1_C",
            //"BP_WoodChestLarge_G1_C",
            //"BP_WoodChestMedium_G1_C",
            //"BP_WoodChestSmall_G1_C",
            //"BP_SimpleChestLarge_G1_C",
            //"BP_SimpleChestMedium_G1_C",
            //"BP_SimpleChestSmall_G1_C",
            "BP_OrnateChestLarge_N1_C",
            "BP_OrnateChestMedium_N1_C",
            "BP_OrnateChestSmall_N1_C",
            //"BP_WoodChestLarge_N1_C",
            //"BP_WoodChestMedium_N1_C",
            //"BP_WoodChestSmall_N1_C",
            //"BP_SimpleChestLarge_N1_C",
            //"BP_SimpleChestMedium_N1_C",
            //"BP_SimpleChestSmall_N1_C",
            //"BP_FlatChestLarge_N1_C",
            //"BP_FlatChestMedium_N1_C",
            //"BP_FlatChestSmall_N1_C",
            //"BP_MarvelousChest_N1_C",
            //"BP_MarvelousChest_N2_C",
            "BP_OrnateChestLarge_N2_C",
            //"BP_SimpleChestSmall_N2_C",
            //"BP_SimpleChestMedium_N2_C",
            //"BP_SimpleChestLarge_N2_C",
            //"BP_FlatChestSmall_N2_C",
            //"BP_FlatChestMedium_N2_C",
            //"BP_FlatChestLarge_N2_C",
            //"BP_WoodChestSmall_N2_C",
            //"BP_WoodChestMedium_N2_C",
            //"BP_WoodChestLarge_N2_C",
            //"BP_MarvelousChest_N3_C",
            "BP_OrnateChestLarge_N3_C",
            //"BP_SimpleChestSmall_N3_C",
            //"BP_SimpleChestMedium_N3_C",
            //"BP_SimpleChestLarge_N3_C",
            //"BP_FlatChestSmall_N3_C",
            //"BP_FlatChestMedium_N3_C",
            //"BP_FlatChestLarge_N3_C",
            //"BP_WoodChestSmall_N3_C",
            //"BP_WoodChestMedium_N3_C",
            //"BP_WoodChestLarge_N3_C",
            //Traps
            //"BP_WallSpike_C",
            //"BP_FloorSpikes_C",
            //Portals
            "BP_FloorPortalScrollEscape_C",
            "BP_FloorPortalScrollDown_C",
            //Goblins
            //"BP_GoblinWarrior_C",
            //"BP_GoblinWarrior_Elite_C",
            //"BP_GoblinWarrior_Nightmare_C",
            //"BP_GoblinAxeman_C",
            //"BP_GoblinAxeman_Elite_C",
            //"BP_GoblinAxeman_Nightmare_C",
            //"BP_GoblinMage_C",
            //"BP_GoblinMage_Elite_C",
            //"BP_GoblinMage_Nightmare_C",
            //"BP_GoblinArcher_C",
            //"BP_GoblinArcher_Elite_C",
            //"BP_GoblinArcher_Nightmare_C",
            //"BP_GoblinBolaslinger_C",
            //"BP_GoblinBolaslinger_Elite_C",
            //"BP_GoblinBolaslinger_Nightmare_C",
            "BP_CaveTroll_C",
            //Undead
            //"BP_Mummy_Common_C",
            //"BP_Mummy_Elite_C",
            //"BP_Mummy_Nightmare_C",
            //"BP_Zombie_Common_C",
            //"BP_Zombie_Elite_C",
            //"BP_SkeletonArcher_Common_C",
            //"BP_SkeletonArcher_Elite_C",
            //"BP_SkeletonArcher_Nightmare_C",
            //"BP_SkeletonSpearman_C",
            //"BP_SkeletonSpearman_Elite_C",
            //"BP_SkeletonSpearman_Nightmare_C",
            //"BP_SkeletonAxeman_C",
            //"BP_SkeletonAxeman_Elite_C",
            //"BP_SkeletonAxeman_Nightmare_C",
            //"BP_SkeletonGuardmanFromFakeDeath_Common_C",
            //"BP_SkeletonGuardmanFromFakeDeath_Elite_C",
            //"BP_SkeletonGuardmanFromFakeDeath_Nightmare_C",
            //"BP_SkeletonCrossbowman_C",
            //"BP_SkeletonCrossbowman_Elite_C",
            //"BP_SkeletonCrossbowman_Nightmare_C",
            //"BP_SkeletonSwordman_C",
            //"BP_SkeletonSwordman_Elite_C",
            //"BP_SkeletonSwordman_Nightmare_C",
            //"BP_DeathSkull_Common_C",
            //"BP_SkeletonWoodenBarrel_N_C",
            //"BP_SkeletonWoodBarrel_Elite_N_C",
            //"BP_SkeletonWoodBarrel_Nightmare_N_C",
            //"BP_SkeletonFootmanFromFakeDeath_Common_C",
            //"BP_SkeletonFootmanFromFakeDeath_Elite_C",
            //"BP_SkeletonFootmanFromFakeDeath_Nightmare_C",
            //"BP_SkeletonMage_Common_C",
            //"BP_SkeletonMage_Elite_C",
            //"BP_SkeletonMage_Nightmare_C",
            //"BP_SkeletonRoyalGuard_C",
            //"BP_SkeletonChampion_Common_C",
            //"BP_SkeletonChampion_Elite_C",
            //"BP_Wraith_Common_C",
            //"BP_Wraith_Elite_C",
            //"BP_Lich_Common_C",
            "BP_GhostKing_Common_C",
            //Demons
            //"BP_DemonBerseker_Common_C",
            //"BP_DemonBerseker_Elite_C",
            //"BP_DemonDog_Common_C",
            //"BP_DemonDog_Elite_C",
            //"BP_DireWolf_Common_C",
            //"BP_DireWolf_Elite_C",
            //"BP_CentaurDemon_Common_C",
            //"BP_CentaurDemon_Elite_C",
            //Special
            "BP_Wisp_C",
            //Bugs
            //"BP_GiantCentipede_C",
            //"BP_GiantCentipede_Elite_C",
            //"BP_GiantWorm_Common_C",
            //"BP_GiantWorm_Elite_C",
            //"BP_DeathBeetle_Common_C",
            //"BP_DeathBeetle_Elite_C",
            //"BP_DeathBeetle_Nightmare_C",
            //"BP_GiantDragonfly_Common_C",
            //"BP_GiantDragonfly_Elite_C",
            //"BP_GiantDragonfly_Nightmare_C",
            //"BP_GiantBat_C",
            //"BP_GiantBat_Elite_C",
            //"BP_GiantSpider_C",
            //"BP_GiantSpider_Elite_C",
            //"BP_GiantSpider_Nightmare_C",
            //"BP_SpiderMummy_Common_C",
            //"BP_SpiderMummy_Elite_C",
            //"BP_SpiderMummy_Nightmare_C",
            //Mimics
            "BP_Mimic_Large_Flat_Common_C",
            "BP_Mimic_Large_Flat_Elite_C",
            "BP_Mimic_Large_Flat_Nightmare_C",
            "BP_Mimic_Large_MidLevel_Common_C",
            "BP_Mimic_Large_MidLevel_Elite_C",
            "BP_Mimic_Large_MidLevel_Nightmare_C",
            "BP_Mimic_Large_Ornate_Common_C",
            "BP_Mimic_Large_Ornate_Elite_C",
            "BP_Mimic_Large_Ornate_Nightmare_C",
            "BP_Mimic_Large_Simple_Common_C",
            "BP_Mimic_Large_Simple_Elite_C",
            "BP_Mimic_Large_Simple_Nightmare_C",
            "BP_Mimic_Medium_MidLevel_Common_C",
            "BP_Mimic_Medium_MidLevel_Elite_C",
            "BP_Mimic_Medium_MidLevel_Nightmare_C",
            "BP_Mimic_Medium_Ornate_Common_C",
            "BP_Mimic_Medium_Ornate_Elite_C",
            "BP_Mimic_Medium_Ornate_Nightmare_C",
            "BP_Mimic_Medium_Simple_Common_C",
            "BP_Mimic_Medium_Simple_Elite_C",
            "BP_Mimic_Medium_Simple_Nightmare_C",
            "BP_Mimic_Small_MidLevel_Common_C",
            "BP_Mimic_Small_MidLevel_Elite_C",
            "BP_Mimic_Small_MidLevel_Nightmare_C",
            "BP_Mimic_Small_Ornate_Common_C",
            "BP_Mimic_Small_Ornate_Elite_C",
            "BP_Mimic_Small_Ornate_Nightmare_C",
            "BP_Mimic_Small_Simple_Common_C",
            "BP_Mimic_Small_Simple_Elite_C",
            //Levers
            //"BP_FloorLever_C",
            //"BP_WallLever_C",
            //"BP_StatueLever_C",
            //"BP_ChaliceLever_C",
            //"BP_MetalGobletLever_C",
        };
*/

        private static bool _running;
        private static Task _worker;
        public static Stopwatch Stopwatch = new Stopwatch();
        public static CancellationTokenSource ts = new();
        public static CancellationToken ct = ts.Token;

        #region Getters
        public static Dictionary<int, Item> Items
        {
            get { return _items; }
        }

        public static bool Running { get { return _running; } }
        #endregion

        public static void Start()
        {
            if (_running) return;
            _worker = Task.Run(() => Worker(), ct);
            Stopwatch.Start();
            _running = true;
        }

        public static void Worker()
        {
            try
            {
                Stopwatch.Start();
                Console.WriteLine($"EntityManager thread started. Waiting for GWorld...");

                // Wait until GWorld is not zero
                while (Memory.game.World == 0)
                {
                    if (ct.IsCancellationRequested) return;
                    Thread.Sleep(100); // Wait for 100 milliseconds before checking again
                    Program.Log($"Waiting for GWorld: 0x{Memory.game.World:X}");
                    _items.Clear();
                }
                Program.Log($"Main Thread Running - GWorld: 0x{Memory.game.World:X}");
                while (true)
                {
                    if (ct.IsCancellationRequested) break;
                    if (!Memory.Ready || !Config.Ready) continue;
                    if (_running && Memory.game.World != 0)
                    {
                        //_gWorld = Memory.GWorld;
                        if (Stopwatch.ElapsedMilliseconds > 1000 * 10)
                        {
                            BulkItemRead();
                            Stopwatch.Restart();
                        }
                        //UpdateItemPositions();
                        Thread.SpinWait(1000 * 150);
                    }
                }
            }
            catch (Exception e)
            {
                Program.Log($"EntityManager Worker Exception: {e.Message}");
            }
        }

        public static bool Shutdown()
        {
            if (_running)
            {
                Console.WriteLine("[SHUTDOWN] Loot Manager Shutting Down");
                ts.Cancel();
                _running = false;
            }
            return true;
        }

        public static void BulkItemRead()
        {
            if (Memory.game.CurrentMapName == "<empty>" || Memory.game.CurrentMapName == "None" || Memory.game.CurrentMapName == null)
            {
                return;
            }
            try
            {
                var newItemsDict = new Dictionary<int, Item>();
                var LevelsArray = Memory.ReadValue<TArray>(Memory.game.World + Offsets.LevelsArray);

                var levelsScatterMap = new ScatterReadMap(LevelsArray.Count);
                var levelRound = levelsScatterMap.AddRound(Memory.PID, false);
                for (int i = 0; i < LevelsArray.Count; i++)
                {
                    levelRound.AddEntry<ulong>(i, 0, LevelsArray.Data + (ulong)(i * 0x8));
                }
                levelsScatterMap.Execute(Memory.Mem);

                for (int h = 0; h < LevelsArray.Count; h++)
                {
                    if (levelsScatterMap.Results[h][0].TryGetResult<ulong>(out var Level))
                    {
                        var Actors = Memory.ReadValue<TArray>(Level + Offsets.ActorsArray);

                        var actorsScatterMap = new ScatterReadMap(Actors.Count);

                        var actorRound = actorsScatterMap.AddRound(Memory.PID, false);
                        var actorNameIndexRound = actorsScatterMap.AddRound(Memory.PID, false);
                        var actorRootComponentRound = actorsScatterMap.AddRound(Memory.PID, false);
                        var actorComponentToWorldRound = actorsScatterMap.AddRound(Memory.PID, false);
                        var actorInstigatorRound = actorsScatterMap.AddRound(Memory.PID, false);
                        var actorAbilitySystemComponentRound = actorsScatterMap.AddRound(Memory.PID, false);
                        var actorSpawnedAttributesRound = actorsScatterMap.AddRound(Memory.PID, false);
                        var actorPrimaryAssetIDRound = actorsScatterMap.AddRound(Memory.PID, false);
                    

                        for (int i = 0; i < Actors.Count; i++)
                        {
                            var actor = actorRound.AddEntry<ulong>(i, 0, Actors.Data + (uint)(i * 0x8));
                            actorNameIndexRound.AddEntry<uint>(i, 1, actor, typeof(int), Offsets.NameIndex);
                            var actorRootComponent = actorRootComponentRound.AddEntry<ulong>(i, 2, actor, typeof(ulong), Offsets.PawnPrivate.RootComponent);
                            actorComponentToWorldRound.AddEntry<FTransform>(i, 3, actorRootComponent, typeof(FTransform), Offsets.ComponentToWorld);
                            var actorInstigator = actorInstigatorRound.AddEntry<ulong>(i, 4, actor, typeof(ulong), Offsets.Instigator);
                            var aactorAbilitySystemComponent = actorAbilitySystemComponentRound.AddEntry<ulong>(i, 5, actorInstigator, typeof(ulong), Offsets.PawnPrivate.AbilitySystemComponent);
                            actorSpawnedAttributesRound.AddEntry<TArray>(i, 6, aactorAbilitySystemComponent, typeof(TArray), Offsets.PawnPrivate.SpawnedAttributes);
                            actorPrimaryAssetIDRound.AddEntry<uint>(i, 7, actor, typeof(uint), 0x320 + 0x8);
                        }

                        actorsScatterMap.Execute(Memory.Mem);

                        for (int i = 0; i < Actors.Count; i++)
                        {
                            if (actorsScatterMap.Results[i][0].TryGetResult<ulong>(out var Actor))
                             {
                                
                                if (!actorsScatterMap.Results[i][1].TryGetResult<uint>(out var NameIndex)) continue;
                                var name = Memory.ReadName(NameIndex).ToString();
                                var type = ActorType.Unknown;
                                if (ActorsToDefine.TryGetValue(name, out var actorInfo))
                                {

                                    //var InteractableTargetComponent = Memory.ReadPtr(Actor + 0x2F8);
                                    //var InteractableDataByStateMap = Memory.ReadValue<FText>(InteractableTargetComponent + 0xF8);
                                    //var test = InteractableDataByStateMap.Text.Data;
                                    //var test2 = Memory.ReadValue<FGameplayTag>((ulong)test);
                                    //Program.Log($"Actor: {name} - test2: {Memory.ReadName((uint)test2.TagName.ComparisonIndex)}");

                                    actorsScatterMap.Results[i][3].TryGetResult<FTransform>(out var ActorCompontentToWorld);
                                    //Console.WriteLine($"Actor: {name} - ActorInfo Name: {actorInfo.Name}");
                                    var ActorPos = new Vector3((float)ActorCompontentToWorld.Translation.X * 0.08f, (float)ActorCompontentToWorld.Translation.Y * 0.08f, (float)ActorCompontentToWorld.Translation.Z * 0.08f);
                                    switch (actorInfo.Type)
                                    {
                                        case ActorType.Statue:
                                            // Process statue-specific properties
                                            name = actorInfo.Name;
                                            type = actorInfo.Type;
                                            break;
                                        case ActorType.Portal:
                                            // Process statue-specific properties
                                            var SpawnedAttributes = Memory.ReadValue<Offsets.ActorStatus>(Actor);
                                            //actorInfo.IsOpen = SpawnedAttributes.IsOpen;
                                            name = SpawnedAttributes.IsOpen ? actorInfo.Name.ToString() + " (Open/Taken)" : actorInfo.Name.ToString() + " Available";
                                            type = ActorType.Portal;
                                            break;
                                        case ActorType.Boss:
                                            // Process NPC-specific properties like health
                                            if (actorsScatterMap.Results[i][4].TryGetResult<ulong>(out var actorInstigator) &&
                                            actorsScatterMap.Results[i][5].TryGetResult<ulong>(out var actorAbilitySystemComponent) &&
                                            actorsScatterMap.Results[i][6].TryGetResult<TArray>(out var actorSpawnedAttributes))
                                            {
                                                var actorSpawnedAttributesData = Memory.ReadPtr(actorSpawnedAttributes.Data);
                                                var actorStats = Memory.ReadValue<FGamePlayAttributeDataSet>(actorSpawnedAttributesData);

                                                if (actorStats.Health.CurrentValue == 0) continue; // Skip dead enemies
                                                actorInfo.Health = (int)actorStats.Health.CurrentValue;
                                                actorInfo.MaxHealth = (int)actorStats.MaxHealth.CurrentValue;
                                                name = actorInfo.Name + actorInfo.Health.ToString() + "HP";
                                                type = ActorType.Boss;
                                            }
                                            break;
                                        case ActorType.Lever:
                                            name = actorInfo.Name;
                                            type = ActorType.Lever;
                                            break;
                                        case ActorType.Key:
                                            name = actorInfo.Name;
                                            type = ActorType.Special;
                                            break;
                                        case ActorType.Item:
                                            actorsScatterMap.Results[i][7].TryGetResult<uint>(out var actorPrimaryAssetID);
                                            name = Memory.ReadName(actorPrimaryAssetID).ToString();
                                            //only show Id_Item_GoldenKey and Id_Item_SkullKey, and rapiers if on game map inferno
                                            if (!(name == "Id_Item_GoldenKey" || name == "Id_Item_SkullKey" || (name.Contains("Rapier") && Memory.game.CurrentMapName.Contains("Inferno")))) continue;
                                            type = ActorType.Special;
                                            break;
                                        case ActorType.Mimic:
                                            name = actorInfo.Name;
                                            type = ActorType.Mimic;
                                            break;
                                        case ActorType.Chest:
                                            name = actorInfo.Name;
                                            type = ActorType.Chest;
                                            break;
                                        case ActorType.Ore:
                                            name = actorInfo.Name;
                                            type = ActorType.Ore;
                                            break;
                                        // Handle other types as needed
                                        default:
                                            name = actorInfo.Name;
                                            break;
                                    }
                                    // Now actorInfo contains all the necessary information about the actor
                                    // Add it to _items or another suitable collection
                                    var item = new Item()
                                    {
                                        Name = name,
                                        Type = type,
                                        //ActorRootComponent = ActorRootComponent,
                                        CompToWorld = ActorCompontentToWorld,
                                        ActorLocation = ActorPos,
                                        EnemyHealth = actorInfo.Type == ActorType.Special ? (uint?)actorInfo.Health : null,
                                        EnemyMaxHealth = actorInfo.Type == ActorType.Special ? (uint?)actorInfo.MaxHealth : null,
                                        //ActorSkillComponent = ActorAbilitySystemComponent,
                                        //ActorRootComponent = ActorRootComponent,
                                        //spawnedAttributes = ActorSpawnedAttributesData
                                    };
                                    //Program.Log($"Added {name} to newItemsDict - EnemyHealth: {actorInfo.Health} - EnemyMaxHealth: {actorInfo.MaxHealth}");
                                    newItemsDict.Add(newItemsDict.Count, item);
                                }
                            }
                        }
                    }
                    //var level = Memory.ReadPtr(LevelsArray.Data + (ulong)(h * 0x8));
                }
                _items = newItemsDict.ToDictionary(entry => entry.Key, entry => entry.Value);
            }
            catch (Exception e)
            {
                Program.Log($"BulkItemRead Exception: {e.Message}");
            }
        }
    }
}
