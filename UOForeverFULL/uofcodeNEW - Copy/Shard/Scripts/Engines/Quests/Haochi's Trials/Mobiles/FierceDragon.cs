using System;
using Server.Mobiles;
using Server.Items;

namespace Server.Engines.Quests.Samurai
{
	public class FierceDragon : BaseCreature
	{
		public override string DefaultName{ get{ return "a fierce dragon"; } }

		[Constructable]
		public FierceDragon() : base( AIType.AI_Melee, FightMode.Aggressor, 10, 1, 0.2, 0.4 )
		{
			Body = 103;
			BaseSoundID = 362;

			SetStr( 6000, 6020 );
			SetDex( 0 );
			SetInt( 850, 870 );

			SetDamage( 50, 80 );

			

			
			
			
			
			

			SetSkill( SkillName.Tactics, 120.0 );
			SetSkill( SkillName.Wrestling, 120.0 );
			SetSkill( SkillName.Magery, 120.0 );

			Fame = 15000;
			Karma = 15000;

			CantWalk = false;
        }
				public override void GenerateLoot()
		{
			AddLoot( LootPack.FilthyRich, 2 );
			AddLoot( LootPack.Rich, 2 );
			AddLoot( LootPack.Average, 2 );
			AddLoot( LootPack.HighScrolls );
			AddLoot( LootPack.Gems, 5 );

            if (0.10 >= Utility.RandomDouble())
            { SkillScroll scroll = new SkillScroll(); scroll.Randomize(); PackItem(scroll); }
		}

		public override int GetIdleSound()
		{
			return 0x2C4;
		}

		public override int GetAttackSound()
		{
			return 0x2C0;
		}

		public override int GetDeathSound()
		{
			return 0x2C1;
		}

		public override int GetAngerSound()
		{
			return 0x2C4;
		}

		public override int GetHurtSound()
		{
			return 0x2C3;
		}

		public override void AggressiveAction( Mobile aggressor, bool criminal )
		{
			base.AggressiveAction( aggressor, criminal );

			PlayerMobile player = aggressor as PlayerMobile;
			if ( player != null )
			{
				QuestSystem qs = player.Quest;
				if ( qs is HaochisTrialsQuest )
				{
					QuestObjective obj = qs.FindObjective( typeof( SecondTrialAttackObjective ) );
					if ( obj != null && !obj.Completed )
					{
						obj.Complete();
						qs.AddObjective( new SecondTrialReturnObjective( true ) );
					}
				}
			}
		}

		public FierceDragon( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.WriteEncodedInt( 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadEncodedInt();
		}
	}
}