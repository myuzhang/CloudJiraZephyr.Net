using System.Collections.Generic;

namespace ZephyrCloudHelper.Net.Models.Zephyr
{
    public class TestToCycle
    {
        public long projectId { get; set; }
        public long versionId { get; set; }
        public List<long> issues { get; set; }
        public string method { get; set; }
    }
}
