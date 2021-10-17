using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ID;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria.ModLoader.IO;
using Terraria.Utilities;
using Terraria.Localization;

namespace MeleeIsRanged
{
	public class ThrowItem : ModProjectile
	{
		public int Timer;
		public Item item = null;
		public int shoot = 0;
		public Player player => Main.player[projectile.owner];
		public override string Texture => "Terraria/CoolDown";
		public override void SetStaticDefaults() => DisplayName.SetDefault("Throwed Sword");
		public override void OnHitNPC(NPC target, int damage, float knockBack, bool crit) {
			if (item != null) {
				ItemLoader.OnHitNPC(item, player, target, damage, knockBack, crit);
				NPCLoader.OnHitByItem(target, player, item, damage, knockBack, crit);
				PlayerHooks.OnHitNPC(player, item, target, damage, knockBack, crit);
			}
		}
		public override void OnHitPvp(Player target, int damage, bool crit) {
			if (item != null) {
				ItemLoader.OnHitPvp(item, player, target, damage, crit);
				PlayerHooks.OnHitPvp(player, item, target, damage, crit);
			}
		}
		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockBack, ref bool crit, ref int hitDirection) {
			if (item != null) {
				ItemLoader.ModifyHitNPC(item, player, target, ref damage, ref knockBack,ref crit);
				PlayerHooks.ModifyHitNPC(player,item, target, ref damage, ref knockBack, ref crit);
				NPCLoader.ModifyHitByItem(target,player,item, ref damage, ref knockBack, ref crit);
			}
		}
		public override void SetDefaults() {
			projectile.CloneDefaults(ProjectileID.EnchantedBoomerang);
			aiType = ProjectileID.EnchantedBoomerang;
		}
		public override void AI() {
			if (item != null) {
				ItemLoader.MeleeEffects(item,player, projectile.Hitbox);
			}
			if (item.useTime > 0 && shoot > 0) {
				Timer++;
				if (Timer > item.useTime) {
					int a = Projectile.NewProjectile(projectile.position, projectile.velocity, shoot, projectile.damage/2, projectile.knockBack, projectile.owner);
					Timer = 0;
				}
			}
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
			if (item != null){
				Texture2D newTexture = Main.itemTexture[item.type];
				spriteBatch.Draw(newTexture, projectile.Center - Main.screenPosition, null, projectile.GetAlpha(lightColor), projectile.rotation, newTexture.Size() * 0.5f, projectile.scale, SpriteEffects.None, 0f);
				return false;
			}
			return true;
		}
	}
	/*
	public class ThrowItem : ModProjectile
	{
		public Texture2D newTexture = null;
		public int shoot = 0;
		public int useTime = 0;
		public int Timer;
		public ModItem moditem = null;
		public override string Texture => "Terraria/CoolDown";
		public override void SetStaticDefaults() => DisplayName.SetDefault("Throwed Sword");
		public override void OnHitNPC(NPC target, int damage, float knockBack, bool crit) {
			if (moditem != null) {
				moditem.OnHitNPC(Main.player[projectile.owner],target,damage,knockBack,crit);
			}
		}
		public override void OnHitPvp(Player target, int damage, bool crit) {
			if (moditem != null) {
				moditem.OnHitPvp(Main.player[projectile.owner],target,damage,crit);
			}
		}
		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockBack, ref bool crit, ref int hitDirection) {
			if (moditem != null) {
				moditem.ModifyHitNPC(Main.player[projectile.owner],target,ref damage,ref knockBack,ref crit);
			}
		}
		public override void SetDefaults() {
			projectile.CloneDefaults(ProjectileID.EnchantedBoomerang);
			aiType = ProjectileID.EnchantedBoomerang;
		}
		Projectile refProj(int id) {
			Projectile sus = new Projectile();
			sus.SetDefaults(shoot);
			return sus;
		}
		public override void AI() {
			if (moditem != null) {
				moditem.MeleeEffects(Main.player[projectile.owner], projectile.Hitbox);
			}
			if (useTime > 0 && shoot > 0) {
				Timer++;
				projectile.tileCollide = refProj(shoot).tileCollide;
				if (Timer > useTime) {
					int a = Projectile.NewProjectile(projectile.position, projectile.velocity, shoot, projectile.damage/2, projectile.knockBack, projectile.owner);
					Timer = 0;
				}
			}
		}
		public override void Kill(int timeLeft) {
			Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);
			Main.PlaySound(SoundID.Item10, projectile.position);
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
			if (newTexture != null){
				spriteBatch.Draw(newTexture, projectile.Center - Main.screenPosition, null, projectile.GetAlpha(lightColor), projectile.rotation, newTexture.Size() * 0.5f, projectile.scale, SpriteEffects.None, 0f);
				return false;
			}
			return true;
		}
	}*/
	public class MyGlobalItem : GlobalItem
	{
		public override bool CloneNewInstances => true;
		public override bool InstancePerEntity => true;
		//Instances
		public bool TrueMelee;
		public bool meleeTool;
		public int oldShoot;
		public override void SetDefaults(Item item) {
			TrueMelee = item.pick < 1 && item.axe < 1 && item.hammer < 1 && item.melee && !item.noUseGraphic && !item.noMelee && !item.channel;
			meleeTool = (item.pick > 0 || item.axe > 0 || item.hammer > 0) && item.melee && !item.noUseGraphic && !item.noMelee && !item.channel;
			if (TrueMelee) {
				item.melee = false;
				item.ranged = true;
				item.noUseGraphic = true;
				item.noMelee = true;
				if (item.shootSpeed == 0f){item.shootSpeed = item.knockBack*2f;}
				if (item.useTime == 100) {
					item.useTime = item.useAnimation;
				}
				oldShoot = item.shoot;
				item.shoot = ModContent.ProjectileType<ThrowItem>();
			}
			if (meleeTool) {
				item.melee = false;
				item.ranged = true;
				item.noMelee = true;
				item.noUseGraphic = true;
			}
		}
		public override bool AltFunctionUse(Item item, Player player) {
			if (meleeTool) {return true;}
			return base.AltFunctionUse(item,player);
		}
		public override bool CanUseItem (Item item, Player player){
			if (meleeTool) {
				if (player.altFunctionUse == 2) {
					item.noUseGraphic = true;
					item.shootSpeed = item.knockBack*3f;
					item.shoot = ModContent.ProjectileType<ThrowItem>();
				} 
				else {
					item.shootSpeed = 0f;
					item.shoot = ProjectileID.None;
					item.noUseGraphic = false;
				}
			}
			return base.CanUseItem(item, player);
		}
		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
			if (meleeTool) {
				tooltips.Add(new TooltipLine(mod, "toolchange", "Right-Click to throw"));
			}
		}
		public override bool Shoot(Item item,Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
			if (type == ModContent.ProjectileType<ThrowItem>()) {
				int a = Projectile.NewProjectile(position.X, position.Y, speedX, speedY, type, damage, knockBack, player.whoAmI);
				Projectile proj = Main.projectile[a];
				if (proj.modProjectile is ThrowItem sus) {
					sus.item = item;
					sus.shoot = oldShoot;
					Projectile nigg = new Projectile();
					nigg.SetDefaults(sus.shoot);
					proj.tileCollide = nigg.tileCollide;
				}
				return false;
			}
			return base.Shoot(item,player,ref position,ref speedX, ref speedY, ref type,ref damage,ref knockBack);
		}
	}
}