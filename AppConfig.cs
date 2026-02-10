using System;
using System.IO;
using System.Text.Json;

namespace MouseJiggler
{
    public sealed class AppConfig
    {
        public int Seconds { get; set; } = 30;
        public int Pixels { get; set; } = 2;

        public bool StartOnLaunch { get; set; } = true;
        public bool IdleAware { get; set; } = true;
        public int IdleThresholdSeconds { get; set; } = 15;

        public bool SafeMode { get; set; } = false;
        public int RandomJitterPercent { get; set; } = 15; // +/- %

        public bool IsFirstRun { get; set; } = true;

        private static string ConfigDir =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MouseJiggler");

        private static string ConfigPath =>
            Path.Combine(ConfigDir, "config.json");

        public static AppConfig Load()
        {
            try
            {
                if (!File.Exists(ConfigPath))
                {
                    return new AppConfig
                    {
                        StartOnLaunch = true,
                        IdleAware = true,
                        IdleThresholdSeconds = 15,
                        SafeMode = false,
                        RandomJitterPercent = 15,
                        IsFirstRun = true
                    };
                }

                var json = File.ReadAllText(ConfigPath);
                var cfg = JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
                cfg.IsFirstRun = false;
                return cfg;
            }
            catch
            {
                return new AppConfig { IsFirstRun = true };
            }
        }

        public static void Save(AppConfig cfg)
        {
            Directory.CreateDirectory(ConfigDir);
            cfg.IsFirstRun = false;

            cfg.Seconds = Math.Clamp(cfg.Seconds, 1, 3600);
            cfg.Pixels = Math.Clamp(cfg.Pixels, 1, 200);
            cfg.IdleThresholdSeconds = Math.Clamp(cfg.IdleThresholdSeconds, 1, 3600);
            cfg.RandomJitterPercent = Math.Clamp(cfg.RandomJitterPercent, 0, 80);

            var json = JsonSerializer.Serialize(cfg, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigPath, json);
        }
    }
}
