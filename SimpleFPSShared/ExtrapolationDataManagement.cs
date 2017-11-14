using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleFPSShared
{
    public class ExtrapolationDataManagement
    {
        public const int MAX_EXTRPOLATIONDATA = 100;

        public List<SFPSPlayerCommand> Cmds = new List<SFPSPlayerCommand>();

        public void AddExtrapolationData(SFPSPlayerCommand cmd)
        {
            Cmds.Add(cmd);
            Cmds.Sort(((cmd1, cmd2) => cmd1.EstimatedServerTime.CompareTo(cmd2.EstimatedServerTime)));
            while (Cmds.Count > MAX_EXTRPOLATIONDATA)
            {
                Cmds.RemoveAt(0);
            }
        }

        public List<SFPSPlayerCommand> GetAllCmdsAfterServerTime(DateTime serverTime)
        {
            return Cmds.Where(x => x.EstimatedServerTime > serverTime).ToList();
        }
    }
}
