using System;
using Server;
using Server.Items;

namespace Server.Mobiles
{
	[CorpseName( "a water elemental corpse" )]
	public class SummonedWaterElemental : BaseCreature
	{
		public override double DispelDifficulty{ get{ return 117.5; } }
		public override double DispelFocus{ get{ return 45.0; } }
		public override bool AlwaysMurderer{ get{ return true; } }
		public override string DefaultName{ get{ return "a water elemental"; } }

		[Constructable]
		public SummonedWaterElemental () : base( AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4 )
		{
			Body = 16;
			BaseSoundID = 278;

			SetStr( 200 );
			SetDex( 70 );
			SetInt( 100 );

			SetHits( 165 );

			SetDamage( 12, 16 );

			
			

			
			
			
			
			

			SetSkill( SkillName.Meditation, 90.0 );
			SetSkill( SkillName.EvalInt, 80.0 );
			SetSkill( SkillName.Magery, 80.0 );
			SetSkill( SkillName.MagicResist, 75.0 );
			SetSkill( SkillName.Tactics, 100.0 );
			SetSkill( SkillName.Wrestling, 85.0 );

			VirtualArmor = 40;
			ControlSlots = 3;
			CanSwim = true;
		}

		public override int DefaultBloodHue{ get{ return -1; } }

		public SummonedWaterElemental( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}