using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    public interface ISignalHandler
    {
        SignalInfo ListenSignal(long flags, long objectId);
        void PostSignal(SignalInfo signal);
    }
}
