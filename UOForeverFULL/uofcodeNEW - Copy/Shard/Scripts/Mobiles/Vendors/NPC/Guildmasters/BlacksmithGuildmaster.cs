#region References
using Server.Items;
#endregion

namespace Server.Mobiles
{
	public class BlacksmithGuildmaster : BaseGuildmaster
	{
		public override NpcGuild NpcGuild { get { return NpcGuild.BlacksmithsGuild; } }

		public override bool IsActiveVendor { get { return true; } }

		public override bool ClickTitle { get { return true; } }

		[Constructable]
		public BlacksmithGuildmaster()
			: base("blacksmith")
		{
			SetSkill(SkillName.ArmsLore, 65.0, 88.0);
			SetSkill(SkillName.Blacksmith, 90.0, 100.0);
			SetSkill(SkillName.Macing, 36.0, 68.0);
			SetSkill(SkillName.Parry, 36.0, 68.0);
		}

		public override void InitSBInfo()
		{
			SBInfos.Add(new SBBlacksmith(Expansion));
		}

		public override VendorShoeType ShoeType { get { return VendorShoeType.ThighBoots; } }

		public override void InitOutfit()
		{
			base.InitOutfit();

			Item item = (Utility.RandomBool() ? null : new RingmailChest());

			if (item != null && !EquipItem(item))
			{
				item.Delete();
				item = null;
			}

			if (item == null)
			{
				AddItem(new FullApron());
			}

			AddItem(new Bascinet());
			AddItem(new SmithHammerWeapon());
		}

		public BlacksmithGuildmaster(Serial serial)
			: base(serial)
		{ }

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}
}