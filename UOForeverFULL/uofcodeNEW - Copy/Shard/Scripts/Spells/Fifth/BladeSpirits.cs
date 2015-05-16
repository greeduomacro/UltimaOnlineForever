#region References
using System;

using Server.Mobiles;
using Server.Targeting;
#endregion

namespace Server.Spells.Fifth
{
	public class BladeSpiritsSpell : MagerySpell
	{
		private static readonly SpellInfo m_Info = new SpellInfo(
			"Blade Spirits", "In Jux Hur Ylem", 266, 9040, false, Reagent.BlackPearl, Reagent.MandrakeRoot, Reagent.Nightshade);

		public override SpellCircle Circle { get { return SpellCircle.Fifth; } }

		public BladeSpiritsSpell(Mobile caster, Item scroll)
			: base(caster, scroll, m_Info)
		{ }

		public override TimeSpan GetCastDelay()
		{
			if (Caster.EraAOS)
			{
				return TimeSpan.FromTicks(base.GetCastDelay().Ticks * (Caster.EraSE ? 3 : 5));
			}

			return base.GetCastDelay() + TimeSpan.FromSeconds(8.0);
		}

		public override bool CheckCast()
		{
			if (!base.CheckCast())
			{
				return false;
			}

			if ((Caster.Followers + (Caster.EraSE ? 2 : 1)) > Caster.FollowersMax)
			{
				Caster.SendLocalizedMessage(1049645); // You have too many followers to summon that creature.
				return false;
			}

			return true;
		}

		public override void OnCast()
		{
			Caster.Target = new InternalTarget(this);
		}

		public void Target(IPoint3D p)
		{
			Map map = Caster.Map;

			SpellHelper.GetSurfaceTop(ref p);

			if (map == null || !map.CanSpawnMobile(p.X, p.Y, p.Z))
			{
				Caster.SendLocalizedMessage(501942); // That location is blocked.
			}
			else if (SpellHelper.CheckTown(p, Caster) && CheckSequence())
			{
				TimeSpan duration;

				if (Caster.EraAOS)
				{
					duration = TimeSpan.FromSeconds(120);
				}
				else if (Caster.EraUOR)
				{
					duration = TimeSpan.FromSeconds(Utility.Random(40, 20));
				}
				else
				{
					// HACK: Convert to T2A mechanics.
					duration = TimeSpan.FromSeconds(Utility.Random(40, 20));
				}

				BaseCreature.Summon(new BladeSpirits(), false, Caster, new Point3D(p), 0x212, duration);
			}

			FinishSequence();
		}

		private class InternalTarget : Target
		{
			private BladeSpiritsSpell m_Owner;

			public InternalTarget(BladeSpiritsSpell owner)
				: base(owner.Caster.EraML ? 10 : 12, true, TargetFlags.None)
			{
				m_Owner = owner;
			}

			protected override void OnTarget(Mobile from, object o)
			{
				if (o is IPoint3D)
				{
					m_Owner.Target((IPoint3D)o);
				}
			}

			protected override void OnTargetOutOfLOS(Mobile from, object o)
			{
				from.SendLocalizedMessage(501943); // Target cannot be seen. Try again.
				from.Target = new InternalTarget(m_Owner);
				from.Target.BeginTimeout(from, TimeoutTime - DateTime.UtcNow);
				m_Owner = null;
			}

			protected override void OnTargetFinish(Mobile from)
			{
				if (m_Owner != null)
				{
					m_Owner.FinishSequence();
				}
			}
		}
	}
}