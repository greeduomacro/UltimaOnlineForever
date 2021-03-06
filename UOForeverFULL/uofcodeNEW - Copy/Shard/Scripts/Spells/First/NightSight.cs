#region References
using Server.Engines.XmlSpawner2;
using Server.Targeting;
#endregion

namespace Server.Spells.First
{
	public class NightSightSpell : MagerySpell
	{
		private static readonly SpellInfo m_Info = new SpellInfo(
			"Night Sight", "In Lor", 236, 9031, Reagent.SulfurousAsh, Reagent.SpidersSilk);

		public override SpellCircle Circle { get { return SpellCircle.First; } }

		public NightSightSpell(Mobile caster, Item scroll)
			: base(caster, scroll, m_Info)
		{ }

		public override void OnCast()
		{
			Caster.Target = new NightSightTarget(this);
		}

		private class NightSightTarget : Target
		{
			private readonly Spell m_Spell;

			public NightSightTarget(Spell spell)
				: base(12, false, TargetFlags.Beneficial)
			{
				m_Spell = spell;
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				var entity = targeted as IEntity;
				if (XmlScript.HasTrigger(entity, TriggerName.onTargeted) &&
					UberScriptTriggers.Trigger(entity, from, TriggerName.onTargeted, null, null, m_Spell))
				{
					return;
				}

				if (targeted is Mobile && m_Spell.CheckBSequence((Mobile)targeted))
				{
					var targ = (Mobile)targeted;

					SpellHelper.Turn(m_Spell.Caster, targ);

					if (targ.BeginAction(typeof(LightCycle)))
					{
						new LightCycle.NightSightTimer(targ).Start();
						var level =
							(int)
							(LightCycle.DungeonLevel *
							 ((from.EraAOS ? targ.Skills[SkillName.Magery].Value : from.Skills[SkillName.Magery].Value) / 100));

						if (level < 0)
						{
							level = 0;
						}

						targ.LightLevel = level;

						targ.FixedParticles(0x376A, 9, 32, 5007, EffectLayer.Waist);
						targ.PlaySound(0x1E3);

						BuffInfo.AddBuff(targ, new BuffInfo(BuffIcon.NightSight, 1075643)); //Night Sight/You ignore lighting effects
					}
					else
					{
						from.SendMessage("{0} already have nightsight.", from == targ ? "You" : "They");
					}
				}

				m_Spell.FinishSequence();
			}

			protected override void OnTargetFinish(Mobile from)
			{
				m_Spell.FinishSequence();
			}
		}
	}
}