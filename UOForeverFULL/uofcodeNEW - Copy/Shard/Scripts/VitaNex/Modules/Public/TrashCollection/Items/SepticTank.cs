﻿#region Header
//   Vorspire    _,-'/-'/  SepticTank.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2014  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System;
using System.Drawing;
using System.Text;

using Server;

using VitaNex.FX;
using VitaNex.Items;
using VitaNex.Network;
#endregion

namespace VitaNex.Modules.TrashCollection
{
	public class SepticTank : BaseTrashContainer
	{
		private TimeSpan _ProductionDelay;

		public PollTimer UpdateTimer { get; protected set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool SwagMode { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool StinkMode { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool ProducesWaste { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public TimeSpan ProductionDelay
		{
			get { return _ProductionDelay; }
			set
			{
				_ProductionDelay = value;
				Produce();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public TimeSpan ProductionTimeBonus { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime LastProduction { get; set; }

		[CommandProperty(AccessLevel.Counselor)]
		public TimeSpan NextProduction
		{
			get
			{
				DateTime now = DateTime.UtcNow;
				DateTime next = LastProduction + (ProductionDelay - ProductionTimeBonus);

				if (now < next)
				{
					return next - now;
				}

				return TimeSpan.Zero;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool ProductReady { get { return NextProduction <= TimeSpan.Zero; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public int Products { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int ProductsMax { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public string Token { get; set; }

		[Constructable]
		public SepticTank()
			: base(0xE77, 0xE77)
		{
			Name = "Septic Tank";
			Hue = 1270;

			StinkMode = true;
			SwagMode = true;

			ProducesWaste = true;
			ProductsMax = 100;
			ProductionDelay = TimeSpan.FromHours(12);
			LastProduction = DateTime.UtcNow;
		}

		public SepticTank(Serial serial)
			: base(serial)
		{ }

		public override void OnAfterDuped(Item newItem)
		{
			base.OnAfterDuped(newItem);

			SepticTank c = newItem as SepticTank;

			if (c == null)
			{
				return;
			}

			c.StinkMode = StinkMode;
			c.SwagMode = SwagMode;
			c.ProducesWaste = ProducesWaste;
			c.ProductsMax = ProductsMax;
			c.ProductionDelay = ProductionDelay;
			c.LastProduction = LastProduction;

			c.UpdateTimer = null;
		}

#if NEWPARENT
		public override void OnAdded(IEntity parent)
#else
		public override void OnAdded(object parent)
#endif
		{
			base.OnAdded(parent);

			if (parent == null && ProducesWaste)
			{
				if (UpdateTimer != null)
				{
					UpdateTimer.Running = true;
				}
				else
				{
					UpdateTimer = PollTimer.CreateInstance(TimeSpan.FromSeconds(1), OnProductionTimerTick);
				}
			}
			else if (UpdateTimer != null)
			{
				UpdateTimer.Running = false;
				UpdateTimer = null;
			}
		}

		public override void OnLocationChange(Point3D oldLocation)
		{
			base.OnLocationChange(oldLocation);

			if (!ProducesWaste || RootParent != null)
			{
				if (UpdateTimer != null)
				{
					UpdateTimer.Running = false;
					UpdateTimer = null;
				}

				return;
			}

			if (UpdateTimer != null)
			{
				UpdateTimer.Running = true;
			}
			else
			{
				UpdateTimer = PollTimer.CreateInstance(TimeSpan.FromSeconds(1), OnProductionTimerTick);
			}
		}

		public override void OnAfterDelete()
		{
			base.OnAfterDelete();

			if (UpdateTimer == null)
			{
				return;
			}

			UpdateTimer.Running = false;
			UpdateTimer = null;
		}

		public override void OnSingleClick(Mobile m)
		{
			base.OnSingleClick(m);

			if (!ProducesWaste)
			{
				return;
			}

			if (UpdateTimer != null && UpdateTimer.Running && RootParent == null && !ProductReady)
			{
				LabelTo(m, "Producing: {0}", NextProduction.ToSimpleString("h:m:s"));
			}

			if (ProductsMax > 0)
			{
				LabelTo(m, "Units: {0:#,0} / {1:#,0}", Products, ProductsMax);
			}
			else if (Products > 0)
			{
				LabelTo(m, "Units: {0:#,0}", Products);
			}
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			if (!ProducesWaste)
			{
				return;
			}

			StringBuilder prop = new StringBuilder();

			if (UpdateTimer != null && UpdateTimer.Running && RootParent == null && !ProductReady)
			{
				prop.AppendFormat("Producing: {0}", NextProduction.ToSimpleString("h:m:s"));
			}

			if (ProductsMax > 0)
			{
				prop.AppendFormat("{0}Units: {1:#,0} / {2:#,0}", prop.Length > 0 ? "\n" : String.Empty, Products, ProductsMax);
			}
			else if (Products > 0)
			{
				prop.AppendFormat("{0}Units: {1:#,0}", prop.Length > 0 ? "\n" : String.Empty, Products);
			}

			string s = prop.ToString();

			if (!String.IsNullOrWhiteSpace(s))
			{
				list.Add(s.WrapUOHtmlColor(Color.Cyan));
			}
		}

		public override bool OnDragDrop(Mobile m, Item trashed)
		{
			if (m == null || trashed == null || trashed.Deleted || !IsAccessibleTo(m))
			{
				return false;
			}

			if (RootParent == null)
			{
				if (trashed is SepticWaste)
				{
					if (!ProducesWaste)
					{
						m.SendMessage("Dumping the waste in the {0} would be lazy, dispose of it properly!", this.ResolveName(m));
						return false;
					}

					if (Products >= ProductsMax)
					{
						m.SendMessage("The {0} is full.", this.ResolveName(m));
						return false;
					}

					SepticWaste waste = (SepticWaste)trashed;

					if (Products + waste.Amount > ProductsMax)
					{
						int diff = (Products + waste.Amount) - ProductsMax;
						Products = ProductsMax;
						waste.Amount = diff;

						m.SendMessage("You dump as much of the waste into the {0} as possible.", this.ResolveName(m));
					}
					else
					{
						Products += waste.Amount;
						waste.Delete();
						m.SendMessage("You dump the waste into the {0}.", this.ResolveName(m));
					}

					return false;
				}

				return Trash(m, trashed);
			}

			m.SendMessage(0x22, "The {0} must be on the ground to dispose your items.", this.ResolveName(m));
			return false;
		}

		protected override void OnTrashed(Mobile m, Item trashed, ref int tokens, bool message = true)
		{
			base.OnTrashed(m, trashed, ref tokens, message);

			if (tokens > 0 && ProductionTimeBonus < TimeSpan.FromSeconds(ProductionDelay.TotalSeconds / 2))
			{
				ProductionTimeBonus += TimeSpan.FromMinutes(tokens);
			}

			BaseExplodeEffect fx = GetProductionEffect();

			if (fx != null)
			{
				fx.Send();
			}
		}

		public virtual bool CanProduce()
		{
			return !Deleted && ProducesWaste && Map != null && Map != Map.Internal && RootParent == null &&
				   Products < ProductsMax && NextProduction <= TimeSpan.Zero;
		}

		protected virtual void Produce()
		{
			BaseExplodeEffect fx;

			if (!CanProduce())
			{
				if (SwagMode)
				{
					TimeSpan next = NextProduction;

					if (next.TotalSeconds <= 60 || next.TotalSeconds % 60 == 0 || Utility.RandomDouble() <= 0.10)
					{
						fx = GetProductionEffect();

						if (fx != null)
						{
							fx.Send();
						}
					}
				}

				return;
			}

			int amount = Utility.RandomMinMax(1, 3);

			if (Products + amount > ProductsMax)
			{
				amount = ProductsMax - Products;
			}

			if (amount > 0)
			{
				Products += amount;
			}

			LastProduction = DateTime.UtcNow;
			ProductionTimeBonus = TimeSpan.Zero;

			if (!SwagMode)
			{
				return;
			}

			fx = GetProductionEffect();

			if (fx != null)
			{
				fx.Send();
			}
		}

		public virtual BaseExplodeEffect GetProductionEffect()
		{
			FireExplodeEffect fx = new FireExplodeEffect(this, Map, 0);

			fx.Effects.ForEach(
				e =>
				{
					e.Hue = 65;

					if (e.EffectID != 13401)
					{
						return;
					}

					e.Render = EffectRender.SemiTransparent;
					e.Duration = 40;
				});

			return fx;
		}

		public virtual BaseExplodeEffect GetClaimEffect(Mobile m)
		{
			WaterRippleEffect fx = new WaterRippleEffect(this, Map, 2);

			fx.Effects.ForEach(
				e =>
				{
					e.Hue = 1270;
					e.Render = EffectRender.SemiTransparent;
				});

			return fx;
		}

		public override void OnDoubleClick(Mobile m)
		{
			if (!this.CheckDoubleClick(m, true, false, 3))
			{
				return;
			}

			if (ProducesWaste && Products > 0)
			{
				double pFactor = Products / Math.Max(Products, ProductsMax);
				TimeSpan pDuration = TimeSpan.FromSeconds(60.0 * pFactor);

				SepticWaste waste = new SepticWaste(Products);

				if (m.PlaceInBackpack(waste))
				{
					m.SendMessage(0x55, "You gather the decomposed waste and place it in your backpack.");
				}
				else
				{
					waste.MoveToWorld(m.Location, m.Map);
					m.SendMessage(0x55, "The waste spills out at your feet.");
				}

				if (SwagMode)
				{
					BaseExplodeEffect fx = GetClaimEffect(m);

					if (fx != null)
					{
						if (StinkMode)
						{
							fx.EffectHandler = e => e.Source.GetMobilesInRange(e.Map, 0).ForEach(mob => MakeStinky(mob, pDuration, pFactor));
						}

						fx.Send();
					}
				}
				else if (StinkMode)
				{
					MakeStinky(m, pDuration, pFactor);
				}

				Products = 0;
			}
			else
			{
				m.SendMessage("You rummage around in the decomposing waste, but find nothing.");

				MakeStinky(m, TimeSpan.FromSeconds(60.0 * Utility.RandomDouble()), 0.10);
			}
		}

		protected virtual void MakeStinky(Mobile m, TimeSpan duration, double chance)
		{
			if (StinkMode && !m.Hidden && m.Alive && Utility.RandomDouble() <= chance)
			{
				ThrowableStinkBomb.MakeStinky(m, duration);
			}
		}

		protected virtual void OnProductionTimerTick()
		{
			InvalidateProperties();
			Delta(ItemDelta.Properties);

			VitaNexCore.TryCatch(Produce, TrashCollection.CMOptions.ToConsole);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			int version = writer.SetVersion(2);

			switch (version)
			{
				case 2:
					writer.Write(Token);
					goto case 1;
				case 1:
					{
						writer.Write(StinkMode);
						writer.Write(SwagMode);
						writer.Write(ProducesWaste);
					}
					goto case 0;
				case 0:
					{
						writer.Write(ProductionDelay);
						writer.Write(ProductionTimeBonus);
						writer.Write(LastProduction);
						writer.Write(Products);
						writer.Write(ProductsMax);
					}
					break;
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.GetVersion();

			switch (version)
			{
				case 2:
					Token = reader.ReadString();
					goto case 1;
				case 1:
					{
						StinkMode = reader.ReadBool();
						SwagMode = reader.ReadBool();
						ProducesWaste = reader.ReadBool();
					}
					goto case 0;
				case 0:
					{
						_ProductionDelay = reader.ReadTimeSpan();
						ProductionTimeBonus = reader.ReadTimeSpan();
						LastProduction = reader.ReadDateTime();
						Products = reader.ReadInt();
						ProductsMax = reader.ReadInt();
					}
					break;
			}

			if (UpdateTimer != null)
			{
				UpdateTimer.Running = true;
			}
			else
			{
				UpdateTimer = PollTimer.CreateInstance(TimeSpan.FromSeconds(1), OnProductionTimerTick);
			}
		}
	}

	public class SepticWaste : ThrowableStinkBomb
	{
		[Constructable]
		public SepticWaste()
			: this(1)
		{ }

		[Constructable]
		public SepticWaste(int amount)
			: base(amount)
		{
			Name = "Septic Waste";
			Token = "Property of the Septic Tank Co.";

			ItemID = 0x20E8;
			EffectID = ItemID;

			StinkyDuration += TimeSpan.FromSeconds(Utility.RandomMinMax(5, 10));

			ExplosionRange = 3;
			RequiredSkillValue = 0;
			ClearHands = false;
		}

		public SepticWaste(Serial serial)
			: base(serial)
		{ }

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.SetVersion(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.GetVersion();
		}
	}
}