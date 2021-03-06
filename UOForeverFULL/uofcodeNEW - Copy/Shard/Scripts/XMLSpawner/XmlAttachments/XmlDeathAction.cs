using System;
using Server;
using Server.Items;
using Server.Network;
using Server.Mobiles;
using System.Collections;

namespace Server.Engines.XmlSpawner2
{
	public class XmlDeathAction : XmlAttachment
	{
		private string m_Action;    // action string
		private string m_Condition;    // condition string

		[CommandProperty(AccessLevel.GameMaster)]
		public string Action { get { return m_Action; } set { m_Action = value; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public string Condition { get { return m_Condition; } set { m_Condition = value; } }

		// These are the various ways in which the message attachment can be constructed.  
		// These can be called via the [addatt interface, via scripts, via the spawner ATTACH keyword.
		// Other overloads could be defined to handle other types of arguments

		// a serial constructor is REQUIRED
		public XmlDeathAction(ASerial serial)
			: base(serial)
		{
		}

		[Attachable]
		public XmlDeathAction(string action)
		{
			Action = action;
		}

		[Attachable]
		public XmlDeathAction()
		{
		}


		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)1);
			// version 1
			writer.Write(m_Condition);
			// version 0
			writer.Write(m_Action);

		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
			switch (version)
			{
				case 1:
					m_Condition = reader.ReadString();
					goto case 0;
				case 0:
					m_Action = reader.ReadString();
					break;
			}

		}

		public override void OnAttach()
		{
			base.OnAttach();

			if (AttachedTo is Item)
			{
				// dont allow item attachments
				Delete();
			}

		}

		public override bool HandlesOnKilled { get { return true; } }

        public override void OnKilled(Mobile killed, Mobile killer)
        {
            base.OnKilled(killed, killer);

            if (killed == null) return;

            // now check for any conditions as well
            // check for any condition that must be met for this entry to be processed
            if (!BaseXmlSpawner.CheckCondition(Condition, killer, killed) || killed == null)
                return;

            BaseXmlSpawner.ExecuteActions(killer, killed, Action);
        }
	}
}
