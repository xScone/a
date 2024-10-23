﻿using System.Reflection;
using UnityEngine;
using BepInEx;
using LethalLib.Modules;
using BepInEx.Logging;
using System.IO;
using AbandonedAssets.Configuration;
using GameNetcodeStuff;

namespace AbandonedAssets
{
	[BepInPlugin(ModGUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
	[BepInDependency(LethalLib.Plugin.ModGUID)]
	public class Plugin : BaseUnityPlugin
	{
		// It is a good idea for our GUID to be more unique than only the plugin name. Notice that it is used in the BepInPlugin attribute.
		// The GUID is also used for the config file name by default.
		public const string ModGUID = "sconeys." + PluginInfo.PLUGIN_NAME;
		internal static new ManualLogSource Logger;
		//internal static PluginConfig BoundConfig { get; private set; } = null;
		public static AssetBundle ModAssets;
		private int poltergeistspawnrate = 100;


		private void Awake()
		{
			Logger = base.Logger;
			loadConfig();
			InitializeNetworkBehaviours();

			var bundleName = "abandonedassets-dev";
			ModAssets = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Info.Location), bundleName));
			if (ModAssets == null)
			{
				Logger.LogError($"Failed to load custom assets.");
				return;
			}



			var CandleReference = ModAssets.LoadAsset<Item>("candleItem");


			// Network Prefabs need to be registered. See https://docs-multiplayer.unity3d.com/netcode/current/basics/object-spawning/
			// LethalLib registers prefabs on GameNetworkManager.Start.
			NetworkPrefabs.RegisterNetworkPrefab(CandleReference.spawnPrefab);
			Utilities.FixMixerGroups(CandleReference.spawnPrefab);
			Items.RegisterShopItem(CandleReference, 10);
			

			Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
		}

		private static void InitializeNetworkBehaviours()
		{
			// See https://github.com/EvaisaDev/UnityNetcodePatcher?tab=readme-ov-file#preparing-mods-for-patching
			var types = Assembly.GetExecutingAssembly().GetTypes();
			foreach (var type in types)
			{
				var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
				foreach (var method in methods)
				{
					var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
					if (attributes.Length > 0)
					{
						method.Invoke(null, null);
					}
				}
			}
		}

		public int RandomNumber(int value)
		{
			System.Random newrandom = new System.Random(StartOfRound.Instance.randomMapSeed);
			return newrandom.Next(0,100);
		}
		private void loadConfig()
		{
			poltergeistspawnrate = Config.Bind<int>("General", "poltergeistSpawnRate", 100, "What should the spawnrate of the poltergeist be?").Value;
		}
	}
}