using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMAW_DND
{
    public class StopWatchMilliseconds
    {
        // Token: 0x06000513 RID: 1299 RVA: 0x000167B4 File Offset: 0x000149B4
        public StopWatchMilliseconds()
        {
            this._stopwatch = new Stopwatch();
            this._stopwatch.Start();
        }

        // Token: 0x06000514 RID: 1300 RVA: 0x000167D4 File Offset: 0x000149D4
        public double Result()
        {
            this._stopwatch.Stop();
            return this._stopwatch.Elapsed.TotalMilliseconds;
        }

        // Token: 0x04000499 RID: 1177
        private Stopwatch _stopwatch;
    }
}
