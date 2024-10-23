using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.ParticleSystem.PlaybackState;

namespace AbandonedAssets.src.ItemScripts
{
	internal class CandleItem : GrabbableObject
	{
		private bool lightCandle;
		private bool currentlyLit;
		private float timeTillDead;
		private float currentTime;
		private System.Random random;
		private int realSeed;
		private float snuffTimer;
		private float snuffTimerEnd;
		private float chanceToSnuff;
		private float snuffRoll;
		private float maxTimeLeft;
		private float defaultMaxVisionLight;

		public Light flameLight;
		public Light visionLight;
		[SerializeField]
		public ParticleSystem smokeParticles;

		private float countdownToDead;

		public float maxVisionLight;
		public float minVisionLight = 5;
		public float maxFlameLight = 30;
		public float minFlameLight = 0;

		public MeshRenderer candleWax;

		public AudioClip igniteCandle;
		public AudioClip snuffCandle;
		public AudioClip candleLitIdle;
		private AudioSource candleAudio;



		public int smoothing = 0;

		public override int GetItemDataToSave()
		{
			base.GetItemDataToSave();
			int saveTime = (int)maxTimeLeft;
			return saveTime;
		}

		public override void LoadItemSaveData(int saveData)
		{
			base.LoadItemSaveData(saveData);
			this.maxTimeLeft = saveData;
			saveData = 0;
		}
		public override void Start()
		{
			base.Start();

			realSeed = StartOfRound.Instance.randomMapSeed;
			smokeParticles = GetComponentInChildren<ParticleSystem>();
			candleAudio = GetComponentInChildren<AudioSource>();
			if (!smokeParticles)
			{
				throw new Exception("Particle System Missing!");
			}
			if (!candleAudio)
			{
				throw new Exception("Candle Audio Source Missing!");
			}
			smokeParticles.Stop();
			random = new System.Random(realSeed);
			maxVisionLight = random.Next(50, 80);
			flameLight.range = 1;
			defaultMaxVisionLight = maxVisionLight;

			SyncSeedServerRpc();	
			if (currentTime == 0 )
			{
				timeTillDead = random.Next(45, 200);
				maxTimeLeft = timeTillDead;
				chanceToSnuff = random.Next(0, 100);
				snuffRoll = random.Next(0, 100);
				snuffTimerEnd = random.Next(30, 100);
			}
			SyncSeedServerRpc();

		}
		public override void Update()
		{
			base.Update();
			//Plugin.Logger.LogInfo((timeTillDead / maxTimeLeft));

			if (timeTillDead <= 0)
			{
				lightCandle = false;
				smokeParticles.Stop();
			}

			if (lightCandle && timeTillDead > 0)
			{
				Plugin.Logger.LogInfo($"Time till Dead: {timeTillDead}");
				FlameFlickering();
				snuffTimer += Time.deltaTime;
				timeTillDead = Mathf.Clamp((timeTillDead - Time.deltaTime), 0, maxTimeLeft);
				maxVisionLight = Mathf.Clamp(defaultMaxVisionLight * (timeTillDead / maxTimeLeft), 0, maxVisionLight);
				candleAudio.clip = candleLitIdle;
				candleAudio.loop = true;
				candleAudio.Play();

				if (visionLight.intensity >= maxVisionLight)
				{
					currentlyLit = true;
				}
				else
				{
					flameLight.intensity = Mathf.Clamp(flameLight.intensity + Time.deltaTime * 100, 0, maxFlameLight);
					visionLight.intensity = Mathf.Clamp(visionLight.intensity + Time.deltaTime * 100, 0, maxVisionLight);
				}
			}
			else
			{
				flameLight.intensity = Mathf.Clamp(flameLight.intensity -= Time.deltaTime * 150, 0, maxFlameLight);
				visionLight.intensity = Mathf.Clamp(visionLight.intensity -= Time.deltaTime * 150, 0, maxVisionLight);
			}

			

			if (snuffTimer > snuffTimerEnd)
			{
				Plugin.Logger.LogInfo("Snuff Failed! Chances rolled: " + snuffRoll + "/" + chanceToSnuff);
				snuffRoll = random.Next(0, 100);
				if (snuffRoll > chanceToSnuff)
				{
					Plugin.Logger.LogInfo("Candle Snuffed! Chances rolled: " + snuffRoll + "/" + chanceToSnuff);
					chanceToSnuff = random.Next(20, 80);
					SyncSeedServerRpc();
					lightCandle = false;
					smokeParticles.Stop();
					timeTillDead = timeTillDead * (float)(random.Next(7, 9) * 0.1);
				}
				snuffTimerEnd = random.Next(30, 100);
				snuffTimer = 0;
			}

		}
		public override void ItemActivate(bool used, bool buttonDown = true)
		{
			base.ItemActivate(used, buttonDown);
			Plugin.Logger.LogInfo("FLIPPED!");
			if (lightCandle)
			{
				lightCandle = false;
				candleAudio.PlayOneShot(snuffCandle);
				candleAudio.loop = false;
				smokeParticles.Stop();
			}
			else
			{
				lightCandle = true;
				candleAudio.PlayOneShot(igniteCandle);
				candleAudio.loop = false;
				smokeParticles.Play();
			}
		}
		public override void PocketItem()
		{
			base.PocketItem();
			if (lightCandle)
			{
				lightCandle = false;
				smokeParticles.Stop();
			}
		}

		private void FlameFlickering()
		{
			float maxReduction = 15;
			float maxIncrease = 15;
			float strength = 7;
			if (currentlyLit)
			{
				visionLight.intensity = Mathf.Lerp(visionLight.intensity, random.Next((int)maxVisionLight - (int)maxReduction, (int)maxVisionLight + (int)maxIncrease), strength * Time.deltaTime);
				flameLight.intensity = Mathf.Lerp(flameLight.intensity, random.Next((int)maxFlameLight - (int)(maxReduction / 2), (int)maxFlameLight + (int)(maxIncrease / 2)), strength * Time.deltaTime);
				SyncSeedServerRpc();
			}
		}
		[ServerRpc(RequireOwnership = false)]
		private void SyncSeedServerRpc()
		{
			if (IsHost)
			{
				realSeed += 1;
				SyncSeedClientRpc(realSeed);
			}
		}
		[ClientRpc]
		private void SyncSeedClientRpc(int currentseed)
		{
			realSeed = currentseed;
		}
	}
}
