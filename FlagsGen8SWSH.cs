﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PKHeX.Core;

namespace MissingEventFlagsCheckerPlugin
{
    internal class FlagsGen8SWSH : FlagsOrganizer
    {
        static string s_flagsList_res = null;

        protected override void InitFlagsData(SaveFile savFile)
        {
            m_savFile = savFile;

            if (s_flagsList_res == null)
            {
                s_flagsList_res = ReadFlagsListRes("flags_gen8swsh.txt");
            }

            AssembleList(s_flagsList_res);
        }

        protected override void AssembleList(string flagsList_res)
        {
            var savEventBlocks = (m_savFile as ISCBlockArray).Accessor;
            m_eventFlagsList.Clear();
            using (System.IO.StringReader reader = new System.IO.StringReader(flagsList_res))
            {
                string s = reader.ReadLine();
                do
                {
                    if (!string.IsNullOrWhiteSpace(s))
                    {
                        var flagDetail = new FlagDetail(s);
                        flagDetail.IsSet = (savEventBlocks.GetBlockSafe((uint)flagDetail.FlagIdx).Type == SCTypeCode.Bool2);
                        m_eventFlagsList.Add(flagDetail);
                    }

                    s = reader.ReadLine();

                } while (s != null);
            }
        }

        public override bool SupportsEditingFlag(FlagType flagType)
        {
            switch (flagType)
            {
                case FlagType.FieldItem:
                case FlagType.HiddenItem:
                case FlagType.TrainerBattle:
                    return true;

                default:
                    return false;
            }
        }

        public override void MarkFlags(FlagType flagType)
        {
            ChangeFlagsVal(flagType, value: true);
        }

        public override void UnmarkFlags(FlagType flagType)
        {
            ChangeFlagsVal(flagType, value: false);
        }

        void ChangeFlagsVal(FlagType flagType, bool value)
        {
            if (SupportsEditingFlag(flagType))
            {
                var blocks = (m_savFile as ISCBlockArray).Accessor;

                foreach (var f in m_eventFlagsList)
                {
                    if (f.FlagTypeVal == flagType)
                    {
                        f.IsSet = value;
                        blocks.GetBlockSafe((uint)f.FlagIdx).ChangeBooleanType(value ? SCTypeCode.Bool2 : SCTypeCode.Bool1);
                    }
                }
            }
        }

        public override void DumpAllFlags()
        {
            StringBuilder sb = new StringBuilder(100 * 1024);

            for (int i = 0; i < m_eventFlagsList.Count; ++i)
            {
#if DEBUG
                sb.AppendFormat("FLAG_0x{0:X8} {1}\t{2}\r\n", m_eventFlagsList[i].FlagIdx, m_eventFlagsList[i].IsSet,
                    m_eventFlagsList[i].FlagTypeVal == FlagType._Unused ? "UNUSED" : m_eventFlagsList[i].ToString());
#else
                sb.AppendFormat("FLAG_0x{0:X8} {1}\r\n", m_eventFlagsList[i].FlagIdx, m_eventFlagsList[i].IsSet);
#endif
            }

            System.IO.File.WriteAllText(string.Format("flags_dump_{0}.txt", m_savFile.Version), sb.ToString());
        }

        protected override bool ShouldExportEvent(FlagDetail eventDetail)
        {
            if (eventDetail.FlagTypeVal == FlagType.GeneralEvent)
            {
                bool shouldInclude = false;

                switch (eventDetail.FlagIdx)
                {
                    default:
                        shouldInclude = false;
                        break;
                }

                return shouldInclude;
            }
            else if (eventDetail.FlagTypeVal == FlagType.StaticBattle)
            {
                // temporary until figuring out which is which
                return false;
            }
            else
            {
                return base.ShouldExportEvent(eventDetail);
            }
        }

    }

}
