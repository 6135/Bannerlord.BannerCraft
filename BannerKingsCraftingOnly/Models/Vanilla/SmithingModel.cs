using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace BannerCraft
{
	public class SmithingModel : DefaultSmithingModel
	{
		public SmithingModel()
        {
			InformationManager.DisplayMessage(new InformationMessage("Initialising SmithingModel"));
        }

		public override int[] GetSmeltingOutputForItem(ItemObject item)
        {
			var result = base.GetSmeltingOutputForItem(item);
			var metalCount = 0;
			for (var i = 0; i < result.Length; i++)
            {
				if (i is >= 2 and <= 6)
                {
					metalCount += result[i];
                }
            }

			if (item.WeaponComponent is { PrimaryWeapon: { } })
            {
				var metalCap = GetMetalMax(item.WeaponComponent.PrimaryWeapon.WeaponClass);
				if (metalCount > 0 && metalCap > 0)
                {
					while (metalCount > metalCap)
                    {
						for (var i = 0; i < result.Length; i++)
                        {
							if (i is >= 2 and <= 6 && result[i] > 0 && metalCount > metalCap)
                            {
								result[i]--;
								metalCount--;
                            }
                        }
                    }
                }

				if (item.WeaponComponent.PrimaryWeapon.WeaponClass == WeaponClass.Dagger)
                {
					result[7] = 0;
                }

				if (result[1] == 0 && metalCap > 0)
                {
					result[1]++;
                }
            }

			return result;
        }

		public int GetMetalMax(WeaponClass weaponClass) => weaponClass switch
		{
			WeaponClass.Dagger => 1,
			WeaponClass.ThrowingAxe => 1,
			WeaponClass.ThrowingKnife => 1,
			WeaponClass.Crossbow => 1,
			WeaponClass.SmallShield => 1,

			WeaponClass.OneHandedSword => 2,
			WeaponClass.LowGripPolearm => 2,
			WeaponClass.OneHandedPolearm => 2,
			WeaponClass.TwoHandedPolearm => 2,
			WeaponClass.OneHandedAxe => 2,
			WeaponClass.Mace => 2,
			WeaponClass.LargeShield => 2,
			WeaponClass.Pick => 2,

			WeaponClass.TwoHandedAxe => 3,
			WeaponClass.TwoHandedMace => 3,
			WeaponClass.TwoHandedSword => 3,
			_ => -1
		};

        public float CalculateBotchingChance(Hero hero, int difficulty)
        {
			float chance = 0.01f * (difficulty - hero.GetSkillValue(DefaultSkills.Crafting));

			return MBMath.ClampFloat(chance, 0f, 0.95f);
        }

		public int CalculateArmorDifficulty(ItemObject item)
        {
			float result = item.Tierf * 20f;

			ArmorCraftingVM.ItemType itemType = ArmorCraftingVM.GetItemType(item);
			if (itemType == ArmorCraftingVM.ItemType.Invalid)
            {
				/*
				 * Vanilla crafting item, should not get here
				 */
				return 0;
            }

			switch (itemType)
            {
				case ArmorCraftingVM.ItemType.Barding:

				case ArmorCraftingVM.ItemType.HeadArmor:
				case ArmorCraftingVM.ItemType.ShoulderArmor:
				case ArmorCraftingVM.ItemType.BodyArmor:
				case ArmorCraftingVM.ItemType.ArmArmor:
				case ArmorCraftingVM.ItemType.LegArmor:
					switch (itemType)
                    {
						case ArmorCraftingVM.ItemType.Barding:
						case ArmorCraftingVM.ItemType.BodyArmor:
							result *= 1.5f;
							break;
						case ArmorCraftingVM.ItemType.HeadArmor:
							result *= 1.2f;
                            break;
                    }

					switch (item.ArmorComponent.MaterialType)
					{
						case ArmorComponent.ArmorMaterialTypes.Cloth:
							result *= 1f;
							break;
						case ArmorComponent.ArmorMaterialTypes.Leather:
							result *= 1.1f;
							break;
						case ArmorComponent.ArmorMaterialTypes.Chainmail:
							result *= 1.25f;
							break;
						case ArmorComponent.ArmorMaterialTypes.Plate:
							result *= 1.4f;
							break;
					}
					break;

				case ArmorCraftingVM.ItemType.Shield:
					result += item.WeaponComponent.PrimaryWeapon.MaxDataValue / 10f;
					break;
				case ArmorCraftingVM.ItemType.Arrows:
				case ArmorCraftingVM.ItemType.Bolts:
					result += item.WeaponComponent.PrimaryWeapon.MaxDataValue * item.WeaponComponent.PrimaryWeapon.MissileDamage;
					break;
            }

			return MBMath.ClampInt((int)result, 10, 300);

        }

		public int[] GetCraftingInputForArmor(ItemObject item)
        {
			var result = new int[11];

			ArmorCraftingVM.ItemType itemType = ArmorCraftingVM.GetItemType(item);
			if (itemType == ArmorCraftingVM.ItemType.Invalid)
            {
				/*
				 * Vanilla crafting item, should not get here
				 */
            }

			switch (itemType)
            {
				case ArmorCraftingVM.ItemType.Barding:

				case ArmorCraftingVM.ItemType.HeadArmor:
				case ArmorCraftingVM.ItemType.ShoulderArmor:
				case ArmorCraftingVM.ItemType.BodyArmor:
				case ArmorCraftingVM.ItemType.ArmArmor:
				case ArmorCraftingVM.ItemType.LegArmor:
					/*
					 * Armor requires some tier dependent metal and some material dependent "cloth"
					 * Plate and chain requires linen and velvet
					 * Tier 4, 5, 6 require velvet
					 * Tier 0, 1, 2, 3 require linen
					 * 
					 * Tier 6 requires velvet
					 * Tier 4 and 5 require furs
					 * Tier 2 and 3 require linen
					 * Tier 0 and 1 require hides
					 */
					var material = item.ArmorComponent.MaterialType;
					switch (material)
                    {
						case ArmorComponent.ArmorMaterialTypes.Plate:
						case ArmorComponent.ArmorMaterialTypes.Chainmail:
							break;
                    }
					
					break;

				case ArmorCraftingVM.ItemType.Shield:
					break;

				case ArmorCraftingVM.ItemType.Arrows:
				case ArmorCraftingVM.ItemType.Bolts:
					break;
			}

			return result;
        }

		public ItemObject GetCraftingMaterialItem(ExtraCraftingMaterials craftingMaterial)
        {
			return Game.Current.ObjectManager.GetObject<ItemObject>(craftingMaterial.ToString().ToLower()); ;
		}
	}
}