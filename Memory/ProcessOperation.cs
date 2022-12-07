using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObisoftNet.Memory
{
    public enum ProcessOperation
    {
        VM_READ = 0x0010,
        VM_WRITE = 0x0020,
        VM_OPERATION = 0x0008
    }
}
