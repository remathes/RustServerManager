using CommunityToolkit.Mvvm.ComponentModel;
using RustServerManager.Models;
using System;

public class RustServerInstance
{
    public string BannerImagePath { get; set; }
    public string Identity { get; set; }
    public string Description { get; set; }
    public string ServerHostname { get; set; }
    public string ServerUrl { get; set; }
    public string ServerIp { get; set; }
    public int ServerPort { get; set; }
    public string RconIp { get; set; }
    public int RconPort { get; set; }
    public string RconPassword { get; set; }
    public int AppPort { get; set; }
    public int RconWeb { get; set; }
    public int MaxPlayers { get; set; }
    public int ServerTickrate { get; set; }
    public int ServerSaveInterval { get; set; }
    public bool UseCustomMap { get; set; }
    public string Level { get; set; }
    public int Seed { get; set; }
    public int WorldSize { get; set; }
    public string MapName { get; set; }
    public string ServerLevelUrl { get; set; }
    public string SteamCmdPath { get; set; }
    public string InstallDirectory { get; set; }
    public string RustDedicatedProcess { get; set; }
    public bool AutoStart { get; set; }
    public bool AutoUpdate { get; set; }
    public DateTime LastWiped {  get; set; }
    public int ProcessId { get; set; }
    public string ServerCfg { get; set; }
    public string MySqlHost { get; set; }
    public int MySqlPort { get; set; }
    public string MySqlUsername { get; set; }
    public string MySqlPassword { get; set; }
    public string MySqlDatabaseName { get; set; }
    public string Uptime { get; set; }
    public DatabaseConfig DatabaseConfig { get; set; }
    public bool EnableGracefulShutdown { get; set; }
    public int ShutdownDelaySeconds { get; set; }
    public string ShutdownMessageCommand { get; set; }
}