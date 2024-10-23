using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace AbandonedAssets.src.ItemScripts
{
	internal class GSPItem : GrabbableObject
	{
		private PlayerControllerB currentPlayer;
		private int glowsticksUsed;
		public Light glowstickLight;
		public AudioClip GlowstickSnap;

		private bool isDroppedStick;


		public override void Start()
		{
			base.Start();
			this.insertedBattery.charge = 560;
			base.EnablePhysics(true);
			base.EnableItemMeshes(true);
		}

		public override void Update()
		{
			base.Update();
			if (isDroppedStick)
			{
				this.insertedBattery.charge -= Time.deltaTime % 60;
				if (this.insertedBattery.charge < 3)
				{
					if (glowstickLight.range > 0.05)
					{
						this.GetComponentInChildren<BoxCollider>().enabled = false;
					}
				}
				if (RoundManager.Instance.begunSpawningEnemies != true && isDroppedStick)
				{
					
				}
			}

		}
		[ServerRpc] public void DeleteSelfServerRpc() { this.gameObject.GetComponent<NetworkObject>().ChangeOwnership(OwnerClientId); }
		[ClientRpc] public void DeleteSelfClientRpc() { GameObject.Destroy(this.gameObject); }

		[ServerRpc] 
		private void SpawnGlowstickServerRpc(Vector3 position, Quaternion rotation)
		{
			//GameObject droppedStick = UnityEngine.Object.Instantiate();

		}
	}
}
