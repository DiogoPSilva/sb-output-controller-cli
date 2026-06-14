using System;
using System.IO;
using System.Diagnostics;
using SBOutputController;

class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: SBCtl.exe <headphones|speakers|toggle> [--direct] [--volume <0-100>]");
            return;
        }

        try
        {
            Directory.SetCurrentDirectory(SBController.SbConnectPath);
            var controller = new SBController(SBController.SbConnectPath);
            var devices = controller.GetDevices();

            if (devices.Count == 0)
            {
                Console.WriteLine("No devices found.");
                return;
            }

            DeviceWrapper device = null;
            foreach (var d in devices)
            {
                if (d.DeviceName == "G6") { device = d; break; }
            }
            if (device == null) device = devices[0];

            string mode = args[0].ToLower();
            bool directMode = false;
            int volume = -1;

            for (int i = 1; i < args.Length; i++)
            {
                if (args[i].ToLower() == "--direct") directMode = true;
                if (args[i].ToLower() == "--volume" && i + 1 < args.Length)
                {
                    int.TryParse(args[i + 1], out volume);
                    i++;
                }
            }

            switch (mode)
            {
                case "headphones":
                    controller.SwitchToOutputMode(device, DeviceOutputModes.Headphones);
                    Console.WriteLine("Switched to Headphones");
                    break;
                case "speakers":
                    controller.SwitchToOutputMode(device, DeviceOutputModes.Speakers);
                    Console.WriteLine("Switched to Speakers");
                    break;
                case "toggle":
                    var current = controller.GetOutputModeForDevice(device);
                    var next = current == DeviceOutputModes.Headphones ? DeviceOutputModes.Speakers : DeviceOutputModes.Headphones;
                    controller.SwitchToOutputMode(device, next);
                    Console.WriteLine("Switched to " + next);
                    break;
                default:
                    Console.WriteLine("Unknown mode. Use: headphones, speakers, or toggle");
                    return;
            }

            controller.SwitchDirectMode(device, directMode ? DirectModeStates.On : DirectModeStates.Off);
            Console.WriteLine("Direct Mode: " + (directMode ? "On" : "Off"));

            if (volume >= 0 && volume <= 100)
            {
                int nircmdVolume = (int)(volume / 100.0 * 65535);
                Process.Start(new ProcessStartInfo
                {
                    FileName = @"C:\Windows\System32\nircmd.exe",
                    Arguments = "setsysvolume " + nircmdVolume,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }).WaitForExit();
                Console.WriteLine("Volume set to " + volume + "%");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
