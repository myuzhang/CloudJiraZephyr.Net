namespace ZephyrCloudHelper.Net.Models
{
    /// <summary>
    /// Zephyr API credential and url info
    /// </summary>
    public class Zapi
    {
        // Zephyr user name
        public string User { get; set; }

        // Zephyr access key
        public string AccessKey { get; set; }

        // Zephyr secret key
        public string SecretKey { get; set; }
    }
}