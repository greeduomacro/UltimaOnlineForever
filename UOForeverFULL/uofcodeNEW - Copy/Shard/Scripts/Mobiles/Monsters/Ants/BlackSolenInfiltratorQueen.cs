using System;
using System.Collections;
using Server.Items;
using Server.Targeting;

namespace Server.Mobiles
{
	[CorpseName( "a solen infiltrator corpse" )]
	public class BlackSolenInfiltratorQueen : BaseCreature, IBlackSolen
	{
		public override string DefaultName{ get{ return "a black solen infiltrator"; } }

		[Constructable]
		public BlackSolenInfiltratorQueen() : base( AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4 )
		{
			Body = 807;
			BaseSoundID = 959;
			Hue = 0x453;

			SetStr( 326, 350 );
			SetDex( 141, 165 );
			SetInt( 96, 120 );

			SetHits( 151, 162 );

			SetDamage( 10, 15 );

			
			

			
			
			
			
			

			SetSkill( SkillName.MagicResist, 90.0 );
			SetSkill( SkillName.Tactics, 90.0 );
			SetSkill( SkillName.Wrestling, 90.0 );

			Fame = 6500;
			Karma = -6500;

			VirtualArmor = 50;

			SolenHelper.PackPicnicBasket( this );

			PackItem( new ZoogiFungus( ( 0.05 > Utility.RandomDouble() ) ? 16 : 4 ) );
		}

		public override int GetAngerSound()
		{
			return 0x259;
		}

		public override int GetIdleSound()
		{
			return 0x259;
		}

		public override int GetAttackSound()
		{
			return 0x195;
		}

		public override int GetHurtSound()
		{
			return 0x250;
		}

		public override int GetDeathSound()
		{
			return 0x25B;
		}

		public override void GenerateLoot()
		{
			AddLoot( LootPack.Rich );
		}

		public override bool IsEnemy( Mobile m )
		{
			if ( SolenHelper.CheckBlackFriendship( m ) )
				return false;
			else
				return base.IsEnemy( m );
		}

		public override void OnDamage( int amount, Mobile from, bool willKill )
		{
			SolenHelper.OnBlackDamage( from );

			base.OnDamage( amount, from, willKill );
		}

		public override int DefaultBloodHue{ get{ return -2; } }
		public override int BloodHueTemplate{ get{ return Utility.RandomGreenHue(); } }

		public BlackSolenInfiltratorQueen( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}