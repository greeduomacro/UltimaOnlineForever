using System;
using Server;
using Server.Mobiles;
using Server.Factions;

namespace Server.Misc
{
	public enum DFAlgorithm
	{
		Standard,
		PainSpike
	}

	public class WeightOverloading
	{
		private const int m_UnmountSteps = 10;
		private const int m_MountSteps = 20;

		public static void Initialize()
		{
			EventSink.Movement += new MovementEventHandler( EventSink_Movement );
		}

		private static DFAlgorithm m_DFA;

		public static DFAlgorithm DFA
		{
			get{ return m_DFA; }
			set{ m_DFA = value; }
		}

		public static void FatigueOnDamage( Mobile m, int damage )
		{
			double fatigue = 0.0;

			switch ( m_DFA )
			{
				case DFAlgorithm.Standard:
				{
					fatigue = (damage * (100.0 / m.Hits) * ((double)m.Stam / 100)) - 5.0;
					break;
				}
				case DFAlgorithm.PainSpike:
				{
					fatigue = (damage * ((100.0 / m.Hits) + ((50.0 + m.Stam) / 100) - 1.0)) - 5.0;
					break;
				}
			}

			if ( fatigue > 0 )
				m.Stam -= (int)fatigue;
		}

		public const int OverloadAllowance = 4; // We can be four stones overweight without getting fatigued

		public static int GetMaxWeight( Mobile m )
		{
			//return ((( Core.ML && m.Race == Race.Human) ? 100 : 40 ) + (int)(3.5 * m.Str));
			//Moved to core virtual method for use there

			return m.MaxWeight;
		}

		public static void EventSink_Movement( MovementEventArgs e )
		{
			Mobile from = e.Mobile;

			if ( !from.Alive || from.AccessLevel > AccessLevel.Player  )
				return;

			if ( !from.Player )
			{
				return;
			}

			int maxWeight = GetMaxWeight( from ) + OverloadAllowance;
			int overWeight = (Mobile.BodyWeight + from.TotalWeight) - maxWeight;

			if ( overWeight > 0 )
			{
				from.Stam -= GetStamLoss( from, overWeight, (e.Direction & Direction.Running) != 0 );

				if ( from.Stam == 0 )
				{
					from.SendLocalizedMessage( 500109 ); // You are too fatigued to move, because you are carrying too much weight!
					e.Blocked = true;
					return;
				}
			}

			if ( ((from.Stam * 100) / Math.Max( from.StamMax, 1 )) < 10 )
				--from.Stam;

            // allow them to move
			/* 
            if ( from.Stam == 0 )
			{
				from.SendLocalizedMessage( 500110 ); // You are too fatigued to move.
				e.Blocked = true;
				return;
			}*/
/*
			if ( from is PlayerMobile & from.Running )
			{
				PlayerMobile pm = (PlayerMobile)from;

				int hits = ( from.Hits * 100 ) / from.HitsMax;

				from.Stam -= Math.Max( (20 - hits) / 5, 0 ); //1 point per step for every 5% health below 20%

				int amt = (int)( ( from.Mounted ? ( m_MountSteps + ((from.Mount is FactionWarHorse) ? 10 : 0) ) : m_UnmountSteps ) * ( Math.Min( from.Dex, 90 ) / 90.0 ) );

				if ( amt == 0 || (++pm.StepsTaken % amt) == 0 )
					--from.Stam;
			}
*/
		}

		public static int GetStamLoss( Mobile from, int overWeight, bool running )
		{
			int loss = 5 + (overWeight / 20);

			if ( from.Mounted )
				loss /= 3;

			if ( running )
				loss *= 2;

			return loss;
		}

		public static bool IsOverloaded( Mobile m )
		{
			if ( !m.Player || !m.Alive || m.AccessLevel > AccessLevel.Player )
				return false;

			return ( (Mobile.BodyWeight + m.TotalWeight) > (GetMaxWeight( m ) + OverloadAllowance) );
		}
	}
}