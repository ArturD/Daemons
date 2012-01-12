using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agents
{
    public static class Schedulers
    {
        public static DefaultScheduler BuildScheduler(int threads = 2)
        {
            return new DefaultScheduler(threads);
        }
    }
}
