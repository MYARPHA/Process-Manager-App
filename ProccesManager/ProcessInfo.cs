using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProccesManager
{

    // классы-модели
    public class ProcessInfo
    {
        public string ProcessName {  get; set; }
        public int Id { get; set; }
        public double MemoryUsage { get; set; }
    }

    public class AppInfo
    {
        public string AppName { get; set; }
        public string StartTime { get; set; }
    }
}
