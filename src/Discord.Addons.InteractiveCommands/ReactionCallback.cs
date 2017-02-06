using System;
using System.Threading.Tasks;

namespace Discord.Addons.ReactionCallbacks
{
    public class ReactionCallback
    {
        public bool ResumeAfterExecution { get; set; }

        public Func<IUser, Task> Callback { get; set; }
    }
}
